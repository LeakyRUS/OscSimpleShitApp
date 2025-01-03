namespace OscSimpleShitApp.PatternHandlers;

[Pattern("anim")]
public class AnimationPatternHandler : IPatternHandler
{
    private int _index = 0;
    private string _thisVal = string.Empty;
    private string[] _vals = [];

    private void ReInitIfNeeded(string value)
    {
        if (_thisVal.Equals(value))
            return;

        ReInit(value);
    }

    private void ReInit(string value)
    {
        _index = 0;
        _thisVal = value;
        _vals = value.Split('|');
    }

    private string GetNext()
    {
        var toReturn = _vals[_index];

        _index++;

        if (_index >= _vals.Length)
            _index = 0;

        return toReturn;
    }

    public string Replace(string value)
    {
        ReInitIfNeeded(value);

        return GetNext();
    }
}
