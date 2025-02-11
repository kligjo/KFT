using System.Diagnostics;
using System.Security.Cryptography;

namespace KFT
{
    class Program
    {
        static int i = 0;
        static int t1Counter = 0;
        static int t2Counter = 0;
        static async Task Main(string[] args)
        {
            string sourcePath = GetPathsInput("Please enter your source file destination: ");
            string destinationPath = GetPathsInput("Please enter your destination file destination: ");
            FileStream? destinationStream = null; //I had to move it here because of finally block

            try
            {
                destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite); // moved the destination filestream here because I had issues with 1 proccess not being able to access the file

                long fileSize = new FileInfo(sourcePath).Length;
                long halfSize = fileSize / 2;
                Thread t1 = new Thread(() => CopyFileToDestination(sourcePath, destinationStream, 0, halfSize, "t1"));
                Thread t2 = new Thread(() => CopyFileToDestination(sourcePath, destinationStream, halfSize, fileSize - halfSize, "t2"));

                t1.Start();
                t2.Start();

                await Task.Run(() =>
                {
                    t1.Join();
                    t2.Join();
                });

                Console.WriteLine($"t1: {t1Counter} t2: {t2Counter}");
                destinationStream.Close();
                destinationStream.Dispose();
                await VerifyHashForBothFiles(sourcePath, destinationPath);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (CorruptChunkException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (destinationStream != null)
                {
                    destinationStream.Close();
                    destinationStream.Dispose();
                }
                Console.WriteLine("Finished");
            }
        }



        static void CopyFileToDestination(string source, FileStream destinationStream, long startPosition, long bytesRemaining, string thread)
        {
            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);

            sourceStream.Position = startPosition;
            destinationStream.Position = startPosition;

            using var md5 = MD5.Create();
            byte[] buffer = new byte[1024 * 1024];
            var actualBuffer = new byte[1024 * 1024];

            int bytesRead;
            bool isDirtyChunk = false;

            while (bytesRemaining > 0)
            {
                bytesRead = sourceStream.Read(buffer);
                if (bytesRead != buffer.Length)
                {
                    actualBuffer = new byte[bytesRead];
                    Array.Copy(buffer, actualBuffer, bytesRead);
                    buffer = actualBuffer;
                }

                var sourceHash = BitConverter.ToString(md5.ComputeHash(buffer));
                string destinationHash = "";
                lock (destinationStream)
                {

                    destinationStream.Write(buffer, 0, bytesRead);
                    destinationHash = ComputeDestionationHash(buffer.Length, destinationStream, md5);
                }

                Console.WriteLine($"Position {thread}: {Interlocked.Increment(ref i)} {sourceStream.Position / 1024} hash {sourceHash}");


                if (!sourceHash.Equals(destinationHash))
                {
                    sourceStream.Position -= bytesRead; //in case we want to add retry policy. For now I am just throwing an exception
                    destinationStream.Position -= bytesRead;
                    isDirtyChunk = true;
                }

                if (isDirtyChunk)
                {
                    throw new CorruptChunkException("Corrupted chunk found");
                }

                bytesRemaining -= bytesRead;
                _ = thread.Equals("t1") ? t1Counter++ : t2Counter++;
            }

        }

        static async Task VerifyHashForBothFiles(string source, string destination)
        {
            SHA256 sha = SHA256.Create();

            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            using FileStream destinationStream = new FileStream(destination, FileMode.Open, FileAccess.Read);

            var sorceFileHash = await sha.ComputeHashAsync(sourceStream);
            var destinationFileHash = await sha.ComputeHashAsync(destinationStream);

            Console.WriteLine($"Source file hash: {BitConverter.ToString(sorceFileHash).Replace("-", "")}");
            Console.WriteLine($"Destination file hash: {BitConverter.ToString(destinationFileHash).Replace("-", "")}");

        }

        static string GetPathsInput(string userMessage)
        {
            string path;

            Console.WriteLine(userMessage);
            path = Console.ReadLine().Trim();

            return path;
        }

        static string ComputeDestionationHash(int bytesRead, FileStream stream, MD5 md5)
        {
            var tempBuffer = new byte[bytesRead];

            stream.Position -= bytesRead;
            stream.Read(tempBuffer, 0, bytesRead);
            return BitConverter.ToString(md5.ComputeHash(tempBuffer));
        }
    }
}