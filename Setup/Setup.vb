Imports System.Xml
Imports System.IO
Imports System.Text
Imports AIT.RF
Imports AIT.SM

Module Setup
    Dim iYPosition As Integer = 0
    Const USER As String = "AdminIt"
    Const PASS As String = "agility1+"
    Const PREFIX_CMD_SETUP As String = "setup:>"
    Const PREFIX_CMD_SETUP_DATABASE As String = "setup\database:>"
    Const PREFIX_CMD_SETUP_USER As String = "setup\user:>"
    Const CMD_QUESTION As String = "/?"
    Const CMD_SETUP_DB As String = "database"
    Const CMD_SETUP_User As String = "user"
    Const CMD_EXIT As String = "exit"
    Const CMD_END As String = "end"
    Const CMD_QUIT As String = "quit"
    Const CMD_LOGOFF As String = "logoff"
    Dim sRequest As String = String.Empty
    Const XML_EXT As String = ".xml"
    Dim fileConnectionString As String = "ConnectionString.xml"
    Dim dtConnInfo As New DataConnectionInfo

    Sub Main()
        iYPosition = 0
        Console.Clear()
        Console.ResetColor()
        p_welcome()
        If p_Login() Then p_setup()
    End Sub

    Private Sub p_welcome()
        Console.BackgroundColor = ConsoleColor.DarkBlue
        p_printLine("************ RF Setup ************")
        p_printLine(" copyright AIT (Agility IT) @2019 ")
        p_printLine("==================================")
        Console.ResetColor()
        p_printLine(String.Empty)
        p_printLine("Login Required")
        p_printLine(String.Empty)
    End Sub

    Private Function p_Login() As Boolean
restartUserId:
        p_printLine("USER Id:>")
        Dim sUser As String = p_getInput()
        If sUser.Trim = String.Empty Then GoTo restartUserId
restartPass:
        p_printLine("Password:>")
        Dim sPass As String = p_getInputPass(sRequest.Length + 1, iYPosition)
        If sPass.Trim = String.Empty Then GoTo restartPass

        If String.Compare(sUser, USER) = 0 And String.Compare(sPass, PASS) = 0 Then
            p_printLine("i> Login succses")
            Return True
        Else
            p_printLine("e> Login fail!")
            GoTo restartUserId
        End If
    End Function

    Private Sub p_setup()
restartSetup:
        Dim sRoot As String = String.Empty
        p_printLine(PREFIX_CMD_SETUP)
        sRoot = PREFIX_CMD_SETUP

        Select Case p_getInput(Len(PREFIX_CMD_SETUP) + 1, iYPosition)
            Case CMD_QUESTION
                p_cmdQuestion(sRoot)
            Case CMD_SETUP_DB
                p_cmdDatabase()
            Case CMD_SETUP_User
                p_cmdUser()
            Case CMD_END
                End
            Case CMD_EXIT
                End
            Case CMD_QUIT
                End
            Case CMD_LOGOFF
                Main()
            Case Else
                p_printLine("e> Unrecoqnize command")
        End Select

        GoTo restartSetup
    End Sub

    Private Sub p_cmdQuestion(sRoot As String)
        If sRoot = PREFIX_CMD_SETUP Then
            p_printLine(CMD_SETUP_DB)
            p_printLine(CMD_SETUP_User)
        End If
    End Sub

    Private Sub p_cmdDatabase()
        Dim data As New SM.DataConnectionInfo
        p_printLine(PREFIX_CMD_SETUP_DATABASE)
restartDataSource:
        p_printLine("Data Source:")
        data.DataSource = p_getInput()
        If data.DataSource.Trim = String.Empty Then GoTo restartDataSource
        dtConnInfo.DataSource = data.DataSource
restartInitialCatalog:
        p_printLine("Initial Catalog:")
        data.InitialCatalog = p_getInput()
        If data.InitialCatalog.Trim = String.Empty Then GoTo restartInitialCatalog
        dtConnInfo.InitialCatalog = data.InitialCatalog
