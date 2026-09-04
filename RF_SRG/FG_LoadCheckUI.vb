Imports AIT.RF
Imports AIT.WMS

Module FG_LoadCheckUI
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
        eActiveUI = RFUiList.FG_LOAD_CHECK
        dtScreen = New DataScreen
        p_main()
    End Sub

    Private Sub p_Main(Optional sMessage As String = "", Optional bError As Boolean = False, Optional bPrintList As Boolean = True)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            If bError Then p_saveLog(sMessage, "WARNING")

            PrintLine(1, 4, "CONT#   :")
            PrintLine(11, 4, dtScreen.TrailerNumber)
            PrintLine(1, 5, "INVOICE :")
            PrintLine(11, 5, dtScreen.ExternOrderKey)
            PrintLine(1, 6, "PALLET  :")
            PrintLine(11, 6, dtScreen.PalletID)
            PrintLine(1, 7, "CONFIRM :")
            PrintLine(11, 7, String.Format("{0} of {1}", dtScreen.TotalScan.ToString(), dtScreen.Total.ToString()))
            PrintLine(1, 8, "-------outstanding-------")

            PrintLine(1, 9, "SCAN QR :")
            If bPrintList Then p_printList(10)
            If dtScreen.ExternOrderKey = String.Empty Then
                PrintFooter()
            Else
                p_setElapseTime()
                PrintFooterWithElapse(elapseMilliSecond)
                elapseMilliSecond = 0
            End If

            If dtScreen.TrailerNumber = String.Empty Then
                dtScreen.TrailerNumber = GetScanFixEnterPrint(11, 4, True, 1)
                If dtScreen.TrailerNumber = String.Empty Then
                    p_Main("EMPTY NOT ALLOW", True)
                Else
                    p_Main()
                End If
            End If

            If dtScreen.OrderKey = String.Empty Then
                dtScreen.alDataOrder = New Orders().GetListDataByTrailerNumber(dtScreen.TrailerNumber, dtConnInfo)
                If dtScreen.alDataOrder.Count = 0 Then
                    dtScreen.TrailerNumber = String.Empty
                    p_Main("CONT# N/A", True)
                End If
            End If

            If sMessage = STATUS_COMPLETED Then
                WaitAnyKey(sMessage, 17)
                FG_LabelCheckUI.Main(eActiveUI, dtScreen.alDataPickChecking)
            End If

            If dtScreen.Total = 0 Or dtScreen.TotalScan < dtScreen.Total Then p_scanAndValidate(11, 9)

            ClearMessage()
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try
    End Sub

    Private Sub p_Main_x(Optional sMessage As String = "", Optional bError As Boolean = False, Optional bPrintList As Boolean = True)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "CONT#   :")
            PrintLine(11, 4, dtScreen.TrailerNumber)
            PrintLine(1, 5, "INVOICE :")
            PrintLine(11, 5, dtScreen.ExternOrderKey)
            PrintLine(1, 6, "PALLET  :")
            PrintLine(11, 6, dtScreen.PalletID)
            PrintLine(1, 7, "CONFIRM :")
            PrintLine(11, 7, String.Format("{0} of {1}", dtScreen.TotalScan.ToString(), dtScreen.Total.ToString()))
            PrintLine(1, 8, "-------outstanding-------")

            PrintLine(1, 9, "SCAN QR :")
            If bPrintList Then p_printList(9)
            If dtScreen.ExternOrderKey = String.Empty Then
                PrintFooter()
            Else
                p_setElapseTime()
                PrintFooterWithElapse(elapseMilliSecond)
                elapseMilliSecond = 0
            End If

            If dtScreen.TrailerNumber = String.Empty Then
                dtScreen.TrailerNumber = GetScanFixEnterPrint(11, 4, True, 1)
                If dtScreen.TrailerNumber = String.Empty Then
                    p_Main("EMPTY NOT ALLOW", True)
                Else
                    p_Main()
                End If
            End If

            If dtScreen.OrderKey = String.Empty Then
                dtScreen.alDataOrder = New FG_PickDetail().GetListOrderKeyByTrailerNumber(dtScreen.TrailerNumber, dtConnInfo)

                If dtScreen.alDataOrder.Count = 0 Then
                    dtScreen.TrailerNumber = String.Empty
                    p_Main("CONT# N/A", True)
                End If
            End If

            If sMessage = STATUS_COMPLETED Then
                WaitAnyKey(sMessage, 17)
                FG_LabelCheckUI.Main(eActiveUI, dtScreen.alDataPickChecking)
            End If

            If dtScreen.Total = 0 Or dtScreen.TotalScan < dtScreen.Total Then p_scanAndValidate(10, 9)

            ClearMessage()
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try
    End Sub

    Private Sub p_gotoPrevScreen()
        If dtScreen.TotalScan < dtScreen.Total Then
            Dim o As New FG_PickedChecking
            o.ClearLoadingByPalletId(dtScreen.PalletID, dtUserActive.UserId, dtConnInfo)
        End If

        GotoUi(RFUiList.MENU_SRGPLB_10)
    End Sub

    Private Sub p_printList(iTop As Int16)
        'DEFAULT_HEIGHT_WINDOW - 3
        For i As Int16 = iTop To MAX_TOP_PRINTLIST
            PrintLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Next

        For Each dtPickCheck As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If iTop = MAX_TOP_PRINTLIST Then Exit Sub
            PrintLine(1, iTop, String.Format("{0} #{1} Q{2}", dtPickCheck.Sku, dtPickCheck.Lottable10, dtPickCheck.Qty.ToString()))
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

    Private Sub p_scanAndValidate(iLeft As Integer, iTop As Integer)
        Try
            dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
            If dtScreen.QrCode = String.Empty Then
                p_Main("EMPTY NOT ALLOW", True)
            End If

            elapseStart = Now()
            Dim dataInList As FG_PickedChecking.DataFGPickedChecking = p_findPickCheckInList(dtScreen.QrCode)
            If dataInList.ID <> String.Empty Then p_Main("ALREADY SCANNED", True)

            Dim data As FG_PickedChecking.DataFGPickedChecking = New FG_PickedChecking().GetData(dtScreen.QrCode, dtConnInfo)
            If data.OrderKey = String.Empty Then p_Main("PICKED N/A", True)
            If data.TrailerNumber <> String.Empty Then p_Main("ALREADY SCANNED", True)

            If dtScreen.alData.Count = 0 Then
                dtScreen.alData = p_GetPickList(data.PalletId)
                If dtScreen.alData.Count = 0 Then
                    p_Main("PICKEDLIST N/A", True)
                End If
            End If

            If dtScreen.OrderKey = String.Empty Then
                For Each dtOrders As Orders.DataOrders In dtScreen.alDataOrder
                    If dtOrders.ExternOrderKey = data.ExternOrderKey Then
                        dtScreen.OrderKey = dtOrders.OrderKey
                        dtScreen.ExternOrderKey = data.ExternOrderKey
                        dtScreen.StorerKey = data.StorerKey
                        PrintLine(11, 5, dtScreen.ExternOrderKey)
                        Exit For
                    End If
                Next
                If dtScreen.OrderKey = String.Empty Then p_Main("ORDERKEY N/A IN CONT", True)
            End If

            If dtScreen.ExternOrderKey <> data.ExternOrderKey Then p_Main("DIFF INVOICE", True)

            Dim bFirst As Boolean = False
            If dtScreen.PalletID = String.Empty Then
                bFirst = True
                dtScreen.PalletID = data.PalletId
                PrintLine(10, 6, dtScreen.PalletID)
            End If
            If dtScreen.PalletID <> data.PalletId Then p_Main("DIFF PALLET", True)

            If dtScreen.Total = 0 Then dtScreen.Total = dtScreen.alData.Count

            If Not bFirst Then
                p_saveToList(data)
                p_findAndRemoveList(data)
                dtScreen.TotalScan = dtScreen.Total - dtScreen.alData.Count
            End If

            p_setElapseTime()

            If dtScreen.TotalScan = dtScreen.Total Then
                p_Main(STATUS_COMPLETED, False)
            Else
                p_Main()
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_scanAndValidateQr(iLeft As Integer, iTop As Integer)
        Try
            dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
            If dtScreen.QrCode = String.Empty Then
                p_Main("EMPTY NOT ALLOW", True)
            End If

            elapseStart = Now()
            Dim dataInList As FG_PickedChecking.DataFGPickedChecking = p_findPickCheckInList(dtScreen.QrCode)
            If dataInList.ID <> String.Empty Then
                p_Main("ALREADY SCANNED", True)
            End If

            Dim data As FG_PickedChecking.DataFGPickedChecking = New FG_PickedChecking().GetData(dtScreen.QrCode, dtConnInfo)
            If data.OrderKey = String.Empty Then
                p_Main("PICKED N/A", True)
            End If

            If data.TrailerNumber <> String.Empty Then
                p_Main("ALREADY SCANNED", True)
            End If

            If dtScreen.alData.Count = 0 Then
                dtScreen.alData = p_GetPickList(data.PalletId)
                If dtScreen.alData.Count = 0 Then
                    p_Main("PICKEDLIST N/A", True)
                End If
            End If

            If dtScreen.OrderKey = String.Empty Then
                For Each dtOrder As FG_PickDetail.DataFGPickDetail In dtScreen.alDataOrder
                    If dtOrder.ExternOrderKey = data.ExternOrderKey Then
                        dtScreen.OrderKey = dtOrder.OrderKey
                        dtScreen.ExternOrderKey = data.ExternOrderKey
                        PrintLine(10, 5, dtScreen.ExternOrderKey)
                        Exit For
                    End If
                Next
                If dtScreen.OrderKey = String.Empty Then
                    p_Main("ORDERKEY N/A IN CONT", True)
                End If
            End If

            If dtScreen.ExternOrderKey <> data.ExternOrderKey Then
                p_Main("DIFF INVOICE", True)
            End If

            Dim bFirst As Boolean = False
            If dtScreen.PalletID = String.Empty Then
                bFirst = True
                dtScreen.PalletID = data.PalletId
                PrintLine(10, 6, dtScreen.PalletID)
            End If
            If dtScreen.PalletID <> data.PalletId Then
                p_Main("DIFF PALLET", True)
            End If

            If dtScreen.Total = 0 Then dtScreen.Total = dtScreen.alData.Count

            If Not bFirst Then
                p_saveToList(data)
                p_findAndRemoveList(data)
                dtScreen.TotalScan = dtScreen.Total - dtScreen.alData.Count
            End If

            p_setElapseTime()

            If dtScreen.TotalScan = dtScreen.Total Then
                p_Main(STATUS_COMPLETED, False)
            Else
                p_Main()
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_setElapseTime()
        Dim elapse As TimeSpan = Now() - elapseStart
        elapseMilliSecond = elapse.TotalMilliseconds
    End Sub

    Private Function p_findPickCheckInList(QR As String) As FG_PickedChecking.DataFGPickedChecking
        For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alDataPickChecking
            If data.QR = QR Then
                Return data
            End If
        Next

        Return New FG_PickedChecking.DataFGPickedChecking
    End Function

    Private Function p_findPickDetailInList(dtQR As FG_QRSource.DataQRSource) As FG_PickDetail.DataFGPickDetail
        For Each dtPickDetail As FG_PickDetail.DataFGPickDetail In dtScreen.alData
            If dtPickDetail.Sku = dtQR.Sku And dtPickDetail.Lottable09 = dtQR.Lottable09 And dtPickDetail.Lottable10 = dtQR.Lottable10 Then
                Return dtPickDetail
            End If
        Next

        Return New FG_PickDetail.DataFGPickDetail
    End Function

    Private Sub p_findAndRemoveList(data As FG_PickedChecking.DataFGPickedChecking)
        Dim bFound As Boolean = False
        Dim xAlData As New ArrayList
        xAlData = dtScreen.alData
        Dim i As Int16 = 0
        For Each dtPickCheck As FG_PickedChecking.DataFGPickedChecking In xAlData
            If dtPickCheck.Sku = data.Sku And dtPickCheck.Lottable09 = data.Lottable09 And dtPickCheck.Lottable10 = data.Lottable10 Then
                bFound = True
                xAlData.RemoveAt(i)
                Exit For
            End If
            i += 1
        Next

        If bFound Then dtScreen.alData = xAlData
    End Sub

    Private Sub p_save()
        Dim o As New FG_PickedChecking
        o.UpdateLoading(dtScreen.alData, dtConnInfo)
    End Sub

    Private Sub p_saveToList(data As FG_PickedChecking.DataFGPickedChecking)
        data.TrailerNumber = dtScreen.TrailerNumber
        data.L_UserID = dtUserActive.UserId

        dtScreen.alDataPickChecking.Add(data)
    End Sub

    Private Function p_GetPickList(PalletID As String) As ArrayList
        Return New FG_PickedChecking().GetListByPalletId(PalletID, dtConnInfo)
    End Function

#End Region
#Region "Data Class"
    Private Const STATUS_COMPLETED As String = "COMPLETED"

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
        Public TrailerNumber As String = String.Empty
        Public alData As New ArrayList
        Public alDataPickChecking As New ArrayList
        Public alDataOrder As New ArrayList
    End Class

#End Region
End Module
