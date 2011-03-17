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
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // calc_screen
      // 
      this.calc_screen.Location = new System.Drawing.Point(11, 17);
      this.calc_screen.Multiline = true;
      this.calc_screen.Name = "calc_screen";
      this.calc_screen.Size = new System.Drawing.Size(274, 52);
      this.calc_screen.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(39, 140);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(201, 71);
      this.button1.TabIndex = 1;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // Calculator
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(291, 362);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.calc_screen);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "Calculator";
      this.Text = "Calculator";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox calc_screen;
    private System.Windows.Forms.Button button1;

  }
}

