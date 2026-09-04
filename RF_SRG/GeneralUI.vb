Imports System.Reflection
Imports AIT.RF
Imports AIT.SM
Imports AIT.GEN
Imports System.Threading


Module GeneralUI

#Region "Public"

    'Public Const TITLE As String = "WMS SUPPORTING APPS"
    Public Const DEFAULT_TEXTBOX_LENGTH As Int16 = 25
    Public Const DEFAULT_WIDTH_WINDOW As Int16 = 30
    Public Const DEFAULT_HEIGHT_WINDOW As Int16 = 30
    Public Const DEFAULT_DOMAIN As String = "DSV"

    Public Const MESSAGE_INFO_SCAN_COMPLETE As String = "SCAN COMPLETE. GOOD JOB!"
    Public Const MESSAGE_INFO_INVALID_IDSTORER As String = "INVALID ID/STORER"
    Public Const MESSAGE_INFO_INVALID_LOGIN As String = "INVALID LOGIN"
    Public Const MESSAGE_INFO_INVALID_STORER As String = "INVALID STORER"
    Public Const MESSAGE_INFO_PALLETID As String = "INVALID PALLET ID"
    Public Const MESSAGE_INFO_INVALID_SKU As String = "INVALID SKU"
    Public Const MESSAGE_INFO_INVALID_QR As String = "INVALID QR"
    Public Const MESSAGE_INFO_QR_IS_USED As String = "QR IS USED"
    Public Const MESSAGE_INFO_OVERSCAN As String = "OVERSCAN"
    Public Const MESSAGE_INFO_SAVE_CANCELED As String = "SAVE CANCELED"
    Public Const MESSAGE_INFO_SAVE_SUCCESS As String = "SAVE SUCCESS"
    Public Const MESSAGE_INFO_ALREADY_SCANNED As String = "ALREADY SCANNED"
    Public Const MESSAGE_INFO_SKU_NOT_FOUND As String = "SKU NOT FOUND"
    Public Const MESSAGE_INFO_SKU_NOT_MATCH As String = "SKU NOT MATCH"
    Public Const MESSAGE_INFO_SKU_MATCH As String = "SKU MATCH"
    Public Const MESSAGE_INFO_CHECKING_DATA As String = "CHECKING DATA"
    Public Const MESSAGE_INFO_PREPARING_WMSDATA As String = "PREPARING WMS DATA"
    Public Const MESSAGE_INFO_UPDATING As String = "UPDATING"
    Public Const MESSAGE_INFO_UPDATE_SUCCESS As String = "UPDATE SUCCESS"
    Public Const MESSAGE_INFO_DELETING As String = "DELETING"
    Public Const MESSAGE_INFO_DELETE_SUCCESS As String = "DELETE SUCCESS"
    Public Const MESSAGE_INFO_DATA_NOTFOUND As String = "DATA NOT FOUND"

    Public Const MESSAGE_CONFIRMATION_RESCAN As String = "DATA EXISTS! RESCAN?"

    Public Const MESSAGE_WARNING_OVERSCAN As String = "O V E R S C A N"
    Public Const MESSAGE_WARNING_SKU_NOT_FOUND As String = "S K U   N O T   F O U N D"
    Public Const MESSAGE_WARNING_INVALID_STORER As String = "I N V A L I D   S T O R E R"
    Public Const MESSAGE_WARNING_INVALID_ENTRY As String = "I N V A L I D   E N T R Y"

    Public dtConnInfo As New DataConnectionInfo
    Public Const APP_ENVIRONMENT As String = "TST"
    Public Const INITIAL_CATALOG As String = "WSOAP"
    Public Const DATASOURCE_TST As String = "i29804"
    Public Const DATASOURCE_QA As String = "i29984"
    Public Const DATASOURCE_PRD As String = "i29810"

    Public dtConnInfoWms As New DataConnectionInfo
    Public Const INITIAL_CATALOG_WMS As String = "PRAPR"
    Public Const DATASOURCE_TST_WMS As String = "i29804"
    Public Const DATASOURCE_QA_WMS As String = "i29804"
    Public Const DATASOURCE_PRD_WMS As String = "i29804"

    Public sTitle As String = String.Format("{0} - {1}", My.Application.Info.Title, APP_ENVIRONMENT)
    Public eActiveUI As RFUiList
    Public VERSION As String = String.Empty
    'Public dtUserActive As UserManager.DataUser
    Public dtUserActive As New DataUserActive


    Public Sub RF_Apps()
        p_getVersion()
        p_Welcome()
        p_Login()
    End Sub

    Public Sub MainMenu()
        p_MenuByStorer()
    End Sub

    Public Sub ClearMessage(Optional iTop As Int16 = 17)
        p_clearMessage(iTop)
    End Sub

    Public Sub PrintMessage(sMessage As String, Optional iTop As Int16 = 17)
        p_printMessage(sMessage, iTop)
    End Sub

    Public Sub PrintMessage(sMessage As String, bError As Boolean, Optional iTop As Int16 = 17)
        If sMessage.Length = 0 Then Exit Sub
        If bError Then
            p_printMessageWithError(sMessage & Chr(7), iTop)
        Else
            p_printMessage(sMessage, iTop)
        End If
    End Sub

    Public Sub PrintHeader(eUI As RFUiList)
        p_printHeader(eUI)
    End Sub

    Public Sub PrintFooter()
        p_printFooter()
    End Sub

    Public Sub PrintFooterWithElapse(elapseStart As Date, Optional iTop As Int16 = 18)
        Dim elapse As TimeSpan = Now() - elapseStart
        p_printFooterWithElapse(elapse.TotalMilliseconds, iTop)
    End Sub

    Public Sub PrintFooterWithElapse(milliSecond As Double, Optional iTop As Int16 = 18)
        p_printFooterWithElapse(milliSecond, iTop)
    End Sub

    Public Sub PrintFooterEsc()
        p_printFooterEsc()
    End Sub

    Public Sub UIWait(Optional sMessage As String = "")
        p_wait(sMessage)
    End Sub

    Public Sub UIError(sMessage As String)
        p_error(sMessage)
    End Sub

    Public Sub UIWarning(sMessage As String, Optional bGotoPrevUi As Boolean = True)
        p_uiWarning(sMessage, bGotoPrevUi)
    End Sub

    Public Sub GotoUi(Optional eUi As RFUiList = RFUiList.EMPTY)
        p_gotoUi(eUi)
    End Sub

    Public Function GetScanQRPalletOutbound(iLeft As Integer, iTop As Integer, Optional iFixEnter As Integer = 2) As DataScan
        Return p_getScanQRPalletOutbound(iLeft, iTop, iFixEnter)
    End Function

    Public Function GetScan(iLeft As Integer, iTop As Integer, Optional bWithDoubleEnter As Boolean = False) As String
        Return p_getScanWDoubleEnter(iLeft, iTop, bWithDoubleEnter)
    End Function

    Public Function GetScanUnknown(iLeft As Integer, iTop As Integer) As String
        Return p_getScan(iLeft, iTop)
    End Function

    Public Function GetScan(iLeft As Integer, iTop As Integer) As String
        Return p_getScan(iLeft, iTop, True)
    End Function

    Public Function GetScanNoPrint(iLeft As Integer, iTop As Integer) As String
        Return p_getScan(iLeft, iTop, False)
    End Function

    Public Function GetScanFixEnterPrint(iLeft As Integer, iTop As Integer, bPrint As Boolean, iFixEnter As Int16) As String
        Return p_getScan(iLeft, iTop, bPrint, iFixEnter)
    End Function

    Public Function GetInput(iLeft As Integer, iTop As Integer, iLength As Integer) As String
        Return p_getInput(iLeft, iTop, iLength, False, False, False)
    End Function

    Public Function GetInput(iLeft As Integer, iTop As Integer, iLength As Integer, bUpperCase As Boolean) As String
        Return p_getInput(iLeft, iTop, iLength, False, False, bUpperCase)
    End Function

    Public Function GetInputPassword(iLeft As Integer, iTop As Integer, iLength As Integer) As String
        Return p_getInput(iLeft, iTop, iLength, True, False, False)
    End Function

    Public Function GetInputNumber(iLeft As Integer, iTop As Integer, iLength As Integer) As Integer
        Dim sInput As String = String.Empty
        sInput = p_getInput(iLeft, iTop, iLength, False, True, False)
        If sInput.Length = 0 Then Return 0
        Return CInt(sInput)
    End Function

    Public Function GetInputEscapeOnly(iLeft As Integer, iTop As Integer) As Boolean
        Return p_getInputEscapeOnly(iLeft, iTop)
    End Function

    Public Function GetInputEnterOrEscapeOnly(iLeft As Integer, iTop As Integer) As Boolean
        Return p_getInputEnterOrEscapeOnly(iLeft, iTop)
    End Function

    Public Sub PrintLine(iLeft As Integer, iTop As Integer, sMessage As String, Optional bLine As Boolean = True, Optional bTextbox As Boolean = False)
        p_printLine(iLeft, iTop, sMessage, bLine, bTextbox)
    End Sub

    Public Sub PrintLineWithBgColor(iLeft As Integer, iTop As Integer, sMessage As String, bgColor As ConsoleColor, fgColor As ConsoleColor, Optional bLine As Boolean = True)
        p_printLineWithColor(iLeft, iTop, sMessage, bgColor, fgColor, bLine)
    End Sub

    Public Function SaveConfirmation(ByRef bEscFlag As Boolean) As Boolean
        Return p_saveConfirmation(bEscFlag)
    End Function

    Public Function DeleteConfirmation(ByRef bEscFlag As Boolean) As Boolean
        Return p_deleteConfirmation(bEscFlag)
    End Function

    Public Sub WaitAnyKey(sMessage As String)
        p_waitAnyKey(sMessage)
    End Sub

    Public Sub WaitAnyKey(sMessage As String, iTop As Int16)
        p_waitAnyKey(sMessage, iTop)
    End Sub

    Public Function GetStringFromQr(sInput As String, data As SkuQrConfig.DataSkuQrConfig) As String
        Return p_getStringFromQr(sInput, data)
    End Function

    Public Sub SaveLog(dtLog As Log.DataLog)
        p_saveLog(dtLog)
    End Sub

    Public Function GetSkuFromQRProduct(sInput As String) As String
        Return p_getSkuFromQRProduct(sInput)
    End Function

    Public Function GetSkuByPrediction(sStorerKey As String, sSku As String) As Sku.DataSku
        Return p_getSkuByPrediction(sStorerKey, sSku)
    End Function

    Public Function GetDataQRInbound(iLeft As Integer, iTop As Integer, Optional eType As TypeQrInbound = TypeQrInbound.GENERAL) As DataQRInbound
        Return p_getDataQRInbound(iLeft, iTop, eType)
    End Function

