namespace CompatibilityChecker.Library
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct ParameterSignature
    {
        private readonly BlobReader reader;

        public ParameterSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public ImmutableArray<CustomModifierSignature> CustomModifiers
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(reader).CustomModifiers;
            }
        }

        public bool IsByRef
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(reader).IsByRef;
            }
        }

        public SignatureTypeCode TypeCode
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(reader).TypeCode;
            }
        }

        public TypeSignature Type
        {
            get
            {
                // this is always a restriction of RetType
                return new ReturnTypeSignature(reader).Type;
            }
        }

        public BlobReader Skip()
        {
            // this is always a restriction of RetType
            return new ReturnTypeSignature(reader).Skip();
        }
    }
}
