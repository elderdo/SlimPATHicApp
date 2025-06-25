using System.Runtime.InteropServices;
using System.Text;

namespace SlimPATHic;

// Helper class for path shortening
static class PathShortener
{
    // Import the GetShortPathName function from kernel32.dll to convert long paths to short (8.3) format
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

    // Returns the short (8.3) path for a given file or directory path
    public static string GetShortPath(string path)
    {
        // If the path is null or empty, return it as is
        if (string.IsNullOrEmpty(path)) return path;
        // If the path does not exist as a file or directory, return it as is
        if (!Directory.Exists(path) && !File.Exists(path)) return path;
        // Create a buffer to hold the short path (max 260 chars)
        var buffer = new StringBuilder(260);
        // Call the native method to get the short path
        GetShortPathName(path, buffer, (uint)buffer.Capacity);
        // Return the resulting short path as a string
        return buffer.ToString();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Processes a PATH-like string, shortens each path, removes duplicates, and returns the result
        string ProcessPath(string? original)
        {
            // If the input is null or empty, return an empty string
            if (string.IsNullOrEmpty(original))
                return string.Empty;

            // Create a HashSet to track unique paths (case-insensitive)
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // LINQ expression to process the PATH string:
            // 1. Split the original string by ';' to get individual path entries
            // 2. Trim whitespace from each path
            // 3. Filter out any empty entries
            // 4. Convert each path to its short (8.3) form
            // 5. Add each path to the 'seen' set and only keep unique ones
            // The result is an IEnumerable<string> of unique, shortened paths
            var parts = original.Split(';')
                .Select(p => p.Trim()) // Remove leading/trailing whitespace from each path
                .Where(p => !string.IsNullOrEmpty(p)) // Exclude empty paths
                .Select(PathShortener.GetShortPath) // Convert to short path format
                .Where(p => seen.Add(p)); // Only keep paths not already in 'seen' (ensures uniqueness)

            // Join the unique, shortened paths back into a single string separated by ';'
            return string.Join(";", parts);
        }

        // Backs up the current PATH variable, shortens it, and sets the new value
        void BackupAndSet(string scopeName, EnvironmentVariableTarget target)
        {
            // Get the current PATH value for the specified environment variable target
            string? original = Environment.GetEnvironmentVariable("Path", target);
            // Create a backup variable name with a timestamp
            string backupName = $"Path_Backup_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Save the original PATH value as a backup
            Environment.SetEnvironmentVariable(backupName, original, target);

            // Process (shorten and deduplicate) the PATH value
            string shortened = ProcessPath(original);
            // Set the shortened PATH value
            Environment.SetEnvironmentVariable("Path", shortened, target);

            // Inform the user that the PATH was updated and backed up
            Console.WriteLine($"[{scopeName}] PATH updated. Backup saved as {backupName}");
        }

        // Inform the user that the process is starting
        Console.WriteLine("🔧 Shortening PATH variables...");

        // Try to update the user PATH variable
        try
        {
            BackupAndSet("USER", EnvironmentVariableTarget.User);
        }
        catch (Exception ex)
        {
            // If an error occurs, display the error message
            Console.WriteLine($"[USER] Failed: {ex.Message}");
        }

        // Try to update the system PATH variable
        try
        {
            BackupAndSet("SYSTEM", EnvironmentVariableTarget.Machine);
        }
        catch (UnauthorizedAccessException)
        {
            // If admin rights are required and not present, inform the user
            Console.WriteLine("[SYSTEM] ❌ Admin rights required to modify system PATH.");
        }
    }
}
