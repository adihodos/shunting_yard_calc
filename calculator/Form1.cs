using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace calculator
{
  public partial class Calculator : Form {
    public Calculator() {
      InitializeComponent();
      shparser_ = new RPN_Calculator();
    }

    private void button1_Click(object sender, EventArgs e) {
      if (shparser_.ParseInput(calc_screen.Text)) {
        try {
          float result = shparser_.Compute();
          MessageBox.Show(String.Format("Result = {0}", result));
        } catch (System.Exception except) {
          MessageBox.Show(except.Message);
        }
      } else {
        MessageBox.Show("Crap error");
      }
    }

    private RPN_Calculator shparser_;
  }
}
