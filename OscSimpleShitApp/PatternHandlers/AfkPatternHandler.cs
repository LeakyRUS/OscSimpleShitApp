using static OscSimpleShitApp.Utils.UserInputTrack;

namespace OscSimpleShitApp.PatternHandlers;

[Pattern("afk")]
public class AfkPatternHandler : IPatternHandler
{
    private string _thisVal = string.Empty;
    private int _seconds = 0;
    private string _message = string.Empty;

    private void ReInitIfNeeded(string value)
    {
        if (_thisVal.Equals(value))
            return;

        ReInit(value);
    }

    private void ReInit(string value)
    {
        var splitted = value.Split('|');

        if (splitted.Length != 2)
            return;

        if (!int.TryParse(splitted[0], out _seconds))
            return;

        _thisVal = value;
        _message = splitted[1];
    }

    public string Replace(string value)
    {
        ReInitIfNeeded(value);

        if (IdleTime.TotalSeconds > _seconds)
            return _message;

        return string.Empty;
    }
}
