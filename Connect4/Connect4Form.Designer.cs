﻿namespace Connect4
{
    partial class Connect4Form
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
            this.plBoard = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // plBoard
            // 
            this.plBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plBoard.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.plBoard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.plBoard.Location = new System.Drawing.Point(12, 12);
            this.plBoard.Name = "plBoard";
            this.plBoard.Size = new System.Drawing.Size(500, 500);
            this.plBoard.TabIndex = 0;
            // 
            // Connect4Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 526);
            this.Controls.Add(this.plBoard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Connect4Form";
            this.Text = "Connect 4";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel plBoard;
    }
}

