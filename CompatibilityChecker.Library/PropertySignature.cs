namespace CompatibilityChecker.Library
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct PropertySignature
    {
        private readonly BlobReader reader;

        public PropertySignature(BlobReader blobReader)
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

        public ImmutableArray<CustomModifierSignature> CustomModifiers
        {
            get
            {
                var reader = this.reader;

                // signature header
                reader.ReadSignatureHeader();

                // parameter count
                reader.ReadCompressedInteger();

                var builder = ImmutableArray.CreateBuilder<CustomModifierSignature>();
                while (reader.IsCustomModifier())
                {
                    var customModifierSignature = new CustomModifierSignature(reader);
                    builder.Add(customModifierSignature);
                    reader = customModifierSignature.Skip();
                }

                return builder.ToImmutable();
            }
        }

        public TypeSignature PropertyType
        {
            get
            {
                var reader = this.reader;

                // header
                reader.ReadSignatureHeader();

                // parameter count
                reader.ReadCompressedInteger();

                while (reader.IsCustomModifier())
                {
                    reader = new CustomModifierSignature(reader).Skip();
                }

                return new TypeSignature(reader);
            }
        }

        public ImmutableArray<ParameterSignature> Parameters
        {
            get
            {
                var reader = this.reader;
                reader.ReadSignatureHeader();

                int parameterCount = reader.ReadCompressedInteger();

                while (reader.IsCustomModifier())
                {
                    reader = new CustomModifierSignature(reader).Skip();
                }

                reader = new TypeSignature(reader).Skip();

                var builder = ImmutableArray.CreateBuilder<ParameterSignature>(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameterSignature = new ParameterSignature(reader);
                    builder.Add(parameterSignature);
                    reader = parameterSignature.Skip();
                }

                return builder.ToImmutable();
            }
        }

        public BlobReader Skip()
        {
            var reader = this.reader;
            reader.ReadSignatureHeader();

            int parameterCount = reader.ReadCompressedInteger();

            while (reader.IsCustomModifier())
            {
                reader = new CustomModifierSignature(reader).Skip();
            }

            reader = new TypeSignature(reader).Skip();

            for (int i = 0; i < parameterCount; i++)
            {
                reader = new ParameterSignature(reader).Skip();
            }

            return reader;
        }
    }
}
