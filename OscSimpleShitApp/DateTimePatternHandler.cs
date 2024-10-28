namespace OscSimpleShitApp;

public class DateTimePatternHandler : IPatternHandler
{
    public string Pattern => "dt";

    public string Replace(string parameter)
    {
        return DateTime.Now.ToString(parameter);
    }
}
