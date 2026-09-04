Imports AIT.RF
Imports AIT.GEN

Module ReceivingUI
#Region "Public"
    Dim dtScreen As New DataScreen

    Public Sub Main()
        p_Menu()
    End Sub

    Public Sub ReceiptCheck()
        p_uiReceiptCheck()
    End Sub
#End Region

#Region "Private"
    Private Sub p_Menu()
        eActiveUI = RFUiList.RECEIVING
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "MENU RECEIVING:")
        PrintLine(1, 4, "1. SUPPLIER VS QRi")
        'PrintLine(1, 5, "2. ")
        'PrintLine(1, 6, "3. ")
        'PrintLine(1, 7, "4. ")
        'PrintLine(1, 8, "5. ")
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
                    GotoUi(RFUiList.RECV_CHECK)
                Case Else
                    PrintMessage("INVLD KEY", True)
                    System.Threading.Thread.Sleep(1000)
                    p_Menu()
            End Select
        End If
    End Sub

    Private Sub p_uiReceiptCheck(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintLine(1, 4, "SCAN QR Inbound")
            PrintLine(1, 5, "SKU")
            'PrintLine(1, 5, "SKU                    QTY")
            PrintLine(1, 6, dtScreen.Sku1)
            'PrintLine(24, 6, dtScreen.Qty1.ToString())
            PrintLine(1, 7, "---------------vs-------------")
            PrintLine(1, 8, dtScreen.Sku2)
            'PrintLine(24, 8, dtScreen.Qty2.ToString())
            PrintFooter()

            dtScreen = New DataScreen
            p_scanQRInbound(20, 4)
            PrintLine(1, 8, New String(" ", 50), True)
            ClearMessage()
            If dtScreen.StorerKey <> dtUserActive.StorerKey Then
                p_uiReceiptCheck(MESSAGE_INFO_INVALID_STORER, True)
            End If

            If dtScreen.Sku1 = String.Empty Then
                p_uiReceiptCheck(MESSAGE_INFO_INVALID_SKU, True)
            End If

            If p_isIDInboundExists() Then
                UIWarning(MESSAGE_INFO_QR_IS_USED, False)
                p_uiReceiptCheck(MESSAGE_INFO_QR_IS_USED, True)
            End If

            PrintLine(1, 6, dtScreen.Sku1)
            'PrintLine(24, 6, dtScreen.Qty1.ToString())

            PrintLine(1, 4, New String(" ", DEFAULT_WIDTH_WINDOW), True)
            dtScreen.Sku2 = String.Empty
            'dtScreen.Qty2 = 0

            PrintLine(1, 4, "SCAN QR PRODUCT")
            p_scanQrProduct(20, 4)
            PrintLine(1, 8, dtScreen.Sku2)
            'PrintLine(24, 8, dtScreen.Qty2.ToString())

            If dtScreen.Sku2 = String.Empty Then
                p_uiReceiptCheck("INVLD QR", True)
            End If

            If dtScreen.Sku1.ToLower.Trim.Replace("-", "").Replace(" ", "") = dtScreen.Sku2.ToLower.Trim.Replace("-", "").Replace(" ", "") Then
                p_saveLog(String.Format("SKU {0}", dtScreen.Sku1), String.Format("SKU {0}", dtScreen.Sku2), String.Empty, Log.LogStatus.MATCH.ToString())
                p_uiReceiptCheck("SKU & QTY MATCH. GOOD JOB!")
            Else
                p_saveLog(String.Format("SKU {0}", dtScreen.Sku1), String.Format("SKU {0}", dtScreen.Sku2), String.Empty, Log.LogStatus.NOT_MATCH.ToString())
                UIWarning("  N O T   M A T C H ")
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_scanQRInbound(iLeft As Integer, iTop As Integer)
        Dim dataQRi As DataQRInbound = GetDataQRInbound(iLeft, iTop)
        dtScreen.StorerKey = dataQRi.StorerKey
        dtScreen.Sku1 = dataQRi.Sku
        dtScreen.Qty1 = dataQRi.Qty
        dtScreen.ID = dataQRi.ID
    End Sub

    Private Function p_isIDInboundExists() As Boolean
        Dim dtLogForValidation As New Log.DataLog
        dtLogForValidation.LabelID = dtScreen.ID
        dtLogForValidation.MenuID = eActiveUI.ToString()

        Return New Log().IsLabelIDExists(dtLogForValidation, dtConnInfo)
    End Function

    Private Sub p_scanQrProduct(iLeft As Int16, iTop As Int16)
        Dim sInput As String = String.Empty
        Console.SetCursorPosition(iLeft, iTop)
        sInput = GetScanUnknown(iLeft, iTop)
        If sInput.Length > 0 Then
            dtScreen.Sku2 = GetSkuFromQRProduct(sInput)

            'expand decode qr
            Dim dtSku As New Sku.DataSku
            dtSku = New Sku().GetSkuFromQRSupplier(dtScreen.StorerKey, sInput, dtConnInfo)
            If dtSku.Sku <> String.Empty Then dtScreen.Sku2 = dtSku.Sku

            If dtScreen.Sku2 = String.Empty Then
                dtScreen = New DataScreen
                p_uiReceiptCheck("INVLD QR", True)
            Else
                If dtSku.Sku = String.Empty Then dtSku = GetSkuByPrediction(dtScreen.StorerKey, dtScreen.Sku2)
                If dtSku.Sku = String.Empty Then
                    p_uiReceiptCheck("SKU NOT FOUND", True)
                Else
                    dtScreen.Sku2 = dtSku.Sku.Replace("-", "")
                    If dtSku.Sku <> dtScreen.Sku1 Then
                        UIWarning("S K U   N O T   M A T C H ")
                    End If
                End If
            End If
        Else
            dtScreen = New DataScreen
            p_uiReceiptCheck("INVLD QR", True)
        End If
    End Sub

    Private Sub p_saveLog(scan1 As String, scan2 As String, scan3 As String, status As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = eActiveUI.ToString()
        dtLog.Scan1 = scan1
        dtLog.Scan2 = scan2
        dtLog.Scan3 = scan3
        dtLog.Status = status
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtUserActive.StorerKey
        SaveLog(dtLog)
    End Sub

#End Region

#Region "Data Class"
    Public Class DataScreen
        Public StorerKey As String = dtUserActive.StorerKey
        Public QrCode As String = String.Empty
        Public ReceiptId As String = String.Empty
        Public Sku1 As String = String.Empty
        Public Sku2 As String = String.Empty
        Public Qty1 As Int16 = 0
        Public Qty2 As Int16 = 0
        Public Match As Boolean
        Public ID As String = String.Empty
    End Class
#End Region
End Module
