using CoreOSC;
using CoreOSC.IO;
using Microsoft.Extensions.Configuration;
using OscSimpleShitApp.PatternHandlers;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace OscSimpleShitApp;

public class Runner(IConfiguration configuration) : IDisposable
{
    private readonly IConfiguration _configuration = configuration;
    private Settings _settings = new(); // Default settings! Check fields.
    private UdpClient _udpClient = new("127.0.0.1", 9000);
    private Dictionary<string, (IPatternHandler, string)> _handles = [];

    private record PatternHolder(string WholeBody, string Pattern, string Parameter);

    private bool _disposed = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            _udpClient.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var stopWatch = new Stopwatch();

        while (!cancellationToken.IsCancellationRequested)
        {
            stopWatch.Start();

            ChangeSettingsIfNeeded();

            await SendOscMessage(cancellationToken);

            stopWatch.Stop();

            await Task.Delay(_settings.Delay - (int)stopWatch.ElapsedMilliseconds, cancellationToken);
        }
    }

    private string HandleReplase(string text)
    {
        var sb = new StringBuilder(text);

        foreach (var kv in _handles)
            sb.Replace(kv.Key, kv.Value.Item1.Replace(kv.Value.Item2));

        return sb.ToString();
    }

    private async Task SendOscMessage(CancellationToken cancellationToken)
    {
        var oscArgs = new object[]
        {
            HandleReplase(_settings.Text),
            OscTrue.True,
            OscFalse.False
        };
        var message = new OscMessage(new Address(_settings.Url), oscArgs);

        Debug(message.Address.Value + " " + string.Join(" ", oscArgs.Select(x => x?.ToString()?.Replace("\n", " ") ?? string.Empty)));

        await _udpClient.SendMessageAsync(message);
    }

    private void ChangeSettingsIfNeeded()
    {
        var settings = new Settings();

        _configuration.Bind(settings);

        if (settings.Equals(_settings))
            return;

        _settings = settings;

        SetupRunner();
    }

    private void SetupRunner()
    {
        _udpClient.Dispose();
        _udpClient = new UdpClient(_settings.BaseUrl, _settings.BasePort);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(_settings.Locale);

        SetupParameters();
    }

    private void SetupParameters()
    {
        var ret = new Dictionary<string, (IPatternHandler, string)>();

        foreach (var match in GetParameters())
        {
            var patternHandler = GetPatternHandler(match.Pattern);
            if (patternHandler == null)
            {
                Warning(string.Format("Can't handle \"{0}\". Pattern will be shown as text.", match.WholeBody));
                continue;
            }

            if (ret.ContainsKey(match.WholeBody))
                continue;

            ret.Add(match.WholeBody, (TryPatternHandler(patternHandler, match.Parameter, match.WholeBody) ? patternHandler : new BoilerplatePatternHandler(), match.Parameter));
        }

        _handles = ret;
    }

    private IEnumerable<PatternHolder> GetParameters()
    {
        var result = new List<PatternHolder>();
        var text = _settings.Text;

        while (true)
        {
            var first = text.IndexOf('{');
            if (first == -1)
                break;

            var second = text.IndexOf('}');
            if (second == -1)
                break;

            second++;

            if (second <= first)
            {
                text = text[second..^0];
                continue;
            }

            var substring = text[first..second];
            var colon = substring.IndexOf(':');
            if (colon != -1)
            {
                var pattern = substring[1..colon];
                var parameter = substring[(colon + 1)..^1];

                if (pattern.Length > 0)
                    result.Add(new PatternHolder(substring, pattern, parameter));
            }

            text = text[second..^0];
        }

        return result;
    }

    private void Debug(string message)
    {
        SendMessageWithColor(Console.Out, message);
    }

    private void Warning(string message)
    {
        SendMessageWithColor(Console.Out, message, ConsoleColor.Cyan, ConsoleColor.Magenta);
    }

    private void Error(string message)
    {
        SendMessageWithColor(Console.Error, message, ConsoleColor.Red);
    }

    private bool TryPatternHandler(IPatternHandler patternHandler, string parameter, string wholePattern)
    {
        try
        {
            patternHandler.Replace(parameter);
            return true;
        }
        catch(Exception ex)
        {
            Error(string.Format("Pattern \"{0}\" does not work for reason \"{1}\". Try changing the pattern parameter or remove the pattern.", wholePattern, ex.Message));

            return false;
        }
    }

    private void SendMessageWithColor(TextWriter writer, string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        if (!_settings.ShowDebug)
            return;

        var fColor = Console.ForegroundColor;
        var bColor = Console.BackgroundColor;

        if (foregroundColor.HasValue)
        {
            Console.ForegroundColor = foregroundColor.Value;
        }

        if (backgroundColor.HasValue)
        {
            Console.BackgroundColor = backgroundColor.Value;
        }

        writer.WriteLine(message);

        Console.ForegroundColor = fColor;
        Console.BackgroundColor = bColor;
    }

    private static IPatternHandler? GetPatternHandler(string pattern)
    {
        var type = typeof(IPatternHandler);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(x => type.IsAssignableFrom(x) && x.IsClass)
            .Where(x => (Attribute.GetCustomAttribute(x, typeof(PatternAttribute)) as PatternAttribute)?.Pattern.Equals(pattern) ?? false)
            .Select(x => Activator.CreateInstance(x) as IPatternHandler ?? null)
            .Where(x => x != null)
            .FirstOrDefault();
    }
}
