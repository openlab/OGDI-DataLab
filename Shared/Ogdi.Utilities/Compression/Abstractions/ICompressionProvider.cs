namespace Ogdi.Utilities.Compression
{
    public interface ICompressionProvider
    {
        void Decompress(string filePath, string outputFolderPath);
    }
}
