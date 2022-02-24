using System.Text.RegularExpressions;
using ReflectionMagic;
using Xunit;

namespace Doppler.HtmlEditorApi.Test.Utils;

public static class AssertHelper
{
    private static readonly Regex LINE_ENDINGS_REGEX = new Regex(@"[\r\n]");
    private static readonly Regex SPACES_REGEX = new Regex(@"\s");

    public static bool GetValueAndContinue<T>(T input, out T output)
    {
        output = input;
        return true;
    }

    public static bool GetDynamicValueAndContinue(object input, out dynamic output)
    {
        output = input.AsDynamic();
        return true;
    }

    public static void EqualIgnoringSpaces(string expected, string actual)
        => Assert.Equal(RemoveSpaces(expected), RemoveSpaces(actual));

    public static string RemoveSpaces(string str)
        => str == null ? null
        : SPACES_REGEX.Replace(
            LINE_ENDINGS_REGEX.Replace(str, string.Empty),
            string.Empty);
}
