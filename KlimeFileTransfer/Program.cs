using System.Diagnostics;
using System.Security.Cryptography;

namespace KFT
{
    class Program
    {
        static async Task Main(string[] args)
        {

            string sourcePath = GetPathsInput("Please enter your source file destination: ");
            string destinationPath = GetPathsInput("Please enter your destination file destination: ");

            await CopyFileToDestination(sourcePath, destinationPath);

        }

        static async Task CopyFileToDestination(string source, string destination)
        {
            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            using FileStream destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write);
            using var md5 = MD5.Create();
            byte[] buffer = new byte[1024*1024];
            
            int bytesRead;
            int position = 0;
            string loadingMessage = "";

            while ((bytesRead = sourceStream.Read(buffer)) > 0)
            {
                var sourceHash = md5.ComputeHash(buffer);
                await destinationStream.WriteAsync(buffer, 0, bytesRead);

                var tempBuffer = new byte[bytesRead];
                Console.WriteLine($"Position {position} hash {BitConverter.ToString(sourceHash)}");
                position += 1024;
               

            }


        }

        static string GetPathsInput(string userMessage)
        {
            string path;

            Console.WriteLine(userMessage);
            path = Console.ReadLine().Trim();

            return path;

        }
    }
}