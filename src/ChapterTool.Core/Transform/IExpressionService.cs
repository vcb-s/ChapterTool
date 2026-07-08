namespace ChapterTool.Core.Transform;

/// <summary>
/// Evaluates mathematical chapter time expressions.
/// </summary>
public interface IExpressionService
{
    /// <summary>
    /// Evaluates an infix expression against a chapter time context.
    /// </summary>
    /// <param name="expression">The expression text.</param>
    /// <param name="timeSeconds">The chapter time in seconds.</param>
    /// <param name="framesPerSecond">The frame rate in frames per second.</param>
    /// <returns>The expression evaluation result.</returns>
    ExpressionEvaluationResult EvaluateInfix(string expression, decimal timeSeconds, decimal framesPerSecond);

    /// <summary>
    /// Evaluates postfix expression tokens against a chapter time context.
    /// </summary>
    /// <param name="tokens">The postfix expression tokens.</param>
    /// <param name="timeSeconds">The chapter time in seconds.</param>
    /// <param name="framesPerSecond">The frame rate in frames per second.</param>
    /// <returns>The expression evaluation result.</returns>
    ExpressionEvaluationResult EvaluatePostfix(IEnumerable<string> tokens, decimal timeSeconds, decimal framesPerSecond);
}
