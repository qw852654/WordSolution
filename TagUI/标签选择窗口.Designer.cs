namespace TagUI
{
    partial class 标签选择窗口
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.退出按钮 = new System.Windows.Forms.Button();
            this.确定按钮 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.退出按钮);
            this.splitContainer1.Panel2.Controls.Add(this.确定按钮);
            this.splitContainer1.Size = new System.Drawing.Size(885, 631);
            this.splitContainer1.SplitterDistance = 566;
            this.splitContainer1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(885, 566);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // 退出按钮
            // 
            this.退出按钮.Location = new System.Drawing.Point(395, 3);
            this.退出按钮.Name = "退出按钮";
            this.退出按钮.Size = new System.Drawing.Size(106, 46);
            this.退出按钮.TabIndex = 1;
            this.退出按钮.Text = "退出";
            this.退出按钮.UseVisualStyleBackColor = true;
            this.退出按钮.Click += new System.EventHandler(this.退出按钮_Click);
            // 
            // 确定按钮
            // 
            this.确定按钮.Location = new System.Drawing.Point(218, 3);
            this.确定按钮.Name = "确定按钮";
            this.确定按钮.Size = new System.Drawing.Size(106, 46);
            this.确定按钮.TabIndex = 0;
            this.确定按钮.Text = "确定";
            this.确定按钮.UseVisualStyleBackColor = true;
            this.确定按钮.Click += new System.EventHandler(this.确定按钮_Click);
            // 
            // 标签选择窗口
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 631);
            this.Controls.Add(this.splitContainer1);
            this.Name = "标签选择窗口";
            this.Text = "标签选择窗口";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button 退出按钮;
        private System.Windows.Forms.Button 确定按钮;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}