#End Region

#Region "Private"

    Private Function p_getSkuFromQRProduct(sInput As String) As String
        Dim bValid As Boolean = False
        Dim sSku As String = String.Empty

        If Not bValid And sInput.Substring(0, 1) = "[" Then
            If sInput.Substring(0, 8) = "[)>06 LT" Then
                sSku = p_getSplit(sInput, " ", 2, "PN")
            End If

            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 1, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 1, "P")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 2, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 2, "P")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 3, "PN")
            If sSku = String.Empty Then sSku = p_getSplit(sInput, "chrw29", 3, "P")

            If Not sSku = String.Empty Then bValid = True
        End If

        If Not bValid And sInput.Length > 9 Then
            If sInput.Substring(0, 9) = "P00000000" Then sSku = Replace(sInput.Substring(10, sInput.Length - 9), "-", "")
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 1) = "_" Then
            sSku = p_getSplit(sInput, "chrw29", 1, "PN")
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 3) = "30S" Then
            sSku = Replace(sInput.Substring(3, sInput.Length - 3), "-", "")
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 4) = "32G8" Then
            sSku = p_getSplit(sInput, ",", 4, String.Empty)
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 1) = "P" Then
            sSku = Replace(sInput.Substring(1, sInput.Length - 1), "-", "")
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 1) = "$" Then
            If sInput.Contains(vbTab) Then sSku = p_getSplit(sInput, vbTab, 2, String.Empty)
            If sSku = String.Empty Then sSku = p_getSplit(sInput, ",", 1, String.Empty)

            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Length > 9 Then
            If sInput.Substring(0, 9) = "108925339" Then sSku = RTrim(Replace(sInput.Substring(9, 12), "-", ""))
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Substring(0, 4) = "EPIC" Then
            sSku = RTrim(Replace(sInput.Substring(5, 15), "-", ""))
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Length > 9 Then
            If sInput.Substring(8, 2) = " L" Then sSku = sInput.Substring(0, 8)
            If Not sSku = String.Empty Then bValid = True
        End If
        If Not bValid And sInput.Length > 23 Then
            If sInput.Substring(sInput.Length - 3, 2) = "GK" Then sSku = sInput.Substring(24, 10)
            If Not sSku = String.Empty Then bValid = True
        End If

        If Not bValid Then sSku = Replace(sInput, "-", "")

        Return sSku
    End Function

    Private Function p_getSplit(sInput As String, sDelimiter As String, iIndex As Int16, sPrefix As String) As String
        Dim str() As String

        If sDelimiter.Length > 4 Then
            If sDelimiter.Substring(0, 4).ToLower = "chrw" Then str = Split(sInput, ChrW(sDelimiter.Substring(4, 2)))
        Else
            str = Split(sInput, sDelimiter)
        End If

        If str.Length > iIndex Then
            If sPrefix = String.Empty Then
                Return Replace(str(iIndex), "-", "")
            ElseIf str(iIndex).Substring(0, sPrefix.Length).ToLower = sPrefix.ToLower Then
                Return Replace(str(iIndex).Substring(sPrefix.Length, str(iIndex).Length - sPrefix.Length), "-", "")
            End If
        End If

        Return String.Empty
    End Function

    Private Function p_getSkuByPrediction(sStorerKey As String, sSku As String) As Sku.DataSku
        UIWait("")
        ClearMessage()

        Try
            Return New Sku().GetByPrediction(sStorerKey, sSku, dtConnInfo)

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return New Sku.DataSku
    End Function

    Private Function p_getStringFromQr(sInput As String, data As SkuQrConfig.DataSkuQrConfig) As String
        Dim sQR() As String = Split(sInput, data.DelimiterText)

        If sQR.Count >= data.LineNumberText Then
            Dim str() As String

            If data.DelimiterInner.Substring(0, 4).ToLower = "chrw" Then
                str = Split(sQR(data.LineNumberText - 1), ChrW(data.DelimiterInner.Substring(5, 2)))
            Else
                str = Split(sQR(data.LineNumberText - 1), data.DelimiterInner)
            End If

            If str.Count >= data.LineNumberInner Then
                If str(data.LineNumberInner - 1).Substring(0, Len(data.Prefix)).ToLower = data.Prefix.ToLower Then
                    If data.StringLen = 0 Then data.StringLen = str(data.LineNumberInner - 1).Length - data.Prefix.Length
                    Return str(data.LineNumberInner - 1).Substring(data.Prefix.Length, data.StringLen)
                End If
            End If
        End If

        Return sInput
    End Function

    Private Function p_getChar(sDelimiter As String) As Char
        If sDelimiter.Substring(1, 4).ToLower = "chrw" Then
            Return ChrW(sDelimiter.Substring(5, 2))
        End If

        Return Nothing
    End Function

    Private Sub p_error(sMessage As String)
        'Console.Clear()
        'Console.ResetColor()
        'p_printLine(1, 1, sTitle)
        p_printHeader(eActiveUI)

        Console.ForegroundColor = ConsoleColor.Yellow
        p_printLine(1, 3, "ERROR OCCURED")
        Console.ForegroundColor = ConsoleColor.White
        p_printLine(1, 5, sMessage)
        p_printLine(1, 4, New String(" ", 0),, True)
        p_printFooter()

        p_beepError()
        Thread.Sleep(5000)

        If p_getInputEscapeOnly(1, 4) Then p_gotoUi(eActiveUI)
    End Sub

    Private Sub p_uiWarning(sMessage As String, bGotoPrevUi As Boolean, Optional iDelay As Int16 = 12)
        Console.Clear()
        Console.ResetColor()
        Console.BackgroundColor = ConsoleColor.Yellow
        Console.ForegroundColor = ConsoleColor.Black

        p_beepError()

        For i As Integer = 1 To DEFAULT_HEIGHT_WINDOW
            Console.WriteLine(New String(" ", DEFAULT_WIDTH_WINDOW))
        Next

        Console.SetCursorPosition(1, 1)
        Console.WriteLine(sTitle)
        Console.WriteLine("")
        Console.WriteLine(" =====================")
        Console.WriteLine(" --  W A R N I N G  --")
        Console.WriteLine(" =====================")
        Console.WriteLine("")

        For i As Integer = 1 To 3
            Console.WriteLine(sMessage)
            Console.WriteLine("")
        Next

        For i As Integer = 0 To iDelay
            Console.SetCursorPosition(1, 12)
            Console.WriteLine(String.Format("{0} {1} {2}", "Wait for", (iDelay - i).ToString(), "sec "))
            Thread.Sleep(1000)
        Next

        Console.WriteLine("")
        Console.WriteLine("")
        Console.WriteLine(" PRESS ESC TO CONTINUE")

        If p_getInputEscapeOnly(1, 4) Then
            Console.Clear()
            Console.ResetColor()
            If bGotoPrevUi Then p_gotoUi(eActiveUI)
        End If
    End Sub

    Private Sub p_wait(Optional sMessage As String = "")
        Console.Clear()
        Console.ResetColor()
        p_printLine(1, 1, sTitle)
        If sMessage.Length > 0 Then PrintLine(1, 2, sMessage)

        Console.ForegroundColor = ConsoleColor.Green
        p_printLine(1, 8, "PLEASE WAIT...")
        Console.ForegroundColor = ConsoleColor.White
    End Sub

    Public Enum RFUiList
        EMPTY
        WELCOME
        LOGIN
        LOGIN_OPERATOR
        USER_INFO
        CHANGE_PIN
        MENU_WHS
        MENU_STORER
        MENU_STD
        MENU_SRGPLB_39
        MENU_SRGPLB_10
        PICKING
        PICK_INPUT
        PICK_ENQUIRY
        PICK_ENQ_LIST
        PICK_ENQ_DETAIL
        PICK_AUDIT
        PICK_CHECK
        PICK_CHECK_QQ
        PALLET_CHECK
        PICK_PALLET_SCAN
        PICK_PALLET_OVER
        PICK_PALLET_COMPLETE
        PICK_PACK_CHECK
        PS_INVOICE
        PS_PALLET
        OTHERS
        QR_CAPTURE
        RECEIVING
        RECV_CHECK
        INV_ENQ_FG
        FG_PICK_CHECK
        FG_LOAD_CHECK
        FG_LABEL_CHECK
        FG_DEL_SCAN
        FG_INV_LBL_CHECK
        QR_DECODE_PLT
        RECEIVING_00
        PICKING_00
        ORDER_SERIAL_NUMBER
    End Enum

    Public Enum TypeQrInbound
        GENERAL
        PALLET_CHECKING
    End Enum

    Private Sub p_gotoUi(Optional eUi As RFUiList = RFUiList.EMPTY)
        If eUi = RFUiList.EMPTY Then
            eUi = eActiveUI
        Else
            eActiveUI = eUi
        End If

        Select Case eUi
            Case RFUiList.WELCOME
                p_Welcome()
                p_Login()
            Case RFUiList.LOGIN
                p_Welcome()
                p_Login()
            Case RFUiList.LOGIN_OPERATOR
                p_LoginOperator()
            Case RFUiList.USER_INFO
                p_UserInfo()
            Case RFUiList.CHANGE_PIN
                p_changePin()
            Case RFUiList.MENU_WHS
                p_MenuWhs()
            Case RFUiList.MENU_STORER
                p_MenuStorer()
            Case RFUiList.MENU_SRGPLB_39
                p_MenuSrgPlb39()
            Case RFUiList.MENU_SRGPLB_10
                p_MenuSrgPlb10()
            Case RFUiList.MENU_STD
                p_MenuStd()

            Case RFUiList.PICKING
                PickingUI.Main()
            Case RFUiList.PICK_INPUT
                PickingUI.PickingInputUi()
            Case RFUiList.PICK_ENQUIRY
                PickingUI.PickingEnquiryUi()
            Case RFUiList.PICK_ENQ_DETAIL
                PickingUI.GetDataScan()
            'Case RFUiList.PICKING_INFO
            '    PalletShipment.Main()
            'Case RFUiList.PS_PALLET
            '    PalletShipment.Input()
            Case RFUiList.PICK_AUDIT
                PickingUI.PickingAuditUi()
            Case RFUiList.PICK_CHECK
                PickingUI.PickingCheckUi()
            Case RFUiList.PICK_CHECK_QQ
                PickingUI.PickingCheckQqUi()
            Case RFUiList.PALLET_CHECK
                'PickingUI.PickingPalletCheckUi()
                RM_PalletCheckUI.Main()
            Case RFUiList.PICK_PACK_CHECK
                PickingUI.PackCheckingUi()
            Case RFUiList.OTHERS
                p_MenuOthers()
            Case RFUiList.OTHERS
                QrEnquiryUI.Main()
            Case RFUiList.QR_CAPTURE
                QrEnquiryUI.Main()
            Case RFUiList.RECEIVING
                ReceivingUI.Main()
            Case RFUiList.RECV_CHECK
                ReceivingUI.ReceiptCheck()
            Case RFUiList.FG_PICK_CHECK
                FG_PickCheckUI.Main()
            Case RFUiList.FG_LOAD_CHECK
                FG_LoadCheckUI.Main()
            Case RFUiList.FG_LABEL_CHECK
                'FG_LabelCheckUI.Main()
            Case RFUiList.FG_DEL_SCAN
                FG_DeleteScanUI.Main()
            Case RFUiList.FG_INV_LBL_CHECK
                FG_ScanInvoiceLabelUI.Main()
            Case RFUiList.QR_DECODE_PLT
                FG_LabelCheckUI.DecodeQRPallet()
            Case RFUiList.RECEIVING_00
                Receiving00Ui.Main()
            Case RFUiList.PICKING_00
                Picking00Ui.Main()
            Case RFUiList.ORDER_SERIAL_NUMBER
                OrderSerialNumber.Main()

        End Select
    End Sub

    Private Sub p_gotoPrevUi(Optional eUi As RFUiList = RFUiList.EMPTY)
        If eUi = RFUiList.EMPTY Then eUi = eActiveUI
        Select Case eUi
            Case RFUiList.WELCOME
                p_Welcome()
                p_Login()
            Case RFUiList.LOGIN
                p_Welcome()
                p_Login()
            Case RFUiList.LOGIN_OPERATOR
                p_Welcome()
                p_Login()
            Case RFUiList.USER_INFO
                p_LoginOperator()
            Case RFUiList.CHANGE_PIN
                p_Welcome()
                p_Login()
            Case RFUiList.MENU_WHS
                'must log off
            Case RFUiList.MENU_STORER
                p_MenuWhs()
            Case RFUiList.MENU_SRGPLB_39
                p_MenuStorer()
            Case RFUiList.MENU_SRGPLB_10
                p_MenuWhs()
            Case RFUiList.MENU_STD
                p_MenuWhs()

            Case RFUiList.PICKING
                p_MenuByStorer()
            Case RFUiList.PICK_INPUT
                PickingUI.Main()
            Case RFUiList.PICK_ENQUIRY
                PickingUI.Main()
            Case RFUiList.PICK_ENQ_LIST
                eActiveUI = RFUiList.PICK_ENQUIRY
                PickingUI.PickingEnquiryUi()
            Case RFUiList.PICK_ENQ_DETAIL
                PickingUI.GetDataScan()
            Case RFUiList.PICK_AUDIT
                PickingUI.Main()
            Case RFUiList.PICK_CHECK
                PickingUI.Main()
            Case RFUiList.PICK_CHECK_QQ
                PickingUI.Main()
            Case RFUiList.PALLET_CHECK
                RM_PalletCheckUI.GoToPrevScreen()
            Case RFUiList.PICK_PALLET_COMPLETE
                PickingUI.PickingPalletCheckUi()
            Case RFUiList.PICK_PACK_CHECK
                PickingUI.PackCheckingPrevUI()
            'Case RFUiList.PS_INVOICE
            '    p_Menu()
            'Case RFUiList.PS_PALLET
            '    PalletShipment.Main()
            Case RFUiList.OTHERS
                p_MenuByStorer()
            Case RFUiList.QR_CAPTURE
                p_MenuOthers()
            Case RFUiList.RECEIVING
                p_MenuByStorer()
            Case RFUiList.RECV_CHECK
                ReceivingUI.Main()
            Case RFUiList.FG_PICK_CHECK
                FG_PickCheckUI.GotoPrevScreen()
            Case RFUiList.FG_LOAD_CHECK
                FG_LoadCheckUI.GotoPrevScreen()
            Case RFUiList.FG_LABEL_CHECK
                FG_LabelCheckUI.GotoPrevScreen()
            Case RFUiList.FG_DEL_SCAN
                FG_DeleteScanUI.GotoPrevScreen()
            Case RFUiList.FG_INV_LBL_CHECK
                FG_ScanInvoiceLabelUI.GotoPrevScreen()

            Case RFUiList.QR_DECODE_PLT
                FG_LabelCheckUI.GotoPrevScreen()
            Case RFUiList.RECEIVING_00
                Receiving00Ui.GotoPrevScreen()
            Case RFUiList.PICKING_00
                Picking00Ui.GotoPrevScreen()
            Case RFUiList.ORDER_SERIAL_NUMBER
                OrderSerialNumber.GoToPrevScreen()

        End Select
    End Sub

    Private Sub p_printLine(iLeft As Integer, iTop As Integer, sMessage As String, Optional bLine As Boolean = True, Optional bTextbox As Boolean = False)
        If bTextbox Then
            Console.BackgroundColor = ConsoleColor.White
            Console.ForegroundColor = ConsoleColor.Black
        End If

        Console.SetCursorPosition(iLeft, iTop)
        If bLine Then
            Console.WriteLine(sMessage)
        Else
            Console.Write(sMessage)
        End If

        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
    End Sub

    Private Sub p_printLineWithColor(iLeft As Integer, iTop As Integer, sMessage As String, bgColor As ConsoleColor, fgColor As ConsoleColor, bLine As Boolean)
        Console.ForegroundColor = fgColor
        Console.BackgroundColor = bgColor

        Console.SetCursorPosition(iLeft, iTop)
        If bLine Then
            Console.WriteLine(sMessage)
        Else
            Console.Write(sMessage)
        End If

        If bgColor > 0 Then
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White
        End If
    End Sub

    Private Sub p_printHeader(eUI As RFUiList)
        eActiveUI = eUI
        Console.Clear()
        Console.ResetColor()
        p_printLine(1, 1, sTitle)
        Console.BackgroundColor = ConsoleColor.White
        p_printLine(1, 2, New String(" ", 25))
        Console.BackgroundColor = ConsoleColor.White
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, 2, String.Format("{0} {1} {2}", "<< ", eActiveUI.ToString, " >>"))
    End Sub

    Private Sub p_printFooterWithElapse(milliSecond As Double, iTop As Int16)
        Dim second As Double = milliSecond / 1000
        Console.ForegroundColor = ConsoleColor.Green
        p_printLine(1, iTop, "[ENTER = CONFIRM] [ESC = BACK]")
        p_printLine(1, iTop + 1, String.Format("{0} {1} {2}{3}", VERSION, " | Elapse ", second.ToString("0.0#"), "s"))
    End Sub

    Private Sub p_printFooter()
        Console.ForegroundColor = ConsoleColor.Green
        p_printLine(1, 18, "[ENTER = CONFIRM] [ESC = BACK]")
        p_printLine(1, 19, VERSION)
    End Sub

    Private Sub p_printFooterEsc()
        Console.ForegroundColor = ConsoleColor.Green
        p_printLine(1, 18, "[ESC = BACK]")
        p_printLine(1, 19, VERSION)
    End Sub

    Private Sub p_printMessageWithError(sMessage As String, Optional iTop As Int16 = 17)
        Console.BackgroundColor = ConsoleColor.Yellow
        p_printLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Console.BackgroundColor = ConsoleColor.Yellow
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, iTop, sMessage)
    End Sub

    Private Sub p_printMessage(sMessage As String, Optional iTop As Int16 = 17)
        Console.BackgroundColor = ConsoleColor.Green
        p_printLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Console.BackgroundColor = ConsoleColor.Green
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, iTop, sMessage)
    End Sub

    Private Sub p_clearMessage(iTop As Int16)
        p_printLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
    End Sub

    Private Sub p_beepError()
        Thread.Sleep(1000)
        Console.WriteLine(Chr(7))
        Console.WriteLine(Chr(7))
        Console.WriteLine(Chr(7))
    End Sub

    Private Sub p_beepStart()
        Console.WriteLine(Chr(7))
    End Sub

    Private Sub p_beepEnd()
        Console.WriteLine(Chr(7))
    End Sub

    Private Sub p_Welcome()
        eActiveUI = RFUiList.WELCOME
        Console.Clear()
        Console.ResetColor()
        'Console.SetWindowSize(DEFAULT_WIDTH_WINDOW + 2, DEFAULT_HEIGHT_WINDOW + 2)
        Console.Title = My.Application.Info.ProductName
        Console.ForegroundColor = ConsoleColor.White
        sTitle = String.Format("{0} - {1}", My.Application.Info.Title, APP_ENVIRONMENT)
        PrintLine(1, 1, sTitle)
        Console.BackgroundColor = ConsoleColor.Blue
        Console.ForegroundColor = ConsoleColor.White
        PrintLine(1, 3, "           D S V           ")
    End Sub

    Private Sub p_UserInfo()
        eActiveUI = RFUiList.USER_INFO
        Console.Clear()
        Console.ResetColor()
        PrintHeader(eActiveUI)
        PrintLine(1, 6, "USERNAME: " & dtUserActive.UserIdRoot)
        PrintLine(1, 7, "FULLNAME: " & dtUserActive.UserNameRoot)
        PrintLine(1, 8, "OPR ID  : " & dtUserActive.UserId)
        PrintLine(1, 9, "OPR NAME: " & dtUserActive.UserName)
        PrintLine(1, 11, "CONFIRM ?")
        PrintFooter()

        If GetInputEnterOrEscapeOnly(11, 11) Then
            p_MenuWhs()
        Else
            p_LoginOperator()
        End If
    End Sub

    Private Sub p_Login()
        dtUserActive = New DataUserActive
        eActiveUI = RFUiList.LOGIN
        PrintLine(1, 6, "USERNAME: ")
        PrintLine(1, 7, New String(" ", 25), , True)
        PrintLine(1, 8, "PASSWORD: ")
        PrintLine(1, 9, New String(" ", 25), , True)
        PrintFooter()

        Console.ForegroundColor = ConsoleColor.White
