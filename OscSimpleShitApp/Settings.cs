namespace OscSimpleShitApp;

public class Settings
{
    public string Url { get; set; } = "/chatbox/input";
    public int Delay { get; set; } = 3000;
    public string Text { get; set; } = "Template text";
    public bool ShowDebug { get; set; } = false;

    public string BaseUrl { get; set; } = "127.0.0.1";
    public int BasePort { get; set; } = 9000;

    public override int GetHashCode()
    {
        return Url.GetHashCode() ^ Delay.GetHashCode() ^ Text.GetHashCode();
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

