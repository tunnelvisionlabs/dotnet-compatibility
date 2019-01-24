namespace CompatibilityChecker
{
    using System.Reflection.Metadata;

    public struct CustomModifierSignature
    {
        private readonly BlobReader _reader;

        public CustomModifierSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public bool IsRequired {
            get {
                return _reader.ReadSignatureTypeCode() == SignatureTypeCode.RequiredModifier;
            }
        }

        public Handle TypeHandle {
            get {
                BlobReader reader = _reader;
                reader.ReadSignatureTypeCode();
                return reader.ReadTypeHandle();
            }
        }

        public BlobReader Skip()
        {
            var reader = _reader;
            reader.ReadSignatureTypeCode();
            reader.ReadTypeHandle();
            return reader;
        }
    }
}
