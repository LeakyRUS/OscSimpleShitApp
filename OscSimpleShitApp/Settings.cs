namespace OscSimpleShitApp;

public class Settings
{
    public string Url { get; set; } = "/chatbox/input";
    public int Delay { get; set; } = 5000;
    public string Text { get; set; } = "{anim:Test01|Test02}\n{anim:Test11|Test12|Test13}\n{dt:t}\n{afk:5|AfkTest}";
    public bool ShowDebug { get; set; } = false;

    public string BaseUrl { get; set; } = "127.0.0.1";
    public int BasePort { get; set; } = 9000;
    public string Locale { get; set; } = "en-US";

    public override int GetHashCode()
    {
        return Url.GetHashCode()
            ^ Delay.GetHashCode()
            ^ Text.GetHashCode()
            ^ ShowDebug.GetHashCode()
            ^ BaseUrl.GetHashCode()
            ^ BasePort.GetHashCode()
            ^ Locale.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is Settings set)
            return GetHashCode() == set.GetHashCode();
        else
            return false;
    }
}

