# Parsley.CodeInvoker

C# Source Code Invoker Library (.NET Library)

* Project Files

** FileUtility.cs
** CodeInvoker.cs

- public static Assembly? CodeToAssembly(string csharpCode, IEnumerable<MetadataReference>? references = null)
- public static object? StaticInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)
- public static object? InstanceInvoke(Assembly assembly, string typeName, string methodName, object[] parameters)


** SpecialCodeInvoker.cs

- public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, IEnumerable<MetadataReference>? references, int parentLevel)
- public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, Assembly caller, int parentLevel)
- public object? StaticInvoke(string typeName, string methodName, object[] parameters)
- public object? InstanceInvoke(string typeName, string methodName, object[] parameters)

** FileUtility.cs

- public static string? GetFullFileName(string fileName, int parentLevel)
- public static string ReadLinesStartHead(string fileName, string head, Encoding encoding)
