using CoreOSC;
using CoreOSC.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace OscSimpleShitApp;

public class Runner : IDisposable
{
    private readonly IConfiguration _configuration;
    private Settings _settings = new(); // Default settings! Check fields.
    private UdpClient _udpClient = new("127.0.0.1", 9000);
    private static readonly IEnumerable<IPatternHandler> _patternHandlers = GetAllPatternHandlers();
    private Dictionary<string, (IPatternHandler, string)> _handles;

    private bool _disposed = false;

    public Runner(IConfiguration configuration)
    {
        _configuration = configuration;
        ChangeSettingsIfNeeded();
    }

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

            var oscArgs = new object[]{
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
    }

    private void ChangeSettingsIfNeeded()
    {
        var settings = new Settings();

        _configuration.Bind(settings);

        if (settings.Equals(_settings))
            return;

        _settings = settings;

        _udpClient = new UdpClient(_settings.BaseUrl, _settings.BasePort);

        SetupParameters(_settings.Text);
    }

    private static IEnumerable<IPatternHandler> GetAllPatternHandlers()
    {
        var type = typeof(IPatternHandler);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(x => type.IsAssignableFrom(x) && x.IsClass)
            .Select(x => Activator.CreateInstance(x) as IPatternHandler ?? null)
            .Where(x => x != null)
            .Cast<IPatternHandler>()
            .ToList();
    }

    private string HandleReplase(string text)
    {
        foreach(var kv in _handles)
        {
            text = text.Replace(kv.Key, kv.Value.Item1.Replace(kv.Value.Item2));
        }

        return text;
    }

    private void SetupParameters(string text)
    {
        var regex = @"\{(\w{1,20}):([^\{\}]{1,50})\}";
        var matches = Regex.Matches(text, regex);

        var ret = new Dictionary<string, (IPatternHandler, string)>();

        foreach (var match in matches.AsEnumerable())
        {
            if(!match.Success)
                continue;

            if(match.Groups.Count != 3)
                continue;

            var wholeBody = match.Groups[0].Value;
            var pattern = match.Groups[1].Value;
            var parameter = match.Groups[2].Value;

            var patternHandler = _patternHandlers.Where(x => x.Pattern.Equals(pattern)).FirstOrDefault();
            if (patternHandler == null)
                continue;

            ret.Add(wholeBody, (patternHandler, parameter));
        }

        _handles = ret;
    }
}
