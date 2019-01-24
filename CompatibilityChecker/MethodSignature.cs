namespace CompatibilityChecker
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
        private readonly BlobReader _reader;

        public MethodSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public SignatureHeader Header {
            get {
                return _reader.ReadSignatureHeader();
            }
        }

        public int GenericParameterCount {
            get {
                BlobReader reader = _reader;
                var header = reader.ReadSignatureHeader();

                if (header.IsGeneric)
                    return reader.ReadCompressedInteger();

                throw new InvalidOperationException("Only generic method signatures include a generic parameter count.");
            }
        }

        public ReturnTypeSignature ReturnType {
            get {
                BlobReader reader = _reader;
                var header = reader.ReadSignatureHeader();

                // skip the GenParamCount if present
                if (header.IsGeneric)
                    reader.ReadCompressedInteger();

                // skip the ParamCount
                reader.ReadCompressedInteger();

                return new ReturnTypeSignature(reader);
            }
        }

        public ImmutableArray<ParameterSignature> Parameters {
            get {
                BlobReader reader = _reader;
                var header = reader.ReadSignatureHeader();

                // skip the GenParamCount if present
                if (header.IsGeneric)
                    reader.ReadCompressedInteger();

                // read the ParamCount
                int parameterCount = reader.ReadCompressedInteger();

                // skip the RetType
                reader = new ReturnTypeSignature(reader).Skip();

                var builder = ImmutableArray.CreateBuilder<ParameterSignature>(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                {
                    ParameterSignature parameterSignature = new ParameterSignature(reader);
                    builder.Add(parameterSignature);
                    reader = parameterSignature.Skip();
                }

                return builder.ToImmutable();
            }
        }

        public BlobReader Skip()
        {
            BlobReader reader = _reader;
            var header = reader.ReadSignatureHeader();

            // skip the GenParamCount if present
            if (header.IsGeneric)
                reader.ReadCompressedInteger();

            // read the ParamCount
            int parameterCount = reader.ReadCompressedInteger();

            // skip the RetType
            reader = new ReturnTypeSignature(reader).Skip();

            for (int i = 0; i < parameterCount; i++)
                reader = new ParameterSignature(reader).Skip();

            return reader;
        }
    }
}
