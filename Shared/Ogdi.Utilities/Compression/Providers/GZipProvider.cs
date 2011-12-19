
namespace Ogdi.Utilities.Compression
{
    using System.IO;
    using System.IO.Compression;

    public class GZipProvider : ICompressionProvider
    {
        public void Decompress(string filePath, string outputFolderPath)
        {
            var file = new FileInfo(filePath);

            using (FileStream inputFile = file.OpenRead())
            {
                string currentFile = file.FullName;
                string originalName = currentFile.Remove(currentFile.Length - file.Extension.Length);

                using (FileStream outputFile = File.Create(originalName))
                {
                    using (var deflate = new GZipStream(inputFile, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(outputFile);
                    }
                }
            }
        }

        private GZipStream CreateGZipStream(Stream fileStream, CompressionMode mode)
        {
            return new GZipStream(fileStream, mode);
        }
    }
}
