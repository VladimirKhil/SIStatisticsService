using SIStatisticsService.Helpers;

namespace SIStatisticsService.UnitTests.Helpers;

internal sealed class ValueNormalizerTests
{
    [TestCase(" aaa  bb", "aaa bb")]
    [TestCase("c    t", "c t")]
    public void Normalize_Ok(string input, string output)
    {
        var result = ValueNormalizer.Normalize(input);
        Assert.That(result, Is.EqualTo(output));
    }
}