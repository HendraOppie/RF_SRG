Imports AIT.RF
Imports AIT.WMS

Module Picking00Ui
#Region "Public"
    Dim dtScreen As New DataScreen
    Dim elapseStart As Date
    Dim elapseMilliSecond As Double = 0
    Const MAX_TOP_PRINTLIST As Int16 = 16
    Const LEFT_INPUT_POS As Int16 = 11

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
        eActiveUI = RFUiList.PICKING_00
        dtScreen = New DataScreen
        p_Main()
    End Sub

    Private Sub p_gotoPrevScreen()
        GotoUi(RFUiList.MENU_STD)
    End Sub

    Private Sub p_Main(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "SO#     :")
            PrintLine(LEFT_INPUT_POS, 4, dtScreen.OrderKey)
            PrintLine(1, 5, "Storer  :")
            PrintLine(LEFT_INPUT_POS, 5, dtScreen.StorerKey)
            PrintLine(1, 6, "CONFIRM :")
            PrintLine(LEFT_INPUT_POS, 6, String.Format("{0} of {1}", dtScreen.TotalScan.ToString(), dtScreen.Total.ToString()))
            PrintLine(1, 7, "-------outstanding-------")
            PrintLine(1, 8, "SCAN QR :")

            p_scanOrderKey()
            p_printList(9)

            p_setElapseTime()
            PrintFooterWithElapse(elapseMilliSecond)
            elapseMilliSecond = 0

            If sMessage = STATUS_COMPLETED Then
                WaitAnyKey(STATUS_COMPLETED)
                p_New()
            End If

            If dtScreen.Total = 0 Or dtScreen.TotalScan < dtScreen.Total Then p_scanAndValidateQr()

            ClearMessage()
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try

    End Sub

    Private Sub p_printList(iTop As Int16)
        Dim bolHasBalance As Boolean = False

        'DEFAULT_HEIGHT_WINDOW - 3
        For i As Int16 = iTop To MAX_TOP_PRINTLIST
            PrintLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Next

        If dtScreen.alDataPickDetailSummary.Count = 0 Then Exit Sub
        For Each dtPickDetailSumary As PickDetail.DataPickDetail In dtScreen.alDataPickDetailSummary
            If iTop = MAX_TOP_PRINTLIST Then Exit For

            Dim dtSumSkuScanned As PickingScan.DataPickingScan = p_getDataScanFromList(dtPickDetailSumary.Sku)
            Dim balanceQty As Int16 = dtPickDetailSumary.Qty - dtSumSkuScanned.Qty

            If balanceQty > 0 Then
                PrintLine(1, iTop, String.Format("{0} Q{1}", dtPickDetailSumary.Sku, balanceQty.ToString()))
                iTop += 1
                bolHasBalance = True
            End If
        Next

        If bolHasBalance = False Then p_Main(STATUS_COMPLETED)
    End Sub

    Private Function p_getDataScanFromList(Sku As String) As PickingScan.DataPickingScan
        Dim dtSumSkuScanned As New PickingScan.DataPickingScan

        For Each dtScanSummary As PickingScan.DataPickingScan In dtScreen.alDataSummary
            If dtScanSummary.Sku = Sku Then
                dtSumSkuScanned = dtScanSummary
                Exit For
            End If
        Next

        Return dtSumSkuScanned
    End Function

    Private Sub p_setElapseTime()
        Dim elapse As TimeSpan = Now() - elapseStart
        elapseMilliSecond = elapse.TotalMilliseconds
    End Sub

    Private Sub p_scanOrderKey()
        Try
            If dtScreen.OrderKey = String.Empty Then
                elapseStart = Now()
                p_getPickDetail()
                p_getList()
                p_Main()
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_getList()
        Try
            If dtScreen.OrderKey <> String.Empty Then
                dtScreen.alData = New RF.PickingScan().GetList(dtScreen.OrderKey, dtScreen.StorerKey, dtConnInfo)
                dtScreen.alDataSummary = New RF.PickingScan().GetSummarySku(dtScreen.OrderKey, dtScreen.StorerKey, dtConnInfo)
            End If

            For Each dtSumScanned As PickingScan.DataPickingScan In dtScreen.alDataSummary
                dtScreen.TotalScan += dtSumScanned.Qty
            Next
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_getPickDetail()
        Try
            dtScreen.OrderKey = GetScanFixEnterPrint(LEFT_INPUT_POS, 4, False, 1)
            If dtScreen.OrderKey = String.Empty Then
                p_Main("EMPTY NOT ALLOW", True)
            End If

            elapseStart = Now()

            dtScreen.alDataPickDetailSummary = New PickDetail().GetDataTotalQtySku(dtScreen.OrderKey, dtConnInfo)
            If dtScreen.alDataPickDetailSummary.Count = 0 Then
                dtScreen = New DataScreen
                p_Main("PICKDETAIL NOT FOUND", True)
            End If

            For Each dtPickDetailSum As PickDetail.DataPickDetail In dtScreen.alDataPickDetailSummary
                dtScreen.Total += dtPickDetailSum.Qty
                dtScreen.StorerKey = dtPickDetailSum.StorerKey
            Next

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_scanAndValidateQr()
        Try
            dtScreen.QrCode = GetScanFixEnterPrint(LEFT_INPUT_POS, 8, False, 1)
            If dtScreen.QrCode = String.Empty Then
                p_Main("EMPTY NOT ALLOW", True)
            End If

            elapseStart = Now()
            p_validateDuplicateScanInList()
            p_splitQR()
            p_validateExistsScanInList()

            elapseStart = Now()
            p_save()
            p_getList()
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub p_save()
        Dim dtScanned As New PickingScan.DataPickingScan
        dtScanned.QRCode = dtScreen.QrCode
        dtScanned.OrderKey = dtScreen.OrderKey
        dtScanned.ExternOrderKey = dtScreen.ExternOrderKey
        dtScanned.PalletID = dtScreen.PalletID
        dtScanned.Sku = dtScreen.Sku
        dtScanned.Qty = dtScreen.Qty
        dtScanned.StorerKey = dtScreen.StorerKey
        dtScanned.UserID = dtUserActive.UserId

        Dim oPickingScan As New PickingScan
        oPickingScan.Save(dtScanned, dtConnInfo)
    End Sub

    Private Sub p_validateExistsScanInList()
        Dim bFound As Boolean = False
        Dim qtyWms As Int16 = 0
        For Each dtPickDetailWms As PickDetail.DataPickDetail In dtScreen.alDataPickDetailSummary
            If dtPickDetailWms.Sku = dtScreen.Sku Then
                bFound = True
                qtyWms = dtPickDetailWms.Qty
                Exit For
            End If
        Next

        If Not bFound Then
            p_Main("SKU NOT EXISTS", True)
        Else
            For Each dtAsnScanSummary As ReceivingScan.DataReceivingScan In dtScreen.alDataSummary
                If dtAsnScanSummary.Sku = dtScreen.Sku Then
                    If dtAsnScanSummary.Qty + dtScreen.Qty >= qtyWms Then
                        p_Main("QTY EXCEED", True)
                    End If
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub p_splitQR()
        If dtScreen.StorerKey = "PLBFMT001" Then
            '109575083;7194-3684-30 ;10/01; ;01191126000574;10025402;300;191126
            '         ;sku          ;     ; ;              ;        ;qty;
            Dim sContent() As String = dtScreen.QrCode.Split(";")
            If sContent.Count = 1 Then Exit Sub
            If sContent.Count > 2 Then dtScreen.Sku = sContent(1).Replace(" ", "")
            If sContent.Count > 7 Then dtScreen.Qty = sContent(6)
        Else
            p_Main("QR CONFIG N/A", True)
        End If

    End Sub

    Private Sub p_validateDuplicateScanInList()
        Dim bFound As Boolean = False
        For Each dtPickingScan As PickingScan.DataPickingScan In dtScreen.alData
            If dtScreen.QrCode = dtPickingScan.QRCode Then
                bFound = True
                Exit For
            End If
        Next
        If bFound Then p_Main("ALREADY SCAN", True)
    End Sub
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
        Public Qty As Int16 = 0
        Public Total As Int16 = 0
        Public TotalScan As Int16 = 0
        Public alData As New ArrayList
        Public alDataSummary As New ArrayList
        Public alDataPickDetailSummary As New ArrayList
    End Class

#End Region

End Module
