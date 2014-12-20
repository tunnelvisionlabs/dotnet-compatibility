namespace CompatibilityChecker
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct ParameterSignature
    {
        private readonly BlobReader _reader;

        public ParameterSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public ImmutableArray<CustomModifierSignature> CustomModifiers
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(_reader).CustomModifiers;
            }
        }

        public bool IsByRef
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(_reader).IsByRef;
            }
        }

        public SignatureTypeCode TypeCode
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(_reader).TypeCode;
            }
        }

        public TypeSignature Type
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(_reader).Type;
            }
        }

        public BlobReader Skip()
        {
            // this is always a restriction of RetType
            return new ReturnTypeSignature(_reader).Skip();
        }
    }
}
