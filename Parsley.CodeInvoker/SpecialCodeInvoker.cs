using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Parsely.CodeInvoker;

public class SpecialCodeInvoker
{
    Assembly? assemblyCache = null;

    public SpecialCodeInvoker(string sorceFileName, Encoding encoding, string head, IEnumerable<MetadataReference>? references, int parentLevel)
        => Initialize(sorceFileName, encoding, head, references, parentLevel);

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
