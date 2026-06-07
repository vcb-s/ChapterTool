using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Tests.Transform;

public sealed class ExpressionServiceTests
{
    private readonly ExpressionService service = new();

    [Theory]
    [InlineData("t + 1", 11)]
    [InlineData("fps / 2", 12)]
    [InlineData("floor(1.9) + ceil(1.1)", 3)]
    [InlineData("max(1, 3) + min(2, 4)", 5)]
    [InlineData("M_PI > 3", 1)]
    public void EvaluateInfix_supports_documented_tokens(string expression, decimal expected)
    {
        var result = service.EvaluateInfix(expression, 10, 24);

        Assert.True(result.Success);
        Assert.Equal(expected, Math.Round(result.Value, 6));
    }

    [Fact]
    public void EvaluatePostfix_stops_at_comment()
    {
        var result = service.EvaluatePostfix(["t", "1", "+", "//", "100", "*"], 10, 24);

        Assert.True(result.Success);
        Assert.Equal(11, result.Value);
    }

    [Theory]
    [InlineData("t +")]
    [InlineData("and(1, 2)")]
    [InlineData("1 and 2")]
    public void Invalid_expression_returns_warning_and_original_time(string expression)
    {
        var result = service.EvaluateInfix(expression, 10, 24);

        Assert.False(result.Success);
        Assert.Equal(10, result.Value);
        Assert.Equal("InvalidExpression", Assert.Single(result.Diagnostics).Code);
    }
}
