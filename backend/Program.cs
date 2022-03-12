using Microsoft.Azure.Devices;

class Program
{
    static ServiceClient serviceClient;
    static string connectionString = "HostName=camerahub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=Eyrt7QEb47BrfN2SrQPJ3Joid44KeDwan0LJiGI2vJo=";

    [Obsolete]
    public static void Main()
    {
        Console.WriteLine("Receive file upload notifications\n");
        serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        ReceiveFileUploadNotificationAsync();
        Console.WriteLine("Press Enter to exit\n");
        Console.ReadLine();
    }

    [Obsolete]
    private async static void ReceiveFileUploadNotificationAsync()
    {
        var notificationReceiver = serviceClient.GetFileNotificationReceiver();
        Console.WriteLine("\nReceiving file upload notification from service");
        while (true)
        {
            var fileUploadNotification = await notificationReceiver.ReceiveAsync();
            if (fileUploadNotification == null) continue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received file upload notification: {0}",
              string.Join(", ", fileUploadNotification.BlobName));
            Console.ResetColor();
            await notificationReceiver.CompleteAsync(fileUploadNotification);
        }
    }
}