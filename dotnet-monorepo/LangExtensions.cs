namespace DotnetMonorepo;

public static class LangExtensions
{
    internal static TOut Let<TIn, TOut>(this TIn obj, Func<TIn, TOut> mapper) => mapper(obj);

    internal static T Apply<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}