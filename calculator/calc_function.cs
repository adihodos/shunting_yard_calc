using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace calculator {
  /// <summary>
  /// Base class for functions implemented by the calculator.
  /// </summary>
  public abstract class ICalcFunction {
    /// <summary>
    /// Name of the function.
    /// </summary>
    private String func_name_;
    /// <summary>
    /// Number of arguments to pass when calling the function.
    /// </summary>
    private int func_args_;

    public ICalcFunction(String name, int args) {
      func_name_ = name;
      func_args_ = args;
    }

    public String FunctionName {
      get {
        return func_name_;
      }
    }

    public int ArgCount {
      get {
        return func_args_;
      }
    }

    /// <summary>
    /// Applies the function to the arguments in args.
    /// </summary>
    /// <param name="args">Array of arguments. The number of elements must
    /// match the number of arguments that the function expects.</param>
    /// <returns>Result of applying the function to the arguments.</returns>
    public abstract float Compute(float[] args);
  }

  public sealed class Function_Sine : ICalcFunction {
    public Function_Sine() : base("sin", 1) {
    }

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return (float) Math.Sin((double)args[0]);
    }
  }

  public sealed class Function_Cosine : ICalcFunction {
    public Function_Cosine() : base("cos", 1) {
    }

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return (float)Math.Cos((double)args[0]);
    }
  }

  public sealed class Function_Plus : ICalcFunction {
    public Function_Plus() : base("+", 2) {}

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return args[0] + args[1];
    }
  }

  public sealed class Function_Minus : ICalcFunction {
    public Function_Minus() : base("-", 2) { }

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return args[0] - args[1];
    }
  }

  public sealed class Function_Mul : ICalcFunction {
    public Function_Mul() : base("*", 2) {}

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == base.ArgCount);
      return args[0] * args[1];
    }
  }

  public sealed class Function_Div : ICalcFunction {
    public Function_Div() : base("/", 2) {}

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return args[0] / args[1];
    }
  }

  public sealed class Function_Exp : ICalcFunction {
    public Function_Exp() : base("^", 2) { }

    public override float Compute(float[] args) {
      Debug.Assert(args.Length == ArgCount);
      return (float)Math.Pow((double)args[0], (double)args[1]);
    }
  }

  /// <summary>
  /// Holds a collection of functions that users of the calculator can use.
  /// </summary>
  public sealed class FunctionTable {
    public FunctionTable() {
      func_table_ = new Dictionary<String, ICalcFunction>(32);
      InitializeFunctionTable();
    }

    /// <summary>
    /// Tests if the specified function is implemented.
    /// </summary>
    /// <param name="func_name">Name of function.</param>
    /// <returns>True if implemented, false otherwise.</returns>
    public bool ImplementsFunction(String func_name) {
      return func_table_.ContainsKey(func_name);
    }

    /// <summary>
    /// Returns the number of arguments that a calling function expects.
    /// </summary>
    /// <param name="func_name">Function name.</param>
    /// <returns>Number of arguments to be supplied when 
    /// calling the function.</returns>
    public int GetFunctionArgumentCount(String func_name) {
      Debug.Assert(ImplementsFunction(func_name),
        "Function " + func_name + " is not implemented!");
      return func_table_[func_name].ArgCount;
    }

    /// <summary>
    /// Calls the specified function with the provided arguments.
    /// </summary>
    /// <param name="func_name">Function name.</param>
    /// <param name="args">Array of arguments.</param>
    /// <returns>The result of applying the function to the arguments.</returns>
    public float CallFunction(String func_name, float[] args) {
      Debug.Assert(ImplementsFunction(func_name),
        "Function " + func_name + " is not implemented!");
      return func_table_[func_name].Compute(args);
    }

    /// <summary>
    /// Initializes the table with the supported functions.
    /// </summary>
    private void InitializeFunctionTable() {
      ICalcFunction curr_func = new Function_Sine();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Cosine();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Plus();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Minus();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Mul();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Div();
      func_table_.Add(curr_func.FunctionName, curr_func);

      curr_func = new Function_Exp();
      func_table_.Add(curr_func.FunctionName, curr_func);
    }

    /// <summary>
    /// Maps each function name to an ICalcFunction object that will perform
    /// the computations.
    /// </summary>
    private Dictionary<String, ICalcFunction> func_table_;
  }
}
