using CommandLine;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Parse application parameters
            Parameters parameters = new Parameters();
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parsedParams =>
                {
                    parameters = parsedParams;
                })
                .WithNotParsed(errors =>
                {
                    Environment.Exit(1);
                });

            using var deviceClient = DeviceClient.CreateFromConnectionString(
                parameters.PrimaryConnectionString,
                parameters.TransportType);
            
            var sample = new FileUpload(deviceClient);
            await sample.RunSampleAsync();

            await deviceClient.CloseAsync();

            Console.WriteLine("Done.");
            return 0;
        }

    }
}