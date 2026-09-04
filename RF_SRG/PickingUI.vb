Imports AIT.RF
Imports AIT.GEN

Module PickingUI

#Region "Public"
    Dim dtScreen As New DataScreen
    Dim dtPickDetail As New OrderManager.DataPickDetailSummary
    Dim dtPicking As New PickingManager.DataPicking

    Const LEN_SKU As Integer = 23
    Const LEN_QTY As Integer = 9
    Const LEN_LOT As Integer = 23
    Const DELIMITER_PICKDETAIL As String = "|"
    Const DELIMITER_QR_OUTBOUND As String = ","
    Const STATUS_SKU_QTY_MATCH As String = "SKU & QTY MATCH. GOOD JOB!"

    Dim bAfterChecking As Boolean = False
    Dim dtSku As New Sku.DataSku

    Public Sub Main()
        p_Menu()
    End Sub

    Public Sub PickingInputUi()
        p_new()
        p_ScanQr()
    End Sub

    Public Sub PickingEnquiryUi()
        p_new()
        p_ScanQr()
    End Sub

    Public Sub PickingAuditUi()
        p_new()
        p_uiAuditOrder()
    End Sub

    Public Sub GetDataScan()
        p_getDataScan()
    End Sub

    Public Sub PickingCheckUi()
        p_new()
        dtScreen = New DataScreen
        p_PickingCheck()
    End Sub

    Public Sub PickingCheckQqUi()
        p_new()
        p_CheckQq()
    End Sub

    Public Sub PickingPalletCheckUi()
        p_palletChecking()
    End Sub

    Public Sub PackCheckingUi()
        p_PackChecking()
    End Sub

    Public Sub PackCheckingPrevUI()
        p_PackCheckingPrevUI()
    End Sub
#End Region

