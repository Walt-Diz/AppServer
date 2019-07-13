<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.LvReceived = New System.Windows.Forms.ListView()
        Me.ClmReceivedNum = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmExpectedBytes = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmReceivedTotalBytes = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmReceivedChunkID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmExpectedChunk = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmActualChunkBytes = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.LblReceivedLength = New System.Windows.Forms.Label()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.LblDecompressed = New System.Windows.Forms.Label()
        Me.LblCompressed = New System.Windows.Forms.Label()
        Me.LblDifference = New System.Windows.Forms.Label()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(525, 39)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(429, 323)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'LvReceived
        '
        Me.LvReceived.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ClmReceivedNum, Me.ClmExpectedBytes, Me.ClmReceivedChunkID, Me.ClmReceivedTotalBytes, Me.ClmExpectedChunk, Me.ClmActualChunkBytes})
        Me.LvReceived.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LvReceived.FullRowSelect = True
        Me.LvReceived.GridLines = True
        Me.LvReceived.Location = New System.Drawing.Point(12, 222)
        Me.LvReceived.MultiSelect = False
        Me.LvReceived.Name = "LvReceived"
        Me.LvReceived.Size = New System.Drawing.Size(507, 140)
        Me.LvReceived.TabIndex = 7
        Me.LvReceived.UseCompatibleStateImageBehavior = False
        Me.LvReceived.View = System.Windows.Forms.View.Details
        '
        'ClmReceivedNum
        '
        Me.ClmReceivedNum.Text = "#"
        Me.ClmReceivedNum.Width = 28
        '
        'ClmExpectedBytes
        '
        Me.ClmExpectedBytes.Text = "Buffer Bytes"
        Me.ClmExpectedBytes.Width = 77
        '
        'ClmReceivedTotalBytes
        '
        Me.ClmReceivedTotalBytes.Text = "Total Bytes"
        Me.ClmReceivedTotalBytes.Width = 81
        '
        'ClmReceivedChunkID
        '
        Me.ClmReceivedChunkID.Text = "Chunk #"
        '
        'ClmExpectedChunk
        '
        Me.ClmExpectedChunk.Text = "Expected Chunk Bytes"
        Me.ClmExpectedChunk.Width = 128
        '
        'ClmActualChunkBytes
        '
        Me.ClmActualChunkBytes.Text = "Actual Chunk Bytes"
        Me.ClmActualChunkBytes.Width = 114
        '
        'TextBox1
        '
        Me.TextBox1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(12, 39)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(507, 177)
        Me.TextBox1.TabIndex = 8
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(139, 10)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 9
        Me.Button1.Text = "Clear"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'LblReceivedLength
        '
        Me.LblReceivedLength.AutoSize = True
        Me.LblReceivedLength.Location = New System.Drawing.Point(386, 20)
        Me.LblReceivedLength.Name = "LblReceivedLength"
        Me.LblReceivedLength.Size = New System.Drawing.Size(52, 13)
        Me.LblReceivedLength.TabIndex = 15
        Me.LblReceivedLength.Text = "Length: 0"
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(17, 10)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(116, 23)
        Me.Button5.TabIndex = 13
        Me.Button5.Text = "Copy Received Data"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'LblDecompressed
        '
        Me.LblDecompressed.AutoSize = True
        Me.LblDecompressed.Location = New System.Drawing.Point(669, 23)
        Me.LblDecompressed.Name = "LblDecompressed"
        Me.LblDecompressed.Size = New System.Drawing.Size(90, 13)
        Me.LblDecompressed.TabIndex = 17
        Me.LblDecompressed.Text = "Decompressed: 0"
        '
        'LblCompressed
        '
        Me.LblCompressed.AutoSize = True
        Me.LblCompressed.Location = New System.Drawing.Point(522, 23)
        Me.LblCompressed.Name = "LblCompressed"
        Me.LblCompressed.Size = New System.Drawing.Size(77, 13)
        Me.LblCompressed.TabIndex = 16
        Me.LblCompressed.Text = "Compressed: 0"
        '
        'LblDifference
        '
        Me.LblDifference.AutoSize = True
        Me.LblDifference.Location = New System.Drawing.Point(819, 23)
        Me.LblDifference.Name = "LblDifference"
        Me.LblDifference.Size = New System.Drawing.Size(68, 13)
        Me.LblDifference.TabIndex = 18
        Me.LblDifference.Text = "Difference: 0"
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(966, 371)
        Me.Controls.Add(Me.LblDifference)
        Me.Controls.Add(Me.LblDecompressed)
        Me.Controls.Add(Me.LblCompressed)
        Me.Controls.Add(Me.LblReceivedLength)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.LvReceived)
        Me.Controls.Add(Me.PictureBox1)
        Me.Name = "FrmMain"
        Me.Text = "Server"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents LvReceived As ListView
    Friend WithEvents ClmReceivedNum As ColumnHeader
    Friend WithEvents ClmExpectedBytes As ColumnHeader
    Friend WithEvents ClmReceivedTotalBytes As ColumnHeader
    Friend WithEvents ClmReceivedChunkID As ColumnHeader
    Friend WithEvents ClmExpectedChunk As ColumnHeader
    Friend WithEvents ClmActualChunkBytes As ColumnHeader
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents LblReceivedLength As Label
    Friend WithEvents Button5 As Button
    Friend WithEvents LblDecompressed As Label
    Friend WithEvents LblCompressed As Label
    Friend WithEvents LblDifference As Label
End Class
