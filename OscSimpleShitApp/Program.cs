using Microsoft.Extensions.Configuration;
using OscSimpleShitApp;

Console.WriteLine("────────────▄▀░░░░░▒▒▒█─\r\n───────────█░░░░░░▒▒▒█▒█\r\n──────────█░░░░░░▒▒▒█▒░█\r\n────────▄▀░░░░░░▒▒▒▄▓░░█\r\n───────█░░░░░░▒▒▒▒▄▓▒░▒▓\r\n──────█▄▀▀▀▄▄▒▒▒▒▓▀▒░░▒▓\r\n────▄▀░░░░░░▒▀▄▒▓▀▒░░░▒▓\r\n───█░░░░░░░░░▒▒▓▀▒░░░░▒▓\r\n───█░░░█░░░░▒▒▓█▒▒░░░▒▒▓\r\n────█░░▀█░░▒▒▒█▒█░░░░▒▓▀\r\n─────▀▄▄▀▀▀▄▄▀░█░░░░▒▒▓─\r\n───────────█▒░░█░░░▒▒▓▀─\r\n────────────█▒░░█▒▒▒▒▓──\r\n─────────────▀▄▄▄▀▄▄▀─");

var configurationRoot = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Console.WriteLine(configurationRoot.GetDebugView());

using var cts = new CancellationTokenSource();

Console.WriteLine("Press Ctrl + C to stop the program.");

Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

try
{
    using var runner = new Runner(configurationRoot);
    runner.Run(cts.Token).Wait();
}
catch (AggregateException)
{
}