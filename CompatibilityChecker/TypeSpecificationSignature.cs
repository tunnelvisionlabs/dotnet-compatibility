namespace CompatibilityChecker
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct TypeSpecificationSignature
    {
        private readonly BlobReader _reader;

        public TypeSpecificationSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public SignatureTypeCode TypeCode
        {
            get
            {
                return _reader.ReadSignatureTypeCode();
            }
        }

        public Handle TypeHandle
        {
            get
            {
                // this particular case is identical to Type signature for valid TypeSpec signatures
                return new TypeSignature(_reader).TypeHandle;
            }
        }

        public ImmutableArray<TypeSignature> GenericTypeArguments
        {
            get
            {
                // this particular case is identical to Type signature for valid TypeSpec signatures
                return new TypeSignature(_reader).GenericTypeArguments;
            }
        }

        public BlobReader Skip()
        {
            // this particular case is identical to Type signature for valid TypeSpec signatures
            return new TypeSignature(_reader).Skip();
        }
    }
}
