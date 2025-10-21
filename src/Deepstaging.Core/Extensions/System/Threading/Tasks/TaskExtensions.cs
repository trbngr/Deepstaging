namespace Deepstaging.Threading.Tasks;

public static class Extensions
{
    public static Task<T> AsTask<T>(this T result) => Task.FromResult(result);
}