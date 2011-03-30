using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace calculator {

/// <summary>
/// Simple class that converts input from infix notation to reverse polish
/// notation (RPN), using the shunting yard algorithm (E. Dijkstra).
/// <see cref="http://en.wikipedia.org/wiki/Shunting-yard_algorithm"/>
/// <example>
/// String input = new String("sin(sin(sin(tan(1 + (2*3)^4/(5*6)^2))))");
/// RPN_CalculatorEngine parser = new RPN_CalculatorEngine();
/// if (parser.ParseInput(input)) {
///   //
///   // Do something with the result.
/// }
/// </summary>
class RPN_CalculatorEngine {

  public class RPN_ArgumentException : System.Exception {
    public RPN_ArgumentException() {
      func_ = null;
      provided_ = 0;
      needed_ = 0;
    }

    public RPN_ArgumentException(String func, int provided, int needed) {
      func_ = func;
      provided_ = provided;
      needed_ = needed;
    }

    public override string Message {
      get {
        if (func_ != null) {
          return String.Format(
            "Function {0} called with {1} argument(s), but needs {2} argument(s).",
            func_, provided_, needed_);
        } else {
          return "Excess arguments.";
        }
      }
    }

    private String func_;
    private int provided_;
    private int needed_;
  }

  public class RPN_MathError : System.Exception {
    public RPN_MathError(String func, float[] args) {
      func_ = func;
      args_ = args;
    }

    public override string Message {
      get {
        return String.Format("Division by zero, function {0}.", func_);
      }
    }

    private String func_;
    private float[] args_;
  }

  public RPN_CalculatorEngine() {
    curr_state = ParserState.ParserState_Ok;
    op_stack = new Stack<CalculatorToken>(32);
    output_queue = new Stack<CalculatorToken>(32);
    func_table_ = new FunctionTable();
    input_string = null;
    input_iter = 0;
    input_end_marker = 0;
  }

  /// <summary>
  /// Converts the input (assumed to be in infix notation) to RPN.
  /// </summary>
  /// <param name="input">Input string in infix notation.</param>
  /// <returns>True if successfull, false if input was not valid.</returns>
  public bool ParseInput(String input) {
    input_iter = 0;
    input_end_marker = input.Length;
    input_string = input;
    curr_state = ParserState.ParserState_Ok;
    output_queue.Clear();
    op_stack.Clear();

    for (bool done = false; 
         !done && curr_state != ParserState.ParserState_Error; ) {
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

  /// <summary>
  /// Returns a string representing the input converted to RPN. 
  /// Only call this function if ParseInput() was successfull.
  /// </summary>
  /// <returns>String representing input converted into RPN.</returns>
  public String GetRPNOutputAsString() {
    Debug.Assert(curr_state != ParserState.ParserState_Error);
    String rpn_output = "";
    Stack<CalculatorToken> tmpstk = new Stack<CalculatorToken>(output_queue);

    while (tmpstk.Any()) {
      CalculatorToken token = tmpstk.Pop();
      switch (token.ct_type) {
        case CalculatorToken.TOKEN_Type.TOKEN_Type_Function : 
        case CalculatorToken.TOKEN_Type.TOKEN_Type_Operator :
          rpn_output = rpn_output + token.ct_func + " ";
          break;

        case CalculatorToken.TOKEN_Type.TOKEN_Type_Number:
          rpn_output = rpn_output + token.ct_fval.ToString() + " ";
          break;

        default:
          break;
      }
    }

    return rpn_output;
  }

  /// <summary>
  /// Computes the mathematical value of the input. Only call this function
  /// if ParseInput() succeeded.
  /// </summary>
  /// <returns>Value that the input evaluates to.</returns>
  /// <exception cref="RPN_ArgumentException"/>
  /// <exception cref="RPN_MathError"/>
  public float Compute() {
    Debug.Assert(curr_state != ParserState.ParserState_Error);
    Debug.Assert(output_queue.Any());

    Stack<float> operands_stk = new Stack<float>(16);
    foreach (CalculatorToken curr_token in output_queue.Reverse()) {

      if (curr_token.ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Number) {
        //
        // Push any values into the operands stack.
        operands_stk.Push(curr_token.ct_fval);
      } else {
        //
        // Try calling the specified function/operator, validating the argument 
        // count.
        Debug.Assert(
          curr_token.ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Operator ||
          curr_token.ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Function);
        int argc = func_table_.GetFunctionArgumentCount(curr_token.ct_func);
        //
        // Too few arguments provided for call.
        if (operands_stk.Count < argc) {
          throw new RPN_ArgumentException(curr_token.ct_func, 
            output_queue.Count, argc);
        }

        //
        // Pop the required number of arguments from the output queue into argv
        // and call the function.
        float[] argv = new float[argc];
        while (argc-- > 0) {
          argv[argc] = operands_stk.Pop();
        }
        //
        // Push result back into stack.
        try {
          operands_stk.Push(func_table_.CallFunction(curr_token.ct_func, argv));
        } catch (System.DivideByZeroException) {
          throw new RPN_MathError(curr_token.ct_func, argv);
        }
      }
    }

    //
    // Computation done.
    if (operands_stk.Count == 1) {
      return operands_stk.Peek();
    }
    
    //
    // Excess arguments provided to function/operator.
    throw new RPN_ArgumentException();
  }

  /// <summary>
  /// Represents lexical tokens. 
  /// </summary>
  private struct CalculatorToken {
    /// <summary>
    /// Token types in the calculator 
    /// (number, function - sin , cos, tan, Operator -, + *, etc)
    /// </summary>
    public enum TOKEN_Type {
      TOKEN_Type_Number,
      TOKEN_Type_Function,
      TOKEN_Type_Comma, 
      TOKEN_Type_LeftParen,
      TOKEN_Type_RightParen,
      TOKEN_Type_Operator
    }

    /// <summary>
    /// Stores the token type.
    /// </summary>
    public TOKEN_Type ct_type;
    /// <summary>
    /// Valid only if token is of type TOKEN_Type_Number
    /// </summary>
    public float ct_fval;
    /// <summary>
    /// Valid for the rest of tokens (operators, functions, parens)
    /// </summary>
    public String ct_func;  
  }

  /// <summary>
  /// Possible states that the parser can be in, at a given moment.
  /// </summary>
  private enum ParserState {
    ParserState_Ok,
    ParserState_EndOfInput, // all of the input has been processed
    ParserState_Error, // input contains tokens that are not valid
    ParserState_Digit,
    ParserState_Func, // sine, cosine, tan, atan, etc
    ParserState_Operator, // -, +, *, %, etc
    ParserState_LeftParen,
    ParserState_RightParen,
    ParserState_Comma // used as an argument separator
  }

  /// <summary>
  /// Reads the next token from the input stream and sets the parser's state
  /// based on that token's type.
  /// </summary>
  private void ReadNextTokenFromInput() {
    //
    // Eat any whitespaces.
    while (!IsEndOfInput() && Char.IsWhiteSpace(GetCurrentAtomFromInput())) {
      AdvanceToNextAtomEx();
    }

    if (IsEndOfInput()) {
      return;
    }

    //
    // Analyze current token and set parser's state.
    Char curr_atom = GetCurrentAtomFromInput();
    if (curr_atom == '-' && CanPeekNextAtomAndIsDigit()) {
      if (!op_stack.Any() || 
          op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_RightParen) {
        CalculatorToken fake_plus = new CalculatorToken {
          ct_func = "+",
          ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Operator,
          ct_fval = 0.0f
        };
        op_stack.Push(fake_plus);
      }
      curr_state = ParserState.ParserState_Digit;
      return;
    }

    if (Char.IsDigit(curr_atom)) {
      curr_state = ParserState.ParserState_Digit;
      return;
    }

    if (TokenIsAnOperator(curr_atom)) {
      curr_state = ParserState.ParserState_Operator;
      return;
    }

    if (Char.IsLetter(curr_atom)) {
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

  /// <summary>
  /// Called when the parser reaches a digit.
  /// </summary>
  private void Handle_Digit() {
    bool is_float = false;
    String curr_num = "";

    //
    // Check for minus sign and skip it if present.
    Char curr_atom = GetCurrentAtomFromInput();
    if (curr_atom == '-') {
      curr_num = curr_num + curr_atom;
      AdvanceToNextAtomEx();
    }

    //
    // Get the rest of the number's digits.
    for (; ; ) {
      if (IsEndOfInput())
        break;

      curr_atom = GetCurrentAtomFromInput();
      if (Char.IsDigit(curr_atom)) {
        curr_num = curr_num + curr_atom;
        AdvanceToNextAtomEx();
      } else if (curr_atom == '.') {
        if (is_float) {
          //
          // Misplaced dot, halt processing.
          curr_state = ParserState.ParserState_Error;
          return;
        }

        //
        // Floating point number.
        is_float = true;
        curr_num = curr_num + curr_atom;
        AdvanceToNextAtomEx();
      } else {
        break;
      }      
    }

    //
    // Numbers get pushed into the output queue.
    Debug.Assert(curr_num.Length > 0);
    CalculatorToken new_token = new CalculatorToken { 
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Number,
      ct_fval = float.Parse(curr_num),
      ct_func = null
    };

    output_queue.Push(new_token);
  }

  /// <summary>
  /// Called when the parser reaches a function.
  /// </summary>
  private void Handle_Function() {
    String func_name = null;
    int fname_start = input_iter;

    //
    // Build function name.
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
    //
    // Check if the calculator implements this function, and if so push it into
    // the operator stack.
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

  /// <summary>
  /// Called when the parser reaches a comma token.
  /// </summary>
  private void Handle_Comma() {
    bool paren_popped = false;

    //
    // While top of the stack contains a left parenthesis, pop operators off
    // the operator stack and into the output queue.
    while (op_stack.Any()) {
      if (op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen) {
        paren_popped = true;
        break;
      }

      output_queue.Push(op_stack.Pop());
    }

    //
    // Since no left parenthesis was encountered, either the comma was misplaced
    // or there are unbalanced parentheses.
    if (!paren_popped) {
      curr_state = ParserState.ParserState_Error;
      return;
    }

    AdvanceToNextAtomEx();
  }

  /// <summary>
  /// Called when the parser reaches an operator.
  /// </summary>
  private void Handle_Operator() {
    Char op1 = GetCurrentAtomFromInput();

    //
    // While there is a seconds operator (op2) at the top of the operators stack
    // and either op1 is left assoc and has a lower or equal precedence to op2
    // or op1 is right assoc and has a lower precedence that op2, pop op2
    // off the operator stack and into the output queue.
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

    //
    // Push op1 into the operator stack.
    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_Operator,
      ct_fval = 0.0f,
      ct_func = new String(op1, 1)
    };

    op_stack.Push(new_token);
    AdvanceToNextAtomEx();
  }

  /// <summary>
  /// Called when the parser reaches a left parenthesis.
  /// </summary>
  private void Handle_LeftParen() {
    //
    // Left parentheses simply get pushed into the operator stack.
    CalculatorToken new_token = new CalculatorToken {
      ct_type = CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen,
      ct_fval = 0.0f,
      ct_func = null
    };
    op_stack.Push(new_token);
    AdvanceToNextAtomEx();
  }

  /// <summary>
  /// Called when the parser reaches a right parenthesis.
  /// </summary>
  private void Handle_RightParen() {
    bool popped_lparen = false;

    //
    // While the top of the stack is not a left paren, pop operators from the
    // operator stack into the output queue.
    while (op_stack.Any()) {
      if (op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen) {
        popped_lparen = true;
        break;
      }

      output_queue.Push(op_stack.Pop());
    }

    //
    // Mismatched parenthesis, halt any further processing.
    if (!popped_lparen) {
      curr_state = ParserState.ParserState_Error;
      return;
    }

    //
    // The left paren that is at the top of the operator stack does not get
    // popped into the output queue.
    Debug.Assert(op_stack.Any() &&
      op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen);
    op_stack.Pop();

    //
    // If there's a function at the top of the stack, pop it into the output
    // queue.
    if (op_stack.Any() &&
        op_stack.Peek().ct_type == CalculatorToken.TOKEN_Type.TOKEN_Type_Function) {
      output_queue.Push(op_stack.Pop());
    }
    //
    // Process rest of input.
    AdvanceToNextAtomEx();
  }

  /// <summary>
  /// Called when the parser reaches the end of the input stream.
  /// </summary>
  private void Handle_EndOfInput() {
    while (op_stack.Any()) {
      CalculatorToken.TOKEN_Type token_type = op_stack.Peek().ct_type;

      //
      // Check for unbalanced parenthesis.
      if (token_type == CalculatorToken.TOKEN_Type.TOKEN_Type_LeftParen ||
        token_type == CalculatorToken.TOKEN_Type.TOKEN_Type_RightParen) {
          curr_state = ParserState.ParserState_Error;
          return;
      }

      //
      // Pop everything else into the output queue.
      output_queue.Push(op_stack.Pop());
    }
  }

  /// <summary>
  /// Tests if the parser has reached the end of the input.
  /// </summary>
  /// <returns>True if parser is at end of input, false if not.</returns>
  private bool IsEndOfInput() {
    if (input_iter == input_end_marker) {
      curr_state = ParserState.ParserState_EndOfInput;
      return true;
    }
    return false;
  }

  /// <summary>
  /// Advances the input iterator to the next token in the input stream.
  /// </summary>
  private void AdvanceToNextAtomEx() {
    ++input_iter;
  }

  /// <summary>
  /// Tests if a token represents an operator that the calculator understands.
  /// </summary>
  /// <param name="token">Operator token.</param>
  /// <returns>True if token is an operator, false if not.</returns>
  private bool TokenIsAnOperator(Char token) {
    return (token == '=') || (token == '^') || (token == '+') || 
           (token == '-') || (token == '/') || (token == '%') ||
           (token == '*');
  }

  /// <summary>
  /// Tests if an operator is left associative.
  /// </summary>
  /// <param name="op">Operator to test.</param>
  /// <returns>True if operator is left associative, false otherwise.</returns>
  private bool IsOperatorLeftAssociative(Char op) {
    return op == '*' || op == '/' || op == '%' || op == '+' || op == '-';
  }

  /// <summary>
  /// Computes the precedence level of an operator.
  /// </summary>
  /// <param name="op">Operator identifier.</param>
  /// <returns>Integer representing precedence level of that operator.</returns>
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

  /// <summary>
  /// Checks if a certain function/operator is implemented by the calculator.
  /// </summary>
  /// <param name="funcname">Function name.</param>
  /// <returns>True if implemented, false otherwise.</returns>
  private bool ValidateFunction(String funcname) {
    return func_table_.ImplementsFunction(funcname);
  }

  /// <summary>
  /// Returns the current atom from the input, that the parser has reached to.
  /// </summary>
  /// <returns></returns>
  private Char GetCurrentAtomFromInput() {
    Debug.Assert(input_iter < input_end_marker);
    return input_string[input_iter];
  }

  /// <summary>
  /// Peeks the next atom in the input string (if any) and tests if it's a digit.
  /// </summary>
  /// <returns>True if following atom is a digit, false if end of input or
  /// not a digit.</returns>
  private bool CanPeekNextAtomAndIsDigit() {
    if (((input_iter + 1) < input_end_marker) &&
        Char.IsDigit(input_string[input_iter + 1])) {
      return true;
    }
    return false;
  }

  /// <summary>
  /// Operands stack.
  /// </summary>
  private Stack<CalculatorToken>  op_stack;
  /// <summary>
  /// The output queue.
  /// </summary>
  private Stack<CalculatorToken>  output_queue;
  /// <summary>
  /// Current state of the parser.
  /// </summary>
  private ParserState curr_state;
  /// <summary>
  /// The input that the parser is currently analyzing.
  /// </summary>
  private String input_string;
  /// <summary>
  /// Table of functions that the calculator implements.
  /// </summary>
  private FunctionTable func_table_;
  /// <summary>
  /// Stores the index of the element reached by the parser.
  /// </summary>
  private int input_iter;
  /// <summary>
  /// Signals the parser that it has reached end of the input.
  /// </summary>
  private int input_end_marker;
}

}