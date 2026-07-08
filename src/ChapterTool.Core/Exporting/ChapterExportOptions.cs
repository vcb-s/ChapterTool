namespace ChapterTool.Core.Exporting;

/// <summary>
/// Describes options for chapter export and output projection.
/// </summary>
/// <param name="Format">The Format value.</param>
/// <param name="XmlLanguage">The XmlLanguage value.</param>
/// <param name="SourceFileName">The SourceFileName value.</param>
/// <param name="AutoGenerateNames">The AutoGenerateNames value.</param>
/// <param name="UseTemplateNames">The UseTemplateNames value.</param>
/// <param name="ChapterNameTemplateText">The ChapterNameTemplateText value.</param>
/// <param name="OrderShift">The OrderShift value.</param>
/// <param name="ApplyExpression">The ApplyExpression value.</param>
/// <param name="Expression">The Expression value.</param>
/// <param name="LuaExpressionPresetId">The LuaExpressionPresetId value.</param>
/// <param name="LuaExpressionSourceName">The LuaExpressionSourceName value.</param>
/// <param name="EmitBom">The EmitBom value.</param>
/// <param name="ProjectOutput">The ProjectOutput value.</param>
public sealed record ChapterExportOptions(
    ChapterExportFormat Format,
    string? XmlLanguage = null,
    string? SourceFileName = null,
    bool AutoGenerateNames = false,
    bool UseTemplateNames = false,
    string ChapterNameTemplateText = "",
    int OrderShift = 0,
    bool ApplyExpression = false,
    string Expression = "t",
    string LuaExpressionPresetId = "",
    string LuaExpressionSourceName = "",
    bool EmitBom = true,
    bool ProjectOutput = true);
