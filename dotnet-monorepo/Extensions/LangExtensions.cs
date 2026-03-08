namespace DotnetMonorepo.Extensions;

public static class LangExtensions
{
    extension<TIn>(TIn obj)
    {
        internal TOut Let<TOut>(Func<TIn, TOut> mapper) => mapper(obj);

        internal TIn Apply(Action<TIn> action)
        {
            action(obj);
            return obj;
        }
    }
}