namespace CompatibilityChecker.Library
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    /// <summary>
    /// This structure represents a metadata Type signature, as described in ECMA-335 §II.23.2.12.
    /// </summary>
    public struct TypeSignature
    {
        private readonly BlobReader reader;

        public TypeSignature(BlobReader blobReader)
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

        /// <summary>
        /// Gets the index of a generic type or method parameter.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>If <see cref="TypeCode"/> is not <see cref="SignatureTypeCode.GenericTypeParameter"/> or
        /// <see cref="SignatureTypeCode.GenericMethodParameter"/>.</para>
        /// </exception>
        public int GenericParameterIndex
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.GenericMethodParameter:
                    case SignatureTypeCode.GenericTypeParameter:
                        return reader.ReadCompressedInteger();

                    default:
                        throw new InvalidOperationException("Only generic parameters have a generic parameter index.");
                }
            }
        }

        /// <summary>
        /// Gets the type handle encoded in the signature.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>If <see cref="TypeCode"/> is not <see cref="SignatureTypeCode.TypeHandle"/> or
        /// <see cref="SignatureTypeCode.GenericTypeInstance"/>.</para>
        /// </exception>
        public Handle TypeHandle
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.TypeHandle:
                        return reader.ReadTypeHandle();

                    case SignatureTypeCode.GenericTypeInstance:
                        reader.ReadSignatureTypeCode();
                        return reader.ReadTypeHandle();

                    default:
                        throw new InvalidOperationException(string.Format("Type code '{0}' does not have a type handle.", typeCode));
                }
            }
        }

        public ImmutableArray<TypeSignature> GenericTypeArguments
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();
                if (typeCode != SignatureTypeCode.GenericTypeInstance)
                {
                    throw new InvalidOperationException(string.Format("Type code '{0}' does not have generic arguments.", typeCode));
                }

                reader.ReadSignatureTypeCode();
                reader.ReadTypeHandle();

                int genericArgumentCount = reader.ReadCompressedInteger();
                var builder = ImmutableArray.CreateBuilder<TypeSignature>(genericArgumentCount);
                for (int i = 0; i < genericArgumentCount; i++)
                {
                    TypeSignature argument = new (reader);
                    builder.Add(argument);
                    reader = argument.Skip();
                }

                return builder.ToImmutable();
            }
        }

        public ImmutableArray<CustomModifierSignature> CustomModifiers
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.Pointer:
                    case SignatureTypeCode.SZArray:
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Type code '{0}' does not have custom modifiers.", typeCode));
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

        public TypeSignature ElementType
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.Array:
                        return new TypeSignature(reader);

                    case SignatureTypeCode.Pointer:
                        while (reader.IsCustomModifier())
                        {
                            reader = new CustomModifierSignature(reader).Skip();
                        }

                        if (reader.PeekSignatureTypeCode() == SignatureTypeCode.Void)
                        {
                            goto default;
                        }

                        return new TypeSignature(reader);

                    case SignatureTypeCode.SZArray:
                        while (reader.IsCustomModifier())
                        {
                            reader = new CustomModifierSignature(reader).Skip();
                        }

                        return new TypeSignature(reader);

                    default:
                        throw new InvalidOperationException(string.Format("Type code '{0}' does not have an element type.", typeCode));
                }
            }
        }

        public ArrayShapeSignature ArrayShape
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.Array:
                        // skip past the type and return the array shape
                        reader = new TypeSignature(reader).Skip();
                        return new ArrayShapeSignature(reader);

                    default:
                        throw new InvalidOperationException(string.Format("Type code '{0}' does not have an array shape signature.", typeCode));
                }
            }
        }

        public MethodSignature MethodSignature
        {
            get
            {
                var reader = this.reader;
                SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

                switch (typeCode)
                {
                    case SignatureTypeCode.FunctionPointer:
                        return new MethodSignature(reader);

                    default:
                        throw new InvalidOperationException(string.Format("Type code '{0}' does not have a method signature.", typeCode));
                }
            }
        }

        public BlobReader Skip()
        {
            var reader = this.reader;
            SignatureTypeCode typeCode = reader.ReadSignatureTypeCode();

            switch (typeCode)
            {
                case SignatureTypeCode.Boolean:
                case SignatureTypeCode.Char:
                case SignatureTypeCode.SByte:
                case SignatureTypeCode.Byte:
                case SignatureTypeCode.Int16:
                case SignatureTypeCode.UInt16:
                case SignatureTypeCode.Int32:
                case SignatureTypeCode.UInt32:
                case SignatureTypeCode.Int64:
                case SignatureTypeCode.UInt64:
                case SignatureTypeCode.IntPtr:
                case SignatureTypeCode.UIntPtr:
                case SignatureTypeCode.Single:
                case SignatureTypeCode.Double:
                    break;

                case SignatureTypeCode.Object:
                case SignatureTypeCode.String:
                    break;

                case SignatureTypeCode.Array:
                    reader = new TypeSignature(reader).Skip();
                    reader = new ArrayShapeSignature(reader).Skip();
                    break;

                case SignatureTypeCode.FunctionPointer:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", typeCode));

                case SignatureTypeCode.GenericTypeInstance:
                    reader.ReadSignatureTypeCode();
                    reader.ReadTypeHandle();

                    int argumentCount = reader.ReadCompressedInteger();
                    for (int i = 0; i < argumentCount; i++)
                    {
                        reader = new TypeSignature(reader).Skip();
                    }

                    break;

                case SignatureTypeCode.GenericMethodParameter:
                case SignatureTypeCode.GenericTypeParameter:
                    // skip the generic parameter index
                    reader.ReadCompressedInteger();
                    break;

                case SignatureTypeCode.TypeHandle:
                    reader.ReadTypeHandle();
                    break;

                case SignatureTypeCode.Pointer:
                    while (reader.IsCustomModifier())
                    {
                        reader = new CustomModifierSignature(reader).Skip();
                    }

                    if (reader.PeekSignatureTypeCode() == SignatureTypeCode.Void)
                    {
                        reader.ReadSignatureTypeCode();
                    }
                    else
                    {
                        reader = new TypeSignature(reader).Skip();
                    }

                    break;

                case SignatureTypeCode.SZArray:
                    while (reader.IsCustomModifier())
                    {
                        reader = new CustomModifierSignature(reader).Skip();
                    }

                    reader = new TypeSignature(reader).Skip();
                    break;

                default:
                    throw new InvalidOperationException("Invalid signature type code.");
            }

            return reader;
        }
    }
}