restartUserId:
        p_printLine("User Id:")
        data.UserId = p_getInput()
        If data.UserId.Trim = String.Empty Then GoTo restartUserId
        dtConnInfo.UserId = data.UserId
restartPassword:
        p_printLine("Password:")
        data.Password = p_getInputPass(sRequest.Length + 1, iYPosition)
        If data.Password.Trim = String.Empty Then GoTo restartPassword
        dtConnInfo.Password = data.Password

        p_printLine("Save to file [empty=default name] (exclude file ext):")
        Dim sInput As String = p_getInput()
        If Not sInput = String.Empty Then fileConnectionString = String.Format("{0}{1}", sInput, XML_EXT)

        If p_testConnection(data) Then
            p_printLine("q> Save? (y/n)")
            If p_getInput() = "y" Then
                p_saveConnectionInfo(data)
            End If
        End If

        p_setup()
    End Sub

    Private Sub p_saveConnectionInfo(data As SM.DataConnectionInfo)
        Try
            p_CreateXml(data)
        Catch ex As Exception
            p_printLine("e> Setup Connection Info is fail for following error")
            p_printLine(ex.Message)
        End Try

        p_printLine("i> Connection Info is successfully updated")
    End Sub

    Private Function p_testConnection(data As SM.DataConnectionInfo) As Boolean
        Dim bTestSuccess As Boolean = False
        Try
            Dim DM As New SM.DatabaseManager
            bTestSuccess = DM.TestConnection(data)
            p_printLine("i> Connection test is success")
        Catch ex As Exception
            p_printLine("e> Connection test is fail for following error")
            p_printLine(ex.Message)
        End Try

        Return bTestSuccess
    End Function

    Private Sub p_cmdUser()
        p_printLine(PREFIX_CMD_SETUP_USER)

        Dim inputData As New SM.UserManager.DataUser
        inputData.Password = "000000"
        inputData.bChangePassword = True
        inputData.bEnabled = True

restartId:
        p_printLine("User Id:")
        inputData.UserId = p_getInput()
        If inputData.UserId.Trim = String.Empty Then GoTo restartId
        Dim data As SM.UserManager.DataUser = p_checNGetkUser(inputData.UserId)

restartName:
        p_printLine("User Name:")
        inputData.UserName = p_getInput()
        If inputData.UserName.Trim = String.Empty Then
            If data.UserName.Trim = String.Empty Then
                GoTo restartId
            Else
                inputData.UserName = data.UserName
                p_printLine(1, iYPosition, String.Format("{0} {1}", "User Name:", data.UserName))
            End If
        End If

