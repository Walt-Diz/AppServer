Option Strict On
Option Explicit On

Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Text

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
    Public Delegate Sub SetPictureBoxImageDelegate(ByVal PictureBox As PictureBox, ByVal Image As Image)
    Public Sub SetPictureBoxImage(ByVal PictureBox As PictureBox, ByVal Image As Image)
        If Me.InvokeRequired Then
            Me.Invoke(New SetPictureBoxImageDelegate(AddressOf SetPictureBoxImage), PictureBox, Image)
        Else
            PictureBox.Image = Image
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
    Public ServerPort As Integer = 9090
    Public ServerBacklog As Integer = 500
    Public ServerSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    Public ServerBufferSize As Integer = 1023
    Public ServerBuffer(ServerBufferSize) As Byte

    Public Sub Start(ByVal Port As Integer, ByVal Backlog As Integer)
        Try
            ServerSocket.Bind(New IPEndPoint(IPAddress.Any, Port))
            ServerSocket.Listen(Backlog)
            ServerSocket.BeginAccept(New AsyncCallback(AddressOf AcceptedCallback), Nothing)
        Catch
            System.Threading.Thread.Sleep(5000)
            Start(Port, Backlog)
        End Try
    End Sub

    Public Sub StartConnection(ByVal Port As Integer, ByVal Backlog As Integer)
        Start(Port, Backlog)
        While True
            System.Threading.Thread.Sleep(50)
        End While
    End Sub

    Public Sub AcceptedCallback(ByVal Result As IAsyncResult)
        Dim ClientSocket As Socket = ServerSocket.EndAccept(Result)
        ReDim ServerBuffer(ServerBufferSize)
        ClientSocket.BeginReceive(ServerBuffer, 0, ServerBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf ReceivedCallback), ClientSocket)
        ServerSocket.BeginAccept(New AsyncCallback(AddressOf AcceptedCallback), Nothing)
        Dim RandomString As String = GetRandomString(GetRandomNumber(2000, 2500), GetRandomNumber(3000, 3500), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        Dim RandomStringBytes As Byte() = UTF8Encoder.GetBytes(RandomString)
        SendChunkedPacket(ClientSocket, RandomStringBytes)
        'AppendTextBoxText(TxtRaw, String.Join(", ", RandomStringBytes))
        'SetLabelText(LblSentLength, String.Format("Length: {0}", CType(RandomStringBytes.Length, String)))
        'AppendTextBoxText(TxtRaw, String.Format("Length: {0}", RandomString.Length))
    End Sub

    Public Sub ReceivedCallback(ByVal Result As IAsyncResult)
        Dim ClientSocket As Socket = CType(Result.AsyncState, Socket)
        Dim SocketError As SocketError
        Dim BufferSize As Integer = ClientSocket.EndReceive(Result, SocketError)
        If SocketError.Equals(SocketError.Success) Then
            Dim Packet(BufferSize) As Byte
            Buffer.BlockCopy(ServerBuffer, 0, Packet, 0, BufferSize)

            HandlePacket(Packet)

            ReDim ServerBuffer(ServerBufferSize)
            ClientSocket.BeginReceive(ServerBuffer, 0, ServerBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf ReceivedCallback), ClientSocket)
        ElseIf SocketError.Equals(SocketError.ConnectionRefused) Then
            'System.Threading.Thread.Sleep(5000)
            'Connect(Server_IP, Server_Port)
        ElseIf SocketError.Equals(SocketError.ConnectionAborted) Then
            'System.Threading.Thread.Sleep(5000)
            'Connect(Server_IP, Server_Port)
        ElseIf SocketError.Equals(SocketError.ConnectionReset) Then
            'System.Threading.Thread.Sleep(5000)
            'Connect(Server_IP, Server_Port)
        End If
    End Sub

    Public Function ReadBytes(ByVal Bytes As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As String
        Return UTF8Encoder.GetString(Bytes, Offset, Count)
    End Function
    Public Function ReturnBytes(ByVal Bytes As Byte(), ByVal Offset As Integer, ByVal Count As Integer) As Byte()
        Dim ByteArray(Count) As Byte
        Buffer.BlockCopy(Bytes, Offset, ByteArray, 0, Count)
        Return ByteArray
    End Function

    Private Function ByteArrayToString(ByVal Bytes As Byte()) As String
        Dim ReturnString As String = String.Empty
        For I = 0 To Bytes.Length - 1
            ReturnString += Chr(Bytes(I))
        Next
        Return ReturnString
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
        'Dim PacketChunk As Byte


        AddPacketBytes(ChunkLength, PacketLength, Chunk, Chunks, PacketType, PacketID, PacketData, PacketBytes)
        'AddListViewItem(LvReceived, CType(LvReceived.Items.Count, String), New String() {CType(Packet.Length, String), CType(Chunk, String), CType(PacketLength, String), CType(ChunkLength, String), CType(PacketBytes.Length, String)})


        If LastChunk(Chunk, Chunks) Then
            Dim ChunkPacket As SocketPacket = SockPacket(PacketID)
            'AppendTextBoxText(TxtRaw, String.Format("Data Length: {0}", ChunkPacket.PacketData.Length))
            'AppendTextBoxText(TxtRaw, String.Format("Byte Length: {0}", ChunkPacket.PacketBytes.Count))
            Select Case ChunkPacket.PacketType
                Case 10
                    'Dim RandomString As String = ReadBytes(ReconstructedPacket, 0, ReconstructedPacket.Length)
                    'Dim ChunkBytes() As Byte = ChunkPacket.PacketBytes.ToArray()
                    'Dim ChunkData As String = UTF8Encoder.GetString(ChunkBytes)
                    'Dim ChunkData2 As String = ByteArrayToString(ChunkBytes)
                    'AppendTextBoxText(TxtRaw, String.Format("{0}Raw Bytes: {1}{2}{3}", vbNewLine, vbNewLine, vbNewLine, String.Join(", ", ChunkPacket.PacketBytes.ToArray())))
                    'AppendTextBoxText(TxtRaw, String.Format("{0}Combined: {1}{2}{3}", vbNewLine, vbNewLine, vbNewLine, ChunkPacket.PacketData))
                    'AppendTextBoxText(TxtRaw, String.Format("{0}Reconstructed: {1}{2}{3}", vbNewLine, vbNewLine, vbNewLine, ChunkData))
                    'AppendTextBoxText(TxtRaw, String.Format("{0}Reconstructed: {1}{2}{3}", vbNewLine, vbNewLine, vbNewLine, ChunkData2))
                    'Dim RebuiltArray As Byte() = RebuildArray(ChunkPacket.PacketBytes.ToArray)
                    Dim RebuiltString As String = UTF8Encoder.GetString(ChunkPacket.PacketBytes.ToArray)
                    AppendTextBoxText(TextBox1, RebuiltString)
                    SetLabelText(LblReceivedLength, String.Format("Length: {0}", RebuiltString.Length.ToString("N0")))
                    'AppendTextBoxText(TextBox1, String.Join(", ", RebuiltArray))
                    'SetLabelText(LblReceivedLength, String.Format("Length: {0}", CType(RebuiltArray.Length, String)))
                Case 200
                    Dim CompressedBytes As Byte() = ChunkPacket.PacketBytes.ToArray
                    Dim DecompressedBytes As Byte() = Decompress(CompressedBytes)
                    'Dim RebuiltArray As Byte() = RebuildArray(ChunkPacket.PacketBytes.ToArray)
                    Dim Image As Image = ConvertBytesToImage(DecompressedBytes)
                    SetPictureBoxImage(PictureBox1, Image)
                    'AppendTextBoxText(TextBox1, String.Join(", ", CompressedBytes))
                    SetLabelText(LblReceivedLength, String.Format("Length: {0}", ChunkPacket.PacketBytes.ToArray.Length.ToString("N0")))
                    SetLabelText(LblCompressed, String.Format("Compressed: {0}", ChunkPacket.PacketBytes.ToArray.Length.ToString("N0")))
                    SetLabelText(LblDecompressed, String.Format("Decompressed: {0}", DecompressedBytes.Length.ToString("N0")))
                    Dim Difference As Integer = DecompressedBytes.Length - CompressedBytes.Length
                    SetLabelText(LblDifference, String.Format("Difference: {0}", Difference.ToString("N0")))

            End Select
            DeletePacket(ChunkPacket)
        Else
            Select Case PacketType
                Case 900 'Keylogger | Update Client in realtime as data is sent
            End Select
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
        Dim Chunks As Integer = CType(Math.Ceiling(Bytes.Length / ((ServerBufferSize + 1) - HeaderBytes)), Integer)

        Dim BytesRead As Integer = 0

        For I As Integer = 1 To Chunks

            Dim BufferLength As Integer = ServerBufferSize - HeaderBytes
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

    Public Sub SendChunkedPacket(ByVal Socket As Socket, ByVal Bytes As Byte())
        Dim PacketChunks As List(Of Byte()) = ReturnChunks(100, Bytes)
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

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim T As New System.Threading.Thread(Sub() StartConnection(ServerPort, ServerBacklog))
        With T
            .IsBackground = True
            .Start()
        End With
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
        Image.Save(MemStream, System.Drawing.Imaging.ImageFormat.Jpeg)
        Return MemStream.GetBuffer()
    End Function

    Public Function ConvertBytesToImage(ByVal BitmapBytes As Byte()) As Image
        Dim MemStream As New IO.MemoryStream(BitmapBytes)
        Dim Image As Image = CType(Drawing.Image.FromStream(MemStream), Image)
        Return Image
    End Function

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

#Region "Packet Compression"
    'Public Shared Function Compress(ByVal Bytes As Byte()) As Byte()
    '    Dim Stream As New IO.MemoryStream()
    '    Dim ZipStream As New System.IO.Compression.DeflateStream(Stream, System.IO.Compression.CompressionMode.Compress, True)
    '    ZipStream.Write(Bytes, 0, Bytes.Length)
    '    ZipStream.Close()
    '    Return Stream.ToArray()
    'End Function
    'Public Shared Function Decompress(ByVal Bytes As Byte()) As Byte()
    '    Dim Stream = New IO.MemoryStream()
    '    Dim ZipStream = New System.IO.Compression.DeflateStream(New IO.MemoryStream(Bytes), System.IO.Compression.CompressionMode.Decompress, True)
    '    Dim DecompressBuffer(4095) As Byte
    '    While True
    '        Dim Size = ZipStream.Read(DecompressBuffer, 0, DecompressBuffer.Length)
    '        If Size > 0 Then
    '            Stream.Write(DecompressBuffer, 0, Size)
    '        Else
    '            Exit While
    '        End If
    '    End While
    '    ZipStream.Close()
    '    Return Stream.ToArray()
    'End Function

    Public Function Compress(ByVal Bytes As Byte()) As Byte()
        'Get the stream of the source file.
        Using InputStream As IO.MemoryStream = New IO.MemoryStream(Bytes)
            'Create the compressed stream.
            Using OutputStream As IO.MemoryStream = New IO.MemoryStream()
                Using CompressionStream As New System.IO.Compression.DeflateStream(OutputStream, System.IO.Compression.CompressionMode.Compress)
                    ' Copy the source file into the compression stream.
                    InputStream.CopyTo(CompressionStream)
                End Using
                Compress = OutputStream.ToArray()
            End Using
        End Using
    End Function
    Public Function Decompress(ByVal Bytes As Byte()) As Byte()
        ' Get the stream of the source file.
        Using InputStream As IO.MemoryStream = New IO.MemoryStream(Bytes)
            ' Create the decompressed stream.
            Using OutputStream As IO.MemoryStream = New IO.MemoryStream()
                Using DecompressionStream As New System.IO.Compression.DeflateStream(InputStream, System.IO.Compression.CompressionMode.Decompress)
                    ' Copy the decompression stream
                    '  into the output file.
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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'LvSent.Items.Clear()
        LvReceived.Items.Clear()
        'TxtRaw.Clear()
        TextBox1.Clear()
        LblReceivedLength.Text = "Length: 0"
        'LblSentLength.Text = "Length: 0"
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs)
        'My.Computer.Clipboard.SetText(TxtRaw.Text)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        My.Computer.Clipboard.SetText(TextBox1.Text)
    End Sub


    'Public Function BytesToString(ByVal Bytes As Byte()) As String
    '    Return Encoding.UTF8.GetString(Bytes)
    'End Function

    'Public Function StringToBytes(ByVal Text As String) As Byte()
    '    Return Encoding.UTF8.GetBytes(Text)
    'End Function
    Public Function RebuildArray(ByVal ByteArray As Byte()) As Byte()
        Dim ByteArrayList As New List(Of Byte)
        For Each Bit As Byte In ByteArray
            If Not Bit.Equals(CType(0, Byte)) Then
                ByteArrayList.Add(Bit)
            End If
        Next
        Return ByteArrayList.ToArray
    End Function


End Class
