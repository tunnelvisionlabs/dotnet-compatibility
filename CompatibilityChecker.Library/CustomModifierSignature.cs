namespace CompatibilityChecker.Library
{
    using System.Reflection.Metadata;

    public struct CustomModifierSignature
    {
        private readonly BlobReader reader;

        public CustomModifierSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public bool IsRequired
        {
            get
            {
                return reader.ReadSignatureTypeCode() == SignatureTypeCode.RequiredModifier;
            }
        }

        public Handle TypeHandle
        {
            get
            {
                var reader = this.reader;
                reader.ReadSignatureTypeCode();
                return reader.ReadTypeHandle();
            }
        }

        public BlobReader Skip()
        {
            var reader = this.reader;
            reader.ReadSignatureTypeCode();
            reader.ReadTypeHandle();
            return reader;
        }
    }
}
