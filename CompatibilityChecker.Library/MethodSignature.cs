namespace CompatibilityChecker.Library
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    /// <summary>
    /// This structure represents a metadata MethodDef or MethodRef signature, as described in ECMA-335 §II.23.2.1 and
    /// §II.23.2.2.
    /// </summary>
    public struct MethodSignature
    {
        private readonly BlobReader reader;

        public MethodSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public SignatureHeader Header
        {
            get
            {
                return reader.ReadSignatureHeader();
            }
        }

        public int GenericParameterCount
        {
            get
            {
                var reader = this.reader;
                var header = reader.ReadSignatureHeader();

                if (header.IsGeneric)
                {
                    return reader.ReadCompressedInteger();
                }

                throw new InvalidOperationException("Only generic method signatures include a generic parameter count.");
            }
        }

        public ReturnTypeSignature ReturnType
        {
            get
            {
                var reader = this.reader;
                var header = reader.ReadSignatureHeader();

                // skip the GenParamCount if present
                if (header.IsGeneric)
                {
                    reader.ReadCompressedInteger();
                }

                // skip the ParamCount
                reader.ReadCompressedInteger();

                return new ReturnTypeSignature(reader);
            }
        }

        public ImmutableArray<ParameterSignature> Parameters
        {
            get
            {
                var reader = this.reader;
                var header = reader.ReadSignatureHeader();

                // skip the GenParamCount if present
                if (header.IsGeneric)
                {
                    reader.ReadCompressedInteger();
                }

                // read the ParamCount
                int parameterCount = reader.ReadCompressedInteger();

                // skip the RetType
                reader = new ReturnTypeSignature(reader).Skip();

                var builder = ImmutableArray.CreateBuilder<ParameterSignature>(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                {
                    ParameterSignature parameterSignature = new (reader);
                    builder.Add(parameterSignature);
                    reader = parameterSignature.Skip();
                }

                return builder.ToImmutable();
            }
        }

        public BlobReader Skip()
        {
            var reader = this.reader;
            var header = reader.ReadSignatureHeader();

            // skip the GenParamCount if present
            if (header.IsGeneric)
            {
                reader.ReadCompressedInteger();
            }

            // read the ParamCount
            int parameterCount = reader.ReadCompressedInteger();

            // skip the RetType
            reader = new ReturnTypeSignature(reader).Skip();

            for (int i = 0; i < parameterCount; i++)
            {
                reader = new ParameterSignature(reader).Skip();
            }

            return reader;
        }
    }
}
