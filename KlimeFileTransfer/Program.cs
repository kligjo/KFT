using System.Diagnostics;
using System.Security.Cryptography;

namespace KFT
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = "C:\\Users\\klime\\Downloads\\testFile.zip";// GetPathsInput("Please enter your source file destination: ");
            string destinationPath = "C:\\Users\\klime\\Desktop\\testFile.zip";// GetPathsInput("Please enter your destination file destination: ");

            try
            {
                await CopyFileToDestination(sourcePath, destinationPath);
            }
            catch (DirtChunkException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        static async Task CopyFileToDestination(string source, string destination)
        {
            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            using FileStream destinationStream = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite);// adding read/write so that we can read the written bytes fromthe new location to hash them and check the hashes
            using var md5 = MD5.Create();
            byte[] buffer = new byte[1024 * 1024];
            var actualBuffer = new byte[1024 * 1024];

            int bytesRead;
            bool isDirtyChunk = false;

            while ((bytesRead = sourceStream.Read(buffer)) > 0)
            {
                if (bytesRead != buffer.Length)
                {
                    actualBuffer = new byte[bytesRead];
                    Array.Copy(buffer, actualBuffer, bytesRead);
                    buffer = actualBuffer;
                }

                var sourceHash = BitConverter.ToString(md5.ComputeHash(buffer));

                await destinationStream.WriteAsync(buffer, 0, bytesRead);

                var destinationHash = await ComputeDestionationHash(buffer.Length, destinationStream, md5);

                Console.WriteLine($"Position {destinationStream.Position / 1024} hash {sourceStream.Position / 1024}");


                if (!sourceHash.Equals(destinationHash))
                {
                    sourceStream.Position -= bytesRead;
                    destinationStream.Position -= bytesRead;
                    isDirtyChunk = true;
                }

                if (isDirtyChunk)
                {
                    throw new DirtChunkException("Dirty chunk found");
                }
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