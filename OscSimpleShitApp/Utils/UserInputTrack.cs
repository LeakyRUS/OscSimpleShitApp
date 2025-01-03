using System.Runtime.InteropServices;

namespace OscSimpleShitApp.Utils;

// https://stackoverflow.com/questions/15845508/get-idle-time-of-machine/
public static class UserInputTrack
{
    [DllImport("user32.dll", SetLastError = false)]
#pragma warning disable SYSLIB1054
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
#pragma warning restore SYSLIB1054

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public int dwTime;
    }

    public static DateTime LastInput
    {
        get
        {
            var bootTime = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
            var lastInput = bootTime.AddMilliseconds(LastInputTicks);
            return lastInput;
        }
    }

    public static TimeSpan IdleTime
    {
        get
        {
            return DateTime.UtcNow.Subtract(LastInput);
        }
    }

    public static int LastInputTicks
    {
        get
        {
            var lii = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            };
            GetLastInputInfo(ref lii);
            return lii.dwTime;
        }
    }
}
