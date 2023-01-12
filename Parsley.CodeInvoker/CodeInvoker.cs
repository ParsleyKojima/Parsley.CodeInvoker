using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Parsely.CodeInvoker;

// dotnet add package Microsoft.CodeAnalysis.CSharp --version 4.4.0
public static class CodeInvoker
{
    public static Assembly? CodeToAssembly(string csharpCode, IEnumerable<MetadataReference>? references = null)
    {
        var fileName = Guid.NewGuid().ToString();

        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);
        var sourceCodeName = $"{fileName}.cs";
        var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode, options, sourceCodeName);

        var dllName = $"{fileName}.dll";
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(dllName, new[] { syntaxTree }, references, compilationOptions);

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (emitResult.Success) {
            stream.Seek(0, SeekOrigin.Begin);
            var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
            return assembly;
        } else {
            foreach (var diagnostic in emitResult.Diagnostics) {
                var pos = diagnostic.Location.GetLineSpan();
                var location =
                    "(" + pos.Path + "@Line" + (pos.StartLinePosition.Line + 1) +
                    ":" +
                    (pos.StartLinePosition.Character + 1) + ")";
                Debug.WriteLine(
                    $"[{diagnostic.Severity}, {location}]{diagnostic.Id}, {diagnostic.GetMessage()}"
                );
            }
            return null;
        }
    }

    static Dictionary<(string, string), MethodInfo> staticMethodCache = new();

    public static object? StaticInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)
    {
        if (!staticMethodCache.TryGetValue((typeName, methodName), out var method)) {
            var type = assembly.GetType(typeName);
            if (type == null)
                return null;

            method = type.GetMethod(methodName);
            if (method == null)
                return null;
            staticMethodCache[(typeName, methodName)] = method;
        }
        return method.Invoke(null, parameters);
    }

    public static object? InstanceInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)
    {
        var type = assembly.GetType(typeName);
        if (type == null)
            return null;

        var instance = Activator.CreateInstance(type);
        var method = type.GetMethod(methodName);
        if (method == null)
            return null;

        return method.Invoke(instance, parameters);
    }
}