backUserId:
        Dim sUserId As String = GetInput(1, 7, 0)
        Dim sPassword As String = GetInputPassword(1, 9, 0)

        If sUserId.Trim = String.Empty Then GoTo backUserId

        p_loginValidation(sUserId, sPassword)
        If dtUserActive.UserId = String.Empty Then
            p_error(MESSAGE_INFO_INVALID_LOGIN)
            p_gotoUi(RFUiList.WELCOME)
        Else
            p_MenuWhs()
        End If
    End Sub

    Private Sub p_LoginOperator(Optional sMessage As String = "", Optional bError As Boolean = False)
        eActiveUI = RFUiList.LOGIN_OPERATOR
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)

        PrintHeader(eActiveUI)
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        dtUserActive.UserId = dtUserActive.UserIdRoot
        dtUserActive.UserName = dtUserActive.UserNameRoot

        PrintLine(1, 6, "USERNAME: " & dtUserActive.UserNameRoot)
        PrintLine(1, 8, "OPERATOR ID: [OPTIONAL]")
        PrintLine(1, 9, New String(" ", 16), , True)
        PrintFooter()

        Console.ForegroundColor = ConsoleColor.White
        Dim sUserId As String = GetInput(1, 9, 15)
        If sUserId.Trim = String.Empty Then
            p_UserInfo()
        Else
            p_loginOperatorValidation(sUserId)
            If dtUserActive.UserId = dtUserActive.UserIdRoot Then
                p_LoginOperator(MESSAGE_INFO_INVALID_LOGIN, True)
            Else
                p_UserInfo()
            End If
        End If

    End Sub

    Private Sub p_MenuWhs()
        eActiveUI = RFUiList.MENU_WHS
        p_wait("Get Whs List")
        Dim elapseStart As Date = Now
        Dim alWhs As ArrayList = New UserWhse().GetList(dtUserActive.UserId, dtConnInfo)

        sTitle = My.Application.Info.Title
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "WAREHOUSE:")
        If alWhs.Count = 0 Then p_error("NO WHSE FOUND")

        Dim iWhsPrint As Int16 = 0
        For Each data As UserWhse.DataUserWhse In alWhs
            Dim sWhsName As String = data.WhseName
            If Left(data.WhseId, 3) = "GEO" Then sWhsName += " [3.9]"

            iWhsPrint += 1
            Dim sWhsPrint As String = String.Format("{0}. {1}", (iWhsPrint).ToString, sWhsName)
            PrintLine(1, iWhsPrint + 3, sWhsPrint)
        Next

        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooterWithElapse(elapseStart)

