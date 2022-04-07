using System.Linq;
using ReflectionMagic;
using Xunit;

namespace Doppler.HtmlEditorApi.Test.Utils;

public static class AssertHelper
{
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

    public static void EqualIgnoringMeaninglessSpaces(string expected, string actual)
        => Assert.Equal(RemoveMeaninglessSpaces(expected), RemoveMeaninglessSpaces(actual));

    public static string RemoveMeaninglessSpaces(string str)
        => str == null ? null
        : string.Join(
            '\n',
            str.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)));
}
