namespace CompatibilityChecker.Library
{
    using System.Reflection.Metadata;

    public static class BlobReaderExtensions
    {
        public static bool IsCustomModifier(this BlobReader blobReader)
        {
            switch (blobReader.ReadSignatureTypeCode())
            {
                case SignatureTypeCode.OptionalModifier:
                case SignatureTypeCode.RequiredModifier:
                    return true;

                default:
                    return false;
            }
        }

        public static SignatureTypeCode PeekSignatureTypeCode(this BlobReader reader)
        {
            return reader.ReadSignatureTypeCode();
        }
    }
}
