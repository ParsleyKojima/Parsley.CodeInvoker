using System.Reflection;
using System.Text;

namespace Parsely.CodeInvoker
{
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
}
