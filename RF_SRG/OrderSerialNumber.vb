Imports AIT.RF
Imports AIT.WMS

Module OrderSerialNumber

#Region "PUBLIC"
    Dim dtScreen As New DataScreen
    Dim eScanPos As ScanPos = ScanPos.ORDERKEY
    Dim eScreenMode As ScreenMode = ScreenMode.SUBMENU
    Dim elapseStart As Date = Now
    Dim iLastRow As Int16 = 8

    Public Sub Main()
        eActiveUI = RFUiList.ORDER_SERIAL_NUMBER
        elapseStart = Now()
        p_subMenu()
    End Sub

    Public Sub GoToPrevScreen()
        p_gotoPrevScreen()
    End Sub

#End Region

#Region "PRIVATE"
    Private Sub p_gotoPrevScreen()
        If eScreenMode = ScreenMode.SUBMENU Then
            GotoUi(RFUiList.MENU_STD)
        Else
            If dtScreen.OrderKey = String.Empty Then
                eScreenMode = ScreenMode.SUBMENU
                p_subMenu()
            Else
                dtScreen = New DataScreen
                eScanPos = ScanPos.ORDERKEY
                p_main()
            End If
        End If
    End Sub

    Private Sub p_subMenu()
        Console.Clear()
        Console.ResetColor()
        PrintLine(1, 1, sTitle)
        PrintLine(1, 3, "ORDER SERIAL#:")
        PrintLine(1, 4, "1. SERIAL#")
        PrintLine(1, 5, "2. SERIAL# & SKU")
        PrintLine(1, 6, "3. SERIAL# & SKU & DROP ID")
        PrintLine(1, 7, "4. SERIAL# & DROP ID")
        'PrintLine(1, 8, String.Empty)
        'PrintLine(1, 9, String.Empty)
        'PrintLine(1, 10, String.Empty)
        'PrintLine(1, 11, "8. CHANGE PIN")
        'PrintLine(1, 12, "9. LOG OFF")
        PrintLine(1, 13, "CHOOSE MENU:")
        PrintLine(14, 13, New String(" ", 1),, True)
        PrintFooter()

