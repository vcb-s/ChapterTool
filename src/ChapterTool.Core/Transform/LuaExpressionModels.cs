using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;

namespace ChapterTool.Core.Transform;

/// <summary>
/// Provides chapter context values to Lua expression scripts.
/// </summary>
/// <param name="Chapter">The Chapter value.</param>
/// <param name="Index">The Index value.</param>
/// <param name="Count">The Count value.</param>
/// <param name="TimeSeconds">The TimeSeconds value.</param>
/// <param name="FramesPerSecond">The FramesPerSecond value.</param>
public sealed record LuaExpressionContext(
    Chapter Chapter,
    int Index,
    int Count,
    decimal TimeSeconds,
    decimal FramesPerSecond);

/// <summary>
/// Represents the result of Lua expression evaluation.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Value">The Value value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record LuaExpressionEvaluationResult(
    bool Success,
    decimal Value,
    IReadOnlyList<ChapterDiagnostic> Diagnostics);

/// <summary>
/// Describes a built-in Lua expression preset.
/// </summary>
/// <param name="Id">The Id value.</param>
/// <param name="DisplayName">The DisplayName value.</param>
/// <param name="Description">The Description value.</param>
/// <param name="ScriptText">The ScriptText value.</param>
public sealed record LuaExpressionScriptPreset(
    string Id,
    string DisplayName,
    string Description,
    string ScriptText);

/// <summary>
/// Evaluates Lua scripts for chapter time transforms.
/// </summary>
public interface ILuaExpressionScriptService
{
    /// <summary>
    /// Gets built-in Lua expression presets.
    /// </summary>
    IReadOnlyList<LuaExpressionScriptPreset> Presets { get; }

    /// <summary>
    /// Evaluates Lua script text against a chapter expression context.
    /// </summary>
    /// <param name="scriptText">The Lua script text.</param>
    /// <param name="context">The expression context.</param>
    /// <returns>The Lua expression evaluation result.</returns>
    LuaExpressionEvaluationResult Evaluate(string scriptText, LuaExpressionContext context);
}