invalidkey:
        Dim sChoosen As String = String.Empty
        If alWhs.Count = 1 Then
            sChoosen = 1
            GoTo singleWhs
        End If
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Or CInt(sChoosen) = 0 Then
            GoTo invalidkey
        ElseIf CInt(sChoosen) = 9 Then
            p_gotoUi(RFUiList.LOGIN)
        Else
            If CInt(sChoosen) > alWhs.Count Then
                PrintMessage("INVALID KEY", True)
                GoTo invalidkey
            Else
singleWhs:
                Dim iCount As Int16 = 0
                For Each data As UserWhse.DataUserWhse In alWhs
                    iCount += 1
                    If iCount = CInt(sChoosen) Then
                        dtUserActive.WhseId = data.WhseId
                        dtUserActive.WhseName = data.WhseName
                        dtUserActive.ConnectionStringFilename = data.CSFilename
                        Exit For
                    End If
                Next
            End If
        End If
        sTitle = String.Format("{0} {1}", My.Application.Info.Title, dtUserActive.WhseName)
        p_setDataConnectionInfoWms(dtConnInfo.UserId, dtConnInfo.Password, dtUserActive.WhseId)
        p_MenuStorer()
    End Sub

    Private Sub p_MenuStorer()
        If Not dtUserActive.WhseId = "GEOAPR30" Then p_MenuByWhse() 'sementara hardcoded; cuma ini yg butuh storerkey diawal 

        eActiveUI = RFUiList.MENU_STORER
        Console.Clear()
        Console.ResetColor()
        sTitle = String.Format("{0} {1}", My.Application.Info.Title, dtUserActive.WhseName)
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "STORERKEY:")
        PrintLine(1, 4, "1. PLBSAM001")
        PrintLine(1, 5, "2. PLBSAJ001")

        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()