invalidKey:
        Dim sChoosen As String = String.Empty
        sChoosen = GetInputNumber(14, 13, 1)
        If sChoosen = String.Empty Or CInt(sChoosen) = 0 Then
            PrintMessage(MESSAGE_WARNING_INVALID_ENTRY, True)
            System.Threading.Thread.Sleep(1000)
            GoTo invalidKey
        Else
            Dim iMenuChoosen As Integer = Convert.ToInt16(sChoosen)
            Select Case iMenuChoosen
                Case 1
                    eScreenMode = ScreenMode.SN
                Case 2
                    eScreenMode = ScreenMode.SNSKU
                Case 3
                    eScreenMode = ScreenMode.SNSKUDROPID
                Case 4
                    eScreenMode = ScreenMode.SNDROPID
                Case Else
                    PrintMessage(MESSAGE_WARNING_INVALID_ENTRY, True)
                    System.Threading.Thread.Sleep(1000)
                    GoTo invalidKey
            End Select
        End If

        dtScreen = New DataScreen
        p_main()
    End Sub

    Private Sub p_main(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()

            PrintHeader(eActiveUI)
            PrintLine(1, 4, String.Format("ORDERKEY : {0}", dtScreen.OrderKey))
            PrintLine(1, 5, String.Format("COUNTER  : {0}", dtScreen.alDetail.Count.ToString))
            PrintLine(1, 6, String.Format("SERIAL   : {0}", dtScreen.SerialScanned))
            iLastRow = 8

            If eScreenMode = ScreenMode.SNSKU Or eScreenMode = ScreenMode.SNSKUDROPID Then
                PrintLine(1, 7, String.Format("SKU      : {0}", dtScreen.SkuScanned))
                iLastRow = 9
            End If
            If eScreenMode = ScreenMode.SNDROPID Then
                PrintLine(1, 7, String.Format("DROP ID  : {0}", dtScreen.DropIdScanned))
                iLastRow = 9
            End If
            If eScreenMode = ScreenMode.SNSKUDROPID Then
                PrintLine(1, 8, String.Format("DROP ID  : {0}", dtScreen.DropIdScanned))
                iLastRow = 10
            End If

            p_printList(iLastRow)
            PrintMessage(sMessage, bError)
            PrintFooterWithElapse(elapseStart)

            Select Case eScanPos
                Case ScanPos.ORDERKEY
                    PrintLine(11, 4, New String(" ", 20), , True)
                    p_scanOrderKey(11, 4)
                    p_dataPreparation(sMessage, bError)
                    If bError Then p_main(sMessage, bError)
                    eScanPos = ScanPos.SERIALNUMBER

                Case ScanPos.SERIALNUMBER
                    PrintLine(11, 6, New String(" ", 20), , True)
                    p_scanSerial(11, 6)
                    If p_isSerialExists() Then p_main(MESSAGE_INFO_ALREADY_SCANNED, True)

                    If eScreenMode = ScreenMode.SN Then
                        p_save()
                    ElseIf eScreenMode = ScreenMode.SNDROPID Then
                        eScanPos = ScanPos.PACKDROPID
                    Else
                        eScanPos = ScanPos.SKU
                    End If

                Case ScanPos.SKU
                    PrintLine(11, 7, New String(" ", 20), , True)
                    p_scanSku(11, 7)
                    If eScreenMode = ScreenMode.SNSKU Then
                        eScanPos = ScanPos.SERIALNUMBER
                        p_save()
                    Else
                        eScanPos = ScanPos.PACKDROPID
                    End If

                Case ScanPos.PACKDROPID
                    Dim iTop_DropId As Int16 = 8
                    If eScreenMode = ScreenMode.SNDROPID Then iTop_DropId = 7
                    PrintLine(11, iTop_DropId, New String(" ", 20), , True)
                    p_scanPackDropId(11, iTop_DropId)
                    eScanPos = ScanPos.SERIALNUMBER
                    p_save()

            End Select

            p_main()

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub

    Private Sub p_addScreenDetail()
        If dtScreen.SerialScanned = String.Empty Then Exit Sub

        Dim data As New DataScreenDetail
        data.Serial = dtScreen.SerialScanned
        data.Sku = dtScreen.SkuScanned
        data.DropId = dtScreen.DropIdScanned
        dtScreen.alDetail.Add(data)

        dtScreen.SerialScanned = String.Empty
        dtScreen.SkuScanned = String.Empty
        dtScreen.DropIdScanned = String.Empty
    End Sub

    Private Function p_isSerialExists() As Boolean
        elapseStart = Now
        Return New OrderSerialNumberManager().IsExistsSerialNumber(dtScreen.StorerKey, dtScreen.SerialScanned, dtConnInfo)

    End Function

    Private Sub p_save()
        If p_saveData() Then
            p_addScreenDetail()
            p_main(MESSAGE_INFO_SAVE_SUCCESS, False)
        End If
    End Sub

    Private Function p_saveData() As Boolean
        elapseStart = Now
        UIWait(MESSAGE_INFO_UPDATING)

        Try
            Dim dtOrderSerial As New OrderSerialNumberManager.DataOrderSerialNumber
            dtOrderSerial.StorerKey = dtScreen.StorerKey
            dtOrderSerial.OrderKey = dtScreen.OrderKey
            dtOrderSerial.ExternOrderKey = dtScreen.ExternOrderKey
            dtOrderSerial.SerialNumber = dtScreen.SerialScanned
            dtOrderSerial.Sku = dtScreen.SkuScanned
            dtOrderSerial.DropId = dtScreen.DropIdScanned
            dtOrderSerial.UserId = dtUserActive.UserId
            dtOrderSerial.WhseId = dtUserActive.WhseId

            Dim oOrderSerial As New OrderSerialNumberManager
            oOrderSerial.Save(dtOrderSerial, dtConnInfo)
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
        End Try

        Return True
    End Function

    Private Sub p_dataPreparation(ByRef sMessage As String, ByRef bError As Boolean)
        If dtScreen.alDetail.Count > 0 Then Exit Sub

        UIWait(MESSAGE_INFO_PREPARING_WMSDATA)
        elapseStart = Now
        sMessage = String.Empty
        bError = False

        Dim alOrderSerialNumber As ArrayList = New OrderSerialNumberManager().GetList(dtScreen.OrderKey, dtUserActive.WhseId, dtConnInfo)
        If alOrderSerialNumber.Count > 0 Then
            For Each dataOrderSerial As OrderSerialNumberManager.DataOrderSerialNumber In alOrderSerialNumber
                dtScreen.ExternOrderKey = dataOrderSerial.ExternOrderKey
                dtScreen.StorerKey = dataOrderSerial.StorerKey

                'Dim data As New DataScreenDetail
                'data.Serial = dataOrderSerial.SerialNumber
                'dtScreen.alDetail.Add(data)
            Next
        Else
            Dim dtWmsOrder As Orders.DataOrders = New Orders().GetData(dtScreen.OrderKey, dtConnInfo)
            If dtWmsOrder.OrderKey = String.Empty Then
                sMessage = MESSAGE_INFO_DATA_NOTFOUND
                bError = True
            Else
                dtScreen.ExternOrderKey = dtWmsOrder.ExternOrderKey
                dtScreen.StorerKey = dtWmsOrder.StorerKey
            End If
        End If
    End Sub

    Private Sub p_printList(ByRef iTop As Int16)
        If dtScreen.alDetail.Count = 0 Then Exit Sub

        For i As Int16 = dtScreen.alDetail.Count - 1 To 0 Step -1
            Dim data As DataScreenDetail = dtScreen.alDetail(i)
            PrintLine(3, iTop, data.Serial)
            iTop += 1
            If iTop = 17 Then Exit For '17 = message top pos
        Next
    End Sub

    Private Sub p_scanOrderKey(iLeft As Integer, iTop As Integer)
        elapseStart = Now
        dtScreen.OrderKey = GetScanFixEnterPrint(iLeft, iTop, True, 1)
        If dtScreen.OrderKey = String.Empty Then
            p_main(MESSAGE_WARNING_INVALID_ENTRY, True)
        Else
            dtScreen.OrderKey = dtScreen.OrderKey.PadLeft(10, "0")
        End If
    End Sub

    Private Sub p_scanSerial(iLeft As Integer, iTop As Integer)
        dtScreen.SerialScanned = GetScanFixEnterPrint(iLeft, iTop, False, 1)
        If dtScreen.SerialScanned = String.Empty Then p_main(MESSAGE_WARNING_INVALID_ENTRY, True)
    End Sub

    Private Sub p_scanSku(iLeft As Integer, iTop As Integer)
        dtScreen.SkuScanned = GetScanFixEnterPrint(iLeft, iTop, False, 1)
        If dtScreen.SkuScanned = String.Empty Then p_main(MESSAGE_WARNING_INVALID_ENTRY, True)
    End Sub

    Private Sub p_scanPackDropId(iLeft As Integer, iTop As Integer)
        dtScreen.DropIdScanned = GetScanFixEnterPrint(iLeft, iTop, False, 1)
        If dtScreen.DropIdScanned = String.Empty Then p_main(MESSAGE_WARNING_INVALID_ENTRY, True)
    End Sub

#End Region

#Region "Data Class"
    Public Enum ScanPos
        ORDERKEY
        SERIALNUMBER
        SKU
        PACKDROPID
    End Enum

    Public Enum ScreenMode
        SUBMENU
        SN
        SNSKU
        SNDROPID
        SNSKUDROPID
    End Enum

    Public Class DataScreen
        Public StorerKey As String = String.Empty
        Public OrderKey As String = String.Empty
        Public ExternOrderKey As String = String.Empty
        Public SerialScanned As String = String.Empty
        Public SkuScanned As String = String.Empty
        Public DropIdScanned As String = String.Empty
        Public QtyTotal As Int32 = 0
        Public QtyTotalScanned As Int32 = 0
        Public alDetail As New ArrayList
    End Class

    Public Class DataScreenDetail
        Public Serial As String = String.Empty
        Public Sku As String = String.Empty
        Public DropId As String = String.Empty
    End Class

#End Region
End Module
