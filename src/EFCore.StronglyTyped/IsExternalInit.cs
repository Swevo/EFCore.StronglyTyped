// Polyfill required to use 'record' and 'init' properties when targeting netstandard2.0.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
