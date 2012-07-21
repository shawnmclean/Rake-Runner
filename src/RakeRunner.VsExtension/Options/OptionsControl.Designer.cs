namespace RakeRunner.VsExtension
{
    partial class OptionsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Txt_RakePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_BrowseRakeFile = new System.Windows.Forms.Button();
            this.FileDiag_RakePath = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // Txt_RakePath
            // 
            this.Txt_RakePath.Location = new System.Drawing.Point(6, 27);
            this.Txt_RakePath.Name = "Txt_RakePath";
            this.Txt_RakePath.Size = new System.Drawing.Size(230, 20);
            this.Txt_RakePath.TabIndex = 0;
            this.Txt_RakePath.TextChanged += new System.EventHandler(this.Txt_RakePath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Path to Rake file (rake.bat)";
            // 
            // btn_BrowseRakeFile
            // 
            this.btn_BrowseRakeFile.Location = new System.Drawing.Point(242, 25);
            this.btn_BrowseRakeFile.Name = "btn_BrowseRakeFile";
            this.btn_BrowseRakeFile.Size = new System.Drawing.Size(75, 23);
            this.btn_BrowseRakeFile.TabIndex = 3;
            this.btn_BrowseRakeFile.Text = "Browse ...";
            this.btn_BrowseRakeFile.UseVisualStyleBackColor = true;
            this.btn_BrowseRakeFile.Click += new System.EventHandler(this.btn_BrowseRakeFile_Click);
            // 
            // FileDiag_RakePath
            // 
            this.FileDiag_RakePath.FileName = "rake.bat";
            // 
            // OptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_BrowseRakeFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Txt_RakePath);
            this.Name = "OptionsControl";
            this.Size = new System.Drawing.Size(602, 414);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Txt_RakePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_BrowseRakeFile;
        private System.Windows.Forms.OpenFileDialog FileDiag_RakePath;
    }
}
