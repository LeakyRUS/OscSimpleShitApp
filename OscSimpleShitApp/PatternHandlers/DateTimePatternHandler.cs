namespace OscSimpleShitApp.PatternHandlers;

[Pattern("dt")]
public class DateTimePatternHandler : IPatternHandler
{
    public string Replace(string parameter)
    {
        return DateTime.Now.ToString(parameter);
    }
}
