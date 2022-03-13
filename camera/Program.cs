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

            // Parse Camera ID
            var folder = parameters.CameraID;
            Console.WriteLine(folder);

            // Upload photo to cloud storge
            var sample = new FileUpload(deviceClient);

            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Sending image: " + folder + "/img_" + (i + 1).ToString() + ".jpg");
                    await sample.RunSampleAsync(folder + "/img_" + (i + 1).ToString() + ".jpg");
                    System.Threading.Thread.Sleep(10_000);
                }
            }
/*
            await deviceClient.CloseAsync();

            Console.WriteLine("Done.");
            return 0;
*/
        }

    }
}