invalidkey:
        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Or CInt(sChoosen) = 0 Then
            PrintMessage("INVALID KEY", True)
            GoTo invalidkey
        ElseIf CInt(sChoosen) = 9 Then
            p_gotoUi(RFUiList.LOGIN)
        Else
            If CInt(sChoosen) = 1 Then
                dtUserActive.StorerKey = "PLBSAM001"
            ElseIf CInt(sChoosen) = 2 Then
                dtUserActive.StorerKey = "PLBSAJ001"
            Else
                PrintMessage("INVALID KEY", True)
                GoTo invalidkey
            End If
            sTitle = String.Format("{0} {1} {2}", My.Application.Info.ProductName, dtUserActive.WhseName, dtUserActive.StorerKey)
            p_MenuByStorer()
        End If
    End Sub

    Private Sub p_changePin()
        p_printHeader(RFUiList.LOGIN)
        PrintLine(1, 6, "NEW PIN (6 DIGIT NUMBER ONLY): ")
        PrintLine(1, 7, New String(" ", 6), , True)
        PrintLine(1, 8, "CONFIRM PIN: ")
        PrintLine(1, 9, New String(" ", 6), , True)
        PrintFooter()

        Console.ForegroundColor = ConsoleColor.White
backNewPin:
        Dim sNewPin As String = GetInputNumber(1, 7, 6)
        If sNewPin.Length < 6 Then
            PrintMessage("MUST BE 6 DIGIT", True)
            GoTo backNewPin
        End If
backConfirmPin:
        Dim sConfirmPin As String = GetInputNumber(1, 9, 6)
        If sConfirmPin.Length < 6 Then
            PrintMessage("MUST BE 6 DIGIT", True)
            GoTo backConfirmPin
        End If

        If Not sNewPin = sConfirmPin Then
            PrintMessage("PIN NOT MATCH", True)
            GoTo backNewPin
        End If

        dtUserActive.Password = sConfirmPin
        p_updateUser()
        p_printMessage("PIN UPDATED")
        System.Threading.Thread.Sleep(1000)
        p_Login()
    End Sub

    Private Sub p_MenuSrgPlb39()
        eActiveUI = RFUiList.MENU_SRGPLB_39
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU:")
        PrintLine(1, 4, "1. PICKING")
        PrintLine(1, 5, "2. QR ENQUIRY")
        PrintLine(1, 6, "3. INBOUND")
        'PrintLine(1, 7, String.Empty)
        'PrintLine(1, 8, String.Empty)
        'PrintLine(1, 9, String.Empty)
        'PrintLine(1, 10, String.Empty)
        If Not dtUserActive.Password = String.Empty Then PrintLine(1, 11, "8. CHANGE PIN")
        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

invalidKey:
        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Or CInt(sChoosen) = 0 Then
            p_gotoPrevUi()
        Else
            Select Case CInt(sChoosen)
                Case 1
                    p_gotoUi(RFUiList.PICKING)
                Case 2
                    p_gotoUi(RFUiList.OTHERS)
                Case 3
                    p_gotoUi(RFUiList.RECEIVING)
                Case 8
                    p_gotoUi(RFUiList.CHANGE_PIN)
                Case 9
                    p_gotoUi(RFUiList.WELCOME)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    GoTo invalidKey
            End Select
        End If
    End Sub

    Private Sub p_MenuByWhse()
        Select Case dtUserActive.WhseName
            Case "SRG_PLB"
                If dtUserActive.WhseId = "GEOAPR30" Then p_MenuSrgPlb39()
                If dtUserActive.WhseId = "WMWHSE36" Then p_MenuSrgPlb10()
            Case "HLP"
                p_MenuStd()
                'Case "CGK_TNG_PLB"
                '    p_MenuStandard()

        End Select
    End Sub

    Private Sub p_MenuByStorer()
        Select Case dtUserActive.StorerKey
            Case "PLBSAM001"
                p_MenuSrgPlb39()
            Case "PLBSAJ001"
                p_MenuSrgPlb39()

        End Select

    End Sub

    Private Sub p_MenuStd()
        eActiveUI = RFUiList.MENU_STD
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU:")
        PrintLine(1, 4, "1. ORDER SERIAL#")
        'PrintLine(1, 5, String.Empty)
        'PrintLine(1, 6, String.Empty)
        'PrintLine(1, 7, String.Empty)
        'PrintLine(1, 8, String.Empty)
        'PrintLine(1, 9, String.Empty)
        'PrintLine(1, 10, String.Empty)
        If Not dtUserActive.Password = String.Empty Then PrintLine(1, 11, "8. CHANGE PIN")
        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

