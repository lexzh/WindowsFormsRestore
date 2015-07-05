namespace WindowsFormsRestore
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.chk_Cover = new System.Windows.Forms.CheckBox();
            this.chk_h2d = new System.Windows.Forms.CheckBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // chk_Cover
            // 
            this.chk_Cover.AutoSize = true;
            this.chk_Cover.Checked = true;
            this.chk_Cover.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Cover.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.chk_Cover.Location = new System.Drawing.Point(326, 65);
            this.chk_Cover.Margin = new System.Windows.Forms.Padding(2);
            this.chk_Cover.Name = "chk_Cover";
            this.chk_Cover.Size = new System.Drawing.Size(74, 17);
            this.chk_Cover.TabIndex = 6;
            this.chk_Cover.Text = "覆盖文件";
            this.chk_Cover.UseVisualStyleBackColor = true;
            // 
            // chk_h2d
            // 
            this.chk_h2d.AutoSize = true;
            this.chk_h2d.Checked = true;
            this.chk_h2d.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_h2d.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.chk_h2d.Location = new System.Drawing.Point(326, 30);
            this.chk_h2d.Margin = new System.Windows.Forms.Padding(2);
            this.chk_h2d.Name = "chk_h2d";
            this.chk_h2d.Size = new System.Drawing.Size(122, 17);
            this.chk_h2d.TabIndex = 7;
            this.chk_h2d.Text = "十六进制转十进制";
            this.chk_h2d.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(326, 200);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(92, 64);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(1, 1);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(311, 284);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 287);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.chk_Cover);
            this.Controls.Add(this.chk_h2d);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chk_Cover;
        private System.Windows.Forms.CheckBox chk_h2d;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

