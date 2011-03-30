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

    private enum CalcState : long {
      CalcState_Ready,
      CalcState_AnswerERROR,
      CalcState_AnswerOK
    }

    private long calc_state_ = (long)CalcState.CalcState_Ready;

    private EventHandler num_or_math_btn_handler_;

    private RPN_CalculatorEngine calc_engine_;

    private Point last_pos_;

    private Font screen_font_;

    private String input_ = null;

    private bool AreButtonsEnabled() {
      return calc_state_ != (long)CalcState.CalcState_AnswerERROR;
    }

    private void Handle_NumericOrMathButtonPress(Object sender, EventArgs e) {
      if (AreButtonsEnabled()) {
        if (calc_screen.Text.Any() && calc_screen.Text[0] == '0')
          calc_screen.Text = "";

        calc_screen.Text = calc_screen.Text + ((Button)sender).Text;
      }
    }

    private void Initialize_NumericButtons() {
      num_or_math_btn_handler_ = new EventHandler(Handle_NumericOrMathButtonPress);
      btn_zero.Click += num_or_math_btn_handler_;
      btn_one.Click += num_or_math_btn_handler_;
      btn_two.Click += num_or_math_btn_handler_;
      btn_three.Click += num_or_math_btn_handler_;
      btn_four.Click += num_or_math_btn_handler_;
      btn_five.Click += num_or_math_btn_handler_;
      btn_six.Click += num_or_math_btn_handler_;
      btn_seven.Click += num_or_math_btn_handler_;
      btn_eight.Click += num_or_math_btn_handler_;
      btn_nine.Click += num_or_math_btn_handler_;
      btn_dot.Click += num_or_math_btn_handler_;
      btn_comma.Click += num_or_math_btn_handler_;
      btn_plus.Click += num_or_math_btn_handler_;
      btn_minus.Click += num_or_math_btn_handler_;
      btn_mul.Click += num_or_math_btn_handler_;
      btn_div.Click += num_or_math_btn_handler_;
      btn_lparen.Click += num_or_math_btn_handler_;
      btn_rparen.Click += num_or_math_btn_handler_;
    }

    public Calculator() {
      InitializeComponent();
      Initialize_NumericButtons();
      
      calc_engine_ = new RPN_CalculatorEngine();
      last_pos_ = new Point(0, 0);
      screen_font_ = new Font("Arial", 14.0f, FontStyle.Bold);
      calc_screen.Font = screen_font_;
      calc_screen.Text = "0";
      calc_screen.TextAlign = HorizontalAlignment.Right;
      //this.TransparencyKey = Color.FromArgb(255, 0, 0);
      //this.BackColor = Color.Black;

      btn_zero.ForeColor = Color.WhiteSmoke;
      btn_zero.BackColor = Color.Black;

      btn_one.ForeColor = Color.WhiteSmoke;
      btn_one.BackColor = Color.Black;
      //btn_one.Font = digit_font_;

      btn_two.ForeColor = Color.WhiteSmoke;
      btn_two.BackColor = Color.Black;
      //btn_two.Font = digit_font_;

      btn_three.ForeColor = Color.WhiteSmoke;
      btn_three.BackColor = Color.Black;
      //btn_three.Font = digit_font_;

      btn_four.ForeColor = Color.WhiteSmoke;
      btn_four.BackColor = Color.Black;
      //btn_four.Font = digit_font_;

      btn_five.ForeColor = Color.WhiteSmoke;
      btn_five.BackColor = Color.Black;
      //btn_five.Font = digit_font_;

      btn_six.ForeColor = Color.WhiteSmoke;
      btn_six.BackColor = Color.Black;
      //btn_six.Font = digit_font_;

      btn_seven.ForeColor = Color.WhiteSmoke;
      btn_seven.BackColor = Color.Black;
      //btn_seven.Font = digit_font_;

      btn_eight.ForeColor = Color.WhiteSmoke;
      btn_eight.BackColor = Color.Black;
      //btn_eight.Font = digit_font_;

      btn_nine.ForeColor = Color.WhiteSmoke;
      btn_nine.BackColor = Color.Black;
      //btn_nine.Font = digit_font_;

      btn_dot.ForeColor = Color.WhiteSmoke;
      btn_dot.BackColor = Color.DarkSlateBlue;
      //btn_dot.Font = digit_font_;

      btn_plus.ForeColor = Color.WhiteSmoke;
      btn_plus.BackColor = Color.DarkSlateBlue;
      //btn_plus.Font = digit_font_;

      btn_minus.ForeColor = Color.WhiteSmoke;
      btn_minus.BackColor = Color.DarkSlateBlue;
      //btn_minus.Font = digit_font_;

      btn_mul.ForeColor = Color.WhiteSmoke;
      btn_mul.BackColor = Color.DarkSlateBlue;
      //btn_mul.Font = digit_font_;

      btn_div.ForeColor = Color.WhiteSmoke;
      btn_div.BackColor = Color.DarkSlateBlue;
      //btn_div.Font = digit_font_;

      btn_mod.ForeColor = Color.WhiteSmoke;
      btn_mod.BackColor = Color.DarkSlateBlue;
      //btn_mod.Font = digit_font_;

      btn_inv.ForeColor = Color.WhiteSmoke;
      btn_inv.BackColor = Color.DarkSlateBlue;
      //btn_inv.Font = medium_font_;

      btn_lparen.ForeColor = Color.WhiteSmoke;
      btn_lparen.BackColor = Color.DarkSlateBlue;

      btn_rparen.ForeColor = Color.WhiteSmoke;
      btn_rparen.BackColor = Color.DarkSlateBlue;

      btn_equal.ForeColor = Color.WhiteSmoke;
      btn_equal.BackColor = Color.DarkSlateBlue;
      //btn_equal.Font = digit_font_;

      btn_leftarrow.BackColor = Color.DarkSlateBlue;
      btn_rightarrow.BackColor = Color.DarkSlateBlue;

      btn_sqrt.ForeColor = Color.WhiteSmoke;
      btn_sqrt.BackColor = Color.DarkSlateBlue;
      //btn_sqrt.Font = digit_font_;

      btn_sine.ForeColor = Color.WhiteSmoke;
      btn_sine.BackColor = Color.Sienna;
      //btn_sine.Font = medium_font_;

      btn_cosine.ForeColor = Color.WhiteSmoke;
      btn_cosine.BackColor = Color.Sienna;
      //btn_cosine.Font = medium_font_;

      btn_tan.ForeColor = Color.WhiteSmoke;
      btn_tan.BackColor = Color.Sienna;
      //btn_tan.Font = medium_font_;

      btn_atan.ForeColor = Color.WhiteSmoke;
      btn_atan.BackColor = Color.Sienna;
      //btn_atan.Font = medium_font_;

      btn_asin.ForeColor = Color.WhiteSmoke;
      btn_asin.BackColor = Color.Sienna;
      //btn_asin.Font = medium_font_;

      btn_acos.ForeColor = Color.WhiteSmoke;
      btn_acos.BackColor = Color.Sienna;
      //btn_acos.Font = medium_font_;

      btn_logln.ForeColor = Color.WhiteSmoke;
      btn_logln.BackColor = Color.Sienna;
      //btn_logln.Font = medium_font_;

      btn_log.ForeColor = Color.WhiteSmoke;
      btn_log.BackColor = Color.Sienna;
      //btn_log.Font = medium_font_;

      btn_pow.ForeColor = Color.WhiteSmoke;
      btn_pow.BackColor = Color.Sienna;
      //btn_pow.Font = medium_font_;

      btn_abs.ForeColor = Color.WhiteSmoke;
      btn_abs.BackColor = Color.Sienna;

      btn_rand.ForeColor = Color.WhiteSmoke;
      btn_rand.BackColor = Color.Sienna;

      btn_fact.ForeColor = Color.WhiteSmoke;
      btn_fact.BackColor = Color.Sienna;
      //btn_fact.Font = medium_font_;

      btn_bkspc.BackColor = Color.Crimson;
      btn_bkspc.ForeColor = Color.WhiteSmoke;

      btn_ce.BackColor = Color.Crimson;
      btn_ce.ForeColor = Color.WhiteSmoke;

      btn_ac.BackColor = Color.Crimson;
      btn_ac.ForeColor = Color.WhiteSmoke;

      btn_comma.BackColor = Color.DarkSlateBlue;
      btn_comma.ForeColor = Color.WhiteSmoke;

      btn_close.BackColor = Color.Crimson;
      btn_min.BackColor = Color.Crimson;
    }

    private void Calculator_MouseDown(object sender, MouseEventArgs e) {
      last_pos_.X = e.X;
      last_pos_.Y = e.Y;
    }

    private void Calculator_MouseMove(object sender, MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) {
        this.Left += e.X - last_pos_.X;
        this.Top += e.Y - last_pos_.Y;
      }
    }

    private void btn_min_Click(object sender, EventArgs e) {
      WindowState = FormWindowState.Minimized;
    }

    private void btn_off_Click(object sender, EventArgs e) {
      Close();
    }

    private void Handle_Error() {
      input_ = calc_screen.Text;
      calc_screen.Text = "Error. Press AC to clear or CE to go back and "
        + "correct the input";
      calc_state_ = (long)CalcState.CalcState_AnswerERROR;
    }

    private void btn_equal_Click(object sender, EventArgs e) {
      if (!AreButtonsEnabled())
        return;

      if (calc_engine_.ParseInput(calc_screen.Text)) {
        try {
          float ret_val = calc_engine_.Compute();
          calc_screen.Text = ret_val.ToString();
          calc_state_ = (long)CalcState.CalcState_AnswerOK;
        } catch (System.Exception) {
          Handle_Error();
        }
      } else {
        Handle_Error();
      }
    }

    private void btn_bkspace_click(object sender, EventArgs e) {
      if (!AreButtonsEnabled())
        return;

      int index = calc_screen.SelectionStart;
      if ((index > 0) && (index <= calc_screen.Text.Length)) {
        calc_screen.Text = calc_screen.Text.Remove(index - 1, 1);
        calc_screen.SelectionStart = index - 1;
      }
    }

    private void btn_rightarrow_Click(object sender, EventArgs e) {
      if (!AreButtonsEnabled())
        return;

      if (calc_screen.SelectionStart < calc_screen.Text.Length) {
        calc_screen.SelectionStart += 1;
      }
    }

    private void btn_leftarrow_Click(object sender, EventArgs e) {
      if (!AreButtonsEnabled())
        return;

      if (calc_screen.SelectionStart > 0)
        calc_screen.SelectionStart -= 1;
    }

    private void btn_ce_Click(object sender, EventArgs e) {
      if (calc_state_ == (long)CalcState.CalcState_AnswerERROR) {
        calc_screen.Text = input_;
        calc_screen.SelectionStart = calc_screen.Text.Length;
      } else {
        calc_screen.Text = "0";
      }

      calc_state_ = (long)CalcState.CalcState_Ready;
      input_ = "";
    }

    private void btn_ac_Click(object sender, EventArgs e) {
      calc_screen.Text = "0";
      calc_state_ = (long)CalcState.CalcState_Ready;
      input_ = "";
    }
  }
}