invalidKey:
        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            p_gotoPrevUi()
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    p_gotoUi(RFUiList.ORDER_SERIAL_NUMBER)
                Case 8
                    p_gotoUi(RFUiList.CHANGE_PIN)
                Case 9
                    p_gotoUi(RFUiList.WELCOME)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    GoTo invalidKey
            End Select
        End If
    End Sub

    Private Sub p_MenuStandard_GaJadiDipake()
        eActiveUI = RFUiList.MENU_SRGPLB_39
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU:")
        PrintLine(1, 4, "1. RECEIVE CHECK")
        PrintLine(1, 5, "2. PICK CHECK")
        'PrintLine(1, 6, String.Empty)
        'PrintLine(1, 7, String.Empty)
        'PrintLine(1, 8, String.Empty)
        'PrintLine(1, 9, String.Empty)
        'PrintLine(1, 10, String.Empty)
        If Not dtUserActive.Password = String.Empty Then PrintLine(1, 11, "8. CHANGE PIN")
        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

invalidKey:
        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            p_gotoPrevUi()
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    p_gotoUi(RFUiList.RECEIVING_00)
                Case 2
                    p_gotoUi(RFUiList.PICKING_00)
                Case 8
                    p_gotoUi(RFUiList.CHANGE_PIN)
                Case 9
                    p_gotoUi(RFUiList.WELCOME)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    GoTo invalidKey
            End Select
        End If
    End Sub

    Private Sub p_MenuSrgPlb10()
        eActiveUI = RFUiList.MENU_STD
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU:")
        PrintLine(1, 4, "1. SCAN PICKED")
        PrintLine(1, 5, "2. SCAN TO LOAD")
        PrintLine(1, 6, "3. TEST QR LABEL")
        PrintLine(1, 7, "4. DELETE SCAN")
        PrintLine(1, 8, "5. SCAN INVOICE LABEL")
        'PrintLine(1, 9, String.Empty)
        'PrintLine(1, 10, String.Empty)
        If Not dtUserActive.Password = String.Empty Then PrintLine(1, 11, "8. CHANGE PIN")
        PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            p_gotoPrevUi()
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    p_gotoUi(RFUiList.FG_PICK_CHECK)
                Case 2
                    p_gotoUi(RFUiList.FG_LOAD_CHECK)
                Case 3
                    p_gotoUi(RFUiList.QR_DECODE_PLT)
                Case 4
                    p_gotoUi(RFUiList.FG_DEL_SCAN)
                Case 5
                    p_gotoUi(RFUiList.FG_INV_LBL_CHECK)
                Case 8
                    p_gotoUi(RFUiList.CHANGE_PIN)
                Case 9
                    p_gotoUi(RFUiList.WELCOME)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    p_MenuSrgPlb10()
            End Select
        End If
    End Sub

    Private Sub p_MenuPicking()
        eActiveUI = RFUiList.PICKING
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU PICKING:")
        PrintLine(1, 4, "1. PICK TO DROP ID")
        PrintLine(1, 5, "2. PICKED ENQUIRY")
        PrintLine(1, 6, "3. AUDIT BY ORDER")
        PrintLine(1, 7, "4. CHECK BY PTF")
        PrintLine(1, 4, "5. QRi VS QRo")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            p_gotoPrevUi()
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                'Case 1
                '    p_gotoUi(RFUiList.PICK_INPUT)
                'Case 2
                '    p_gotoUi(RFUiList.PICK_ENQUIRY)
                'Case 3
                '    p_gotoUi(RFUiList.PICK_AUDIT)
                'Case 4
                '    p_gotoUi(RFUiList.PICK_CHECK)
                Case 5
                    p_gotoUi(RFUiList.PICK_CHECK_QQ)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    p_MenuByStorer()
            End Select
        End If
    End Sub

    Private Sub p_MenuOthers()
        eActiveUI = RFUiList.OTHERS
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU OTHERS:")
        PrintLine(1, 4, "1. QR CAPTURE")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Then
            p_gotoPrevUi()
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    p_gotoUi(RFUiList.QR_CAPTURE)
                Case Else
                    PrintMessage("INVALID KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    p_MenuByStorer()
            End Select
        End If
    End Sub

    Private Function p_saveConfirmation(ByRef bEscFlag As Boolean) As Boolean
        Return p_confimation("======CONTINUE SAVE?======", bEscFlag)
    End Function

    Private Function p_deleteConfirmation(ByRef bEscFlag As Boolean) As Boolean
        Return p_confimation("=====CONTINUE DELETE?=====", bEscFlag)
    End Function

    Private Sub p_waitAnyKey(sMessage As String)
        Console.BackgroundColor = ConsoleColor.Yellow
        Console.ForegroundColor = ConsoleColor.Black
        PrintLine(1, 17, sMessage)
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        Dim sReturn As String = p_getInput(21, 17, 1, False, False, False)
    End Sub

    Private Sub p_waitAnyKey(sMessage As String, iTop As Int16)
        Console.BackgroundColor = ConsoleColor.Green
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
        Console.BackgroundColor = ConsoleColor.Green
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, iTop, String.Format("{0}. {1}", sMessage, "PRESS ANYKEY"))
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        Dim sReturn As String = p_getInput(15, iTop, 1, False, False, False)
    End Sub

    Private Function p_confimation(sMessage As String, ByRef bEscFlag As Boolean) As Boolean
        Console.BackgroundColor = ConsoleColor.Yellow
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, 17, New String(" ", DEFAULT_WIDTH_WINDOW))
        Console.BackgroundColor = ConsoleColor.Yellow
        Console.ForegroundColor = ConsoleColor.Black
        p_printLine(1, 17, sMessage)
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        If p_getInputEnterOrEscapeOnly(21, 17) Then
            Return True
        Else
            bEscFlag = True
            Return False
        End If

        Return True
    End Function

    Private Function p_getInputEscapeOnly(iLeft As Integer, iTop As Integer) As Boolean
        Console.SetCursorPosition(iLeft, iTop)
        Dim bEnter As Boolean = True
        Do While bEnter
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Escape
                    Return True
            End Select
        Loop
        Return False
    End Function

    Private Function p_getInputEnterOrEscapeOnly(iLeft As Integer, iTop As Integer) As Boolean
        Console.SetCursorPosition(iLeft, iTop)
        Dim bEnter As Boolean = True
        Do While bEnter
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Enter
                    Return True
                Case ConsoleKey.Escape
                    Return False
            End Select
        Loop
        Return False
    End Function

    Private Function p_getInput(iLeft As Integer, iTop As Integer, iLength As Integer, bPasswordMask As Boolean, bNumberOnly As Boolean, bUpperCase As Boolean) As String
        Dim sInput As String = String.Empty
        Dim bEnter As Boolean = False
        Dim bRead As Boolean = True
        Dim iCurLeft As Integer = iLeft

        Console.SetCursorPosition(iLeft, iTop)

        Do While Not bEnter
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)

            Select Case vKey.Key
                Case ConsoleKey.Enter
                    Return sInput
                Case ConsoleKey.Escape
                    p_gotoPrevUi(eActiveUI)
                    Return String.Empty
                Case ConsoleKey.Backspace
                    p_backwardStringInput(sInput, iCurLeft)
                Case ConsoleKey.Tab
                    'do nothing
                Case Else
                    If Not vKey.KeyChar = vbNullChar Then
                        If bNumberOnly Then bRead = IsNumeric(vKey.KeyChar)

                        If bRead Then
                            p_keepStringInput(sInput, iCurLeft, vKey)
                            If bUpperCase Then sInput = sInput.ToUpper
                        End If
                    End If
            End Select

            p_printLine(iLeft, iTop, New String(" ", iLength),, True)

            If bPasswordMask Then
                p_printLine(iLeft, iTop, New String("*", sInput.Length),, True)
            Else
                p_printLine(iLeft, iTop, sInput,, True)
            End If
            Console.SetCursorPosition(iCurLeft, iTop)

            If iLength > 0 And sInput.Length = iLength Then Return sInput
        Loop

        Return sInput
    End Function

    Private Function p_getScan(iLeft As Integer, iTop As Integer) As String
        'unknown number of enter
        Dim sInput As String = String.Empty
        Dim bExit As Boolean = False
        Dim bEsc As Boolean = False
        Dim iCurLeft As Integer = iLeft
        Console.SetCursorPosition(iLeft, iTop)

        Do While Not bExit
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Escape
                    bEsc = True
                    Exit Do
                Case ConsoleKey.DownArrow
                    Exit Do
                Case ConsoleKey.Backspace
                    p_backwardStringInput(sInput, iCurLeft)
                Case Else
                    If Not vKey.KeyChar = vbNullChar Then p_keepStringInput(sInput, iCurLeft, vKey)
            End Select

            p_printLine(iLeft, iTop, "[PRESS DOWN ARROW]")
        Loop

        If bEsc Then p_gotoPrevUi(eActiveUI)
        p_printLine(iLeft, iTop, New String(" ", DEFAULT_WIDTH_WINDOW - 10))
        p_printLine(iLeft, iTop, "SCANNED")

        If sInput.Substring(sInput.Length - 1, 1) = vbCr Then sInput = sInput.Substring(0, sInput.Length - 1)

        Return sInput
    End Function

    Private Function p_getScan(iLeft As Integer, iTop As Integer, bPrint As Boolean, iFixEnter As Int16) As String
        Dim sInput As String = String.Empty
        Dim bExit As Boolean = False
        Dim bEsc As Boolean = False
        Dim iEnterCount As Int16 = 0
        Dim xLen As Int16 = 0
        Dim iCurLeft As Integer = iLeft
        Console.SetCursorPosition(iLeft, iTop)

        Do While Not bExit
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Enter
                    p_keepStringInput(sInput, iCurLeft, vKey)
                    xLen += 1
                    If xLen = iFixEnter Then
                        If sInput.Substring(sInput.Length - 1, 1) = vbCr Then sInput = sInput.Substring(0, sInput.Length - 1)
                        Exit Do
                    End If
                Case ConsoleKey.Escape
                    bEsc = True
                    Exit Do
                Case ConsoleKey.Backspace
                    p_backwardStringInput(sInput, iCurLeft)
                Case Else
                    If Not vKey.KeyChar = vbNullChar Then p_keepStringInput(sInput, iCurLeft, vKey)
            End Select

            If bPrint Then
                p_printLine(iLeft, iTop, New String(" ", sInput.Length + 1),, True)
                p_printLine(iLeft, iTop, sInput,, True)
                Console.SetCursorPosition(iCurLeft, iTop)
            Else
                p_printLine(iLeft, iTop, "[ENTER]")
            End If
        Loop

        If bEsc Then p_gotoPrevUi(eActiveUI)
        If bPrint Then
            p_printLine(iLeft, iTop, New String(" ", sInput.Length + 1),, False)
        End If
        p_printLine(iLeft, iTop, "SCANNED")
        Return sInput
    End Function

    Private Function p_getScan(iLeft As Integer, iTop As Integer, bPrint As Boolean) As String
        Dim sInput As String = String.Empty
        Dim bExit As Boolean = False
        Dim bEsc As Boolean = False
        Dim iEnterCount As Int16 = 0
        Dim xLen As Int16 = 0
        Dim iCurLeft As Integer = iLeft
        Console.SetCursorPosition(iLeft, iTop)

        Do While Not bExit
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Enter
                    p_keepStringInput(sInput, iCurLeft, vKey)
                    If sInput.Length > 1 Then
                        If xLen = sInput.Length - 1 Then
                            sInput = sInput.Substring(0, sInput.Length - 1)
                            If sInput.Substring(sInput.Length - 1, 1) = vbCr Then sInput = sInput.Substring(0, sInput.Length - 1)
                            Exit Do
                        End If
                    End If
                    xLen = sInput.Length
                Case ConsoleKey.Escape
                    bEsc = True
                    Exit Do
                Case ConsoleKey.Backspace
                    p_backwardStringInput(sInput, iCurLeft)
                Case Else
                    If Not vKey.KeyChar = vbNullChar Then p_keepStringInput(sInput, iCurLeft, vKey)
            End Select

            If bPrint Then
                p_printLine(iLeft, iTop, New String(" ", sInput.Length + 1),, True)
                p_printLine(iLeft, iTop, sInput,, True)
                Console.SetCursorPosition(iCurLeft, iTop)
            End If
        Loop

        If bEsc Then p_gotoPrevUi(eActiveUI)
        If bPrint Then
            p_printLine(iLeft, iTop, New String(" ", sInput.Length + 1),, False)
        End If
        p_printLine(iLeft, iTop, "SCANNED")
        Return sInput
    End Function

    Private Function p_getScanWDoubleEnter(iLeft As Integer, iTop As Integer, Optional bWithDoubleEnter As Boolean = False) As String
        Dim sInput As String = String.Empty
        Dim bEnter As Boolean = False
        Dim iCurLeft As Integer = iLeft
        Dim vLastKey As ConsoleKey
        Dim iEnter As Int16 = 0
        Console.SetCursorPosition(iLeft, iTop)

        Do While Not bEnter
            Dim vKey As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case vKey.Key
                Case ConsoleKey.Enter
                    p_keepStringInput(sInput, iCurLeft, vKey)
                    If bWithDoubleEnter Then
                        If vLastKey = ConsoleKey.Enter Then Return sInput
                    Else
                        Return sInput
                    End If
                Case ConsoleKey.Escape
                    p_gotoPrevUi(eActiveUI)
                    Return String.Empty
                Case ConsoleKey.Backspace
                    p_backwardStringInput(sInput, iCurLeft)
                Case ConsoleKey.Tab
                    'do nothing
                Case Else
                    If Not vKey.KeyChar = vbNullChar Then p_keepStringInput(sInput, iCurLeft, vKey)
            End Select

            p_printLine(iLeft, iTop, New String(" ", sInput.Length + 1),, True)
            p_printLine(iLeft, iTop, sInput,, True)
            Console.SetCursorPosition(iCurLeft, iTop)
            vLastKey = vKey.Key
        Loop

        Return sInput
    End Function

    Private Sub p_backwardStringInput(ByRef sInput As String, ByRef iCurLeft As Integer)
        If sInput.Length > 1 Then
            sInput = sInput.Substring(0, sInput.Length - 1)
            iCurLeft -= 1
        Else
            iCurLeft = 1
            sInput = String.Empty
        End If
    End Sub

    Private Sub p_keepStringInput(ByRef sInput As String, ByRef iCurLeft As Integer, vKey As ConsoleKeyInfo)
        sInput += vKey.KeyChar
        iCurLeft += 1
    End Sub

    Private Sub p_loginOperatorValidation(sUser As String)
        UIWait()
        Dim dtUser As New UserManager.DataUser

        Try
            Dim UM As New UserManager
            dtUser = UM.GetData(sUser, dtConnInfo)

            Dim dtLog As New Log.DataLog
            dtLog.MenuID = eActiveUI.ToString()
            dtLog.Event = "Login"
            If dtUser.UserId = String.Empty Then
                dtLog.Status = Log.LogStatus.FAIL.ToString()
                dtLog.Scan1 = dtUserActive.UserIdRoot
                dtLog.UserID = sUser
            Else
                dtLog.Status = Log.LogStatus.SUCCESS.ToString()
                dtLog.Scan1 = dtUserActive.UserIdRoot
                dtLog.UserID = dtUser.UserId
            End If
            p_saveLog(dtLog)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_LoginOperator()
        End Try

        If Not dtUser.bEnabled Then p_LoginOperator(MESSAGE_INFO_INVALID_LOGIN, True)

        If dtUser.UserId.Length > 0 Then
            dtUserActive.UserId = dtUser.UserId
            dtUserActive.UserName = dtUser.UserName
            dtUserActive.bEnabled = dtUser.bEnabled
            dtUserActive.RoleId = dtUser.RoleId

            PrintMessage("WELCOME " + dtUser.UserName)
            System.Threading.Thread.Sleep(1000)
        End If
    End Sub

    Private Sub p_setDataConnectionInfo(sUser As String, sPassword As String)

        If APP_ENVIRONMENT = "TST" Then dtConnInfo.DataSource = DATASOURCE_TST
        If APP_ENVIRONMENT = "QA" Then dtConnInfo.DataSource = DATASOURCE_QA
        If APP_ENVIRONMENT = "PRD" Then dtConnInfo.DataSource = DATASOURCE_PRD

        dtConnInfo.InitialCatalog = INITIAL_CATALOG
        dtConnInfo.UserId = sUser
        dtConnInfo.Password = sPassword
    End Sub

    Private Sub p_setDataConnectionInfoWms(sUser As String, sPassword As String, sWhse As String)

        If APP_ENVIRONMENT = "TST" Then dtConnInfoWms.DataSource = DATASOURCE_TST_WMS
        If APP_ENVIRONMENT = "QA" Then dtConnInfoWms.DataSource = DATASOURCE_QA_WMS
        If APP_ENVIRONMENT = "PRD" Then dtConnInfoWms.DataSource = DATASOURCE_PRD_WMS

        dtConnInfoWms.InitialCatalog = INITIAL_CATALOG_WMS
        dtConnInfoWms.UserId = sWhse
        dtConnInfoWms.Password = "Pr0d@wm10x"
    End Sub

    Private Sub p_loginValidation(sUser As String, sPassword As String)
        UIWait()

        p_setDataConnectionInfo(sUser, sPassword)

        Dim dtUser As New UserManager.DataUser

        Try
            Dim UM As New UserManager
            dtUser = UM.GetValidDataWithLdapExam(sUser, sPassword, Environment.UserDomainName, dtConnInfo)
            'dtUser = UM.GetValidDataWithLdapExam(sUser, sPassword, DEFAULT_DOMAIN, String.Empty)
            'dtUser = UM.GetValidDataLogin(sUser, sPassword, String.Empty)

            Dim dtLog As New Log.DataLog
            dtLog.MenuID = eActiveUI.ToString()
            dtLog.Event = "Login AD"
            'dtLog.Event = "Login"
            If dtUser.UserId = String.Empty Then
                dtLog.Status = Log.LogStatus.FAIL.ToString()
                dtLog.UserID = sUser
            Else
                dtLog.Status = Log.LogStatus.SUCCESS.ToString()
                dtLog.UserID = dtUser.UserId
            End If
            p_saveLog(dtLog)
        Catch ex As Exception
            If ex.Message = "The user name or password is incorrect." & vbCrLf Then
                UIError(ex.Message)
            Else
                UIError(ex.Message & vbCrLf & ex.StackTrace)
            End If

            p_Login()
        End Try

        If dtUser.UserId.Length > 0 Then
            dtUserActive.UserId = dtUser.UserId
            dtUserActive.Password = String.Empty
            dtUserActive.UserIdRoot = dtUser.UserId
            dtUserActive.UserNameRoot = dtUser.UserName
            dtUserActive.UserName = dtUser.UserName
            dtUserActive.Email = dtUser.Email
            dtUserActive.StorerKey = dtUser.StorerKey
            dtUserActive.bChangePassword = dtUser.bChangePassword
            dtUserActive.bEnabled = dtUser.bEnabled
            dtUserActive.RoleId = dtUser.RoleId

            PrintMessage("WELCOME " + dtUser.UserName)
            System.Threading.Thread.Sleep(1000)
        End If
    End Sub

    Private Function p_getScanQRPalletOutbound(iLeft As Integer, iTop As Integer, iFixEnter As Integer) As DataScan
        Dim dtReturn As New DataScan
        Dim sScan As String = String.Empty

        'qrcode = C011.SJ19080106C[?]ORDER
        sScan = GetScanFixEnterPrint(iLeft, iTop, False, iFixEnter)

        'If sScan.Substring(sScan.Length - 1) = vbCr Then sScan = sScan.Substring(0, sScan.Length - 1)
        Dim sQR() As String = Split(sScan, "ORDER")

        'Dim sQR() As String = Split(sScan, vbCr)
        If sQR.Count >= 1 Then dtReturn.Component0 = sQR(0)
        dtReturn.Component1 = "ORDER"
        'If sQR.Count >= 2 Then dtReturn.Component1 = sQR(1)
        'If sQR.Count >= 3 Then dtReturn.Component2 = sQR(2)
        'If sQR.Count >= 4 Then dtReturn.Component3 = sQR(3)
        'If sQR.Count >= 5 Then dtReturn.Component4 = sQR(4)
        'If sQR.Count >= 6 Then dtReturn.Component5 = sQR(5)
        'If sQR.Count >= 7 Then dtReturn.Component6 = sQR(6)
        'If sQR.Count >= 8 Then dtReturn.Component7 = sQR(7)
        'If sQR.Count >= 9 Then dtReturn.Component8 = sQR(8)
        'If sQR.Count >= 10 Then dtReturn.Component9 = sQR(9)

        Return dtReturn
    End Function

    Private Sub p_updateUser()
        UIWait()

        dtUserActive.bChangePassword = 0
        Try
            Dim sNewPin As String = dtUserActive.Password
            Dim UM As New UserManager
            Dim dtUser As New UserManager.DataUser
            dtUser = UM.GetData(dtUserActive.UserId, dtConnInfo)
            Dim sOldPin As String = dtUser.Password

            dtUser.Password = dtUserActive.Password
            dtUser.bChangePassword = dtUserActive.bChangePassword
            UM.Save(dtUser, dtConnInfo)

            Dim dtLog As New Log.DataLog
            dtLog.MenuID = eActiveUI.ToString()
            dtLog.Event = "Change PIN"
            dtLog.Scan1 = sOldPin
            dtLog.Scan2 = sNewPin
            dtLog.Status = Log.LogStatus.SUCCESS.ToString()
            dtLog.UserID = dtUserActive.UserId
            dtLog.StorerKey = dtUserActive.StorerKey
            p_saveLog(dtLog)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_saveLog(dtLog As Log.DataLog)
        Dim LM As New Log
        Dim worker As New Thread(New ThreadStart(Sub()
                                                     LM.Save(dtLog, dtConnInfo)
                                                 End Sub))
        LM.Save(dtLog, dtConnInfo)
    End Sub

    Private Sub p_getVersion()
        Dim ver As Version
        ver = Assembly.GetExecutingAssembly().GetName().Version
        VERSION = String.Format("{0} {1}.{2}.{3}", My.Application.Info.ProductName, ver.Build.ToString, ver.Major.ToString, ver.Minor.ToString)
    End Sub

    Private Function p_getDataQRInbound(iLeft As Integer, iTop As Integer, eType As TypeQrInbound) As DataQRInbound

        Dim strQR As String = p_getScan(iLeft, iTop, False, 5)
        Dim sQR() As String = Split(strQR, vbCr)

        Dim data As New DataQRInbound

        If eType = TypeQrInbound.GENERAL Then 'qrcode = receiptkey + storerkey + sku + qty + unique id = format QR Inbound utk Pallet Checking dijadikan general
            If sQR.Count = 5 Then
                data.ReceiptKey = sQR(0).Trim()
                data.StorerKey = sQR(1).Trim()
                data.Sku = sQR(2).Trim()
                data.Qty = sQR(3).Trim()
                data.ID = sQR(4).Trim()
            End If
            'ElseIf eType = TypeQrInbound.PALLET_CHECKING Then '??? format 

        End If

        Return data
    End Function

