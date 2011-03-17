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
    input_iterator = null;
  }

  public bool ParseInput(String input) {
    input_iterator = input.GetEnumerator();
    curr_state = ParserState.ParserState_Ok;
    output_queue.Clear();
    op_stack.Clear();

    if (!AdvanceToNextAtom()) {
      return false;
    }

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
    if (curr_state == ParserState.ParserState_EndOfInput)
      return;

    for (; ;) {
      if (Char.IsWhiteSpace(input_iterator.Current)) {
        if (!AdvanceToNextAtom())
          return;
      } else {
        break;
      }
    }

    if (TokenIsAnOperator(input_iterator.Current)) {
      curr_state = ParserState.ParserState_Operator;
      return;
    }

    if (Char.IsDigit(input_iterator.Current)) {
      curr_state = ParserState.ParserState_Digit;
      return;
    }

    if (Char.IsLetterOrDigit(input_iterator.Current)) {
      curr_state = ParserState.ParserState_Func;
      return;
    }

    if (input_iterator.Current == '(') {
      curr_state = ParserState.ParserState_LeftParen;
      return;
    }

    if (input_iterator.Current == ')') {
      curr_state = ParserState.ParserState_RightParen;
      return;
    }

    if (input_iterator.Current == ',') {
      curr_state = ParserState.ParserState_Comma;
      return;
    }

    curr_state = ParserState.ParserState_Error;
  }

  private void Handle_Digit() {
    bool is_float = false;
    String curr_num = "";

    for (; ; ) {
      if (Char.IsDigit(input_iterator.Current)) {
        curr_num = curr_num + input_iterator.Current;
      } else if (input_iterator.Current == '.') {
        if (is_float) {
          curr_state = ParserState.ParserState_Error;
          return;
        }

        is_float = true;
        curr_num = curr_num + input_iterator.Current;
      } else {
        break;
      }

      if (!AdvanceToNextAtom())
        break;
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
    String func_name = "";

    for (; ; ) {
      if (Char.IsLetterOrDigit(input_iterator.Current)) {
        func_name = func_name + input_iterator.Current;
      } else {
        break;
      }

      if (!AdvanceToNextAtom())
        break;
    }

    Debug.Assert(func_name.Length > 0);
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

    AdvanceToNextAtom();
  }

  private void Handle_Operator() {
    Char op1 = input_iterator.Current;

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
    AdvanceToNextAtom();
  }

  private void Handle_LeftParen() {
    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen,
      ct_fval = 0.0f,
      ct_func = null
    };
    op_stack.Push(new_token);
    AdvanceToNextAtom();
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
    AdvanceToNextAtom();
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

  private bool AdvanceToNextAtom() {
    if (input_iterator.MoveNext()) {
      return true;
    }
    curr_state = ParserState.ParserState_EndOfInput;
    return false;
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

  private Stack<CalculatorToken>  op_stack;
  private Stack<CalculatorToken>  output_queue;
  private ParserState curr_state;
  private IEnumerator<Char> input_iterator;
}

}