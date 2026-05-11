namespace TextExtractor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnBrowse = new Button();
            btnExtract = new Button();
            tbxOutput = new TextBox();
            tbxpath = new TextBox();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(39, 30);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(150, 34);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Browse Pdf";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnExtract
            // 
            btnExtract.Location = new Point(617, 30);
            btnExtract.Name = "btnExtract";
            btnExtract.Size = new Size(137, 34);
            btnExtract.TabIndex = 1;
            btnExtract.Text = "Extract Text ";
            btnExtract.UseVisualStyleBackColor = true;
            btnExtract.Click += btnExtract_Click;
            // 
            // tbxOutput
            // 
            tbxOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbxOutput.Location = new Point(39, 83);
            tbxOutput.Multiline = true;
            tbxOutput.Name = "tbxOutput";
            tbxOutput.ScrollBars = ScrollBars.Vertical;
            tbxOutput.Size = new Size(715, 340);
            tbxOutput.TabIndex = 2;
            tbxOutput.Visible = false;
            // 
            // tbxpath
            // 
            tbxpath.Location = new Point(205, 30);
            tbxpath.Name = "tbxpath";
            tbxpath.Size = new Size(406, 31);
            tbxpath.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tbxpath);
            Controls.Add(tbxOutput);
            Controls.Add(btnExtract);
            Controls.Add(btnBrowse);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowse;
        private Button btnExtract;
        private TextBox tbxOutput;
        private TextBox tbxpath;
    }
}
