

Module PalletShipment
    Dim dtScreen As DataScreen
#Region "Public"
    Public Sub Main()
        dtScreen = New DataScreen
        p_mainUi()
    End Sub

    Public Sub Input()
        p_dropidUi()
    End Sub
#End Region

#Region "Private"
    Private Sub p_mainUi(Optional sMessage As String = "", Optional bError As Boolean = False)
        p_invoiceUi(sMessage, bError)
        p_dropidUi()
    End Sub

    Private Sub p_invoiceUi(Optional sMessage As String = "", Optional bError As Boolean = False)
        PrintHeader(RFUiList.PS_INVOICE)

        PrintLine(1, 4, "INVOICE NO.:")
        PrintLine(1, 5, New String(" ", 25),, True)
        PrintLine(1, 6, "VEHICLE NO.:")
        PrintLine(1, 7, New String(" ", 8),, True)
        PrintFooter()
        If sMessage.Length > 0 Then PrintMessage(sMessage, bError)

        p_scanInvoice()
        p_scanVehicle()
    End Sub

    Private Sub p_scanInvoice()
invalidInvoice:
        dtScreen.InvoiceNo = GetInput(1, 5, 25, True)
        If dtScreen.InvoiceNo.Length = 0 Then
            PrintMessage("INVOICE BLANK", True)
            GoTo invalidInvoice
        End If
    End Sub

    Private Sub p_scanVehicle()
        dtScreen.VehicleNo = GetInput(1, 7, 8, True)
        PrintLine(1, 5, dtScreen.VehicleNo)
    End Sub

    Private Sub p_dropidUi()
        PrintHeader(RFUiList.PS_PALLET)

        PrintLine(1, 4, String.Format("{0} {1}", "INVOICE NO.: ", dtScreen.InvoiceNo))
        PrintLine(1, 5, String.Format("{0} {1}", "VEHICLE NO.: ", dtScreen.VehicleNo))
        PrintLine(1, 6, String.Format("{0} {1}", "TTL DROP ID: ", dtScreen.alDropId.Count.ToString))
        PrintLine(1, 8, "SCAN DROP ID / PALLET ID :")
        PrintFooter()

        p_scanDropId()
    End Sub

    Private Sub p_scanDropId()
rescan:
        PrintLine(1, 9, New String(" ", 12),, True)
        dtScreen.DropId = GetInput(1, 9, 12, True)
        If dtScreen.DropId.Length > 0 Then
            dtScreen.alDropId.Add(dtScreen.DropId)
            PrintLine(1, 6, String.Format("{0} {1}", "TTL DROP ID: ", dtScreen.alDropId.Count.ToString))
            GoTo rescan
        Else
            If dtScreen.alDropId.Count > 0 Then
                p_save()
            Else
                GoTo rescan
            End If
        End If
    End Sub

    Private Sub p_save()
        Dim bEscFlag As Boolean = False
        If SaveConfirmation(bEscFlag) Then
            p_saveData()
        Else
            If bEscFlag Then
                p_dropidUi()
            Else
                p_mainUi("scan failed", True)
            End If
        End If
    End Sub

    Private Function p_saveData() As Boolean
        UIWait()
        Try

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        p_mainUi("scan success", False)
        Return True
    End Function

#End Region

#Region "Data Class"
    Public Class DataScreen
        Public InvoiceNo As String = String.Empty
        Public VehicleNo As String = String.Empty
        Public DropId As String = String.Empty
        Public alDropId As ArrayList = New ArrayList
    End Class
#End Region
End Module
