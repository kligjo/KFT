
namespace KFT
{
    internal class CorruptChunkException : Exception
    {
        public CorruptChunkException()
        {
        }

        public CorruptChunkException(string? message) : base(message)
        {
        }
    }
}