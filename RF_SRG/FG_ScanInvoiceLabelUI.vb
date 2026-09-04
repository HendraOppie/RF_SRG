Imports AIT.RF
Imports AIT.WMS

Module FG_ScanInvoiceLabelUI
#Region "Public"
    Dim dtScreen As New DataScreen
    Dim elapseStart As Date
    Dim ScreenMode As ScreenModeList

    Public Sub Main()
        p_New()
    End Sub

    Public Sub GotoPrevScreen()
        p_gotoPrevScreen()
    End Sub
#End Region
#Region "Private"
    Private Enum ScreenModeList
        BEGIN
        INVOICE
        PALLET
        SHOW
        COMPLETE
    End Enum

    Private Sub p_New()
        p_Menu()
    End Sub

    Private Sub p_Menu()
        eActiveUI = RFUiList.FG_INV_LBL_CHECK
        ScreenMode = ScreenModeList.BEGIN
        dtScreen = New DataScreen
        elapseStart = Now
        p_Main()
    End Sub

    Private Sub p_Main(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

            PrintLine(1, 4, "INVOICE:")
            PrintLine(1, 5, New String(" ", 25), , True)
            PrintLine(1, 5, dtScreen.ExternOrderKey, True, True)
            PrintLine(1, 6, "PALLET QR:")
            PrintLine(1, 7, New String(" ", 25), , True)
            PrintLine(1, 7, dtScreen.PalletId.Replace(" ", ""), True, True)

            p_printFooterWithElapse()
            If ScreenMode = ScreenModeList.BEGIN Then p_scanInvoice(1, 5)
            If ScreenMode = ScreenModeList.INVOICE Then p_scanPalletId(1, 7)
            If ScreenMode = ScreenModeList.PALLET Then p_validate(1, 9)

            If ScreenMode = ScreenModeList.SHOW Then
                If GetInputEnterOrEscapeOnly(21, 17) Then
                    ClearMessage()
                    p_Menu()
                Else
                    ClearMessage()
                    p_Menu()
                End If
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try
    End Sub

    Private Sub p_gotoPrevScreen()
        GotoUi(RFUiList.MENU_SRGPLB_10)
    End Sub

    Private Sub p_printFooterWithElapse()
        PrintFooterWithElapse(p_setElapseTime())
        elapseStart = Now
    End Sub

    Private Sub p_scanInvoice(iLeft As Integer, iTop As Integer)
        dtScreen.ExternOrderKey = GetScanFixEnterPrint(iLeft, iTop, True, 1)
        elapseStart = Now

        dtScreen.alPalletID = New PickDetail().GetListIdByInvoice(dtScreen.ExternOrderKey, dtConnInfoWms)
        If dtScreen.alPalletID.Count = 0 Then
            dtScreen.ExternOrderKey = String.Empty
            p_Main("INVOICE NOT FOUND", True)
            ScreenMode = ScreenModeList.BEGIN
        Else
            ScreenMode = ScreenModeList.INVOICE
        End If

        For Each data As PickDetail.DataPickDetail In dtScreen.alPalletID
            dtScreen.StorerKey = data.StorerKey
            Exit For
        Next

        p_Main()
    End Sub

    Private Sub p_scanPalletId(iLeft As Integer, iTop As Integer)
        dtScreen.PalletId = String.Empty
        dtScreen.QRCode = GetScanFixEnterPrint(iLeft, iTop, False, 1)
        ClearMessage()
        elapseStart = Now()
        If dtScreen.QRCode = String.Empty Then p_Main("EMPTY NOT ALLOW", True)

        p_decodeQRPallet()
        p_decodePalletId()
        ScreenMode = ScreenModeList.PALLET
        p_Main()
    End Sub

    Private Sub p_decodeQRPallet()
        Dim sContent() As String = dtScreen.QRCode.Split(",")
        If sContent.Count = 1 Then
            dtScreen.PalletId = dtScreen.QRCode
        Else
            Dim alContent As New ArrayList
            Dim alContentQrPallet As ArrayList = New FG_SplitQrPallet().GetList(dtScreen.QRCode, dtConnInfo)
            For Each data As FG_SplitQrPallet.DataSplitQrPallet In alContentQrPallet
                If data.ContentType = "PALLETID" Then
                    dtScreen.PalletId = data.Content
                    Exit For
                End If
            Next

            If dtScreen.PalletId = String.Empty Then
                p_saveLog("DECODE QR", Log.LogStatus.FAIL.ToString())
                p_Main("DECODE QR FAAILED", True)
            End If
        End If
    End Sub

    Private Sub p_decodePalletId()
        If InStr(dtScreen.PalletId, "TS.") Or InStr(dtScreen.PalletId, "JS.") Then dtScreen.PalletId = dtScreen.PalletId.Substring(3, dtScreen.PalletId.Length - 3)
        If InStr(dtScreen.PalletId, "BS.") Or InStr(dtScreen.PalletId, "DS.") Or InStr(dtScreen.PalletId, "BS*") Or InStr(dtScreen.PalletId, "DS*") Then dtScreen.PalletId = dtScreen.PalletId.Substring(3, dtScreen.PalletId.Length - 3)
        If InStr(dtScreen.PalletId, "S.") Then dtScreen.PalletId = dtScreen.PalletId.Substring(2, dtScreen.PalletId.Length - 2)
        dtScreen.PalletId = dtScreen.PalletId.Replace(" ", String.Empty)

        Dim palletComp() As String = dtScreen.PalletId.Split("-")
        If palletComp.Length > 1 Then dtScreen.PalletId = palletComp(0) & "-" & palletComp(1)
        If palletComp.Length = 3 Then dtScreen.PalletId = palletComp(2)
    End Sub

    Private Sub p_saveLog(sEvent As String, logStatus As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = sEvent
        dtLog.Scan1 = dtScreen.ExternOrderKey
        dtLog.Scan2 = dtScreen.PalletId
        dtLog.Scan3 = dtScreen.QRCode
        dtLog.Status = logStatus
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtScreen.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_validate(iLeft As Integer, iTop As Integer)
        Dim bFound As Boolean = False
        For Each data As PickDetail.DataPickDetail In dtScreen.alPalletID
            If InStr(data.ID.Replace(" ", ""), dtScreen.PalletId.Replace(" ", "")) Then
                bFound = True
                Exit For
            End If
        Next

        ScreenMode = ScreenModeList.SHOW
        If bFound Then
            p_saveLog(eActiveUI.ToString(), Log.LogStatus.MATCH.ToString())
            p_Main("INVOICE " & STATUS_MATCH)
        Else
            p_saveLog(eActiveUI.ToString(), Log.LogStatus.NOT_MATCH.ToString())
            p_Main("INVOICE " & STATUS_NOT_MATCH, True)
        End If
    End Sub

    Private Function p_setElapseTime() As Double
        Dim elapse As TimeSpan = Now() - elapseStart
        Return elapse.TotalMilliseconds
    End Function

#End Region
#Region "Data Class"
    Private Const STATUS_MATCH = "MATCH"
    Private Const STATUS_NOT_MATCH = "NOT MATCH"

    Public Class DataScreen
        Public ExternOrderKey As String = String.Empty
        Public StorerKey As String = String.Empty
        Public PalletId As String = String.Empty
        Public QRCode As String = String.Empty
        Public alPalletID As ArrayList = New ArrayList
    End Class
#End Region
End Module
