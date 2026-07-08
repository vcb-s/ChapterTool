using ChapterTool.Core.Diagnostics;

namespace ChapterTool.Core.Transform;

/// <summary>
/// Identifies the kind of token recognized by expression authoring.
/// </summary>
public enum ExpressionTokenKind
{
    /// <summary>
    /// Identifies the Unknown value.
    /// </summary>
    Unknown,
    /// <summary>
    /// Identifies the Number value.
    /// </summary>
    Number,
    /// <summary>
    /// Identifies the Variable value.
    /// </summary>
    Variable,
    /// <summary>
    /// Identifies the Constant value.
    /// </summary>
    Constant,
    /// <summary>
    /// Identifies the Function value.
    /// </summary>
    Function,
    /// <summary>
    /// Identifies the Keyword value.
    /// </summary>
    Keyword,
    /// <summary>
    /// Identifies the Snippet value.
    /// </summary>
    Snippet,
    /// <summary>
    /// Identifies the String value.
    /// </summary>
    String,
    /// <summary>
    /// Identifies the Operator value.
    /// </summary>
    Operator,
    /// <summary>
    /// Identifies the Punctuation value.
    /// </summary>
    Punctuation,
    /// <summary>
    /// Identifies the Comment value.
    /// </summary>
    Comment
}

/// <summary>
/// Describes one token span in an expression.
/// </summary>
/// <param name="Start">The Start value.</param>
/// <param name="Length">The Length value.</param>
/// <param name="Text">The Text value.</param>
/// <param name="Kind">The Kind value.</param>
public sealed record ExpressionTokenSpan(
    int Start,
    int Length,
    string Text,
    ExpressionTokenKind Kind);

/// <summary>
/// Describes an expression symbol available to authoring tools.
/// </summary>
/// <param name="Text">The Text value.</param>
/// <param name="Kind">The Kind value.</param>
/// <param name="Description">The Description value.</param>
/// <param name="Arity">The Arity value.</param>
/// <param name="InsertText">The InsertText value.</param>
public sealed record ExpressionSymbol(
    string Text,
    ExpressionTokenKind Kind,
    string Description,
    int? Arity = null,
    string InsertText = "");

/// <summary>
/// Describes a completion item for expression authoring.
/// </summary>
/// <param name="Text">The Text value.</param>
/// <param name="Kind">The Kind value.</param>
/// <param name="Description">The Description value.</param>
/// <param name="ReplacementStart">The ReplacementStart value.</param>
/// <param name="ReplacementLength">The ReplacementLength value.</param>
/// <param name="InsertText">The InsertText value.</param>
public sealed record ExpressionCompletion(
    string Text,
    ExpressionTokenKind Kind,
    string Description,
    int ReplacementStart,
    int ReplacementLength,
    string InsertText)
{
    /// <summary>
    /// Gets the KindLabel value.
    /// </summary>
    public string KindLabel => Kind switch
    {
        ExpressionTokenKind.Variable => "VAR",
        ExpressionTokenKind.Constant => "CONST",
        ExpressionTokenKind.Function => "FUNC",
        ExpressionTokenKind.Keyword => "KEY",
        ExpressionTokenKind.Snippet => "PRESET",
        ExpressionTokenKind.String => "STR",
        ExpressionTokenKind.Number => "NUM",
        _ => Kind.ToString().ToUpperInvariant()
    };
}


/// <summary>
/// Describes a suggested fix for an expression diagnostic.
/// </summary>
/// <param name="Code">The Code value.</param>
/// <param name="Message">The Message value.</param>
public sealed record ExpressionDiagnosticSuggestion(
    string Code,
    string Message);

/// <summary>
/// Describes an expression diagnostic and its source span.
/// </summary>
/// <param name="Diagnostic">The Diagnostic value.</param>
/// <param name="Suggestion">The Suggestion value.</param>
/// <param name="Start">The Start value.</param>
/// <param name="Length">The Length value.</param>
public sealed record ExpressionAuthoringDiagnostic(
    ChapterDiagnostic Diagnostic,
    ExpressionDiagnosticSuggestion Suggestion,
    int Start,
    int Length);

/// <summary>
/// Represents token, completion, and diagnostic analysis for an expression.
/// </summary>
/// <param name="Spans">The Spans value.</param>
/// <param name="Completions">The Completions value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record ExpressionAnalysisResult(
    IReadOnlyList<ExpressionTokenSpan> Spans,
    IReadOnlyList<ExpressionCompletion> Completions,
    IReadOnlyList<ExpressionAuthoringDiagnostic> Diagnostics);

/// <summary>
/// Defines expression authoring analysis operations.
/// </summary>
public interface IExpressionAuthoringService
{
    /// <summary>
    /// Gets symbols available to expression authoring.
    /// </summary>
    IReadOnlyList<ExpressionSymbol> Symbols { get; }

    /// <summary>
    /// Analyzes expression text for tokens, completions, and diagnostics.
    /// </summary>
    /// <param name="expression">The expression text.</param>
    /// <param name="caretIndex">The caret index in the expression.</param>
    /// <param name="timeSeconds">The chapter time in seconds.</param>
    /// <param name="framesPerSecond">The frame rate in frames per second.</param>
    /// <returns>The expression analysis result.</returns>
    ExpressionAnalysisResult Analyze(string expression, int caretIndex, decimal timeSeconds = 0, decimal framesPerSecond = 24);
}
