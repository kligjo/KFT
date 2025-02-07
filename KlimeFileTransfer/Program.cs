namespace KFT
{
    class Program
    {
        static void Main(string[] args)
        {

            string sourcePath = GetPathsInput("Please enter your source file destination");
            string destinationPath = GetPathsInput("Please enter your destination file destination");

            FileStream sourceStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Read);
            FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
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