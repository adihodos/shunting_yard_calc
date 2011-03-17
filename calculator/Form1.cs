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
      shparser_ = new ShuntingYard();
    }

    private void button1_Click(object sender, EventArgs e) {
      if (shparser_.ParseInput(calc_screen.Text)) {
        MessageBox.Show(shparser_.GetRPNOutputAsString());
      } else {
        MessageBox.Show("Crap error");
      }
    }

    private ShuntingYard shparser_;
  }
}
