
namespace Ogdi.Utilities.Compression
{
    using System;
    using System.IO;

    public class CompressionHelper
    {
        private readonly ICompressionProvider provider;

        public CompressionHelper(ICompressionProvider compressionProvider)
        {
            this.provider = compressionProvider;
        }

        public void Decompress(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException();

            provider.Decompress(file.FullName, file.DirectoryName);
        }
    }
}
