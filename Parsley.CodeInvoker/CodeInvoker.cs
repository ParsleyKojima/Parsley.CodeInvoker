using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

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

public static class FileUtility
{
    public static string? GetFullFileName(string fileName, int parentLevel)
    {
        var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (directoryName is null)
            return null;

        var parentDirectory = new DirectoryInfo(directoryName);
        for (int level = 0; level < parentLevel; level++) {
            if (parentDirectory.Parent is null)
                break;
            parentDirectory = parentDirectory.Parent;
        }

        var files = Directory.GetFiles(parentDirectory.FullName, fileName, SearchOption.AllDirectories);
        return files.Length == 0 ? null : files[0];
    }

    public static string ReadLinesStartHead(string fileName, string head, Encoding encoding)
        => string.Join("\n", Read(fileName, head, encoding));

    static IEnumerable<string> Read(string fileName, string head, Encoding encoding)
    {
        var headLength = head.Length;
        return Read(fileName, encoding).Select(行 => 行.Trim()).Where(行 => 行.StartsWith(head)).Select(行 => 行.Remove(0, headLength));
    }

    static IEnumerable<string> Read(string fileName, Encoding encoding)
    {
        using var stream = new StreamReader(fileName, encoding);
        string? line;
        while ((line = stream.ReadLine()) != null)
            yield return line;
    }
}

public class SpecialCodeInvoker
{
    Assembly? assemblyCache = null;

    public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, IEnumerable<MetadataReference>? references, int parentLevel)
        =>  Initialize(sorceFileName, encoding,head, references, parentLevel);

    public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, Assembly caller, int parentLevel)
    {
        IEnumerable<MetadataReference> references = caller.GetReferencedAssemblies()
                                                          .Select(assembluName => Assembly.Load(assembluName))
                                                          .Select(assemply => MetadataReference.CreateFromFile(assemply.Location))
                                                          .Concat(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        Initialize(sorceFileName, encoding, head, references, parentLevel);
    }

    void Initialize(string sorceFileName, Encoding encoding, string head, IEnumerable<MetadataReference>? references, int parentLevel)
    {
        if (assemblyCache is null) {
            var fileFullName = FileUtility.GetFullFileName(sorceFileName, parentLevel);
            if (fileFullName != null) {
                var code = FileUtility.ReadLinesStartHead(fileFullName, head, encoding);
                assemblyCache = CodeInvoker.CodeToAssembly(code, references);
            }
        }
    }

    public object? StaticInvoke(string typeName, string methodName, object[] parameters)
        => assemblyCache is null ? null : CodeInvoker.StaticInvoke(assemblyCache, typeName, methodName, parameters);

    public object? InstanceInvoke(string typeName, string methodName, object[] parameters)
        => assemblyCache is null ? null : CodeInvoker.InstanceInvoke(assemblyCache, typeName, methodName, parameters);
}
