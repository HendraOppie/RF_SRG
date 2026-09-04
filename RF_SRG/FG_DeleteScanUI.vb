Imports AIT.RF

Module FG_DeleteScanUI
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
        eActiveUI = RFUiList.FG_DEL_SCAN
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

            PrintLine(1, 4, "BY INVOICE:")
            PrintLine(1, 5, New String(" ", 25), , True)
            PrintLine(1, 5, dtScreen.ExternOrderKey, True, True)
            PrintLine(1, 6, "OR BY PALLET ID:")
            PrintLine(1, 7, New String(" ", 25), , True)
            PrintLine(1, 7, dtScreen.PalletID.Replace(" ", ""), True, True)
            PrintLine(1, 8, "---------status---------")

            p_printFooterWithElapse()
            If ScreenMode = ScreenModeList.BEGIN Then p_scanInvoice(1, 5)
            If ScreenMode = ScreenModeList.INVOICE Then p_scanPalletId(1, 7)
            If ScreenMode = ScreenModeList.PALLET Then p_validate(1, 9)

            If ScreenMode = ScreenModeList.SHOW Then
                PrintMessage("CONTINUE TO DELETE ?", False)
                If GetInputEnterOrEscapeOnly(21, 17) Then
                    p_delete()
                Else
                    p_Menu()
                End If
            End If

            ClearMessage()
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

    Private Sub p_delete()
        Dim sEvent As String = "DEL_PALLET"
        elapseStart = Now
        Dim oPickedChecking As New FG_PickedChecking

        If dtScreen.ExternOrderKey = String.Empty Then
            oPickedChecking.DeleteByPallet(dtScreen.PalletID, dtConnInfo)
        Else
            If dtScreen.PalletID = String.Empty Then
                oPickedChecking.DeleteByExternOrderKey(dtScreen.ExternOrderKey, dtConnInfo)
                sEvent = "DEL_INVOICE"
            Else
                oPickedChecking.DeleteByPallet(dtScreen.PalletID, dtConnInfo)
            End If
        End If

        p_saveLog(sEvent)
        p_renewMainScreen("DELETE COMPLETED", False)
    End Sub

    Private Sub p_scanInvoice(iLeft As Integer, iTop As Integer)
        dtScreen.ExternOrderKey = GetScanFixEnterPrint(iLeft, iTop, True, 1)
        elapseStart = Now
        If dtScreen.ExternOrderKey = String.Empty Then
            ScreenMode = ScreenModeList.INVOICE
        Else
            ScreenMode = ScreenModeList.PALLET
        End If
        p_Main()
    End Sub

    Private Sub p_scanPalletId(iLeft As Integer, iTop As Integer)
        dtScreen.PalletID = GetScanFixEnterPrint(iLeft, iTop, True, 1)
        ScreenMode = ScreenModeList.PALLET
        elapseStart = Now

        If dtScreen.PalletID = String.Empty Then
            If dtScreen.ExternOrderKey = String.Empty Then p_renewMainScreen(String.Empty, False)
        Else
            dtScreen.ExternOrderKey = String.Empty
            p_Main()
        End If
    End Sub

    Private Sub p_saveLog(sEvent As String)
        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = sEvent
        dtLog.Scan1 = dtScreen.ExternOrderKey
        dtLog.Scan2 = dtScreen.PalletID
        dtLog.Status = Log.LogStatus.SUCCESS.ToString()
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtScreen.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_renewMainScreen(sMessage As String, bError As Boolean)
        dtScreen = New DataScreen
        ScreenMode = ScreenModeList.BEGIN
        elapseStart = Now
        p_Main(sMessage, bError)
    End Sub

    Private Sub p_validate(iLeft As Integer, iTop As Integer)
        Try
            If dtScreen.ExternOrderKey <> String.Empty Then
                If New FG_PickedChecking().isExistsExternOrderKey(dtScreen.ExternOrderKey, dtConnInfo) Then
                    ScreenMode = ScreenModeList.SHOW
                    PrintLine(iLeft, iTop, "ORDER FOUND")
                    iTop += 1
                Else
                    p_renewMainScreen("ORDER N/A", True)
                End If
            End If

            If dtScreen.PalletID <> String.Empty Then
                If New FG_PickedChecking().isExistsPalletId(dtScreen.PalletID, dtConnInfo) Then
                    ScreenMode = ScreenModeList.SHOW
                    PrintLine(iLeft, iTop, "PALLET FOUND")
                    iTop += 1
                Else
                    p_renewMainScreen("PALLET N/A", True)
                End If
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Function p_setElapseTime() As Double
        Dim elapse As TimeSpan = Now() - elapseStart
        Return elapse.TotalMilliseconds
    End Function

#End Region
#Region "Data Class"
    Public Class DataScreen
        Public ExternOrderKey As String = String.Empty
        Public PalletID As String = String.Empty
        Public StorerKey As String = dtUserActive.StorerKey
    End Class

#End Region
End Module
