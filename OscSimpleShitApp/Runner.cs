using CoreOSC;
using CoreOSC.IO;
using Microsoft.Extensions.Configuration;
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
        while (!cancellationToken.IsCancellationRequested)
        {
            ChangeSettingsIfNeeded();

            await SendOscMessage(cancellationToken);
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

        if (_settings.ShowDebug)
            Console.WriteLine(message.Address.Value + " " + string.Join(" ", oscArgs.Select(x => x?.ToString()?.Replace("\n", " ") ?? string.Empty)));

        await _udpClient.SendMessageAsync(message);

        await Task.Delay(_settings.Delay, cancellationToken);
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
                continue;

            if (ret.ContainsKey(match.WholeBody))
                continue;
            ret.Add(match.WholeBody, (patternHandler, match.Parameter));
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
