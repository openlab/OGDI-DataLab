
namespace Ogdi.Utilities.Compression
{
    using System.IO;
    using System.IO.Compression;

    public class DeflateStreamProvider : ICompressionProvider
    {
        public void Decompress(string filePath, string outputFolderPath)
        {
            var file = new FileInfo(filePath);

            using (FileStream inputFile = file.OpenRead())
            {
                string currentFile = file.FullName;
                string originalName = currentFile.Remove(currentFile.Length - file.Extension.Length);
                originalName = string.Concat(originalName);

                using (FileStream outputFile = File.Create(originalName))
                {
                    using (var deflate = new DeflateStream(inputFile, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(outputFile);
                    }
                }
            }
        }

        private DeflateStream CreateDeflateStream(Stream fileStream, CompressionMode mode)
        {
            return new DeflateStream(fileStream, mode);
        }
    }
}
