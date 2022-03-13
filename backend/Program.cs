using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.Devices;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static ServiceClient serviceClient = new ServiceClient();
    static string connectionString = "HostName=camerahub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=Eyrt7QEb47BrfN2SrQPJ3Joid44KeDwan0LJiGI2vJo=";
    static string blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=camerastore;AccountKey=PvlCGnkE83s9CQrx19jAKNo6Q7JmDV4WNCHzXCfacbf3RrIPaRAp6vkHZFn9ASZqjyD6BT0TmhQv+AStje9Gsw==;EndpointSuffix=core.windows.net";
    private static string endpoint = "https://southcentralus.api.cognitive.microsoft.com/";
    private static string key = "e9bb87c37fbf45099441f5edeb5677a1";
    private static string publishedName = "cow_detector";
    private static Guid projectID = Guid.Parse("1b56a559-e57c-4187-9f8d-930b124d4e6a");

    // 2d array representing cow stalls. 1st dimension is camera_id, 
    // 2nd dimension is corresponding stalls. In this case, we have two
    // Cameras, with 3 stalls each.
    static Boolean[,] stalls = new Boolean[2, 3];

    [Obsolete]
    public static void Main()
    {
        Console.WriteLine("Receive file upload notifications\n");

        ReceiveFileUploadNotificationAsync();
        Console.WriteLine("Press Enter to exit\n");
        Console.ReadLine();
    }

    [Obsolete]
    private async static void ReceiveFileUploadNotificationAsync()
    {
        serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        // Create a BlobServiceClient object which will be used to create a container client
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);

        //Create a unique name for the container
        string containerName = "imgblobs" + Guid.NewGuid().ToString();

        // Create the container and return a container client object
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("camerastorecontainer");

        var notificationReceiver = serviceClient.GetFileNotificationReceiver();
        var predictionApi = AuthenticatePrediction(endpoint, key);
        Console.WriteLine("\nReceiving file upload notification from service");
        while (true)
        {
            var fileUploadNotification = await notificationReceiver.ReceiveAsync();
            if (fileUploadNotification == null) continue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received file upload notification: {0}",
              string.Join(", ", fileUploadNotification.BlobName));
            HandleFileUpload("TODO", fileUploadNotification.BlobName, predictionApi, containerClient);
            Console.ResetColor();
            await notificationReceiver.CompleteAsync(fileUploadNotification);
        }
    }
    private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
    {
        // Create a prediction endpoint, passing in the obtained prediction key
        CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
        {
            Endpoint = endpoint
        };
        return predictionApi;
    }

    private static async void HandleFileUpload(string id, string uri, CustomVisionPredictionClient predictionApi, BlobContainerClient containerClient)
    {
        BlobClient blobClient = containerClient.GetBlobClient(uri);
        //var img = new MemoryStream();
        //blobClient.DownloadTo(img);

        string downloadFilePath = "test_download.jpg";
        await blobClient.DownloadToAsync(downloadFilePath);
        var img = new MemoryStream(File.ReadAllBytes(downloadFilePath));

        Console.WriteLine("Making a prediction:");
        try
        {
            var result = predictionApi.DetectImage(projectID, publishedName, img);
            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                if (c.Probability > .9)
                {
                    Console.WriteLine($"\t{c.TagName}: {c.Probability:P1}");
                    Console.WriteLine($"\t{c.TagName}: {c.BoundingBox.Left}, {c.BoundingBox.Top}\n");
                }
            }
        }
        catch (Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.CustomVisionErrorException ex)
        {
            Console.WriteLine(ex.Request.Content);
            Console.WriteLine(ex.Response.Content);
        }
    }
}