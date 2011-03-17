namespace calculator
{
  partial class Calculator
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
      this.calc_screen = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // calc_screen
      // 
      this.calc_screen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.calc_screen.Cursor = System.Windows.Forms.Cursors.Arrow;
      this.calc_screen.Location = new System.Drawing.Point(11, 10);
      this.calc_screen.Multiline = true;
      this.calc_screen.Name = "calc_screen";
      this.calc_screen.ReadOnly = true;
      this.calc_screen.Size = new System.Drawing.Size(274, 77);
      this.calc_screen.TabIndex = 0;
      // 
      // Calculator
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(295, 362);
      this.Controls.Add(this.calc_screen);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.Name = "Calculator";
      this.Text = "Calculator";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox calc_screen;

  }
}

