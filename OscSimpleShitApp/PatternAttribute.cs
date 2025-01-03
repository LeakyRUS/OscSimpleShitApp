namespace OscSimpleShitApp;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PatternAttribute(string Pattern) : Attribute
{
    public string Pattern { get; } = Pattern;
}
