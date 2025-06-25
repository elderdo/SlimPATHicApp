using System.Runtime.InteropServices;
using System.Text;

namespace SlimPATHic
{
    // Helper class for path shortening
    static class PathShortener
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        public static string GetShortPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (!Directory.Exists(path) && !File.Exists(path)) return path;
            var buffer = new StringBuilder(260);
            GetShortPathName(path, buffer, (uint)buffer.Capacity);
            return buffer.ToString();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string ProcessPath(string? original)
            {
                if (string.IsNullOrEmpty(original))
                    return string.Empty;

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var parts = original.Split(';')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(PathShortener.GetShortPath)
                    .Where(p => seen.Add(p));

                return string.Join(";", parts);
            }

            void BackupAndSet(string scopeName, EnvironmentVariableTarget target)
            {
                string? original = Environment.GetEnvironmentVariable("Path", target);
                string backupName = $"Path_Backup_{DateTime.Now:yyyyMMdd_HHmmss}";

                Environment.SetEnvironmentVariable(backupName, original, target);

                string shortened = ProcessPath(original);
                Environment.SetEnvironmentVariable("Path", shortened, target);

                Console.WriteLine($"[{scopeName}] PATH updated. Backup saved as {backupName}");
            }

            Console.WriteLine("🔧 Shortening PATH variables...");

            try
            {
                BackupAndSet("USER", EnvironmentVariableTarget.User);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[USER] Failed: {ex.Message}");
            }

            try
            {
                BackupAndSet("SYSTEM", EnvironmentVariableTarget.Machine);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("[SYSTEM] ❌ Admin rights required to modify system PATH.");
            }
        }
    }
}