#Region "Private"

    Private Sub p_Menu()
        eActiveUI = RFUiList.PICKING
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU PICKING:")
        PrintLine(1, 4, "1. QRi VS QRo")
        PrintLine(1, 5, "2. PALLET CHECKING")
        PrintLine(1, 6, "3. CHECK BY PTF")
        PrintLine(1, 7, "4. PACK CHECKING")
        'PrintLine(1, 8, "5. ?")
        'PrintLine(1, 9, "6. ?")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            GotoUi(RFUiList.MENU_SRGPLB_39)
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    GotoUi(RFUiList.PICK_CHECK_QQ)
                Case 2
                    GotoUi(RFUiList.PALLET_CHECK)
                    'Case 1
                    '    p_gotoUi(RFUiList.PICK_INPUT)
                    'Case 2
                    '    p_gotoUi(RFUiList.PICK_ENQUIRY)
                    'Case 3
                    '    p_gotoUi(RFUiList.PICK_AUDIT)
                Case 3
                    GotoUi(RFUiList.PICK_CHECK)
                Case 4
                    GotoUi(RFUiList.PICK_PACK_CHECK)
                Case Else
                    PrintMessage("INVLD KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    p_Menu()
            End Select
        End If
    End Sub

    Private Sub p_new()
        dtPickDetail = New OrderManager.DataPickDetailSummary
        dtPicking = New PickingManager.DataPicking
        dtPicking.ScanBy = dtUserActive.UserName
    End Sub

    Private Sub p_ScanQr(Optional sMessage As String = "", Optional bError As Boolean = False)
        Dim iSOLevel As Integer = 0
        Dim iSkuLevel As Integer = 0
        Dim iSOSku As Integer = 0
        p_checkOutstanding(iSOLevel, iSkuLevel, iSOSku)

        PrintHeader(eActiveUI)
        PrintLine(1, 4, "SCAN PICKTICKET QR")
        PrintLine(1, 5, String.Empty,, True)
        PrintLine(1, 8, "OUTSTANDING SCAN:")
        PrintLine(1, 9, iSOLevel.ToString & " ORDER")
        PrintLine(1, 10, iSkuLevel.ToString & " SKU")
        If iSOSku > 0 Then PrintLine(1, 11, iSOSku.ToString & " SKU (ORD#" & dtPickDetail.OrderKey & ")")
        PrintFooter()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        p_splitDataScanQR(1, 5)
        p_getDataPickDetail()

        If eActiveUI = RFUiList.PICK_INPUT Then
            p_getTotalScanned()
            p_uiInput()
        ElseIf eActiveUI = RFUiList.PICK_ENQUIRY Then
            p_getDataScan()
        End If
    End Sub

    Private Sub p_PackCheckingPrevUI()
        p_Menu()
    End Sub

    Private Sub p_PackChecking(Optional bNewData As Boolean = True, Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            PrintHeader(eActiveUI)
            If bNewData Then
                dtScreen = New DataScreen
                dtSku = New Sku.DataSku
            End If
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

            PrintLine(1, 4, "QR LABEL ---------------------")
            PrintLine(1, 5, "SKU      :")
            PrintLine(11, 5, Left(dtScreen.Sku, DEFAULT_WIDTH_WINDOW - 11))
            PrintLine(1, 6, "QTY      :")
            PrintLine(11, 6, dtScreen.Qty1)
            PrintLine(1, 7, "PACK QTY :")
            PrintLine(11, 7, dtSku.QtyInnerPack)
            PrintLine(1, 9, "INNER PACK -------------------")
            PrintLine(1, 10, "SKU      :")
            PrintLine(11, 10, Left(dtScreen.SkuFromQr, DEFAULT_WIDTH_WINDOW - 11))
            PrintLine(1, 11, "QTY      :")
            PrintLine(11, 11, "     ", True, True)
            PrintLine(1, 12, dtScreen.Status)
            PrintFooter()

            p_scanQRLabelPackage(11, 5)
            p_scanSkuConfirmation(11, 10)

entryQty:
            dtScreen.Qty2 = 0
            dtScreen.Qty2 = GetInputNumber(11, 11, 2)
            PrintLine(11, 11, dtScreen.Qty2, True, True)
            If dtScreen.Qty2 * dtSku.QtyInnerPack = dtScreen.Qty1 Then
                PrintMessage(STATUS_SKU_QTY_MATCH)
                p_saveLog(dtScreen.QrCode, dtScreen.SkuFromQr, dtScreen.Qty2, Log.LogStatus.MATCH.ToString())
            Else
                PrintMessage("QTY NOT MATCH", True)
                GoTo entryQty
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_PickingCheck(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "SCAN PICKTICKET QR")
            PrintLine(1, 5, String.Format("SKU TO PICK = {0}", dtScreen.Sku))
            PrintLine(1, 7, "SCAN PRODUCT QR")
            PrintLine(1, 8, String.Format("SKU PRODUCT = {0}", dtScreen.SkuFromQr))
            PrintLine(1, 10, "SCAN ID PART QR")
            PrintLine(1, 11, String.Format("SKU ID PART = {0}", dtScreen.SkuIdPart))
            PrintFooter()

            p_scanQR_rawmat(20, 4)
            p_scanQrProduct(20, 7)
            p_scanIdPart(20, 10)

            If String.Equals(dtScreen.Sku.Replace("-", ""), dtScreen.SkuFromQr.Replace("-", "")) _
                And String.Equals(dtScreen.Sku.Replace("-", ""), dtScreen.SkuIdPart.Replace("-", "")) Then
                p_saveLog(String.Format("CaseID {0}", dtScreen.CaseId), String.Format("SKUs {0};{1};{2}", dtScreen.Sku, dtScreen.SkuFromQr, dtScreen.SkuIdPart), String.Empty, Log.LogStatus.MATCH.ToString())
                dtScreen = New DataScreen
                p_PickingCheck(MESSAGE_INFO_SKU_MATCH & ". GOOD JOB!")
            End If

            p_saveLog(String.Format("CaseID {0}", dtScreen.CaseId), String.Format("SKUs {0};{1};{2}", dtScreen.Sku, dtScreen.SkuFromQr, dtScreen.SkuIdPart), String.Empty, Log.LogStatus.NOT_MATCH.ToString())
            dtScreen = New DataScreen
            UIWarning(MESSAGE_INFO_SKU_NOT_MATCH)

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_CheckQq(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "SCAN QR Inbound")
            PrintLine(1, 5, "SKU                    QTY")
            PrintLine(1, 6, dtScreen.Sku)
            PrintLine(24, 6, dtScreen.Qty1.ToString())
            PrintLine(1, 7, "---------------vs-------------")
            PrintLine(1, 8, dtScreen.SkuFromQr)
            PrintLine(24, 8, dtScreen.Qty2.ToString())
            PrintFooter()

            dtScreen = New DataScreen
            Dim logEventName As String = eActiveUI.ToString()
            p_scanQRInbound(20, 4)
            PrintLine(1, 8, New String(" ", 50), True)
            ClearMessage()
            If dtScreen.StorerKey <> dtUserActive.StorerKey Then
                p_CheckQq(MESSAGE_INFO_INVALID_STORER, True)
            End If

            If dtScreen.Sku = String.Empty Then
                p_CheckQq(MESSAGE_INFO_INVALID_SKU, True)
            End If

            If p_isIDInboundExists() Then
                'UIWarning(MESSAGE_INFO_QR_IS_USED, False)
                'p_CheckQq(MESSAGE_INFO_QR_IS_USED, True)
                PrintMessage(MESSAGE_INFO_QR_IS_USED, bError)
                logEventName = MESSAGE_INFO_QR_IS_USED
            End If

            PrintLine(1, 6, dtScreen.Sku)
            PrintLine(24, 6, dtScreen.Qty1.ToString())

            PrintLine(1, 4, New String(" ", DEFAULT_WIDTH_WINDOW), True)
            dtScreen.SkuFromQr = String.Empty
            dtScreen.Qty2 = 0

            PrintLine(1, 4, "SCAN QR Outbound")
            p_scanQROutbound(20, 4)
            PrintLine(1, 8, dtScreen.SkuFromQr)
            PrintLine(24, 8, dtScreen.Qty2.ToString())

            If dtScreen.SkuFromQr = String.Empty Then
                p_CheckQq("INVLD QR", True)
            End If

            If dtScreen.Sku = dtScreen.SkuFromQr And dtScreen.Qty1 = dtScreen.Qty2 Then
                p_saveLogWithEventName(String.Format("SKU {0} Qty {1}", dtScreen.Sku, dtScreen.Qty1), String.Format("SKU {0} Qty {1}", dtScreen.SkuFromQr, dtScreen.Qty2), dtScreen.LogScan3, Log.LogStatus.MATCH.ToString(), logEventName)
                p_CheckQq(STATUS_SKU_QTY_MATCH)
            Else
                p_saveLogWithEventName(String.Format("SKU {0} Qty {1}", dtScreen.Sku, dtScreen.Qty1), String.Format("SKU {0} Qty {1}", dtScreen.SkuFromQr, dtScreen.Qty2), dtScreen.LogScan3, Log.LogStatus.NOT_MATCH.ToString(), logEventName)
                UIWarning("  N O T   M A T C H ")
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_palletCheckingComplete(dtPallet As OrderStock.DataOrderStockPallet, sMessage As String)
        eActiveUI = RFUiList.PICK_PALLET_COMPLETE
        PrintHeader(eActiveUI)
        PrintFooterEsc()
        PrintMessage(sMessage, False)
        PrintLine(1, 4, "SCAN ID :")
        PrintLine(1, 6, "STATUS | JUMLAH  | SCANNED")
        PrintLine(1, 7, "SKU    |         |")
        PrintLine(1, 8, "QTY    |         |")

        PrintLine(10, 4, dtScreen.PalletId)
        PrintLine(10, 7, dtPallet.TotalSku)
        PrintLine(20, 7, dtPallet.TotalSkuScan)
        PrintLine(10, 8, dtPallet.TotalQty)
        PrintLine(20, 8, dtPallet.TotalQtyScan)

        If GetInputEscapeOnly(15, 18) Then
            p_palletChecking()
        Else
            p_palletCheckingComplete(dtPallet, sMessage)
        End If
    End Sub

    Private Sub p_palletChecking(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            eActiveUI = RFUiList.PALLET_CHECK
            Dim dtPallet As New OrderStock.DataOrderStockPallet
            Dim dtOrderStock As New OrderStock.DataOrderStock
            Dim bRescan As Boolean = False
            Dim iCounter As Int16 = 0
            dtScreen = New DataScreen

labelRescan:
            PrintHeader(eActiveUI)
            PrintFooter()
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "SCAN ID :")
            PrintLine(10, 4, New String(" ", 18), , True)
            PrintLine(1, 6, "STATUS | JUMLAH  | SCANNED")
            PrintLine(1, 7, "SKU    |         |")
            PrintLine(1, 8, "QTY    |         |")

            If Not bRescan Then
                p_scanQRPalletOutbound(10, 4)
                ClearMessage()
            End If
            PrintLine(10, 4, dtScreen.PalletId,, True)

            If dtScreen.PalletId = String.Empty Then p_palletChecking()

            dtPallet = p_getDataPallet(dtScreen.PalletId)
            If dtPallet.PalletID = String.Empty Then p_palletChecking("INVLD ID", True)
            If dtPallet.StorerKey <> dtUserActive.StorerKey Then p_palletChecking("INVLD STORER", True)

            PrintLine(10, 7, dtPallet.TotalSku)
            PrintLine(20, 7, dtPallet.TotalSkuScan)
            PrintLine(10, 8, dtPallet.TotalQty)
            PrintLine(20, 8, dtPallet.TotalQtyScan)

            If dtPallet.TotalSku = dtPallet.TotalSkuScan And dtPallet.TotalQty = dtPallet.TotalQtyScan Then p_palletCheckingComplete(dtPallet, "COMPLETED. GOOD JOB!")

            If dtScreen.Sku = String.Empty Then p_getDataLastScan(dtScreen.PalletId, iCounter)

            PrintLine(1, 10, "SCAN QR INBOUND :")
            Console.ForegroundColor = ConsoleColor.Black
            PrintLine(18, 10, New String(" ", 8))
            Console.ResetColor()
            PrintLine(18, 10, New String(" ", 1), , True)
            PrintLine(1, 11, "SCAN COUNTER    :")
            PrintLine(18, 11, iCounter)
            PrintLine(1, 12, "LAST SCANNED    :")
            PrintLine(3, 13, "SKU " & dtScreen.Sku)
            PrintLine(3, 14, "QTY " & dtScreen.Qty1)
            PrintLine(3, 15, "STATUS " & dtScreen.Status)

            bRescan = True
            p_scanQRInbound(18, 10)
            ClearMessage()
            If dtPallet.StorerKey <> dtScreen.StorerKey Then p_palletChecking(MESSAGE_INFO_INVALID_STORER, True)

            If p_isIDInboundExists() Then
                UIWarning(MESSAGE_INFO_QR_IS_USED, False)
                p_palletChecking(MESSAGE_INFO_QR_IS_USED, True)
            End If

            dtOrderStock = p_getDataOrderStock(dtScreen.PalletId, dtScreen.Sku)
            If dtOrderStock.StorerKey <> dtPallet.StorerKey Then
                dtScreen.Status = MESSAGE_INFO_INVALID_STORER
                p_palletChecking(MESSAGE_INFO_INVALID_STORER, True)
            End If
            If dtOrderStock.PalletID <> dtPallet.PalletID Then
                dtScreen.Status = MESSAGE_INFO_INVALID_IDSTORER
                p_palletChecking(MESSAGE_INFO_INVALID_IDSTORER, True)
            End If

            If dtOrderStock.Qty - dtOrderStock.QtyScanned = dtScreen.Qty1 Then dtScreen.Status = OrderStock.ScanStatus.MATCH.ToString()
            If dtOrderStock.Qty - dtOrderStock.QtyScanned < dtScreen.Qty1 Then dtScreen.Status = OrderStock.ScanStatus.OVER.ToString()
            If dtOrderStock.Qty - dtOrderStock.QtyScanned > dtScreen.Qty1 Then dtScreen.Status = OrderStock.ScanStatus.LESS.ToString()

            p_saveLog(String.Format("ID {0}", dtOrderStock.PalletID), String.Format("SKU {0}", dtOrderStock.Sku), String.Format("QTY {0}", dtOrderStock.Qty), dtScreen.Status)
            If dtScreen.Status = OrderStock.ScanStatus.OVER.ToString() Then
                UIWarning("O V E R   S C A N", False)
                sMessage = "OVER SCAN"
                bError = False
            Else
                Dim data As New OrderStock.DataOrderStockScan
                data.SerialKey = dtOrderStock.SerialKey
                data.StorerKey = dtOrderStock.StorerKey
                data.PalletID = dtOrderStock.PalletID
                data.Sku = dtOrderStock.Sku
                data.Qty = dtScreen.Qty1
                data.ScanBy = dtUserActive.UserId

                Dim oOrderStock As New OrderStock
                oOrderStock.SaveScan(data, dtConnInfo)

                If dtScreen.Status = OrderStock.ScanStatus.MATCH.ToString() Then
                    sMessage = STATUS_SKU_QTY_MATCH
                    bError = False
                End If
            End If

            iCounter += 1
            GoTo labelRescan

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_getDataLastScan(ID As String, ByRef iCounter As Int16)
        Dim dtScan As OrderStock.DataOrderStockScan = New OrderStock().GetDataLastScan(ID, dtConnInfo)
        If dtScan.ID <> String.Empty Then
            iCounter = Convert.ToInt16(dtScan.ID.Substring(Len(dtScan.ID) - 4, 4))
            dtScreen.Sku = dtScan.Sku
            dtScreen.Qty1 = dtScan.Qty
        End If
    End Sub

    Private Function p_getDataOrderStock(ID As String, SKU As String) As OrderStock.DataOrderStock
        Return New OrderStock().GetData(ID, SKU, dtConnInfo)
    End Function

    Private Function p_getDataPallet(ID As String) As OrderStock.DataOrderStockPallet
        Return New OrderStock().GetSummaryPalletAndScan(ID, dtConnInfo)
    End Function

    Private Sub p_saveLog(scan1 As String, scan2 As String, scan3 As String, status As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = eActiveUI.ToString()
        dtLog.LabelID = dtScreen.ID
        dtLog.Scan1 = scan1
        dtLog.Scan2 = scan2
        dtLog.Scan3 = scan3
        dtLog.Status = status
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtUserActive.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_saveLogWithEventName(scan1 As String, scan2 As String, scan3 As String, status As String, sEvent As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = sEvent
        dtLog.LabelID = dtScreen.ID
        dtLog.Scan1 = scan1
        dtLog.Scan2 = scan2
        dtLog.Scan3 = scan3
        dtLog.Status = status
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtUserActive.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_validateInputQty(iLeft As Int16, iTop As Int16)
        dtScreen.Qty2 = 0
        dtScreen.Qty2 = GetInputNumber(iLeft, iTop, 2)

        If dtScreen.Qty2 * dtSku.QtyInnerPack = dtScreen.Qty1 Then
            p_PackChecking(False, "SUCCESS")
        Else
            p_PackChecking(False, "QTY NOT MATCH", True)
        End If
    End Sub

    Private Sub p_scanSkuConfirmation(iLeft As Int16, iTop As Int16)
        If dtScreen.SkuFromQr <> String.Empty Then Exit Sub

        Dim sInput As String = String.Empty
        sInput = GetScanFixEnterPrint(iLeft, iTop, True, 1)

        If sInput.ToLower.Trim.Replace("-", "") = dtSku.Sku.ToLower.Trim.Replace("-", "") _
            Or sInput.ToLower.Trim.Replace("-", "") = dtSku.AltSku.ToLower.Trim.Replace("-", "") _
            Or sInput.ToLower.Trim.Replace("-", "") = dtSku.SUsr4.ToLower.Trim.Replace("-", "") _
            Or sInput.ToLower.Trim.Replace("-", "") = dtSku.BUsr2.ToLower.Trim.Replace("-", "") _
            Or sInput.ToLower.Trim.Replace("-", "") = dtSku.BUsr3.ToLower.Trim.Replace("-", "") Then
            dtScreen.SkuFromQr = dtSku.Sku
            p_PackChecking(False, "SKU MATCH")
        Else
            dtScreen.SkuFromQr = String.Empty
            p_PackChecking(False, "SKU NOT MATCH", True)
        End If

    End Sub

    Private Sub p_scanQrProduct(iLeft As Int16, iTop As Int16)
        If Not dtScreen.SkuFromQr = String.Empty Then Exit Sub

        Dim sInput As String = String.Empty
        Console.SetCursorPosition(iLeft, iTop)
        sInput = Console.ReadLine()
        'sInput = GetScan(iLeft, iTop)
        ClearMessage()

        If sInput.Length > 0 Then
            dtScreen.SkuFromQr = p_getSkuFromQR(sInput)
            If dtScreen.SkuFromQr = String.Empty Then
                p_saveLogWithEventName("INVLD QR", sInput, String.Empty, Log.LogStatus.FAIL.ToString(), "ScanQRProduct")
                p_PickingCheck("INVLD QR", True)
            Else
                Dim dtSku As Sku.DataSku = p_getSkuByPrediction(dtScreen.SkuFromQr)
                If dtSku.Sku = String.Empty Then
                    p_saveLogWithEventName("SKU NOT FOUND", dtScreen.SkuFromQr, String.Empty, Log.LogStatus.FAIL.ToString(), "ScanQRProduct")
                    dtScreen.SkuFromQr = String.Empty
                    p_PickingCheck("SKU NOT FOUND", True)
                Else
                    dtScreen.SkuFromQr = dtSku.Sku
                    p_PickingCheck()
                End If
            End If
        Else
            p_PickingCheck("INVLD QR", True)
        End If
    End Sub

    Private Sub p_scanIdPart(iLeft As Int16, iTop As Int16)
        If Not dtScreen.SkuIdPart = String.Empty Then Exit Sub

        p_scanQRInbound(iLeft, iTop)
        ClearMessage()

        If p_isIDInboundExists() Then
            UIWarning(MESSAGE_INFO_QR_IS_USED, False)
            p_PickingCheck(MESSAGE_INFO_QR_IS_USED, True)
        End If

        dtScreen.SkuIdPart = dtScreen.Sku

        If dtScreen.SkuIdPart = String.Empty Then
            p_saveLogWithEventName("INVLD QR", dtScreen.QrCode, String.Empty, Log.LogStatus.FAIL.ToString(), "ScanIdPart")
            p_PickingCheck("INVLD QR", True)
        End If
        p_PickingCheck()
    End Sub

    Private Sub p_uiAuditOrder(Optional sMessage As String = "", Optional bError As Boolean = False)
        PrintHeader(RFUiList.PICK_AUDIT)
        PrintLine(1, 4, "ORDERKEY  : " & dtPickDetail.OrderKey)
        PrintLine(13, 4, New String(" ", 10),, True)
        PrintFooter()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        Dim sInput As String = String.Empty
invalidScan:
        sInput = GetInput(13, 4, 10)
        sInput = New String("0", 10 - sInput.Length) & sInput
        If sInput.Length > 0 Then
            ClearMessage()
            p_getDataPickDetail(p_getSkuFromQR(sInput))
        Else
            PrintMessage("INVLD ORDER", True)
            GoTo invalidScan
        End If

        p_uiAuditSku()
    End Sub

    Private Sub p_getOrder(sOrderKey As String)
        Dim om As New OrderManager
        dtPickDetail = om.GetDataByOrderKey(sOrderKey, dtUserActive.StorerKey, dtConnInfo)
    End Sub

    Private Sub p_uiAuditSku(Optional sMessage As String = "", Optional bError As Boolean = False)
        PrintHeader(RFUiList.PICK_AUDIT)
nextScan:
        PrintLine(1, 4, "ORDERKEY  : " & dtPickDetail.OrderKey)
        PrintLine(1, 5, "SCAN SKU / QR PRODUCT:")
        PrintLine(1, 6, New String(" ", LEN_LOT),, True)
        PrintFooter()
        ClearMessage()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        If dtPickDetail.OrderKey = String.Empty Then
            If bError Then PrintLine(1, 8, "SKU NOT FOUND OR QR CONFIG NOT RECOGNIZED")
        Else
            If bAfterChecking Then
                PrintLine(1, 8, "SKU FOUND:" & dtPickDetail.Sku)
                PrintLine(1, 9, "FROM LOC :" & dtPickDetail.Loc)
                PrintLine(1, 10, "QTY      :" & dtPickDetail.Qty.ToString)
            End If
        End If

        Dim sInput As String = String.Empty
invalidScan:
        Console.SetCursorPosition(1, 6)
        sInput = Console.ReadLine()
        'sInput = GetScan(iLeft, iTop, True)
        If sInput.Length > 0 Then
            ClearMessage()
            p_getDataPickDetail(p_getSkuFromQR(sInput))
        Else
            PrintMessage("INVLD QR", True)
            GetInput(21, 17, 1)
            GoTo invalidScan
        End If

        GoTo nextScan
    End Sub

    Private Function p_getSkuFromQR(sInput As String) As String
        Dim sSku As String = String.Empty

        If sInput.Substring(0, 1) = "[" Then
            If sInput.Substring(0, 8) = "[)>06 LT" Then
                sSku = p_getSplit(sInput, " ", 2, "PN")
            End If
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 1, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 1, "P")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 2, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 2, "P")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 3, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 3, "P")

            If Not sSku = String.Empty Then Return sSku
        End If

        If sInput.Length > 9 Then
            If sInput.Substring(0, 9) = "P00000000" Then sSku = Replace(sInput.Substring(10, sInput.Length - 9), "-", "")
            If Not sSku = String.Empty Then Return sSku

            If sInput.Substring(9, 1) = vbTab Then
                sSku = p_getSplit(sInput, vbTab, 2, String.Empty)
                If Not sSku = String.Empty Then Return sSku
            End If
        End If
        If sInput.Substring(0, 1) = "_" Then
            sSku = p_getSplit(sInput, "chrw29", 1, "PN")
            If Not sSku = String.Empty Then Return sSku
        End If
        If sInput.Substring(0, 3) = "30S" Then
            sSku = Replace(sInput.Substring(3, sInput.Length - 3), "-", "")
            If Not sSku = String.Empty Then Return sSku
        End If
        If sInput.Substring(0, 4) = "32G8" Then
            sSku = p_getSplit(sInput, ",", 4, String.Empty)
            If Not sSku = String.Empty Then Return sSku
        End If
        If sInput.Substring(0, 1) = "P" Then
            sSku = Replace(sInput.Substring(1, sInput.Length - 1), "-", "")
            If Not sSku = String.Empty Then Return sSku
        End If
        If sInput.Substring(0, 1) = "$" Then
            sSku = p_getSplit(sInput, vbTab, 2, String.Empty)
            If Not sSku = String.Empty Then Return sSku
        End If
        If sInput.Substring(0, 4) = "EPIC" Then
            sSku = RTrim(Replace(sInput.Substring(5, 15), "-", ""))
            If Not sSku = String.Empty Then Return sSku
        End If

        Return Replace(sInput, "-", "")
    End Function

    Private Function p_getSplit(sInput As String, sDelimiter As String, iIndex As Int16, sPrefix As String) As String
        Dim str() As String

        If sDelimiter.Length > 4 Then
            If sDelimiter.Substring(0, 4).ToLower = "chrw" Then str = Split(sInput, ChrW(sDelimiter.Substring(4, 2)))
        Else
            str = Split(sInput, sDelimiter)
        End If

        If sPrefix = String.Empty Then
            Return Replace(str(iIndex), "-", "")
        ElseIf str(iIndex).Substring(0, sPrefix.Length).ToLower = sPrefix.ToLower Then
            Return Replace(str(iIndex).Substring(sPrefix.Length, str(iIndex).Length - sPrefix.Length), "-", "")
        End If

        Return String.Empty
    End Function

    Private Function p_getSkuByPrediction(sSku As String) As Sku.DataSku
        UIWait("")
        ClearMessage()

        Try
            Return New Sku().GetByPrediction(dtScreen.StorerKey, sSku, dtConnInfo)

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return New Sku.DataSku
    End Function

    Private Sub p_getDataPickDetail(sSku As String)
        UIWait("")
        ClearMessage()

        Try
            Dim xOrderKey As String = dtPickDetail.OrderKey
            bAfterChecking = True
            Dim OM As New OrderManager
            dtPickDetail = OM.GetData(dtPickDetail.OrderKey, dtUserActive.StorerKey, sSku, dtConnInfo)

            If dtPickDetail.OrderKey = String.Empty Then
                dtPickDetail.OrderKey = xOrderKey
                p_uiAuditSku("DATA NOT FOUND", True)
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_getDataPickDetail()
        UIWait("")
        ClearMessage()

        Try
            Dim OM As New OrderManager
            dtPickDetail = OM.GetData(dtScreen.PickId, dtUserActive.StorerKey, dtConnInfo)

            If dtPickDetail.OrderKey = String.Empty Then
                p_ScanQr("DATA NOT FOUND", True)
            End If

            dtPicking.PickId = dtPickDetail.PickId
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_getTotalScanned()
        UIWait("")
        ClearMessage()

        Try
            Dim PM As New PickingManager
            Dim iTotalScannedQtyCase As Integer = 0
            Dim iTotalScannedQty As Integer = 0

            PM.GetTotalQtyScanned(dtPickDetail.PickId, iTotalScannedQtyCase, iTotalScannedQty, dtConnInfo)

            If dtPickDetail.Qty = iTotalScannedQty Then
                p_ScanQr("CASEID COMPLETE", True)
            End If

            dtPickDetail.Qty = dtPickDetail.Qty - iTotalScannedQty
            dtPicking.PickId = dtPickDetail.PickId

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_uiInput()
restart:
        PrintHeader(RFUiList.PICK_INPUT)
        PrintLine(1, 4, "ORDERKEY  :" & dtPickDetail.OrderKey)
        PrintLine(1, 5, "CASE ID   :" & dtPickDetail.CaseId)
        PrintLine(1, 6, "LOT       :" & dtPickDetail.Lot)
        PrintLine(1, 7, "SKU       :" & dtPickDetail.Sku)
        PrintLine(1, 8, "FROM LOC  :" & dtPickDetail.Loc)
        PrintLine(1, 9, "QTY       | CASECNT")
        PrintLine(1, 10, dtPickDetail.Qty)
        PrintLine(LEN_QTY + 4, 10, dtPickDetail.CaseCnt)
        PrintLine(1, 11, "BATCH NUMBER:")
        PrintLine(1, 12, New String(" ", LEN_LOT),, True)
        PrintLine(1, 13, "JML BOX   | QTY PER BOX")
        PrintLine(1, 14, New String(" ", LEN_QTY),, True)
        PrintLine(LEN_QTY + 4, 14, New String(" ", LEN_QTY),, True)
        PrintLine(1, 15, "DROP ID (PALLET ID):")
        PrintLine(1, 16, New String(" ", LEN_LOT),, True)
        PrintFooter()

backSku:
        'p_scanSku(1, 8) baypass
        p_scanBatch(1, 12)
        p_scanQty(1, 14)
        p_scanDropId(1, 16)
        Dim bEscFlag As Boolean = False
        If SaveConfirmation(bEscFlag) Then
            If p_save() Then p_ScanQr("SCAN SUCCESS", False)
        Else
            If bEscFlag Then
                GoTo restart
            Else
                p_ScanQr("SCAN FAILED!", True)
            End If
        End If
    End Sub

    Private Sub p_getDataScan(Optional sMessage As String = "", Optional bError As Boolean = False)
        UIWait("")
        ClearMessage()

        Try
            Dim PM As New PickingManager
            Dim alData As ArrayList = PM.GetListByPickId(dtScreen.PickId, dtConnInfo)

            If alData.Count = 0 Then
                If sMessage.Length = 0 Then
                    sMessage = "NOT FOUND"
                    bError = True
                End If
                eActiveUI = RFUiList.PICK_ENQUIRY
                p_ScanQr(sMessage, bError)
            Else
                p_uiScannedList(alData, sMessage, bError)
            End If
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_uiScannedList(alData As ArrayList, sMessage As String, bError As Boolean)
        PrintHeader(RFUiList.PICK_ENQ_LIST)
        PrintLine(1, 4, String.Format("{0} {1} {2}", "FOUND", alData.Count, "SCANNED :"))
        PrintLine(1, 14, "CHOOSE ID: ")
        PrintFooter()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        Dim bClear As Boolean = True
        Dim bMatch As Boolean = False
        Dim iMaxLine As Int16 = 8
        Dim iRec As Int16 = 1
        Dim iChoose As Int16 = 0
        Dim iPos As Int16 = 0
        Dim iLap As Int16 = 0

        For iPos = 0 To alData.Count - 1
            If bClear Then
                PrintLine(11, 15, " ")
                For yPos As Int16 = 5 To 13
                    PrintLine(1, yPos, New String(" ", 25))
                Next
                bClear = False
                bMatch = False
            End If

            dtPicking = alData(iPos)
            PrintLine(1, iRec + 4, String.Format("{0}. {1}", iRec.ToString, dtPicking.ScanId))

            If iRec = iMaxLine Then
                bClear = True
                bMatch = True
                If alData.Count > ((iLap + 1) * iMaxLine) Then PrintLine(1, 13, String.Format("{0}. {1}", "9", "NEXT"))
restart1:
                PrintLine(11, 14, New String(" "))
                iChoose = GetInputNumber(11, 14, 1)
                If iChoose = 0 Then
                    PrintMessage("INVLD ID", True)
                    GoTo restart1
                ElseIf iChoose < 9 Then
                    dtPicking = alData((iChoose - 1) + (iLap * iMaxLine))
                    p_uiInfo()
                End If
                iLap += 1
            End If

            If iPos < alData.Count - 1 Then iRec += 1
            If iRec > iMaxLine Then iRec = 1
        Next

restart2:
        PrintLine(11, 14, New String(" "))
        If bMatch Then
            iLap -= 1
        Else
            iChoose = GetInputNumber(11, 14, 1)
        End If

        If iChoose = 0 Or iChoose > iRec Then
            PrintMessage("INVLD ID", True)
            bMatch = False 'just tricky to avoid nesting restart2
            GoTo restart2
        Else
            dtPicking = alData((iChoose - 1) + (iLap * iMaxLine))
            p_uiInfo()
        End If
    End Sub

    Private Sub p_uiInfo()
        PrintHeader(RFUiList.PICK_ENQ_DETAIL)
        PrintLine(1, 4, "ORDERKEY   :" & dtPickDetail.OrderKey)
        PrintLine(1, 5, "CASE ID    :" & dtPickDetail.CaseId)
        PrintLine(1, 6, "LOT        :" & dtPickDetail.Lot)
        PrintLine(1, 7, "SKU        :" & dtPickDetail.Sku)
        PrintLine(1, 8, "FROM LOC   :" & dtPickDetail.Loc)
        PrintLine(1, 9, "QTY TO PICK:" & dtPickDetail.Qty.ToString & " x " & dtPickDetail.CaseCnt.ToString)
        PrintLine(1, 10, "SCAN ID    :" & dtPicking.ScanId)
        PrintLine(1, 11, "SCAN BY    :" & dtPicking.ScanBy)
        PrintLine(1, 12, "QTY SCANNED:" & dtPicking.QtyCase.ToString & " x " & dtPicking.CaseCnt.ToString)
        PrintLine(1, 13, "BATCH      :" & dtPicking.Batch)
        PrintLine(1, 14, "DROP ID    :" & dtPicking.DropId)
        PrintFooter()

        Dim sMessage As String = String.Empty
        Dim bError As Boolean = False

        If dtPicking.bTransfered Then
            WaitAnyKey("==DATA SCAN IS INVOICED===")
        Else
            Dim bEscFlag As Boolean = False
            If DeleteConfirmation(bEscFlag) Then
                If p_delete() Then sMessage = "DEL SUCCESS"
            Else
                If Not bEscFlag Then
                    sMessage = "DEL FAILED"
                    bError = True
                End If
            End If
        End If

        p_getDataScan(sMessage, bError)
    End Sub

    Private Sub p_scanSku(iLeft As Integer, iTop As Integer)
        ClearMessage()
        Dim sInput As String = String.Empty
invalidSku:
        sInput = GetInput(iLeft, iTop, LEN_SKU, True)
        If sInput.Length > 0 Then
            dtScreen.Sku = sInput
        Else
            sInput = dtScreen.Sku
        End If

        If dtPickDetail.Sku.ToUpper = sInput.ToUpper _
            Or dtPickDetail.BUsr2 = sInput.ToUpper _
            Or dtPickDetail.BUsr3 = sInput.ToUpper Then
            PrintLine(iLeft, iTop, New String(" ", LEN_SKU),, True)
            PrintLine(iLeft, iTop, sInput,, True)
        Else
            PrintMessage("SKU NOT MATCH", True)
            PrintLine(iLeft, iTop, New String(" ", LEN_SKU),, True)
            GoTo invalidSku
        End If
    End Sub

    Private Sub p_scanQty(iLeft As Integer, iTop As Integer)
        ClearMessage()
        Dim bValid As Boolean = False
invalidQty:
        PrintLine(iLeft, iTop, dtPicking.QtyCase,, True)
        PrintLine(LEN_QTY + 4, iTop, dtPicking.CaseCnt,, True)

        Dim iQtyCase As Integer = GetInputNumber(iLeft, iTop, LEN_QTY)
        Dim iCaseCnt As Integer = GetInputNumber(LEN_QTY + 4, iTop, LEN_QTY)

        If iQtyCase > 0 Then dtPicking.QtyCase = iQtyCase
        If iCaseCnt > 0 Then dtPicking.CaseCnt = iCaseCnt

        If dtPicking.QtyCase * dtPicking.CaseCnt = 0 Then
            bValid = False
        ElseIf dtPickDetail.Qty * IIf(dtPickDetail.CaseCnt = 0, 1, dtPickDetail.CaseCnt) >= dtPicking.QtyCase * dtPicking.CaseCnt Then
            bValid = True
        Else
            bValid = False
        End If

        If bValid Then
            PrintMessage(New String(" ", 25))
        Else
            PrintMessage("INVLD QTY", True)
            PrintLine(iLeft, iTop, New String(" ", LEN_QTY),, True)
            PrintLine(LEN_QTY + 4, iTop, New String(" ", LEN_QTY),, True)
            GoTo invalidQty
        End If
    End Sub

    Private Sub p_scanBatch(iLeft As Integer, iTop As Integer)
        ClearMessage()
restart:
        Dim sInput As String = String.Empty
        Dim sInputForCasecnt As String = String.Empty
        Dim iCasecnt As Integer = 0

        sInput = GetScan(iLeft, iTop, True)
        sInputForCasecnt = sInput
        PrintLine(iLeft, iTop, New String(" ", sInput.Length))
        PrintLine(iLeft, iTop, New String(" ", LEN_LOT),, True)

        If sInput.Trim.Length = 0 Then
            PrintMessage("INVLD BATCH", True)
            GoTo restart
        Else
            If sInput.Length > 20 Then
                sInput = p_tryGetLotFromQr(sInput)
                If sInput.Trim.Length > 25 Then
                    PrintMessage("INVLD BATCH / NO CONFIG", True)
                    GoTo restart
                End If

                iCasecnt = p_tryGetCasecntFromQr(sInputForCasecnt)
            End If
            dtPicking.Batch = sInput
            PrintLine(iLeft, iTop, dtPicking.Batch,, True)
            If iCasecnt > 0 Then
                dtPicking.CaseCnt = iCasecnt
                dtPicking.QtyCase = dtPickDetail.Qty / iCasecnt
            End If
        End If
    End Sub

    Private Function p_tryGetLotFromQr(sInput As String) As String
        Dim data As SkuQrConfig.DataSkuQrConfig = p_getQrConfig(dtPickDetail.Sku, dtPickDetail.StorerKey, SkuQrConfig.QrPart.LOT)
        If data.Sku = String.Empty Then Return sInput
        Return GetStringFromQr(sInput, data)
    End Function

    Private Function p_tryGetCasecntFromQr(sInput As String) As Integer
        Dim data As SkuQrConfig.DataSkuQrConfig = p_getQrConfig(dtPickDetail.Sku, dtPickDetail.StorerKey, SkuQrConfig.QrPart.QTY)
        If data.Sku = String.Empty Then
            Return 0
        End If

        Dim sOut As String = GetStringFromQr(sInput, data)
        If sOut = String.Empty Then
            Return 0
        Else
            Return CInt(sOut)
        End If

    End Function

    Private Function p_getQrConfig(sSku As String, sStorerKey As String, eQrPart As SkuQrConfig.QrPart) As SkuQrConfig.DataSkuQrConfig
        'UIWait("")

        Dim data As New SkuQrConfig.DataSkuQrConfig
        Try
            Return New SkuQrConfig().Get(sSku, sStorerKey, eQrPart.ToString, dtConnInfo)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return data
    End Function

    Private Sub p_scanDropId(iLeft As Integer, iTop As Integer)
        ClearMessage()
restart:
        Dim sInput As String = String.Empty
        sInput = GetInput(iLeft, iTop, 20, True)
        If sInput.Trim.Length = 0 Then
            PrintMessage("INVLD DROP ID", True)
            GoTo restart
        Else
            dtPicking.DropId = sInput
        End If
    End Sub

    Private Sub p_splitDataScanQR(iLeft As Integer, iTop As Integer)
        'qrcode = caseid + sku + lot + loc + enter
        dtScreen.QrCode = GetInput(iLeft, iTop, 100)
        Dim sQR() As String = Split(dtScreen.QrCode, DELIMITER_PICKDETAIL)
        If sQR.Count >= 1 Then dtScreen.CaseId = sQR(0)
        If sQR.Count >= 2 Then dtScreen.Sku = sQR(1)
        If sQR.Count >= 3 Then dtScreen.Lot = sQR(2)
        If sQR.Count >= 4 Then dtScreen.Loc = sQR(3)

        dtScreen.PickId = dtScreen.CaseId & dtScreen.Sku & dtScreen.Lot & dtScreen.Loc
    End Sub

    Private Sub p_scanQRPalletOutbound(iLeft As Integer, iTop As Integer)
        'qrcode = C011.SJ19080106C [enter] ORDER
        dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 2)
        Dim sQR() As String = Split(dtScreen.QrCode, vbCr)
        If sQR.Count >= 1 Then dtScreen.PalletId = sQR(0)
    End Sub

    Private Sub p_scanQROutbound(iLeft As Integer, iTop As Integer)
        'qrcode = CIVUS0.35 R,3000,MTR,20190626,PC19060264J,1/1,4713,022146,19062D.3342,C014,-,50 B-04 sd 50 B-06,201906-066728
        'qrcode = SKU        ,Qty              ,Invoice#                               ,Pallet#                  ,Unique#
        'qrcode = 0          ,1                ,4                                      ,9                        ,12
        dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
        Dim sQR() As String = Split(dtScreen.QrCode, DELIMITER_QR_OUTBOUND)
        If sQR.Count >= 1 Then dtScreen.SkuFromQr = sQR(0)
        If sQR.Count >= 2 Then dtScreen.Qty2 = sQR(1)
        If sQR.Count >= 5 Then dtScreen.LogScan3 = sQR(4)
        If sQR.Count >= 10 Then dtScreen.LogScan3 = String.Format("{0},{1}", dtScreen.LogScan3, sQR(9))
        If sQR.Count >= 13 Then dtScreen.LogScan3 = String.Format("{0},{1}", dtScreen.LogScan3, sQR(12))
    End Sub

    Private Sub p_scanQRInbound(iLeft As Integer, iTop As Integer)
        Dim dataQRi As DataQRInbound = GetDataQRInbound(iLeft, iTop)
        dtScreen.StorerKey = dataQRi.StorerKey
        dtScreen.Sku = dataQRi.Sku
        dtScreen.Qty1 = dataQRi.Qty
        dtScreen.ID = dataQRi.ID
    End Sub

    Private Function p_isIDInboundExists() As Boolean
        Dim dtLogForValidation As New Log.DataLog
        dtLogForValidation.LabelID = dtScreen.ID
        dtLogForValidation.MenuID = eActiveUI.ToString()

        Return New Log().IsLabelIDExists(dtLogForValidation, dtConnInfo)
    End Function

    Private Sub p_scanQRLabelPackage(iLeft As Integer, iTop As Integer)
        If dtScreen.Sku <> String.Empty Then Exit Sub

        'qrcode = 000000 [enter] 000 [enter] PLBSAM001 [enter] ? [enter] 4304B1930 [enter] 500
        dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 6)
        If dtScreen.QrCode = String.Empty Then p_PackChecking("INVLD QR", True)

        Dim sQR() As String = Split(dtScreen.QrCode, vbCr)
        If sQR.Count >= 3 Then dtScreen.StorerKey = sQR(2).Trim()
        If sQR.Count >= 5 Then dtScreen.Sku = sQR(4).Trim()
        If sQR.Count >= 6 Then dtScreen.Qty1 = sQR(5).Trim()

        If dtScreen.Sku = String.Empty Then p_PackChecking("EMPTY NOT ALLOWED", True)
        If dtScreen.StorerKey <> dtUserActive.StorerKey Then p_PackChecking("INVLD STORER", True)

        dtSku = p_getSkuByPrediction(dtScreen.Sku.Replace("-", ""))
        If dtSku.Sku = String.Empty Then p_PackChecking("SKU N/A", True)
        p_PackChecking(False)
    End Sub

    Private Sub p_scanQR_rawmat(iLeft As Integer, iTop As Integer)
        If Not dtScreen.Sku = String.Empty Then Exit Sub

        'qrcode = storerkey + caseid + sku + fromid + toid
        dtScreen.QrCode = GetScanFixEnterPrint(iLeft, iTop, False, 5)
        'dtScreen.QrCode = GetScan(iLeft, iTop)
        Dim sQR() As String = Split(dtScreen.QrCode, vbCr)
        If sQR.Count >= 1 Then dtScreen.StorerKey = sQR(0).Trim()
        If sQR.Count >= 2 Then dtScreen.CaseId = sQR(1)
        If sQR.Count >= 3 Then dtScreen.Sku = sQR(2).Trim()

        ClearMessage()
        If dtScreen.StorerKey <> dtUserActive.StorerKey Then p_PickingCheck("INVLD STORER", True)
        p_PickingCheck()
    End Sub

    Private Sub p_checkOutstanding(ByRef iSOLevel As Integer, ByRef iSkuLevel As Integer, ByRef iSOSku As Integer)
        UIWait("")

        Try
            Dim PM As New PickingManager
            PM.GetOutstanding(iSOLevel, iSkuLevel, dtUserActive.StorerKey, dtConnInfo)
            If dtPickDetail.OrderKey.Length > 0 Then
                PM.GetOutstanding(dtPickDetail.OrderKey, iSOSku, dtConnInfo)
            End If
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    'Private Function p_splitPickingByAvailableLot() As ArrayList
    '    Dim alPicking As New ArrayList
    '    Dim alOrder As New ArrayList
    '    Dim OM As New OrderManager
    '    alOrder = OM.GetAvailableLot(dtPickDetail.OrderKey, dtPickDetail.Sku)
    '    For Each dt As OrderManager.DataAllocationSummary In alOrder
    '        Dim dtPickingSplitted As New PickingManager.DataPicking
    '        dtPickingSplitted = dtPicking
    '        If dtPicking.QtyCase * dtPicking.CaseCnt > dt.Qty Then
    '            dtPickingSplitted.QtyCase = dt.Qty / dtPickingSplitted.CaseCnt
    '            dtPicking.QtyCase = dtPicking.QtyCase - dtPickingSplitted.QtyCase
    '        End If
    '        dtPickingSplitted.Lot = dt.Lot
    '        alPicking.Add(dtPickingSplitted)
    '        If dtPicking.QtyCase <= 0 Or dtPicking.QtyCase * dtPicking.CaseCnt <= dt.Qty Then Exit For
    '    Next

    '    Return alPicking
    'End Function

    'Private Function p_save() As Boolean
    '    UIWait("")
    '    Dim bInserted As Boolean = False

    '    Try
    '        Dim alPicking As ArrayList = p_splitPickingByAvailableLot()
    '        For Each dt As PickingManager.DataPicking In alPicking
    '            dt.dtmScanned = Now
    '            dt.dtmLastUpdated = Now
    '            Dim PM As New PickingManager
    '            PM.SetData(dt)
    '        Next
    '    Catch ex As Exception
    '        UIError(ex.Message & vbCrLf & ex.StackTrace)
    '    End Try

    '    Return True
    'End Function

    Private Function p_save() As Boolean
        UIWait("")
        Try
            Dim PM As New PickingManager
            PM.Save(dtPicking, dtConnInfo)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return True
    End Function

    Private Function p_delete() As Boolean
        UIWait("")
        Try
            Dim PM As New PickingManager
            dtPicking.EditBy = dtUserActive.UserName
            PM.Delete(dtPicking, dtConnInfo)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return True
    End Function


#End Region

#Region "Data Class"
    Public Class DataScreen
        Public StorerKey As String = String.Empty
        Public QrCode As String = String.Empty
        Public PalletId As String = String.Empty
        Public PickId As String = String.Empty
        Public CaseId As String = String.Empty
        Public Sku As String = String.Empty
        Public SkuFromQr As String = String.Empty
        Public SkuIdPart As String = String.Empty
        Public Lot As String = String.Empty
        Public Loc As String = String.Empty
        Public Qty1 As Int32 = 0
        Public Qty2 As Int32 = 0
        Public Status As String = String.Empty
        Public LogScan3 As String = String.Empty
        Public ID As String = String.Empty
    End Class
#End Region


End Module
