namespace CompatibilityChecker
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct ReturnTypeSignature
    {
        private readonly BlobReader _reader;

        public ReturnTypeSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public ImmutableArray<CustomModifierSignature> CustomModifiers {
            get {
                var reader = _reader;
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

        public bool IsByRef {
            get {
                var reader = _reader;
                while (reader.IsCustomModifier())
                    reader = new CustomModifierSignature(reader).Skip();

                return reader.ReadSignatureTypeCode() == SignatureTypeCode.ByReference;
            }
        }

        public SignatureTypeCode TypeCode {
            get {
                var reader = _reader;
                while (reader.IsCustomModifier())
                    reader = new CustomModifierSignature(reader).Skip();

                switch (reader.PeekSignatureTypeCode())
                {
                    case SignatureTypeCode.TypedReference:
                    case SignatureTypeCode.Void:
                        return reader.PeekSignatureTypeCode();

                    case SignatureTypeCode.ByReference:
                        reader.ReadSignatureTypeCode();
                        goto default;

                    default:
                        return new TypeSignature(reader).TypeCode;
                }
            }
        }

        public bool IsTypedByRef {
            get {
                return TypeCode == SignatureTypeCode.TypedReference;
            }
        }

        public bool IsVoid {
            get {
                return TypeCode == SignatureTypeCode.Void;
            }
        }

        public TypeSignature Type {
            get {
                var reader = _reader;
                while (reader.IsCustomModifier())
                    reader = new CustomModifierSignature(reader).Skip();

                switch (reader.PeekSignatureTypeCode())
                {
                    case SignatureTypeCode.ByReference:
                        reader.ReadSignatureTypeCode();
                        return new TypeSignature(reader);

                    case SignatureTypeCode.TypedReference:
                    case SignatureTypeCode.Void:
                        throw new InvalidOperationException(string.Format("RetType signatures with type code {0} do not have a Type signature.", reader.PeekSignatureTypeCode()));

                    default:
                        return new TypeSignature(reader);
                }
            }
        }

        public BlobReader Skip()
        {
            var reader = _reader;
            while (reader.IsCustomModifier())
                reader = new CustomModifierSignature(reader).Skip();

            switch (reader.PeekSignatureTypeCode())
            {
                case SignatureTypeCode.ByReference:
                    reader.ReadSignatureTypeCode();
                    goto default;

                case SignatureTypeCode.TypedReference:
                case SignatureTypeCode.Void:
                    reader.ReadSignatureTypeCode();
                    break;

                default:
                    reader = new TypeSignature(reader).Skip();
                    break;
            }

            return reader;
        }
    }
}
