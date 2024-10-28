namespace OscSimpleShitApp;

public interface IPatternHandler
{
    string Pattern { get; }
    string Replace(string value);
}