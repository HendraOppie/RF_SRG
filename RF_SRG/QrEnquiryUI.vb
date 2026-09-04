Imports AIT.RF
Imports AIT.SM
Imports AIT.GEN

Module QrEnquiryUI
#Region "Public"
    Dim data As New QrEnquiry.DataQrEnquiry

    Public Sub Main()
        p_main()
    End Sub
#End Region

#Region "Private"
    Private Sub p_main(Optional sMessage As String = "", Optional bError As Boolean = False)
        data.ScanBy = dtUserActive.UserName
        PrintHeader(RFUiList.QR_CAPTURE)
        PrintLine(1, 4, "Remarks: ")
        PrintLine(1, 5, New String(" ", DEFAULT_TEXTBOX_LENGTH),, True)
        PrintLine(1, 6, "Scan QR:")
        PrintLine(1, 7, New String(" ", DEFAULT_TEXTBOX_LENGTH),, True)
        PrintFooter()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        If data.Remarks = String.Empty Then
            p_scanRemarks(1, 5)
        Else
            PrintLine(1, 5, data.Remarks)
        End If

        If data.ScanResult = String.Empty Then
            p_scanQr(10, 6)

            If data.ScanResult.Contains(vbCr) Then
                Dim sSplit() As String = data.ScanResult.Split(vbCr)
                Dim iTop As Int16 = 6
                For i As Int16 = 0 To sSplit.Count - 1
                    iTop += 1
                    PrintLine(1, iTop, New String(" ", DEFAULT_TEXTBOX_LENGTH),, True)
                    PrintLine(1, iTop, sSplit(i),, True)
                Next
            End If
        Else
            PrintLine(1, 7, data.ScanResult)
        End If

        p_save()
    End Sub

    Private Sub p_scanRemarks(iLeft As Integer, iTop As Integer)
        data.Remarks = String.Empty

        Dim sInput As String = String.Empty
invalidScan:
        sInput = GetInput(iLeft, iTop, DEFAULT_TEXTBOX_LENGTH, True)
        If sInput.Length > 0 Then
            ClearMessage()
            data.Remarks = sInput
        Else
            PrintMessage("INVLD REMARKS", True)
            GoTo invalidScan
        End If
    End Sub

    Private Sub p_scanQr(iLeft As Integer, iTop As Integer)
        data.ScanResult = String.Empty

        Dim sInput As String = String.Empty
invalidScan:
        Console.SetCursorPosition(iLeft, iTop)
        'sInput = Console.ReadLine()
        sInput = GetScanUnknown(iLeft, iTop)
        If sInput.Length > 0 Then
            data.ScanResult = sInput
            ClearMessage()
        Else
            PrintMessage("INVLD QR", True)
            GoTo invalidScan
        End If
    End Sub

    Private Sub p_save()
        Dim bEscFlag As Boolean = False
        If SaveConfirmation(bEscFlag) Then
            p_saveLog(data.Remarks, data.ScanResult, String.Empty, Log.LogStatus.SUCCESS.ToString())
            data = New QrEnquiry.DataQrEnquiry
            p_main("SCAN SUCCESS", False)
            'If p_insert() Then p_main("SCAN SUCCESS", False)
        Else
            data = New QrEnquiry.DataQrEnquiry
            If bEscFlag Then
                p_main()
            Else
                p_main("SCAN FAILED!", True)
            End If
        End If
    End Sub

    Private Function p_insert() As Boolean
        UIWait()

        Try
            Dim QE As New QrEnquiry
            data.dtmLastUpdated = DateTime.Now
            QE.Save(data, dtConnInfo)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            Return False
        End Try

        Return True
    End Function

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

#End Region

End Module