restartStorer:
        p_printLine("StorerKey (use ';' as separator):")
        Dim sStorerKey = p_getInput()
        If inputData.StorerKey.Trim = String.Empty Then
            If data.StorerKey.Trim = String.Empty Then
                GoTo restartId
            Else
                inputData.StorerKey = data.StorerKey
                p_printLine(1, iYPosition, String.Format("{0} {1}", "StorerKey (use ';' as separator):", data.StorerKey))
            End If
        End If

        p_printLine("q> Proceed generate user? (y/n)")
        If p_getInput() = "y" Then
            p_saveUser(inputData)
        End If

        p_setup()
    End Sub

    Private Sub p_saveUser(data As SM.UserManager.DataUser)
        Try
            Dim UM As New SM.UserManager
            UM.Save(data, dtconninfo)

            p_printLine("i> User is successfully updated to default PIN = 000000 (6 digit of 0) and user must change PIN when login")
        Catch ex As Exception
            p_printLine("e> Save user data is fail for following error")
            p_printLine(ex.Message)
        End Try
    End Sub

    Private Function p_checNGetkUser(sUserId As String) As SM.UserManager.DataUser
        Dim data As New SM.UserManager.DataUser
        Try
            Dim UM As New SM.UserManager
            data = UM.GetData(sUserId, dtConnInfo)
            If data.UserId = String.Empty Then
                p_printLine("i> User Id not found. Continue to create new user.")
            Else
                p_printLine(String.Format("{0} {1}", "i> User Name :", data.UserName))
                p_printLine(String.Format("{0} {1}", "i> StorerKey :", data.StorerKey))
                p_printLine("i> User Id found. Continue to reset password and activate.")
            End If
        Catch ex As Exception
            p_printLine("e> Checking user data is fail for following error")
            p_printLine(ex.Message)
        End Try

        Return data
    End Function

    Private Function p_getInput() As String
        Console.SetCursorPosition(sRequest.Length + 1, iYPosition)
        Return Console.ReadLine()
    End Function

    Private Function p_getInput(iLeft As Integer, iTop As Integer) As String
        Console.SetCursorPosition(iLeft, iTop)
        Return Console.ReadLine()
    End Function

    Private Function p_getInputPass(iLeft As Integer, iTop As Integer) As String
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
                    Return String.Empty
                Case ConsoleKey.Backspace
                    If sInput.Length > 1 Then
                        sInput = sInput.Substring(0, sInput.Length - 1)
                        iCurLeft -= 1
                    Else
                        iCurLeft = 1
                        sInput = String.Empty
                    End If
                Case Else
                    If vKey.KeyChar = vbNullChar Then
                        If sInput.Length > 1 Then
                            sInput = sInput.Substring(0, sInput.Length - 1)
                            iCurLeft -= 1
                        Else
                            iCurLeft = 1
                            sInput = String.Empty
                        End If
                    Else
                        sInput += vKey.KeyChar
                        iCurLeft += 1
                    End If
            End Select

            p_printLine(iLeft, iTop, New String("*", sInput.Length))
            Console.SetCursorPosition(iCurLeft, iTop)
        Loop

        Return sInput
    End Function

    Private Sub p_printLine(iLeft As Integer, iTop As Integer, sMessage As String)
        Console.SetCursorPosition(iLeft, iTop)
        Console.WriteLine(sMessage)
    End Sub

    Private Sub p_printLine(iLeft As Integer, sMessage As String)
        iYPosition += 1
        p_printLine(iLeft, iYPosition, sMessage)
    End Sub

    Private Sub p_printLine(sMessage As String)
        sRequest = sMessage
        iYPosition += 1
        p_printLine(1, iYPosition, sMessage)
    End Sub

    Private Sub p_CreateXml(data As SM.DataConnectionInfo)
        Dim writer As New XmlTextWriter(fileConnectionString, System.Text.Encoding.UTF8)
        writer.WriteStartDocument(True)
        writer.Formatting = Formatting.Indented
        writer.Indentation = 2
        writer.WriteStartElement("ConnectionString")

        writer.WriteStartElement("DataSource")
        writer.WriteString(data.DataSource)
        writer.WriteEndElement()
        writer.WriteStartElement("InitialCatalog")
        writer.WriteString(data.InitialCatalog)
        writer.WriteEndElement()
        writer.WriteStartElement("UserId")
        writer.WriteString(data.UserId)
        writer.WriteEndElement()
        writer.WriteStartElement("Password")
        writer.WriteString(data.Password)
        writer.WriteEndElement()

        writer.WriteEndElement()
        writer.WriteEndDocument()
        writer.Close()

        p_printLine("i> Generate file is success")
        p_encryptFile(fileConnectionString)
    End Sub

    Private Sub p_encryptFile(sFilename As String)
        Dim SM As New SM.SystemManager
        Try
            Dim sXml As String = My.Computer.FileSystem.ReadAllText(sFilename)
            Using sr As New StreamWriter(sFilename)
                sr.Write(SM.Encrypt(sXml))
            End Using
        Catch ex As Exception
            p_printLine("e> Encryption is fail!")
        End Try

        p_printLine("i> Encryption is success")
    End Sub
End Module
