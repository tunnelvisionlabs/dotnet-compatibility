namespace CompatibilityChecker.Library
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct FieldSignature
    {
        private readonly BlobReader reader;

        public FieldSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public ImmutableArray<CustomModifierSignature> CustomModifiers
        {
            get
            {
                var reader = this.reader;
                var header = reader.ReadSignatureHeader();
                if (header.Kind != SignatureKind.Field)
                {
                    throw new InvalidOperationException("Expected a field signature.");
                }

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

        public TypeSignature Type
        {
            get
            {
                var reader = this.reader;

                reader.ReadSignatureHeader();
                while (reader.IsCustomModifier())
                {
                    reader = new CustomModifierSignature(reader).Skip();
                }

                return new TypeSignature(reader);
            }
        }
    }
}
