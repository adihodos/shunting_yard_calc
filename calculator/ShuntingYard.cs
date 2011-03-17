using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace calculator {

class ShuntingYard {
  public ShuntingYard() {
    curr_state = ParserState.ParserState_Ok;
    op_stack = new Stack<CalculatorToken>(32);
    output_queue = new Stack<CalculatorToken>(32);
    input_string = null;
    input_iter = 0;
    input_end_marker = 0;
  }

  public bool ParseInput(String input) {
    input_iter = 0;
    input_end_marker = input.Length;
    input_string = input;
    curr_state = ParserState.ParserState_Ok;
    output_queue.Clear();
    op_stack.Clear();

    for (bool done = false; !done && curr_state != ParserState.ParserState_Error; ) {
      ReadNextTokenFromInput();

      switch (curr_state) {
        case ParserState.ParserState_Digit:
          Handle_Digit();
          break;

        case ParserState.ParserState_Func:
          Handle_Function();
          break;

        case ParserState.ParserState_Comma:
          Handle_Comma();
          break;

        case ParserState.ParserState_Operator:
          Handle_Operator();
          break;

        case ParserState.ParserState_LeftParen:
          Handle_LeftParen();
          break;

        case ParserState.ParserState_RightParen:
          Handle_RightParen();
          break;

        case ParserState.ParserState_EndOfInput:
          Handle_EndOfInput();
          done = true;
          break;

        case ParserState.ParserState_Error:
          break;

        default:
          Debug.Assert(false, "Unknown state");
          curr_state = ParserState.ParserState_Error;
          break;
      }
    }

    return curr_state != ParserState.ParserState_Error;
  }

  public String GetRPNOutputAsString() {
    Debug.Assert(curr_state != ParserState.ParserState_Error);
    String rpn_output = "";
    Stack<CalculatorToken> tmpstk = new Stack<CalculatorToken>(output_queue);

    while (tmpstk.Any()) {
      CalculatorToken token = tmpstk.Pop();
      switch (token.ct_type) {
        case CalculatorToken.TOKEN_Type.TOKEN_Type_Function:
          rpn_output = rpn_output + token.ct_func + " ";
          break;

        case CalculatorToken.TOKEN_Type.TOKEN_Type_Number:
          rpn_output = rpn_output + token.ct_fval.ToString() + " ";
          break;

        case CalculatorToken.TOKEN_Type.TOKEN_Type_Operator:
          rpn_output = rpn_output + token.ct_func + " ";
          break;

        default:
          break;
      }
    }

    return rpn_output;
  }

  public float Compute() {
    Debug.Assert(curr_state != ParserState.ParserState_Error);
    Debug.Assert(output_queue.Any());

    Stack<float> operands_stk = new Stack<float>(16);
    while (output_queue.Any()) {
      CalculatorToken curr_token = output_queue.Peek();

      if (curr_token.ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Number) {
        operands_stk.Push(curr_token.ct_fval);
        output_queue.Pop();
      } else {
        break;
      }
    }

    return 0.0f;
  }

  private struct CalculatorToken {

    public enum TOKEN_Type {
      TOKEN_Type_Number,
      TOKEN_Type_Function,
      TOKEN_Type_Comma,
      TOKEN_Type_LeftParen,
      TOKEN_Type_RightParen,
      TOKEN_Type_Operator
    }

    public TOKEN_Type ct_type;
    public float ct_fval;
    public String ct_func;  
  }

  private enum ParserState {
    ParserState_Ok,
    ParserState_EndOfInput,
    ParserState_Error,
    ParserState_Digit,
    ParserState_Func,
    ParserState_Operator,
    ParserState_LeftParen,
    ParserState_RightParen,
    ParserState_Comma
  }

  private void ReadNextTokenFromInput() {
    for (; ;) {
      if (IsEndOfInput())
        return;

      if (Char.IsWhiteSpace(GetCurrentAtomFromInput())) {
        AdvanceToNextAtomEx();
      } else {
        break;
      }
    }

    Char curr_atom = GetCurrentAtomFromInput();
    if (TokenIsAnOperator(curr_atom)) {
      curr_state = ParserState.ParserState_Operator;
      return;
    }

    if (Char.IsDigit(curr_atom)) {
      curr_state = ParserState.ParserState_Digit;
      return;
    }

    if (Char.IsLetterOrDigit(curr_atom)) {
      curr_state = ParserState.ParserState_Func;
      return;
    }

    if (curr_atom == '(') {
      curr_state = ParserState.ParserState_LeftParen;
      return;
    }

    if (curr_atom == ')') {
      curr_state = ParserState.ParserState_RightParen;
      return;
    }

    if (curr_atom == ',') {
      curr_state = ParserState.ParserState_Comma;
      return;
    }

    curr_state = ParserState.ParserState_Error;
  }

  private void Handle_Digit() {
    bool is_float = false;
    String curr_num = "";

    for (; ; ) {
      if (IsEndOfInput())
        break;

      Char curr_atom = GetCurrentAtomFromInput();
      if (Char.IsDigit(curr_atom)) {
        curr_num = curr_num + curr_atom;
        AdvanceToNextAtomEx();
      } else if (curr_atom == '.') {
        if (is_float) {
          curr_state = ParserState.ParserState_Error;
          return;
        }

        is_float = true;
        curr_num = curr_num + curr_atom;
        AdvanceToNextAtomEx();
      } else {
        break;
      }      
    }

    Debug.Assert(curr_num.Length > 0);
    CalculatorToken new_token = new CalculatorToken { 
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Number,
      ct_fval = float.Parse(curr_num),
      ct_func = null
    };

    output_queue.Push(new_token);
  }

  private void Handle_Function() {
    String func_name = null;
    int fname_start = input_iter;

    for (; ; ) {
      if (IsEndOfInput())
        break;

      Char curr_atom = GetCurrentAtomFromInput();
      if (Char.IsLetterOrDigit(curr_atom)) {
        AdvanceToNextAtomEx();
      } else {
        break;
      }
    }

    Debug.Assert(fname_start < input_iter);
    func_name = input_string.Substring(fname_start, input_iter - fname_start);
    if (!ValidateFunction(func_name)) {
      curr_state = ParserState.ParserState_Error;
      return;
    }

    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Function,
      ct_fval = 0.0f,
      ct_func = func_name
    };
    op_stack.Push(new_token);
  }

  private void Handle_Comma() {
    bool paren_popped = false;

    while (op_stack.Any()) {
      if (op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen) {
        paren_popped = true;
        break;
      }

      output_queue.Push(op_stack.Pop());
    }

    if (!paren_popped) {
      curr_state = ParserState.ParserState_Error;
      return;
    }

    AdvanceToNextAtomEx();
  }

  private void Handle_Operator() {
    Char op1 = GetCurrentAtomFromInput();

    while (op_stack.Any() &&
           op_stack.Peek().ct_type ==
               CalculatorToken.TOKEN_Type.TOKEN_Type_Operator) {

      Char op2 = op_stack.Peek().ct_func[0];

      if ((IsOperatorLeftAssociative(op1) &&
          (GetOperatorPrecedenceLevel(op1) <= GetOperatorPrecedenceLevel(op2))) ||
          (!IsOperatorLeftAssociative(op1) &&
          (GetOperatorPrecedenceLevel(op1) < GetOperatorPrecedenceLevel(op2)))) {
        output_queue.Push(op_stack.Pop());
      } else {
        break;
      }
    }

    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Operator,
      ct_fval = 0.0f,
      ct_func = new String(op1, 1)
    };

    op_stack.Push(new_token);
    AdvanceToNextAtomEx();
  }

  private void Handle_LeftParen() {
    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen,
      ct_fval = 0.0f,
      ct_func = null
    };
    op_stack.Push(new_token);
    AdvanceToNextAtomEx();
  }

  private void Handle_RightParen() {
    bool popped_lparen = false;

    while (op_stack.Any()) {
      if (op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen) {
        popped_lparen = true;
        break;
      }

      output_queue.Push(op_stack.Pop());
    }

    if (!popped_lparen) {
      curr_state = ParserState.ParserState_Error;
      return;
    }

    Debug.Assert(op_stack.Any() &&
      op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen);
    op_stack.Pop();

    if (op_stack.Any() &&
        op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Function) {
      output_queue.Push(op_stack.Pop());
    }
    AdvanceToNextAtomEx();
  }

  private void Handle_EndOfInput() {
    while (op_stack.Any()) {
      CalculatorToken.TOKEN_Type token_type = op_stack.Peek().ct_type;

      if (token_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen ||
        token_type == CalculatorToken.TOKEN_Type.TOKEN_Type_RightParen) {
          curr_state = ParserState.ParserState_Error;
          return;
      }

      output_queue.Push(op_stack.Pop());
    }
  }

  private bool IsEndOfInput() {
    if (input_iter == input_end_marker) {
      curr_state = ParserState.ParserState_EndOfInput;
      return true;
    }
    return false;
  }

  private void AdvanceToNextAtomEx() {
    ++input_iter;
  }

  private bool TokenIsAnOperator(Char token) {
    return (token == '=') || (token == '^') || (token == '+') || 
           (token == '-') || (token == '/') || (token == '%') ||
           (token == '*');
  }

  private bool IsOperatorLeftAssociative(Char op) {
    return op == '*' || op == '/' || op == '%' || op == '+' || op == '-';
  }

  int GetOperatorPrecedenceLevel(Char op) {
    switch (op) {
      case '^' :
        return 4;

      case '*' : case '%' : case '/' :
        return 3;

      case '+' : case '-' :
        return 2;

      case '=' :
        return 1;

      default :
        return 0;
    }
  }

  private bool ValidateFunction(String funcname) {
    return true;
  }

  private Char GetCurrentAtomFromInput() {
    Debug.Assert(input_iter < input_end_marker);
    return input_string[input_iter];
  }

  private Stack<CalculatorToken>  op_stack;
  private Stack<CalculatorToken>  output_queue;
  private ParserState curr_state;
  private String input_string;
  int input_iter;
  int input_end_marker;
}

}