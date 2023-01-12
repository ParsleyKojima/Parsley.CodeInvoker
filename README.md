# Parsley.CodeInvoker

C# Source Code Invoker Library (.NET Library)


## NuGet

You can install Shos.CsvHelper to your project with [NuGet](https://www.nuget.org) on Visual Studio.

* [NuGet Gallery | Parsley.CodeInvoker](https://github.com/ParsleyKojima/Parsley.CodeInvoker)

### Package Manager

    PM>Install-Package Parsley.CodeInvoker -version 0.1.0.1

### .NET CLI

    >dotnet add package Parsley.CodeInvoker --version 0.1.0.1

### PackageReference

    <PackageReference Include="Parsley.CodeInvoker" Version="0.1.0.1" />

## Projects

* CodeInvoker.cs
    * public static Assembly? CodeToAssembly(string csharpCode, IEnumerable<MetadataReference>? references = null)
    * public static object? StaticInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)
    * public static object? InstanceInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)

* SpecialCodeInvoker.cs
    * public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, IEnumerable<MetadataReference>? references, int parentLevel)
    * public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, Assembly caller, int parentLevel)
    * public object? StaticInvoke(string typeName, string methodName, object[] parameters)
    * public object? InstanceInvoke(string typeName, string methodName, object[] parameters)

* FileUtility.cs
    * public static string? GetFullFileName(string fileName, int parentLevel)
    * public static string ReadLinesStartHead(string fileName, string head, Encoding encoding)

## Author Info

Parsley Kojima

## License

This library is under the MIT License.
