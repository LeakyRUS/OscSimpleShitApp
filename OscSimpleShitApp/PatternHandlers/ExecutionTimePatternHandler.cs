using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OscSimpleShitApp.PatternHandlers;

[Pattern("pid")]
public class ExecutionTimePatternHandler : IPatternHandler
{
    public string Replace(string value)
    {
        var process = Process.GetProcessesByName(value).FirstOrDefault();
        if (process == null)
            return string.Empty;

        return string.Format("{0} uptime: {1}", process.ProcessName, (DateTime.Now - process.StartTime).ToString("hh\\:mm\\:ss"));
    }
}
