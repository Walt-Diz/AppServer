<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.TxtRaw = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.LvSent = New System.Windows.Forms.ListView()
        Me.ClmSentNum = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmBytesSent = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmSentChunkSize = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmSentChunkID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ClmSentBufferBytes = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.LblSentLength = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Button5 = New System.Windows.Forms.Button()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TxtRaw
        '
        Me.TxtRaw.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtRaw.Location = New System.Drawing.Point(12, 38)
        Me.TxtRaw.Multiline = True
        Me.TxtRaw.Name = "TxtRaw"
        Me.TxtRaw.Size = New System.Drawing.Size(434, 201)
        Me.TxtRaw.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(452, 341)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(109, 23)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "Send Image"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(567, 341)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(116, 23)
        Me.Button2.TabIndex = 3
        Me.Button2.Text = "Send Packet"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'LvSent
        '
        Me.LvSent.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ClmSentNum, Me.ClmSentBufferBytes, Me.ClmSentChunkID, Me.ClmBytesSent, Me.ClmSentChunkSize})
        Me.LvSent.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LvSent.FullRowSelect = True
        Me.LvSent.GridLines = True
        Me.LvSent.Location = New System.Drawing.Point(12, 245)
        Me.LvSent.MultiSelect = False
        Me.LvSent.Name = "LvSent"
        Me.LvSent.Size = New System.Drawing.Size(434, 140)
        Me.LvSent.TabIndex = 4
        Me.LvSent.UseCompatibleStateImageBehavior = False
        Me.LvSent.View = System.Windows.Forms.View.Details
        '
        'ClmSentNum
        '
        Me.ClmSentNum.Text = "#"
        Me.ClmSentNum.Width = 33
        '
        'ClmBytesSent
        '
        Me.ClmBytesSent.Text = "Total Bytes"
        Me.ClmBytesSent.Width = 106
        '
        'ClmSentChunkSize
        '
        Me.ClmSentChunkSize.Text = "Chunk Bytes"
        Me.ClmSentChunkSize.Width = 109
        '
        'ClmSentChunkID
        '
        Me.ClmSentChunkID.Text = "Chunk #"
        Me.ClmSentChunkID.Width = 58
        '
        'ClmSentBufferBytes
        '
        Me.ClmSentBufferBytes.Text = "Buffer Bytes"
        Me.ClmSentBufferBytes.Width = 78
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(134, 12)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(75, 23)
        Me.Button3.TabIndex = 7
        Me.Button3.Text = "Clear"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(12, 12)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(116, 23)
        Me.Button4.TabIndex = 8
        Me.Button4.Text = "Copy Sent Data"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'LblSentLength
        '
        Me.LblSentLength.AutoSize = True
        Me.LblSentLength.Location = New System.Drawing.Point(215, 22)
        Me.LblSentLength.Name = "LblSentLength"
        Me.LblSentLength.Size = New System.Drawing.Size(52, 13)
        Me.LblSentLength.TabIndex = 10
        Me.LblSentLength.Text = "Length: 0"
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(452, 12)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(429, 323)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox1.TabIndex = 11
        Me.PictureBox1.TabStop = False
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(689, 341)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(109, 23)
        Me.Button5.TabIndex = 12
        Me.Button5.Text = "Stream Window"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(891, 393)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.LblSentLength)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.LvSent)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TxtRaw)
        Me.Name = "FrmMain"
        Me.Text = "Client"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TxtRaw As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents LvSent As ListView
    Friend WithEvents ClmSentNum As ColumnHeader
    Friend WithEvents ClmBytesSent As ColumnHeader
    Friend WithEvents ClmSentChunkSize As ColumnHeader
    Friend WithEvents ClmSentChunkID As ColumnHeader
    Friend WithEvents ClmSentBufferBytes As ColumnHeader
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents LblSentLength As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Button5 As Button
End Class
