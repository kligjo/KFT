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
            try
            {
                await CopyFileToDestination(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greskaa");
            }

        }

        static async Task CopyFileToDestination(string source, string destination)
        {
            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            using FileStream destinationStream = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite);// adding read/write so that we can read the written bytes fromthe new location to hash them and check the hashes
            using var md5 = MD5.Create();
            byte[] buffer = new byte[1024 * 1024];

            int bytesRead;
            int position = 0;

            while ((bytesRead = sourceStream.Read(buffer)) > 0)
            {
                var sourceHash = BitConverter.ToString(md5.ComputeHash(buffer));

                await destinationStream.WriteAsync(buffer, 0, bytesRead);


                var destinationHash = await ComputeDestionationHash(bytesRead, destinationStream, md5); ;

                Console.WriteLine($"Position {position} hash {destinationHash}");
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

        static async Task<string> ComputeDestionationHash(int bytesRead, FileStream stream, MD5 md5)
        {
            var tempBuffer = new byte[bytesRead];

            stream.Position -= bytesRead;
            await stream.ReadAsync(tempBuffer, 0, bytesRead);
            return BitConverter.ToString(md5.ComputeHash(tempBuffer));
        }
    }
}