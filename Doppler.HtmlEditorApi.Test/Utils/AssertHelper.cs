using ReflectionMagic;

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
}
