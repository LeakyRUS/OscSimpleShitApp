using Windows.Media.Control;

namespace OscSimpleShitApp.PatternHandlers;

[Pattern("media")]
public class MediaPatternHandler : IPatternHandler
{
    private static readonly GlobalSystemMediaTransportControlsSessionManager _manager =
        GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetAwaiter().GetResult();

    public string Replace(string parameter)
    {
        var cs = _manager.GetCurrentSession();
        if (cs == null)
            return string.Empty;

        var pi = cs.GetPlaybackInfo();
        var tp = cs.GetTimelineProperties();
        var mp = cs.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();

        var str = $"{pi.PlaybackStatus}: {mp.Artist} - {mp.Title}";

        if (tp.EndTime != TimeSpan.Zero)
        {
            str += $"\n{tp.Position:mm\\:ss} : {tp.EndTime:mm\\:ss}";
        }

        return str;
    }
}
