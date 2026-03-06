// Required to use C# 9+ 'record' and 'init' when targeting netstandard2.0
// See: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/records
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
