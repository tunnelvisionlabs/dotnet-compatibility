namespace CompatibilityChecker.Library
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct TypeSpecificationSignature
    {
        private readonly BlobReader reader;

        public TypeSpecificationSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public SignatureTypeCode TypeCode
        {
            get
            {
                return reader.ReadSignatureTypeCode();
            }
        }

        public Handle TypeHandle
        {
            get
            {
                // this particular case is identical to Type signature for valid TypeSpec signatures
                return new TypeSignature(reader).TypeHandle;
            }
        }

        public ImmutableArray<TypeSignature> GenericTypeArguments
        {
            get
            {
                // this particular case is identical to Type signature for valid TypeSpec signatures
                return new TypeSignature(reader).GenericTypeArguments;
            }
        }

        public BlobReader Skip()
        {
            // this particular case is identical to Type signature for valid TypeSpec signatures
            return new TypeSignature(reader).Skip();
        }
    }
}
