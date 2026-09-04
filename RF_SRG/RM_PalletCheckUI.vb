Imports AIT.RF
Imports AIT.WMS

Module RM_PalletCheckUI

#Region "PUBLIC"
    Dim dtScreen As New DataScreen
    Dim elapseStart As Date = Now

    Public Sub Main()
        eActiveUI = RFUiList.PALLET_CHECK
        elapseStart = Now()
        dtScreen = New DataScreen
        p_main()
    End Sub

    Public Sub GoToPrevScreen()
        p_gotoPrevScreen()
    End Sub

#End Region

#Region "PRIVATE"
    Private Sub p_gotoPrevScreen()
        If dtScreen.PalletId = String.Empty Then
            GotoUi(RFUiList.PICKING)
        Else
            Main()
        End If
        GeneralUI.MainMenu()
    End Sub

    Private Sub p_main(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()

            PrintHeader(eActiveUI)

            If dtScreen.PalletId.Length < 17 Then
                PrintLine(1, 4, String.Format("PALLET ID  : {0}", dtScreen.PalletId))
            Else
                PrintLine(1, 4, String.Format("PALLET ID  : {0}", dtScreen.PalletId.Substring(0, 16)))
            End If

            PrintLine(1, 5, String.Format("QR INBOUND : {0}", dtScreen.SkuScanned))
            p_printOutstandingSummaryInfo()

            If dtScreen.PalletId = String.Empty Or dtScreen.alDetail.Count = 0 Then
                PrintMessage(sMessage, bError)
                PrintFooterWithElapse(elapseStart)
                p_scanQRPalletOutbound(13, 4)
                p_dataPreparation()
            End If

            Dim iLastRow As Int16 = 9
            p_printList(iLastRow)
            p_printOutstandingSummaryInfo()
            PrintMessage(sMessage, bError, iLastRow)
            PrintFooterWithElapse(elapseStart, iLastRow + 1)

            PrintLine(13, 5, New String(" ", 1), , True)

            Dim sMsg As String = String.Empty, bErr As Boolean = False
            p_scanQRInbound(13, 5)

            If p_save() Then
                sMsg = MESSAGE_INFO_SAVE_SUCCESS
            End If

            Dim sMsgUpdateScanList As String = String.Empty
            p_updateScanList(sMsgUpdateScanList)
            If Not sMsgUpdateScanList = String.Empty Then
                bErr = True
                sMsg = sMsgUpdateScanList
            End If

            p_main(sMsg, bErr)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_printOutstandingSummaryInfo()
        PrintLineWithBgColor(1, 7, String.Format("Hasil Scan Sku: {0} Of {1}", dtScreen.SkuCountScanned.ToString("N0"), dtScreen.SkuCount.ToString("N0")), ConsoleColor.Blue, ConsoleColor.White)
        'PrintLine(1, 8, String.Format("QTY SCAN   : {0} / {1}", dtScreen.QtyTotalScanned.ToString("N0"), dtScreen.QtyTotal.ToString("N0")))
    End Sub

    Private Function p_delete() As Boolean
        elapseStart = Now
        UIWait(MESSAGE_INFO_UPDATING)
        Try
            Dim o As New RM_PalletCheck
            o.DeleteByPalletId(dtScreen.PalletId, dtConnInfo)
            p_saveLog("DELETE", Log.LogStatus.SUCCESS.ToString())
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return True
    End Function

    Private Function p_save() As Boolean
        elapseStart = Now
        UIWait(MESSAGE_INFO_UPDATING)
        Try
            Dim o As New RM_PalletCheck
            o.Save(p_setDataPalletCheck(), dtConnInfo)
            p_saveLog(LOG_EVENT_REGISTER_QRINBOUND, Log.LogStatus.SUCCESS.ToString())
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return True
    End Function

    Private Sub p_dataPreparation()
        If dtScreen.alDetail.Count > 0 Then Exit Sub
        elapseStart = Now
        Dim alStock As New ArrayList
        alStock = p_getListStockStorer()
        If alStock.Count = 0 Then
            p_main(MESSAGE_INFO_DATA_NOTFOUND, True)
        Else
            dtScreen.SkuCount = alStock.Count
            For Each dtStock As RM_PalletStock.DataPalletStock In alStock
                Dim dataDetail As New DataScreenDetail
                dataDetail.StorerKey = dtStock.StorerKey
                dataDetail.Sku = dtStock.Sku
                dataDetail.QtyWms = dtStock.Qty
                dataDetail.QtyOutstanding = dtStock.Qty
                dtScreen.alDetail.Add(dataDetail)
                dtScreen.QtyTotal += dtStock.Qty
            Next
        End If

        Dim bSkuFound As Boolean = False
        Dim alPalletCheck As ArrayList = New RM_PalletCheck().GetListByPalletId(dtScreen.PalletId, dtConnInfo)
        If alPalletCheck.Count > 0 Then
            For Each dataScanned As RM_PalletCheck.DataPalletCheck In alPalletCheck
                bSkuFound = False
                For Each dataDetail As DataScreenDetail In dtScreen.alDetail
                    If dataScanned.Sku = dataDetail.Sku And dataScanned.StorerKey = dataDetail.StorerKey Then
                        dataDetail.QtyScanned += dataScanned.Qty
                        dataDetail.QtyOutstanding = dataDetail.QtyWms - dataDetail.QtyScanned
                        bSkuFound = True
                        Exit For
                    End If
                Next
                If Not bSkuFound Then
                    Dim dataScanExceedSku As New DataScreenDetail
                    dataScanExceedSku.StorerKey = dataScanned.StorerKey
                    dataScanExceedSku.Sku = dataScanned.Sku
                    dataScanExceedSku.QtyScanned = dataScanned.Qty
                    dataScanExceedSku.QtyOutstanding = dataScanExceedSku.QtyWms - dataScanned.Qty
                    dtScreen.alDetail.Add(dataScanExceedSku)
                End If
            Next
        End If

        p_main()
    End Sub

    Private Sub p_updateScanList(ByRef sMessage As String)
        Dim iCountSkuScanned As Int16 = 0, bSkuFound As Boolean = False, bOverScan As Boolean = False
        For Each dtDetail As DataScreenDetail In dtScreen.alDetail
            If dtDetail.Sku = dtScreen.SkuScanned Then
                dtDetail.QtyScanned += dtScreen.QtyScanned
                dtDetail.QtyOutstanding = dtDetail.QtyWms - dtDetail.QtyScanned
                dtScreen.QtyTotalScanned += dtScreen.QtyScanned

                If dtDetail.QtyOutstanding < 0 Then bOverScan = True
                bSkuFound = True
            End If

            If dtDetail.QtyScanned > 0 Then iCountSkuScanned += 1
        Next
        dtScreen.SkuCountScanned = iCountSkuScanned

        If bSkuFound = False Then
            Dim dataScan As New DataScreenDetail
            dataScan.Sku = dtScreen.SkuScanned
            dataScan.QtyScanned = dtScreen.QtyScanned
            dataScan.QtyOutstanding = dataScan.QtyWms - dtScreen.QtyScanned
            dtScreen.alDetail.Add(dataScan)

            If Not sMessage = String.Empty Then
                UIWarning(MESSAGE_INFO_SKU_NOT_FOUND)
                sMessage = MESSAGE_INFO_SKU_NOT_FOUND
            End If
        End If

        If bOverScan And Not sMessage = String.Empty Then
            UIWarning(MESSAGE_INFO_OVERSCAN)
            sMessage = MESSAGE_INFO_OVERSCAN
        End If
    End Sub

    Private Function p_getListStockStorer() As ArrayList
        UIWait(MESSAGE_INFO_PREPARING_WMSDATA)

        Dim alData As New ArrayList

        Dim searchDataItrn As New Itrn.DataItrn
        searchDataItrn.TranType = "MV"
        searchDataItrn.StorerKey = dtUserActive.StorerKey
        searchDataItrn.ToLoc = dtScreen.Loc
        searchDataItrn.ToID = dtScreen.PalletId

        Dim alDataWms As ArrayList = New Itrn().GetSumQtyByTrantypeStorerkeySkuTolocToid(searchDataItrn, dtConnInfo)
        If alDataWms.Count > 0 Then
            For Each dataWms As Itrn.DataItrn In alDataWms
                Dim dataPalletStock As New RM_PalletStock.DataPalletStock
                dataPalletStock.PalletId = dataWms.ToID
                dataPalletStock.StorerKey = dataWms.StorerKey
                dataPalletStock.Sku = dataWms.Sku
                dataPalletStock.Qty = dataWms.Qty
                dataPalletStock.UserId = dtUserActive.UserId
                alData.Add(dataPalletStock)
            Next

            Dim oPalletStock As New RM_PalletStock
            oPalletStock.DeleteInsert(dtScreen.PalletId, alData, dtConnInfo)
            Return alData
        End If

        Return New RM_PalletStock().GetListByPalletId(dtScreen.PalletId, dtUserActive.StorerKey, dtConnInfo)
    End Function

    Private Sub p_scanQRInbound(iLeft As Integer, iTop As Integer)

        Dim dtQrContent As New DataScan
        Dim dataQRi As DataQRInbound = GetDataQRInbound(iLeft, iTop)
        dtScreen.StorerKeyScanned = dataQRi.StorerKey
        dtScreen.SkuScanned = dataQRi.Sku
        dtScreen.QtyScanned = dataQRi.Qty
        dtScreen.LabelID = dataQRi.ID

        If Not p_isStorerInTheList() Then
            UIWarning(MESSAGE_INFO_INVALID_STORER, False)
            p_main(MESSAGE_INFO_INVALID_STORER, True)
        End If
        If dtScreen.SkuScanned = String.Empty Then p_main(MESSAGE_INFO_INVALID_SKU, True)

        Dim dtLogForValidation As New Log.DataLog
        dtLogForValidation.LabelID = dataQRi.ID
        dtLogForValidation.MenuID = eActiveUI.ToString()
        If New Log().IsLabelIDExists(dtLogForValidation, dtConnInfo) Then
            UIWarning(MESSAGE_INFO_QR_IS_USED, False)
            p_main(MESSAGE_INFO_QR_IS_USED, True)
        End If

    End Sub

    Private Function p_isStorerInTheList() As Boolean
        Select Case dtScreen.StorerKeyScanned
            Case "PLBSAJ001"
                Return True
            Case "PLBSAJ01A"
                Return True
            Case "PLBSAJ01B"
                Return True
            Case "PLBSAM001"
                Return True
            Case "PLBSAM01A"
                Return True
            Case "PLBSAM01B"
                Return True
        End Select
        Return False
    End Function

    Private Sub p_scanQRPalletOutbound(iLeft As Integer, iTop As Integer)
        Dim dtQrContent As New DataScan
        dtQrContent = GetScanQRPalletOutbound(iLeft, iTop, 1)
        dtScreen.PalletId = dtQrContent.Component0
        dtScreen.Loc = dtQrContent.Component1
        If dtScreen.Loc = String.Empty Then dtScreen.Loc = "ORDER"
        If dtScreen.PalletId = String.Empty Then p_main()
    End Sub

    Private Sub p_printList(ByRef iTop As Int16)
        p_sortList()
        Dim alDetail As ArrayList = dtScreen.alDetail

        Dim iMaxLenSku As Int16 = 0
        For Each data As DataScreenDetail In alDetail
            If iMaxLenSku < data.Sku.Length Then iMaxLenSku = data.Sku.Length
        Next

        'header:
        Dim sHeaderSku = ("SKU").PadRight(iMaxLenSku, " ")
        Dim sHeaderQtyWms = ("QTY WMS").PadLeft(7, " ")
        Dim sHeaderQtyScan = ("QTY SCAN").PadLeft(8, " ")
        Dim sHeaderStatus = ("STATUS").PadRight(11, " ")
        PrintLineWithBgColor(1, iTop, String.Format("{0} | {1} | {2} | {3}", sHeaderSku, sHeaderQtyWms, sHeaderQtyScan, sHeaderStatus), ConsoleColor.White, ConsoleColor.Black)
        iTop += 1
        PrintLineWithBgColor(1, iTop, New String("-", iMaxLenSku + 26 + 9), ConsoleColor.White, ConsoleColor.Black)
        iTop += 1

        'detail:
        For Each data As DataScreenDetail In alDetail
            Dim sQtyWms As String = String.Empty, sQtyScan As String = String.Empty
            sQtyWms = data.QtyWms.ToString("N0")
            sQtyScan = data.QtyScanned.ToString("N0")

            Dim bgColor As ConsoleColor = ConsoleColor.White
            Dim fgColor As ConsoleColor = ConsoleColor.Black
            Dim sStatus As String = String.Empty
            If data.QtyScanned = 0 Then
                bgColor = ConsoleColor.Yellow
                sStatus = "BELUM SCAN"
            Else
                If data.QtyWms <> data.QtyScanned Then
                    bgColor = ConsoleColor.Red
                    sStatus = "NOT MATCH"
                    dtScreen.SkuCountScanned += 1
                Else
                    sStatus = "MATCH"
                    dtScreen.SkuCountScanned += 1
                End If
            End If

            PrintLineWithBgColor(1, iTop, String.Format("{0} | {1} | {2} | {3}", data.Sku.PadRight(iMaxLenSku, " "), sQtyWms.PadLeft(7, " "), sQtyScan.PadLeft(8, " "), sStatus.PadRight(11, " ")), bgColor, fgColor)
            iTop += 1
        Next

        PrintLineWithBgColor(1, iTop, New String("-", iMaxLenSku + 26 + 9), ConsoleColor.White, ConsoleColor.Black)
        iTop += 1
    End Sub

    Private Sub p_sortList()
        Dim alSorted As New ArrayList
        Dim alDetail As ArrayList = dtScreen.alDetail

        '0 scan:
        For Each dataDetail As DataScreenDetail In alDetail
            If dataDetail.QtyScanned = 0 Then
                alSorted.Add(dataDetail)
            End If
        Next

        For Each dataSorted As DataScreenDetail In alSorted
            alDetail.Remove(dataSorted)
        Next

        'not match:
        For Each dataDetail As DataScreenDetail In alDetail
            If dataDetail.QtyOutstanding <> 0 Then
                alSorted.Add(dataDetail)
            End If
        Next

        For Each dataSorted As DataScreenDetail In alSorted
            alDetail.Remove(dataSorted)
        Next

        'else:
        For Each dataDetail As DataScreenDetail In alDetail
            alSorted.Add(dataDetail)
        Next

        dtScreen.alDetail = alSorted
    End Sub

    Private Function p_setDataPalletCheck() As RM_PalletCheck.DataPalletCheck
        Dim dataPalletCheck As New RM_PalletCheck.DataPalletCheck
        dataPalletCheck.PalletId = dtScreen.PalletId
        dataPalletCheck.StorerKey = dtScreen.StorerKeyScanned
        dataPalletCheck.Sku = dtScreen.SkuScanned
        dataPalletCheck.Qty = dtScreen.QtyScanned
        dataPalletCheck.UserId = dtUserActive.UserId

        Return dataPalletCheck
    End Function

    Private Sub p_saveLog(sEvent As String, sStatus As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = sEvent
        If sEvent = LOG_EVENT_REGISTER_QRINBOUND Then dtLog.LabelID = dtScreen.LabelID
        dtLog.Scan1 = dtScreen.PalletId
        dtLog.Status = sStatus
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtUserActive.StorerKey
        SaveLog(dtLog)
    End Sub

#End Region

#Region "Data Class"

    Public Const LOG_EVENT_REGISTER_QRINBOUND As String = "QRI_REG"
    Public Class DataScreen
        Public LabelID As String = String.Empty
        Public QrCode As String = String.Empty
        Public Loc As String = String.Empty
        Public PalletId As String = String.Empty
        Public StorerKeyScanned As String = String.Empty
        Public SkuScanned As String = String.Empty
        Public SkuCount As Int32 = 0
        Public SkuCountScanned As Int32 = 0
        Public QtyScanned As Int32 = 0
        Public QtyTotal As Int32 = 0
        Public QtyTotalScanned As Int32 = 0
        Public alDetail As New ArrayList
    End Class

    Public Class DataScreenDetail
        Public Sku As String = String.Empty
        Public StorerKey As String = String.Empty
        Public QtyWms As Int32 = 0
        Public QtyScanned As Int32 = 0
        Public QtyOutstanding As Int32 = 0
    End Class

#End Region

End Module
