Imports AIT.RF
Imports AIT.WMS

Module FG_PickCheckUI
#Region "Public"
    Dim dtScreen As New DataScreen
    Dim elapseStart As Date
    Dim elapseMilliSecond As Double = 0
    Const MAX_TOP_PRINTLIST As Int16 = 16

    Public Sub Main()
        p_New()
    End Sub

    Public Sub GotoPrevScreen()
        p_gotoPrevScreen()
    End Sub
#End Region
#Region "Private"
    Private Sub p_New()
        p_Menu()
    End Sub

    Private Sub p_Menu()
        eActiveUI = RFUiList.FG_PICK_CHECK
        dtScreen = New DataScreen
        p_Main()
    End Sub

    Private Sub p_Main(Optional sMessage As String = "", Optional bError As Boolean = False, Optional bPrintList As Boolean = True)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

            If bError Then p_saveLog(sMessage, "WARNING")

            PrintLine(1, 4, "INVOICE :")
            PrintLine(11, 4, dtScreen.ExternOrderKey)
            PrintLine(1, 5, "PALLET  :")
            PrintLine(11, 5, dtScreen.PalletID.Replace(" ", ""))
            PrintLine(1, 6, "CONFIRM :")
            PrintLine(11, 6, String.Format("{0} of {1}", dtScreen.TotalScan.ToString(), dtScreen.Total.ToString()))
            PrintLine(1, 7, "-------outstanding-------")

            PrintLine(1, 8, "SCAN QR :")
            If bPrintList Then p_printList(9)
            If dtScreen.ExternOrderKey = String.Empty Then
                PrintFooter()
            Else
                p_setElapseTime()
                PrintFooterWithElapse(elapseMilliSecond)
                elapseMilliSecond = 0
            End If

            If sMessage = STATUS_COMPLETED Then
                WaitAnyKey(sMessage, 17)
                FG_LabelCheckUI.Main(eActiveUI, dtScreen.alData)
            ElseIf sMessage = STATUS_RESCAN Then
                If GetInputEnterOrEscapeOnly(Len(STATUS_RESCAN) + 1, 17) Then
                    Dim o As New FG_PickedChecking
                    o.DeleteByPallet(dtScreen.PalletID, dtConnInfo)
                    p_saveLog("DELETE", Log.LogStatus.SUCCESS.ToString())
                    dtScreen.alData = New ArrayList
                    dtScreen.TotalScan = 0
                    p_Main(STATUS_SCANNED_DELETED)
                Else
                    p_Menu()
                End If
            End If

            If dtScreen.Total = 0 Or dtScreen.TotalScan < dtScreen.Total Then p_scanAndValidate(11, 8)

            ClearMessage()
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try

    End Sub

    Private Sub p_gotoPrevScreen()
        GotoUi(RFUiList.MENU_SRGPLB_10)
    End Sub

    Private Sub p_printList(iTop As Int16)
        'DEFAULT_HEIGHT_WINDOW - 3
        For i As Int16 = iTop To MAX_TOP_PRINTLIST
            PrintLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Next

        For Each dtPickDetail As PickDetail.DataPickDetail In dtScreen.alDataPickDetail
            If iTop = MAX_TOP_PRINTLIST Then Exit Sub
            PrintLine(1, iTop, String.Format("{0} #{1} Q{2}", dtPickDetail.Sku, dtPickDetail.Lottable10, dtPickDetail.Qty.ToString()))
            iTop += 1
        Next

        If iTop = MAX_TOP_PRINTLIST Then Exit Sub
    End Sub

    Private Sub p_saveLog(sEvent As String, sStatus As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = sEvent
        dtLog.Scan1 = dtScreen.ExternOrderKey
        dtLog.Scan2 = dtScreen.PalletID
        dtLog.Scan3 = dtScreen.QrCode
        dtLog.Status = sStatus
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtScreen.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_dataPreparation()
        UIWait("Data Initiation.....")

        dtScreen.alDataAsn = New Receipt().GetListDetailByNotesWithListPickDetail(dtScreen.QrCode, dtConnInfoWms, dtScreen.alDataPickDetail)

        If dtScreen.alDataPickDetail.Count = 0 Then p_Main("PICKDETAIL N/A", True, False)
        If dtScreen.alDataAsn.Count = 0 Then p_Main("ASN N/A", True, False)
        For Each dataPickDetail As PickDetail.DataPickDetail In dtScreen.alDataPickDetail
            If dataPickDetail.OrderStatus = FG_PickDetail.STATUS_ORDER_SHIPPED Then
                p_Main("ORDER SHIPPED", True, False)
            Else
                dtScreen.OrderKey = dataPickDetail.OrderKey
                dtScreen.ExternOrderKey = dataPickDetail.ExternOrderKey
                dtScreen.PalletID = dataPickDetail.ID
                dtScreen.StorerKey = dataPickDetail.StorerKey
                dtScreen.Total = dtScreen.alDataPickDetail.Count
            End If
            Exit For
        Next

        dtScreen.alData = New FG_PickedChecking().GetListByPalletId(dtScreen.PalletID, dtConnInfo)
        dtScreen.TotalScan = dtScreen.alData.Count

        If dtScreen.TotalScan = 0 Then
            p_Main()
        Else
            p_Main(STATUS_RESCAN, True, False)
        End If
    End Sub

    Private Sub p_scanAndValidate(iLeft As Integer, iTop As Integer)
        Try
            dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
            If dtScreen.QrCode = String.Empty Then
                p_Main("EMPTY NOT ALLOW", True)
            End If

            elapseStart = Now()

            If dtScreen.PalletID = String.Empty Then
                p_dataPreparation()
            Else
                If dtScreen.QrCode.Length < EMPTY_BOX_NAME.Length Then p_Main("WRONG SCAN", True)
                If dtScreen.QrCode.Substring(0, EMPTY_BOX_NAME.Length).ToUpper() = EMPTY_BOX_NAME Then p_emptyBoxHandler()

                For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
                    If data.QR = dtScreen.QrCode Then p_Main("ALREADY SCANNED", True)
                Next

                Dim dtReceiptDetail As New Receipt.DataReceiptDetail
                For Each dtAsn As Receipt.DataReceiptDetail In dtScreen.alDataAsn
                    If dtAsn.Notes.TrimEnd = dtScreen.QrCode.TrimEnd Then
                        dtReceiptDetail = dtAsn
                        Exit For
                    End If
                Next

                'try to get from db since some have diff char set after set data
                If dtReceiptDetail.ReceiptKey = String.Empty Then dtReceiptDetail = New Receipt().GetDataDetailByNotes(dtScreen.QrCode, dtConnInfoWms)

                If dtReceiptDetail.ReceiptKey = String.Empty Then p_Main("DIFF PALLET", True)
                If dtReceiptDetail.CharIdxQtyInQr = 0 Then p_Main("INVLD QTY IN QR", True)

                Dim dtPickDetail As New PickDetail.DataPickDetail
                For Each dtPDetail As PickDetail.DataPickDetail In dtScreen.alDataPickDetail
                    'If dtPDetail.Sku = dtReceiptDetail.Sku And dtPDetail.Lottable09 = dtReceiptDetail.Lottable09 And dtPDetail.Lottable10 = dtReceiptDetail.Lottable10 Then
                    If dtPDetail.Sku = dtReceiptDetail.Sku And dtPDetail.Lot = dtReceiptDetail.ToLot Then
                        dtPickDetail = dtPDetail
                        Exit For
                    End If
                Next
                If dtReceiptDetail.QtyReceived <> dtPickDetail.Qty Then p_Main("INVLD QTY", True)

                dtScreen.Sku = dtPickDetail.Sku
                dtScreen.Qty = dtPickDetail.Qty
                p_saveToList(dtPickDetail)
                p_findAndRemoveList(dtPickDetail)

                Dim iTotalScan As Int16 = 0
                For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
                    If data.QR.Substring(0, EMPTY_BOX_NAME.Length).ToUpper() <> EMPTY_BOX_NAME Then iTotalScan += 1
                Next

                dtScreen.TotalScan = iTotalScan
                If dtScreen.TotalScan = dtScreen.Total Then
                    p_Main(STATUS_COMPLETED)
                Else
                    p_Main()
                End If

            End If

        Catch ex As Exception
            Throw
        End Try
    End Sub

    'Private Sub p_scanAndValidateQr(iLeft As Integer, iTop As Integer)
    '    Try
    '        dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
    '        If dtScreen.QrCode = String.Empty Then
    '            p_Main("EMPTY NOT ALLOW", True)
    '        End If

    '        elapseStart = Now()
    '        Dim dataInList As FG_PickedChecking.DataFGPickedChecking = p_findPickCheckInList(dtScreen.QrCode)
    '        If dataInList.ID <> String.Empty Then
    '            p_Main("ALREADY SCANNED", True)
    '        End If

    '        Dim data As FG_PickedChecking.DataFGPickedChecking = New FG_PickedChecking().GetData(dtScreen.QrCode)
    '        If data.ID <> String.Empty Then
    '            Dim dataOrderStatus As FG_PickDetail.DataFGPickDetail = New FG_PickDetail().GetOrderStatus(data.ExternOrderKey)
    '            If dataOrderStatus.Status <> FG_PickDetail.STATUS_ORDER_SHIPPED Then p_Main("ALREADY SCANNED", True)
    '        End If

    '        If dtScreen.QrCode.Length < EMPTY_BOX_NAME.Length Then p_Main("WRONG SCAN", True)
    '        If dtScreen.QrCode.Substring(0, EMPTY_BOX_NAME.Length).ToUpper() = EMPTY_BOX_NAME Then p_emptyBoxHandler()

    '        'Dim alAsn As ArrayList = New FG_QRSource().GetListByQR(dtScreen.QrCode)
    '        'If alAsn.Count = 0 Then
    '        '    p_Main("SKU N/A", True)
    '        'End If

    '        Dim dtQR As FG_QRSource.DataQRSource = New FG_QRSource().GetDataByQR(dtScreen.QrCode)
    '        If dtQR.Sku = String.Empty Then
    '            p_Main("SKU N/A", True)
    '        End If

    '        Dim dtPD1st As FG_PickDetail.DataFGPickDetail = New FG_PickDetail().GetDataBySkuLot(dtQR.Sku, dtQR.Lot)
    '        'For Each dtAsn As FG_QRSource.DataQRSource In alAsn
    '        '    dtPD1st = New FG_PickDetail().GetDataBySkuLot(dtAsn.Sku, dtAsn.Lot)

    '        '    If dtPD1st.OrderKey <> String.Empty Then
    '        '        dtQR = dtAsn
    '        '        Exit For
    '        '    End If
    '        'Next

    '        If dtScreen.OrderKey = String.Empty Then
    '            If dtPD1st.OrderKey = String.Empty Then
    '                p_Main("PICKDETAIL N/A", True)
    '            Else
    '                dtScreen.alDataPickDetail = p_GetPickList(dtPD1st.PalletId)
    '                If dtScreen.alDataPickDetail.Count = 0 Then
    '                    p_Main("PICKLIST N/A", True)
    '                End If
    '            End If
    '        End If

    '        If dtQR.CharIdxQtyInQr = 0 Then
    '            p_Main("INVLD QTY IN QR", True)
    '        End If

    '        Dim dtPickDetail As FG_PickDetail.DataFGPickDetail = p_findPickDetailInList(dtQR)
    '        If dtPickDetail.OrderKey = String.Empty Then
    '            p_Main("PICKDETAIL N/A", True)
    '        End If

    '        If dtQR.QtyReceived <> dtPickDetail.Qty Then
    '            p_Main("INVALID QTY", True)
    '        End If

    '        Dim bFirst As Boolean = False
    '        If dtScreen.OrderKey = String.Empty Then
    '            bFirst = True
    '            dtScreen.OrderKey = dtPickDetail.OrderKey
    '        End If
    '        If dtScreen.ExternOrderKey = String.Empty Then dtScreen.ExternOrderKey = dtPickDetail.ExternOrderKey
    '        If dtScreen.ExternOrderKey <> dtPickDetail.ExternOrderKey Then
    '            p_Main("DIFF INVOICE", True)
    '        End If

    '        If dtScreen.PalletID = String.Empty Then dtScreen.PalletID = dtPickDetail.PalletId
    '        If dtScreen.PalletID <> dtPickDetail.PalletId Then
    '            p_Main("DIFF PALLET", True)
    '        End If

    '        If dtScreen.Total = 0 Then dtScreen.Total = dtScreen.alDataPickDetail.Count

    '        dtScreen.Sku = dtPickDetail.Sku
    '        dtScreen.Qty = dtPickDetail.Qty
    '        dtScreen.StorerKey = dtPickDetail.StorerKey

    '        If Not bFirst Then
    '            p_saveToList(dtPickDetail)
    '            p_findAndRemoveList(dtQR)
    '            dtScreen.TotalScan = dtScreen.Total - dtScreen.alDataPickDetail.Count
    '        End If

    '        If dtScreen.TotalScan = dtScreen.Total Then
    '            p_Main(STATUS_COMPLETED, False)
    '        Else
    '            p_Main()
    '        End If
    '    Catch ex As Exception
    '        Throw
    '    End Try
    'End Sub

    Private Sub p_setElapseTime()
        Dim elapse As TimeSpan = Now() - elapseStart
        elapseMilliSecond = elapse.TotalMilliseconds
    End Sub

    Private Function p_findPickCheckInList(QR As String) As FG_PickedChecking.DataFGPickedChecking
        For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If data.QR = QR Then
                Return data
            End If
        Next

        Return New FG_PickedChecking.DataFGPickedChecking
    End Function

    Private Function p_findPickDetailInList(dtQR As FG_QRSource.DataQRSource) As FG_PickDetail.DataFGPickDetail
        For Each dtPickDetail As FG_PickDetail.DataFGPickDetail In dtScreen.alDataPickDetail
            If dtPickDetail.Sku = dtQR.Sku And dtPickDetail.Lottable10 = dtQR.Lottable10 Then
                'And dtPickDetail.Lottable09 = dtQR.Lottable09 
                Return dtPickDetail
            End If
        Next

        Return New FG_PickDetail.DataFGPickDetail
    End Function

    Private Sub p_findAndRemoveList(dtPickDetail As PickDetail.DataPickDetail)
        Dim bFound As Boolean = False
        Dim xAlData As New ArrayList
        xAlData = dtScreen.alDataPickDetail
        Dim i As Int16 = 0
        For Each xdtPickDetail As PickDetail.DataPickDetail In xAlData
            If xdtPickDetail.Sku = dtPickDetail.Sku And xdtPickDetail.Lottable09 = dtPickDetail.Lottable09 And xdtPickDetail.Lottable10 = dtPickDetail.Lottable10 Then
                bFound = True
                xAlData.RemoveAt(i)
                Exit For
            End If
            i += 1
        Next

        If bFound Then dtScreen.alDataPickDetail = xAlData
    End Sub

    Private Const EMPTY_BOX_NAME As String = "EMPTY"
    Private Const EMPTY_POLYTAINER_NAME As String = "EMPTY POLYTAINER"

    Private Sub p_emptyBoxHandler()
        If dtScreen.OrderKey = String.Empty Then
            p_Main("INVALID 1ST SCAN", True)
        End If

        If dtScreen.QrCode = EMPTY_POLYTAINER_NAME Then
            p_Main("INVALID QR SCAN", True)
        End If

        dtScreen.Sku = EMPTY_POLYTAINER_NAME
        dtScreen.Qty = 1

        p_saveToList(New PickDetail.DataPickDetail)
        p_Main("EMPTYBOX")
    End Sub

    Private Sub p_save()
        Dim o As New FG_PickedChecking
        o.Save(dtScreen.alData, dtConnInfo)
    End Sub

    Private Sub p_saveToList(dtPickDetail As PickDetail.DataPickDetail)
        Dim data As New FG_PickedChecking.DataFGPickedChecking
        data.QR = dtScreen.QrCode
        data.OrderKey = dtScreen.OrderKey
        data.ExternOrderKey = dtScreen.ExternOrderKey
        data.PalletId = dtScreen.PalletID
        data.CaseId = dtPickDetail.PickDetailKey
        data.Sku = dtScreen.Sku
        data.Qty = dtScreen.Qty
        data.Lottable09 = dtPickDetail.Lottable09
        data.Lottable10 = dtPickDetail.Lottable10
        data.StorerKey = dtScreen.StorerKey
        data.UserId = dtUserActive.UserId

        Dim bExistsInList As Boolean = False
        For Each dataScanned As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If dataScanned.QR = data.QR Then
                bExistsInList = True
                Exit For
            End If
        Next

        If Not bExistsInList Then dtScreen.alData.Add(data)
    End Sub

    Private Sub p_GetListAsn()
        dtScreen.alDataAsn = New FG_QRSource().GetListByDispatchPalletId(dtScreen.PalletID, dtConnInfo)
    End Sub

    Private Function p_GetDetailAsnFromQR() As FG_QRSource.DataQRSource
        Return New FG_QRSource().GetDataByQR(dtScreen.QrCode, dtConnInfo)
    End Function

    Private Function p_GetPickDetail(dtQR As FG_QRSource.DataQRSource) As FG_PickDetail.DataFGPickDetail
        Return New FG_PickDetail().GetDataBySkuLot(dtQR.Sku, dtQR.Lot, dtConnInfo)
    End Function

    Private Function p_GetPickList(PalletID As String) As ArrayList
        Return New FG_PickDetail().GetListByPalletId(PalletID, dtConnInfo)
    End Function

#End Region
#Region "Data Class"
    Private Const STATUS_COMPLETED As String = "COMPLETED"
    Private Const STATUS_RESCAN As String = "RESCAN ?"
    Private Const STATUS_SCANNED_DELETED As String = "SCANNED DELETED"

    Public Class DataScreen
        Public StorerKey As String = String.Empty
        Public QrCode As String = String.Empty
        Public OrderKey As String = String.Empty
        Public ExternOrderKey As String = String.Empty
        Public PalletID As String = String.Empty
        Public Sku As String = String.Empty
        Public Qty As Int32 = 0
        Public Total As Int32 = 0
        Public TotalScan As Int32 = 0
        Public alData As New ArrayList
        Public alDataAsn As New ArrayList
        Public alDataPickDetail As New ArrayList
    End Class

#End Region
End Module
