Imports AIT.RF
Imports AIT.WMS

Module FG_LabelCheckUI

#Region "Public"
    Dim dtScreen As New DataScreen
    Dim elapseStart As Date
    Dim elapseMilliSecond As Double = 0

    Dim sender As RFUiList
    Dim alSku As New ArrayList
    Dim bScan1 As Boolean = True
    Dim iTop As Int16 = 4

    Dim alContent As New ArrayList

    Public Sub Main(lSender As RFUiList, alData As ArrayList)
        dtScreen = New DataScreen
        sender = lSender
        dtScreen.alData = alData
        p_Menu()
    End Sub

    Public Sub GotoPrevScreen()
        p_gotoPrevScreen()
    End Sub

    Public Sub DecodeQRPallet()
        dtScreen = New DataScreen
        sender = RFUiList.MENU_SRGPLB_10
        p_justDecodeQRPallet()
    End Sub

#End Region
#Region "Private"
    Private Sub p_Menu()
        eActiveUI = RFUiList.FG_LABEL_CHECK
        p_dataPreparation()
        p_Main()
    End Sub

    Private Sub p_Main(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            Console.Clear()
            Console.ResetColor()
            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintFooter()

            Dim sQ1 As String = String.Empty
            If dtScreen.QrCode1.Length > 12 Then
                sQ1 = dtScreen.QrCode1.Substring(0, 12)
            Else
                sQ1 = dtScreen.QrCode1
            End If
            PrintLine(1, 4, String.Format("{0} {1}", "LABEL 1 :", sQ1))
            PrintLine(1, 5, String.Format("{0} {1}", "- STATUS:", dtScreen.Status1))

            Dim sQ2 As String = String.Empty
            If dtScreen.QrCode2.Length > 12 Then
                sQ2 = dtScreen.QrCode2.Substring(0, 12)
            Else
                sQ2 = dtScreen.QrCode2
            End If
            PrintLine(1, 6, String.Format("{0} {1}", "LABEL 2 :", sQ2))
            PrintLine(1, 7, String.Format("{0} {1}", "- STATUS:", dtScreen.Status2))
            PrintLine(1, 8, "SCAN QR PALLET:")
            iTop = 9

            dtScreen.QrCode = GetScanFixEnterPrint(16, 8, False, 1)
            ClearMessage()
            elapseStart = Now()
            If dtScreen.QrCode = String.Empty Then p_Main("EMPTY NOT ALLOW", True)

            Dim sContent() As String = dtScreen.QrCode.Split(",")
            If sContent.Count = 1 Then
                If dtScreen.PalletId = dtScreen.QrCode Then
                    If dtScreen.QrCode1 = String.Empty Then
                        dtScreen.QrCode1 = dtScreen.QrCode
                    Else
                        dtScreen.QrCode2 = dtScreen.QrCode
                    End If
                    GoTo justCheckPalletId
                End If
                If dtScreen.QrCode2 = String.Empty Then
                    dtScreen.QrCode1 = String.Empty
                    dtScreen.Status1 = "INVALID"
                Else
                    dtScreen.QrCode2 = String.Empty
                    dtScreen.Status2 = "INVALID"
                End If

                p_Main("INVALID PALLET ID", True)
            End If

            If dtScreen.QrCode1 = String.Empty Then
                dtScreen.QrCode1 = dtScreen.QrCode
            Else
                dtScreen.QrCode2 = dtScreen.QrCode
            End If
            If dtScreen.QrCode1 = dtScreen.QrCode2 And dtScreen.bSpecialCasePalletLabelSame = False Then p_Main("SAME LABEL", True)


            p_decodeQRPallet()

            Dim sValid As String = String.Empty
            Dim sValidType As String = String.Empty
            For Each content As FG_PickDetail.DataQR In alContent
                Select Case content.Type
                    Case ContenType.PALLETID.ToString
                        sValid = p_validatePalletId(content.Value)
                    Case ContenType.SKU.ToString
                        sValid = p_validateSku(content.Value)
                    Case ContenType.QTY.ToString
                        sValid = p_validateQty(content.Value)
                    Case ContenType.SERIAL.ToString
                        sValid = p_validateSerial(content.Value)
                    Case ContenType.EMPTYBOX.ToString
                        sValid = p_validateEmpty(content.Value)

                End Select

                If iTop < 23 Then
                    PrintLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
                    PrintLine(1, iTop, String.Format("{0} {1} {2}", sValid, content.Type, content.Value))
                    iTop += 1
                End If

                If sValid = STATUS_INVALID Then
                    sValidType = content.Type
                    Exit For
                End If
            Next

            If sValid = STATUS_INVALID Then
                If dtScreen.QrCode2 = String.Empty Then
                    dtScreen.QrCode1 = String.Empty
                Else
                    dtScreen.QrCode2 = String.Empty
                End If

                p_logQrPallet(Log.LogStatus.NOT_MATCH.ToString())
                p_Main(String.Format("{0} {1}", STATUS_INVALID, sValidType), True)
            Else
justCheckPalletId:
                If dtScreen.QrCode2 = String.Empty Then
                    dtScreen.Status1 = STATUS_VALID
                    p_Main("SCAN LABEL 2")
                Else
                    Dim sQ21 As String = String.Empty
                    If dtScreen.QrCode2.Length > 12 Then
                        sQ21 = dtScreen.QrCode2.Substring(0, 12)
                    Else
                        sQ21 = dtScreen.QrCode2
                    End If
                    PrintLine(1, 6, String.Format("{0} {1}", "LABEL 2 :", sQ21))

                    dtScreen.Status2 = STATUS_VALID
                    PrintLine(1, 7, String.Format("{0} {1}", "- STATUS:", STATUS_VALID))
                End If

                p_save()
                p_promoteReport()
                If iTop < 17 Then iTop = 17
                WaitAnyKey("COMPLETED", iTop)
                p_gotoPrevScreen()
            End If

        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try
    End Sub

    Private Sub p_justDecodeQRPallet(Optional sMessage As String = "", Optional bError As Boolean = False)
        Try
            dtScreen.QrCode = String.Empty
            Console.Clear()
            Console.ResetColor()
            PrintLine(1, 1, sTitle)

            PrintHeader(eActiveUI)
            If sMessage.Length > 0 Then PrintMessage(sMessage, bError)
            PrintFooter()

            PrintLine(1, 4, String.Format("{0} {1}", "SCAN QR PALLET:", sMessage))
            dtScreen.QrCode = GetScanFixEnterPrint(1, 5, False, 1)
            ClearMessage()
            elapseStart = Now()
            PrintLine(1, 4, String.Format("{0} {1}", "SCAN QR PALLET:", dtScreen.QrCode))
            iTop = 7

            p_decodeQRPallet()
            For Each content As FG_PickDetail.DataQR In alContent
                If iTop < 23 Then
                    PrintLine(1, iTop, New String(" ", DEFAULT_WIDTH_WINDOW))
                    PrintLine(1, iTop, String.Format("{0} {1}", content.Type, content.Value))
                    iTop += 1
                Else
                    Exit For
                End If
            Next

            If Not dtScreen.QrCode = String.Empty Then
                p_logQrPallet()
                p_printElapse()
                WaitAnyKey("COMPLETED", iTop)
            End If

            p_justDecodeQRPallet()
        Catch ex As Exception
            UIError(ex.Message & vbCrLf & ex.StackTrace)
            p_Main("SYS ERROR", True)
        End Try
    End Sub

    Private Sub p_printElapse()
        Dim elapse As TimeSpan = Now() - elapseStart
        elapseMilliSecond = elapse.TotalMilliseconds

        If iTop < 18 Then
            PrintFooterWithElapse(elapseMilliSecond)
        Else
            PrintFooterWithElapse(elapseMilliSecond, iTop + 1)
        End If

        elapseMilliSecond = 0
    End Sub

    Private Sub p_logQrPallet(Optional sStatus As String = "")
        If sStatus = "" Then sStatus = Log.LogStatus.SUCCESS.ToString()

        Dim dtLog As New Log.DataLog
        dtLog.MenuID = eActiveUI.ToString()
        dtLog.Event = "SCAN QR PALLET"
        dtLog.Scan1 = dtScreen.QrCode
        dtLog.Status = sStatus
        dtLog.UserID = dtUserActive.UserId
        dtLog.StorerKey = dtScreen.StorerKey
        SaveLog(dtLog)
    End Sub

    Private Sub p_gotoPrevScreen()
        GotoUi(sender)
    End Sub

    Private Sub p_contentPreparationStandard()
        Dim sContent() As String = dtScreen.QrCode.Split(",")
        If sContent.Count = 1 Then Exit Sub
        alContent.Clear()
        Dim flagSku As Boolean = False

        For i As Int16 = 0 To sContent.Count - 1
            Dim dtQR As New FG_PickDetail.DataQR
            If i = 0 Then dtQR.Type = ContenType.PALLETID.ToString
            If i = 1 Then dtQR.Type = ContenType.SKU.ToString
            If i = 2 Then dtQR.Type = ContenType.QTY.ToString
            If i = 3 Then dtQR.Type = ContenType.SERIAL.ToString

            If i > 3 And i < sContent.Count - 1 Then
                If sContent(i).Length > 10 Then
                    dtQR.Type = ContenType.SKU.ToString
                    flagSku = True
                Else
                    If flagSku Then
                        dtQR.Type = ContenType.QTY.ToString
                        flagSku = False
                    Else
                        dtQR.Type = ContenType.SERIAL.ToString
                    End If

                End If
            End If

            If i = sContent.Count - 1 Then dtQR.Type = ContenType.EMPTYBOX.ToString

            dtQR.Value = sContent(i)
            If dtQR.Type = ContenType.PALLETID.ToString Then
                Dim sContentPalletId() As String = sContent(i).Split("-")
                If sContentPalletId.Count > 2 Then dtQR.Value = sContentPalletId(0) + "-" + sContentPalletId(1)
            End If

            alContent.Add(dtQR)
        Next
    End Sub

    Private Sub p_contentPreparationModel1()
        Dim sContent() As String = dtScreen.QrCode.Split(",")
        If sContent.Count = 1 Then Exit Sub
        alContent.Clear()
        Dim flagSku As Boolean = False

        For i As Int16 = 0 To sContent.Count - 1
            Dim dtQR As New FG_PickDetail.DataQR
            If i = 0 Then dtQR.Type = ContenType.PALLETID.ToString
            If i = 1 Then dtQR.Type = ContenType.SKU.ToString

            If i > 1 And i < sContent.Count - 1 Then
                If sContent(i).Length > 10 Then
                    dtQR.Type = ContenType.SKU.ToString
                    flagSku = True
                Else
                    If flagSku Then
                        dtQR.Type = ContenType.QTY.ToString
                        flagSku = False
                    Else
                        dtQR.Type = ContenType.SERIAL.ToString
                    End If

                End If
            End If

            If i = sContent.Count - 1 Then dtQR.Type = ContenType.EMPTYBOX.ToString

            If i = 1 Then
                Dim split1() As String = sContent(1).Split(".")
                dtQR.Value = split1(0)
                alContent.Add(dtQR)

                Dim dtQ As New FG_PickDetail.DataQR
                dtQ.Type = ContenType.QTY.ToString
                dtQ.Value = split1(1)
                alContent.Add(dtQ)

                Dim split2() As String = split1(2).Split("-")
                Dim dtR As New FG_PickDetail.DataQR
                dtR.Type = ContenType.SERIAL.ToString
                dtR.Value = split2(split2.Length - 1)
                If dtR.Value = String.Empty Then
                    dtR.Value = split1(3)
                End If
                alContent.Add(dtR)
            Else
                If Not sContent(i).Trim.Replace(".", String.Empty) = String.Empty Then
                    dtQR.Value = sContent(i)
                    alContent.Add(dtQR)
                End If
            End If
        Next
    End Sub

    Private Sub p_decodeQRPallet()
        Dim sContent() As String = dtScreen.QrCode.Split(",")
        If sContent.Count = 1 Then Exit Sub
        alContent.Clear()
        'Dim flagSku As Boolean = False

        'p_splitQRforPallet(sContent(0))
        'For i As Int16 = 1 To sContent.Count - 2
        '    If Not sContent(i).Trim = String.Empty Then p_splitQR(sContent(i), flagSku)
        'Next
        'p_splitQRforEmptyBox(sContent.Count - 1)

        Dim alContentQrPallet As ArrayList = New FG_SplitQrPallet().GetList(dtScreen.QrCode, dtConnInfo)
        For Each data As FG_SplitQrPallet.DataSplitQrPallet In alContentQrPallet
            Dim dtQR As New FG_PickDetail.DataQR
            dtQR.Type = data.ContentType
            dtQR.Value = data.Content
            alContent.Add(dtQR)
        Next
    End Sub

    Private Sub p_splitQRforPallet(sContent As String)
        Dim dtQR As New FG_PickDetail.DataQR
        dtQR.Type = ContenType.PALLETID.ToString
        dtQR.Value = sContent
        Dim splitPallet() As String = sContent.Split("-")
        If splitPallet.Count > 2 Then dtQR.Value = splitPallet(0) + "-" + splitPallet(1)
        alContent.Add(dtQR)
    End Sub

    Private Sub p_splitQRforEmptyBox(sContent As String)
        Dim dtQR As New FG_PickDetail.DataQR
        dtQR.Type = ContenType.EMPTYBOX.ToString
        dtQR.Value = sContent
        alContent.Add(dtQR)
    End Sub

    Private Sub p_splitQR(sContent As String, ByRef flagSku As Boolean)
        Dim dtQR As New FG_PickDetail.DataQR
        Dim splitContent() As String = sContent.Split(".")
        If splitContent.Count = 1 Then
            If sContent.Length > 10 Then
                dtQR.Type = ContenType.SKU.ToString
                dtQR.Value = sContent

                flagSku = True
            Else
                If flagSku Then
                    dtQR.Type = ContenType.QTY.ToString

                    flagSku = False
                End If
                dtQR.Type = ContenType.SERIAL.ToString
                dtQR.Value = sContent.Trim.Replace(".", String.Empty)
            End If
            Exit Sub
        End If

        Dim dtSKU As New FG_PickDetail.DataQR
        dtSKU.Type = ContenType.SKU.ToString
        dtSKU.Value = splitContent(0)
        alContent.Add(dtSKU)

        Dim dtQty As New FG_PickDetail.DataQR
        dtQty.Type = ContenType.QTY.ToString
        dtQty.Value = splitContent(1).Trim.Replace(".", String.Empty)
        alContent.Add(dtQty)

        If splitContent.Count > 2 Then
            Dim splitSplitContent2() As String = splitContent(2).Split("-")
            Dim dtSerial As New FG_PickDetail.DataQR
            dtSerial.Type = ContenType.SERIAL.ToString
            If splitSplitContent2.Count = 1 Then
                dtSerial.Value = splitSplitContent2(0).Trim.Replace(".", String.Empty)
            Else dtSerial.Value = splitSplitContent2(splitSplitContent2.Count - 1).Trim.Replace(".", String.Empty)
            End If
            alContent.Add(dtSerial)
        End If
    End Sub

    Private Sub p_dataPreparation()
        'dtScreen.alData = p_GetPickList(dtScreen.PalletId)
        For Each dtPickedChecking As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            dtScreen.PalletId = dtPickedChecking.PalletId
            dtScreen.StorerKey = dtPickedChecking.StorerKey
            Exit For
        Next

    End Sub

    Private Function p_validatePalletId(suspect As String) As String
        Dim palletId As String = dtScreen.PalletId

        If InStr(dtScreen.PalletId, "TS.") Or InStr(dtScreen.PalletId, "JS.") Then palletId = palletId.Substring(3, palletId.Length - 3)
        palletId = palletId.Replace(" ", String.Empty)

        If InStr(suspect, "BS.") Or InStr(suspect, "DS.") Or InStr(suspect, "BS*") Or InStr(suspect, "DS*") Then suspect = suspect.Substring(3, suspect.Length - 3)
        If InStr(suspect, "S.") Then
            suspect = suspect.Substring(2, suspect.Length - 2)
            dtScreen.bSpecialCasePalletLabelSame = True
        End If
        suspect = suspect.Replace(" ", String.Empty)

        Dim palletComp() As String = suspect.Split("-")
        If palletComp.Length > 1 Then suspect = palletComp(0) & "-" & palletComp(1)

        If palletComp.Length = 3 Then
            'case pallet id = 1mzd9: just ignore prefix
            If InStr(palletId, palletComp(2)) Then
                dtScreen.bSpecialCasePalletLabelSame = True
                Return STATUS_VALID
            Else
                Return STATUS_INVALID
            End If
        Else
            If palletId = suspect Then
                Return STATUS_VALID
            Else

                Return STATUS_INVALID
            End If
        End If

    End Function

    Private Function p_validateSku(suspect As String) As String
        Dim bFound As Boolean = False

        For Each dtPickedChecking As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If dtPickedChecking.Sku = suspect Then
                dtScreen.Sku = dtPickedChecking.Sku
                bFound = True
                Exit For
            End If
        Next
        If bFound Then
            Return STATUS_VALID
        Else
            Return STATUS_INVALID
        End If
    End Function

    Private Function p_validateQty(suspect As Int16) As String
        Dim qty As Int16 = 0

        For Each dtPickedChecking As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If dtPickedChecking.Sku = dtScreen.Sku Then qty += dtPickedChecking.Qty
        Next
        If qty = suspect Then
            Return STATUS_VALID
        Else
            Return STATUS_INVALID
        End If
    End Function

    Private Function p_validateSerial(suspect As String) As String
        Dim bFound As Boolean = False

        For Each dtPickedChecking As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If dtPickedChecking.Sku = dtScreen.Sku And dtPickedChecking.Lottable10 = suspect Then
                bFound = True
                Exit For
            End If
        Next
        If bFound Then
            Return STATUS_VALID
        Else
            Return STATUS_INVALID
        End If

    End Function

    Private Function p_validateEmpty(suspect As Int16) As String
        Dim qty As Int16 = 0

        For Each dtPickedChecking As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            If dtPickedChecking.QR.Substring(0, EMPTY_BOX_NAME.Length).ToUpper() = EMPTY_BOX_NAME Then qty += dtPickedChecking.Qty
        Next
        If qty = suspect Then
            Return STATUS_VALID
        Else
            Return STATUS_INVALID
        End If
    End Function

    Private Function p_GetPickList(PalletID As String) As ArrayList
        Return New FG_PickedChecking().GetListByPalletId(PalletID, dtConnInfo)
    End Function

    Private Sub p_promoteReport()
        elapseStart = Now()
        Dim sOrderKey As String = String.Empty
        Dim sExternOrderKey As String = String.Empty
        Dim sStorerKey As String = String.Empty

        For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
            sOrderKey = data.OrderKey
            sExternOrderKey = data.ExternOrderKey
            sStorerKey = data.StorerKey
            Exit For
        Next
        If sOrderKey = String.Empty Then p_Main("ORDERKEY EMPTY", True)

        Dim dtPickDetailPalletCount As New PickDetail.DataPickDetail
        dtPickDetailPalletCount = New PickDetail().GetDataPalletCount(sExternOrderKey, dtConnInfoWms)

        Dim dtOrderDetailTotalQty As New Orders.DataOrderDetail
        dtOrderDetailTotalQty = New Orders().GetDataDetailTotalQty(sExternOrderKey, dtConnInfoWms)

        Dim dtScannedTotalQty As New FG_PickedChecking.DataFGPickedChecking
        If sender = RFUiList.FG_PICK_CHECK Then
            dtScannedTotalQty = New FG_PickedChecking().GetDataTotalQtyExclEmpty(sExternOrderKey, dtConnInfo)
        ElseIf sender = RFUiList.FG_LOAD_CHECK Then
            dtScannedTotalQty = New FG_PickedChecking().GetDataTotalQtyExclEmpty(sExternOrderKey, dtConnInfo, True)
        End If

        If dtScannedTotalQty.Qty = dtOrderDetailTotalQty.OriginalQty Then
            Dim sEvent As String = String.Empty
            Dim dtScanned As New FG_PickedChecking.DataFGPickedChecking
            If sender = RFUiList.FG_PICK_CHECK Then
                sEvent = "DISPATCH"
                dtScanned = New FG_PickedChecking().GetDataPalletCount(sExternOrderKey, dtConnInfo)
            ElseIf sender = RFUiList.FG_LOAD_CHECK Then
                sEvent = "INFO_SC"
                dtScanned = New FG_PickedChecking().GetDataPalletCount(sExternOrderKey, dtConnInfo, True)
            End If

            If dtPickDetailPalletCount.Qty = dtScanned.Qty Then
                Dim dtLog As New Log.DataLog
                dtLog.MenuID = eActiveUI.ToString()
                dtLog.Event = sEvent
                dtLog.Scan1 = sOrderKey
                dtLog.Scan2 = sExternOrderKey
                dtLog.Scan3 = sStorerKey
                dtLog.Status = "PROMOTE"
                dtLog.UserID = dtUserActive.UserId
                dtLog.StorerKey = sStorerKey
                SaveLog(dtLog)
            End If
        End If

        p_printElapse()
    End Sub

    Private Sub p_save()
        elapseStart = Now()
        Dim o As New FG_PickedChecking
        If sender = RFUiList.FG_PICK_CHECK Then
            'o.DeleteInsertList(dtScreen.alData)
            o.Save(dtScreen.alData, dtConnInfo)
        ElseIf sender = RFUiList.FG_LOAD_CHECK Then
            Dim data As New FG_PickedChecking.DataFGPickedChecking
            For Each dt As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
                data = dt
                Exit For
            Next

            o.UpdateLoadingByPalletId(data.TrailerNumber, data.PalletId, data.L_UserID, dtConnInfo)
        End If
        p_printElapse()
    End Sub

    'Private Sub p_save()
    '    elapseStart = Now()
    '    Dim o As New FG_PickedChecking
    '    If sender = RFUiList.FG_PICK_CHECK Then
    '        Dim dtPalletExists As New FG_PickedChecking.DataFGPickedChecking
    '        For Each data As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
    '            dtPalletExists = New FG_PickedChecking().CheckExistsPalletId(data.PalletId)
    '            Exit For
    '        Next

    '        If dtPalletExists.PalletId = String.Empty Then
    '            o.Save(dtScreen.alData)
    '        Else
    '            Dim dataOrderStatus As FG_PickDetail.DataFGPickDetail = New FG_PickDetail().GetOrderStatus(dtPalletExists.ExternOrderKey)
    '            If dataOrderStatus.Status <> FG_PickDetail.STATUS_ORDER_SHIPPED Then
    '                p_Main("ALREADY SAVED BY OTHERS", True)
    '            Else
    '                o.Save(dtScreen.alData)
    '            End If
    '        End If
    '    ElseIf sender = RFUiList.FG_LOAD_CHECK Then
    '        'o.UpdateLoading(dtScreen.alData)
    '        Dim data As New FG_PickedChecking.DataFGPickedChecking
    '        For Each dt As FG_PickedChecking.DataFGPickedChecking In dtScreen.alData
    '            data = dt
    '            Exit For
    '        Next

    '        o.UpdateLoadingByPalletId(data.TrailerNumber, data.PalletId, data.L_UserID)
    '    End If
    '    p_printElapse()
    'End Sub

#End Region
#Region "Data Class"
    Private Const STATUS_VALID = "VALID"
    Private Const STATUS_INVALID = "INVALID"
    Private Const EMPTY_BOX_NAME As String = "EMPTY"

    Public Class DataScreen
        Public StorerKey As String = String.Empty
        Public QrCode As String = String.Empty
        Public QrCode1 As String = String.Empty
        Public Status1 As String = String.Empty
        Public QrCode2 As String = String.Empty
        Public Status2 As String = String.Empty
        Public PalletId As String = String.Empty
        Public Sku As String = String.Empty
        Public Qty As Int32 = 0
        Public alData As New ArrayList
        Public bSpecialCasePalletLabelSame As Boolean = False
    End Class

    Private Enum ContenType
        PALLETID
        SKU
        QTY
        SERIAL
        EMPTYBOX
    End Enum

#End Region
End Module
