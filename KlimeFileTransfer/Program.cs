namespace KFT
{
    class Program
    {
        static void Main(string[] args)
        {

            string sourcePath = GetPathsInput("Please enter your source file destination: ");
            string destinationPath = GetPathsInput("Please enter your destination file destination: ");

            CopyFileToDestination(sourcePath, destinationPath);

        }

        static void CopyFileToDestination(string source, string destination)
        {
            FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            FileStream destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write);

            
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