#End Region

#Region "DATACLASS"

    Public Class DataUserActive
        Public UserId As String = String.Empty
        Public UserName As String = String.Empty
        Public Password As String = String.Empty
        Public Email As String = String.Empty
        Public StorerKey As String = String.Empty
        Public bChangePassword As Boolean = False
        Public bEnabled As Boolean = False
        Public RoleId As String = String.Empty
        Public RoleName As String = String.Empty
        Public WhseId As String = String.Empty
        Public WhseName As String = String.Empty
        Public ConnectionStringFilename As String = String.Empty
        Public UserIdRoot As String = String.Empty
        Public UserNameRoot As String = String.Empty
    End Class

    Public Class DataScan
        Public Component0 As String = String.Empty
        Public Component1 As String = String.Empty
        Public Component2 As String = String.Empty
        Public Component3 As String = String.Empty
        Public Component4 As String = String.Empty
        Public Component5 As String = String.Empty
        Public Component6 As String = String.Empty
        Public Component7 As String = String.Empty
        Public Component8 As String = String.Empty
        Public Component9 As String = String.Empty
    End Class

    Public Class DataQRInbound
        Public ID As String = String.Empty
        Public ReceiptKey As String = String.Empty
        Public ReceiptLineNumber As String = String.Empty
        Public StorerKey As String = String.Empty
        Public Sku As String = String.Empty
        Public Qty As Int32 = 0
    End Class
#End Region

End Module
