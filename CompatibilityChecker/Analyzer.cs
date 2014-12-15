namespace CompatibilityChecker
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;

    public class Analyzer
    {
        private readonly PEReader _referenceAssembly;
        private readonly PEReader _newAssembly;

        public Analyzer(PEReader referenceAssembly, PEReader newAssembly)
        {
            _referenceAssembly = referenceAssembly;
            _newAssembly = newAssembly;
        }

        public void Run()
        {
            MetadataReader referenceMetadata = _referenceAssembly.GetMetadataReader();
            MetadataReader newMetadata = _newAssembly.GetMetadataReader();
            foreach (var typeDefinition in referenceMetadata.TypeDefinitions.Select(referenceMetadata.GetTypeDefinition))
            {
                if (!IsPubliclyVisible(referenceMetadata, typeDefinition))
                    continue;

                if (IsMarkedPreliminary(referenceMetadata, typeDefinition))
                    continue;

                // make sure the type still exists
                TypeDefinition newTypeDefinition;
                if (!TryMapTypeToNewAssembly(referenceMetadata, newMetadata, typeDefinition, out newTypeDefinition))
                    throw new NotImplementedException("Publicly visible type was removed.");

                if (typeDefinition.Attributes != newTypeDefinition.Attributes)
                    throw new NotImplementedException("Attributes of publicly visible type changed.");

                if (IsMarkedPreliminary(newMetadata, newTypeDefinition))
                    throw new NotImplementedException("Publicly visible type changed from stable to preliminary.");

                // check base type
                Handle baseTypeHandle = typeDefinition.BaseType;
                if (!baseTypeHandle.IsNil)
                {
                    Handle newBaseTypeHandle = newTypeDefinition.BaseType;
                    if (newBaseTypeHandle.IsNil)
                        throw new NotImplementedException("Base type changed.");

                    if (baseTypeHandle.Kind != newBaseTypeHandle.Kind)
                        throw new NotImplementedException("Base type changed.");

                    switch (baseTypeHandle.Kind)
                    {
                    case HandleKind.TypeDefinition:
                        TypeDefinition referenceBaseTypeDefinition = referenceMetadata.GetTypeDefinition((TypeDefinitionHandle)baseTypeHandle);
                        CheckBaseType(referenceMetadata, newMetadata, referenceBaseTypeDefinition, (TypeDefinitionHandle)newBaseTypeHandle);
                        break;

                    case HandleKind.TypeReference:
                        TypeReference referenceBaseTypeReference = referenceMetadata.GetTypeReference((TypeReferenceHandle)baseTypeHandle);
                        TypeReference newBaseTypeReference = newMetadata.GetTypeReference((TypeReferenceHandle)newBaseTypeHandle);
                        CheckBaseType(referenceMetadata, newMetadata, referenceBaseTypeReference, newBaseTypeReference);
                        break;

                    case HandleKind.TypeSpecification:
                        TypeSpecification referenceBaseTypeSpecification = referenceMetadata.GetTypeSpecification((TypeSpecificationHandle)baseTypeHandle);
                        TypeSpecification newBaseTypeSpecification = newMetadata.GetTypeSpecification((TypeSpecificationHandle)newBaseTypeHandle);
                        CheckBaseType(referenceMetadata, newMetadata, referenceBaseTypeSpecification, newBaseTypeSpecification);
                        break;

                    default:
                        throw new InvalidOperationException("Unrecognized base type handle kind.");
                    }
                }

                // check interfaces
                foreach (var interfaceImplementation in typeDefinition.GetInterfaceImplementations().Select(referenceMetadata.GetInterfaceImplementation))
                {
                    if (interfaceImplementation.Interface.Kind == HandleKind.TypeDefinition)
                    {
                        TypeDefinition interfaceImplementationTypeDefinition = referenceMetadata.GetTypeDefinition((TypeDefinitionHandle)interfaceImplementation.Interface);
                        if (!IsPubliclyVisible(referenceMetadata, interfaceImplementationTypeDefinition))
                            continue;
                    }

                    bool foundMatchingInterface = false;
                    foreach (var newInterfaceImplementation in newTypeDefinition.GetInterfaceImplementations().Select(newMetadata.GetInterfaceImplementation))
                    {
                        foundMatchingInterface = IsSameType(referenceMetadata, newMetadata, interfaceImplementation.Interface, newInterfaceImplementation.Interface);
                        if (foundMatchingInterface)
                            break;
                    }

                    if (!foundMatchingInterface)
                        throw new NotImplementedException("Implemented interface was removed from a type.");
                }

                // check fields
                foreach (var fieldDefinition in typeDefinition.GetFields().Select(referenceMetadata.GetFieldDefinition))
                {
                    if (!IsPubliclyVisible(referenceMetadata, fieldDefinition))
                        continue;

                    string referenceName = referenceMetadata.GetString(fieldDefinition.Name);

                    FieldDefinitionHandle newFieldDefinitionHandle = default(FieldDefinitionHandle);
                    foreach (var fieldDefinitionHandle in newTypeDefinition.GetFields())
                    {
                        string newName = newMetadata.GetString(newMetadata.GetFieldDefinition(fieldDefinitionHandle).Name);
                        if (string.Equals(referenceName, newName, StringComparison.Ordinal))
                        {
                            newFieldDefinitionHandle = fieldDefinitionHandle;
                            break;
                        }
                    }

                    if (newFieldDefinitionHandle.IsNil)
                        throw new NotImplementedException(string.Format("Publicly-visible field '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, fieldDefinition)));

                    FieldDefinition newFieldDefinition = newMetadata.GetFieldDefinition(newFieldDefinitionHandle);
                    if (fieldDefinition.Attributes != newFieldDefinition.Attributes)
                        throw new NotImplementedException("Attributes of publicly-visible field changed.");

                    BlobReader referenceSignatureReader = referenceMetadata.GetBlobReader(fieldDefinition.Signature);
                    BlobReader newSignatureReader = newMetadata.GetBlobReader(newFieldDefinition.Signature);
                    if (!IsSameFieldSignature(referenceMetadata, newMetadata, ref referenceSignatureReader, ref newSignatureReader))
                        throw new NotImplementedException("Signature of publicly-visible field changed.");
                }

                // check methods

                // check events

                // check properties
            }
        }

        private string GetMetadataName(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            if (typeDefinition.GetDeclaringType().IsNil)
            {
                string namespaceName = metadataReader.GetString(typeDefinition.Namespace);
                string typeName = metadataReader.GetString(typeDefinition.Name);
                return string.Format("{0}.{1}", namespaceName, typeName);
            }
            else
            {
                TypeDefinition declaringTypeDefinition = metadataReader.GetTypeDefinition(typeDefinition.GetDeclaringType());
                string declaringTypeName = GetMetadataName(metadataReader, declaringTypeDefinition);
                string name = metadataReader.GetString(typeDefinition.Name);
                return string.Format("{0}+{1}", declaringTypeName, name);
            }
        }

        private string GetMetadataName(MetadataReader metadataReader, FieldDefinition fieldDefinition)
        {
            TypeDefinition declaringTypeDefinition = metadataReader.GetTypeDefinition(fieldDefinition.GetDeclaringType());
            string typeName = GetMetadataName(metadataReader, declaringTypeDefinition);
            string fieldName = metadataReader.GetString(fieldDefinition.Name);
            return string.Format("{0}.{1}", typeName, fieldName);
        }

        private void CheckBaseType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeDefinition referenceBaseTypeDefinition, TypeDefinitionHandle newBaseTypeDefinitionHandle)
        {
            TypeDefinitionHandle mappedTypeDefinitionHandle;
            if (!TryMapTypeToNewAssembly(referenceMetadata, newMetadata, referenceBaseTypeDefinition, out mappedTypeDefinitionHandle))
                throw new NotImplementedException("Base type no longer in assembly.");

            if (mappedTypeDefinitionHandle != newBaseTypeDefinitionHandle)
                throw new NotImplementedException("Base type changed.");
        }

        private void CheckBaseType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeReference referenceBaseTypeReference, TypeReference newBaseTypeReference)
        {
            CheckResolutionScope(referenceMetadata, newMetadata, referenceBaseTypeReference.ResolutionScope, newBaseTypeReference.ResolutionScope);

            string referenceName = referenceMetadata.GetString(referenceBaseTypeReference.Name);
            string newName = newMetadata.GetString(newBaseTypeReference.Name);
            if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                throw new NotImplementedException("Base type changed.");

            string referenceNamespace = referenceMetadata.GetString(referenceBaseTypeReference.Namespace);
            string newNamespace = newMetadata.GetString(newBaseTypeReference.Namespace);
            if (!string.Equals(referenceNamespace, newNamespace, StringComparison.Ordinal))
                throw new NotImplementedException("Base type changed.");
        }

        private void CheckBaseType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeSpecification referenceBaseTypeSpecification, TypeSpecification newBaseTypeSpecification)
        {
            Console.WriteLine("Base type checking for TypeSpecification not yet implemented.");
        }

        private bool IsSameType(MetadataReader referenceMetadata, MetadataReader newMetadata, Handle referenceTypeHandle, Handle newTypeHandle)
        {
            if (referenceTypeHandle.IsNil != newTypeHandle.IsNil)
                return false;

            if (referenceTypeHandle.Kind != newTypeHandle.Kind)
                return false;

            switch (referenceTypeHandle.Kind)
            {
            case HandleKind.TypeDefinition:
                TypeDefinitionHandle mappedTypeDefinitionHandle;
                if (!TryMapTypeToNewAssembly(referenceMetadata, newMetadata, referenceMetadata.GetTypeDefinition((TypeDefinitionHandle)referenceTypeHandle), out mappedTypeDefinitionHandle))
                    return false;

                return mappedTypeDefinitionHandle == (TypeDefinitionHandle)newTypeHandle;

            case HandleKind.TypeReference:
                TypeReference referenceTypeReference = referenceMetadata.GetTypeReference((TypeReferenceHandle)referenceTypeHandle);
                TypeReference newTypeReference = newMetadata.GetTypeReference((TypeReferenceHandle)newTypeHandle);
                return IsSameType(referenceMetadata, newMetadata, referenceTypeReference, newTypeReference);

            case HandleKind.TypeSpecification:
                TypeSpecification referenceTypeSpecification = referenceMetadata.GetTypeSpecification((TypeSpecificationHandle)referenceTypeHandle);
                TypeSpecification newTypeSpecification = newMetadata.GetTypeSpecification((TypeSpecificationHandle)newTypeHandle);
                return IsSameType(referenceMetadata, newMetadata, referenceTypeSpecification, newTypeSpecification);

            default:
                throw new InvalidOperationException("Invalid type handle.");
            }
        }

        private bool IsSameType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeReference referenceTypeReference, TypeReference newTypeReference)
        {
            if (!IsSameResolutionScope(referenceMetadata, newMetadata, referenceTypeReference.ResolutionScope, newTypeReference.ResolutionScope))
                return false;

            string referenceName = referenceMetadata.GetString(referenceTypeReference.Name);
            string newName = newMetadata.GetString(newTypeReference.Name);
            if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                return false;

            string referenceNamespace = referenceMetadata.GetString(referenceTypeReference.Namespace);
            string newNamespace = newMetadata.GetString(newTypeReference.Namespace);
            if (!string.Equals(referenceNamespace, newNamespace, StringComparison.Ordinal))
                return false;

            return true;
        }

        private bool IsSameType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeSpecification referenceTypeSpecification, TypeSpecification newTypeSpecification)
        {
            BlobReader referenceSignatureReader = referenceMetadata.GetBlobReader(referenceTypeSpecification.Signature);
            BlobReader newSignatureReader = newMetadata.GetBlobReader(newTypeSpecification.Signature);

            SignatureTypeCode referenceTypeCode = referenceSignatureReader.ReadSignatureTypeCode();
            SignatureTypeCode newTypeCode = newSignatureReader.ReadSignatureTypeCode();
            if (referenceTypeCode != newTypeCode)
                return false;

            switch (referenceTypeCode)
            {
            case SignatureTypeCode.Pointer:
                Console.WriteLine("IsSameType not yet implemented for {0}.", referenceTypeCode);
                return true;

            case SignatureTypeCode.FunctionPointer:
                Console.WriteLine("IsSameType not yet implemented for {0}.", referenceTypeCode);
                return true;

            case SignatureTypeCode.Array:
                Console.WriteLine("IsSameType not yet implemented for {0}.", referenceTypeCode);
                return true;

            case SignatureTypeCode.SZArray:
                Console.WriteLine("IsSameType not yet implemented for {0}.", referenceTypeCode);
                return true;

            case SignatureTypeCode.GenericTypeInstance:
                referenceSignatureReader.ReadSignatureTypeCode();
                newSignatureReader.ReadSignatureTypeCode();
                Handle referenceGenericType = referenceSignatureReader.ReadTypeHandle();
                Handle newGenericType = newSignatureReader.ReadTypeHandle();
                if (!IsSameType(referenceMetadata, newMetadata, referenceGenericType, newGenericType))
                    return false;

                int referenceGenericArgumentCount = referenceSignatureReader.ReadCompressedInteger();
                int newGenericArgumentCount = newSignatureReader.ReadCompressedInteger();
                if (referenceGenericArgumentCount != newGenericArgumentCount)
                    return false;

                for (int i = 0; i < referenceGenericArgumentCount; i++)
                {
                    if (!IsSameTypeSignature(referenceMetadata, newMetadata, ref referenceSignatureReader, ref newSignatureReader))
                        return false;
                }

                return true;

            default:
                return false;
            }
        }

        private bool IsSameFieldSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, ref BlobReader referenceSignatureReader, ref BlobReader newSignatureReader)
        {
            SignatureHeader referenceHeader = referenceSignatureReader.ReadSignatureHeader();
            SignatureHeader newHeader = newSignatureReader.ReadSignatureHeader();
            if (referenceHeader.Kind != SignatureKind.Field || newHeader.Kind != SignatureKind.Field)
                throw new InvalidOperationException("Expected field signatures.");

            SkipCustomModifiers(ref referenceSignatureReader);
            SkipCustomModifiers(ref newSignatureReader);
            return IsSameTypeSignature(referenceMetadata, newMetadata, ref referenceSignatureReader, ref newSignatureReader);
        }

        private bool IsSameTypeSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, ref BlobReader referenceSignatureReader, ref BlobReader newSignatureReader)
        {
            SignatureTypeCode referenceTypeCode = referenceSignatureReader.ReadSignatureTypeCode();
            SignatureTypeCode newTypeCode = newSignatureReader.ReadSignatureTypeCode();
            if (referenceTypeCode != newTypeCode)
                return false;

            switch (referenceTypeCode)
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
                return true;

            case SignatureTypeCode.Object:
            case SignatureTypeCode.String:
                return true;

            case SignatureTypeCode.Array:
                throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceTypeCode));

            case SignatureTypeCode.FunctionPointer:
                throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceTypeCode));

            case SignatureTypeCode.GenericTypeInstance:
                referenceSignatureReader.ReadSignatureTypeCode();
                newSignatureReader.ReadSignatureTypeCode();
                Handle referenceGenericType = referenceSignatureReader.ReadTypeHandle();
                Handle newGenericType = newSignatureReader.ReadTypeHandle();
                if (!IsSameType(referenceMetadata, newMetadata, referenceGenericType, newGenericType))
                    return false;

                int referenceGenericArgumentCount = referenceSignatureReader.ReadCompressedInteger();
                int newGenericArgumentCount = newSignatureReader.ReadCompressedInteger();
                if (referenceGenericArgumentCount != newGenericArgumentCount)
                    return false;

                for (int i = 0; i < referenceGenericArgumentCount; i++)
                {
                    if (!IsSameTypeSignature(referenceMetadata, newMetadata, ref referenceSignatureReader, ref newSignatureReader))
                        return false;
                }

                return true;

            case SignatureTypeCode.GenericMethodParameter:
            case SignatureTypeCode.GenericTypeParameter:
                return referenceSignatureReader.ReadCompressedInteger() == newSignatureReader.ReadCompressedInteger();

            case SignatureTypeCode.TypeHandle:
                Handle referenceTypeHandle = referenceSignatureReader.ReadTypeHandle();
                Handle newTypeHandle = newSignatureReader.ReadTypeHandle();
                return IsSameType(referenceMetadata, newMetadata, referenceTypeHandle, newTypeHandle);

            case SignatureTypeCode.Pointer:
                throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceTypeCode));

            case SignatureTypeCode.SZArray:
                throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceTypeCode));

            default:
                throw new InvalidOperationException("Invalid signature type code.");
            }
        }

        private bool IsTypeHandle(BlobReader blobReader)
        {
            return blobReader.ReadSignatureTypeCode() == SignatureTypeCode.TypeHandle;
        }

        private void SkipCustomModifiers(ref BlobReader blobReader)
        {
            while (IsCustomModifier(blobReader))
            {
                throw new NotImplementedException("Custom modifiers are not yet supported.");
            }
        }

        private bool IsCustomModifier(BlobReader blobReader)
        {
            switch (blobReader.ReadSignatureTypeCode())
            {
            case SignatureTypeCode.OptionalModifier:
            case SignatureTypeCode.RequiredModifier:
                return true;

            default:
                return false;
            }
        }

        private bool IsSameResolutionScope(MetadataReader referenceMetadata, MetadataReader newMetadata, Handle referenceResolutionScope, Handle newResolutionScope)
        {
            if (referenceResolutionScope.IsNil != newResolutionScope.IsNil)
                return false;

            if (referenceResolutionScope.Kind != newResolutionScope.Kind)
                return false;

            switch (referenceResolutionScope.Kind)
            {
            case HandleKind.ModuleDefinition:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            case HandleKind.ModuleReference:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            case HandleKind.AssemblyReference:
                AssemblyReference referenceResolutionScopeAssemblyReference = referenceMetadata.GetAssemblyReference((AssemblyReferenceHandle)referenceResolutionScope);
                AssemblyReference newResolutionScopeAssemblyReference = newMetadata.GetAssemblyReference((AssemblyReferenceHandle)newResolutionScope);
                return IsSameResolutionScope(referenceMetadata, newMetadata, referenceResolutionScopeAssemblyReference, newResolutionScopeAssemblyReference);

            case HandleKind.TypeReference:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            default:
                throw new InvalidOperationException("Invalid ResolutionScope kind.");
            }

            return true;
        }

        private bool IsSameResolutionScope(MetadataReader referenceMetadata, MetadataReader newMetadata, AssemblyReference referenceResolutionScope, AssemblyReference newResolutionScope)
        {
            return IsSameAssembly(referenceMetadata, newMetadata, referenceResolutionScope, newResolutionScope);
        }

        private void CheckResolutionScope(MetadataReader referenceMetadata, MetadataReader newMetadata, Handle referenceResolutionScope, Handle newResolutionScope)
        {
            if (referenceResolutionScope.IsNil != newResolutionScope.IsNil)
                throw new NotImplementedException("ResolutionScope changed.");

            if (referenceResolutionScope.Kind != newResolutionScope.Kind)
                throw new NotImplementedException("ResolutionScope changed.");

            switch (referenceResolutionScope.Kind)
            {
            case HandleKind.ModuleDefinition:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            case HandleKind.ModuleReference:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            case HandleKind.AssemblyReference:
                AssemblyReference referenceResolutionScopeAssemblyReference = referenceMetadata.GetAssemblyReference((AssemblyReferenceHandle)referenceResolutionScope);
                AssemblyReference newResolutionScopeAssemblyReference = newMetadata.GetAssemblyReference((AssemblyReferenceHandle)newResolutionScope);
                CheckResolutionScope(referenceMetadata, newMetadata, referenceResolutionScopeAssemblyReference, newResolutionScopeAssemblyReference);
                break;

            case HandleKind.TypeReference:
                Console.WriteLine("ResolutionScope:{0} checking not yet implemented.", referenceResolutionScope.Kind);
                break;

            default:
                throw new InvalidOperationException("Invalid ResolutionScope kind.");
            }
        }

        private void CheckResolutionScope(MetadataReader referenceMetadata, MetadataReader newMetadata, AssemblyReference referenceResolutionScope, AssemblyReference newResolutionScope)
        {
            if (!IsSameAssembly(referenceMetadata, newMetadata, referenceResolutionScope, newResolutionScope))
                throw new NotImplementedException("ResolutionScope assembly reference changed.");
        }

        private bool IsSameAssembly(MetadataReader referenceMetadata, MetadataReader newMetadata, AssemblyReference referenceAssemblyReference, AssemblyReference newAssemblyReference)
        {
            string referenceName = referenceMetadata.GetString(referenceAssemblyReference.Name);
            string newName = newMetadata.GetString(newAssemblyReference.Name);
            if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                return false;

            string referenceCulture = referenceMetadata.GetString(referenceAssemblyReference.Culture);
            string newCulture = newMetadata.GetString(newAssemblyReference.Culture);
            if (!string.Equals(referenceCulture, newCulture, StringComparison.Ordinal))
                return false;

            Version referenceVersion = referenceAssemblyReference.Version;
            Version newVersion = newAssemblyReference.Version;
            if (referenceVersion != newVersion)
                return false;

            byte[] referencePublicKeyOrToken = referenceMetadata.GetBlobBytes(referenceAssemblyReference.PublicKeyOrToken);
            byte[] newPublicKeyOrToken = newMetadata.GetBlobBytes(newAssemblyReference.PublicKeyOrToken);
            if (referencePublicKeyOrToken != null)
            {
                if (newPublicKeyOrToken == null || referencePublicKeyOrToken.Length != newPublicKeyOrToken.Length)
                    return false;

                for (int i = 0; i < referencePublicKeyOrToken.Length; i++)
                {
                    if (referencePublicKeyOrToken[i] != newPublicKeyOrToken[i])
                        return false;
                }
            }
            else if (newPublicKeyOrToken != null)
            {
                return false;
            }

            return true;
        }

        private bool IsPubliclyVisible(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            switch (typeDefinition.Attributes & TypeAttributes.VisibilityMask)
            {
            case TypeAttributes.Public:
                return true;

            case TypeAttributes.NestedPublic:
            case TypeAttributes.NestedFamORAssem:
            case TypeAttributes.NestedFamily:
                TypeDefinition declaringType = metadataReader.GetTypeDefinition(typeDefinition.GetDeclaringType());
                return IsPubliclyVisible(metadataReader, declaringType);

            case TypeAttributes.NestedFamANDAssem:
            case TypeAttributes.NestedPrivate:
            case TypeAttributes.NotPublic:
            default:
                return false;
            }
        }

        private bool IsPubliclyVisible(MetadataReader referenceMetadata, FieldDefinition fieldDefinition, bool checkDeclaringType = false)
        {
            switch (fieldDefinition.Attributes & FieldAttributes.FieldAccessMask)
            {
            case FieldAttributes.Public:
            case FieldAttributes.Family:
            case FieldAttributes.FamORAssem:
                break;

            case FieldAttributes.FamANDAssem:
            case FieldAttributes.Assembly:
            case FieldAttributes.Private:
            case FieldAttributes.PrivateScope:
            default:
                return false;
            }

            if (checkDeclaringType)
            {
                TypeDefinition declaringTypeDefinition = referenceMetadata.GetTypeDefinition(fieldDefinition.GetDeclaringType());
                if (!IsPubliclyVisible(referenceMetadata, declaringTypeDefinition))
                    return false;
            }

            return true;
        }

        private bool IsMarkedPreliminary(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            return false;
        }

        private bool TryMapTypeToNewAssembly(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeDefinition referenceTypeDefinition, out TypeDefinitionHandle newTypeDefinitionHandle)
        {
            string referenceName = referenceMetadata.GetString(referenceTypeDefinition.Name);
            string referenceNamespace = referenceMetadata.GetString(referenceTypeDefinition.Namespace);

            foreach (var typeDefinitionHandle in newMetadata.TypeDefinitions)
            {
                var typeDefinition = newMetadata.GetTypeDefinition(typeDefinitionHandle);

                string name = newMetadata.GetString(typeDefinition.Name);
                if (!string.Equals(referenceName, name, StringComparison.Ordinal))
                    continue;

                string @namespace = newMetadata.GetString(typeDefinition.Namespace);
                if (!string.Equals(referenceNamespace, @namespace, StringComparison.Ordinal))
                    continue;

                if (!typeDefinition.GetDeclaringType().IsNil)
                {
                    if (referenceTypeDefinition.GetDeclaringType().IsNil)
                        continue;

                    TypeDefinition referenceDeclaringTypeDefinition = referenceMetadata.GetTypeDefinition(referenceTypeDefinition.GetDeclaringType());
                    TypeDefinitionHandle newDeclaringTypeDefinitionHandle;
                    if (!TryMapTypeToNewAssembly(referenceMetadata, newMetadata, referenceDeclaringTypeDefinition, out newDeclaringTypeDefinitionHandle))
                        continue;

                    if (newDeclaringTypeDefinitionHandle != typeDefinition.GetDeclaringType())
                        continue;
                }

                newTypeDefinitionHandle = typeDefinitionHandle;
                return true;
            }

            newTypeDefinitionHandle = default(TypeDefinitionHandle);
            return false;
        }

        private bool TryMapTypeToNewAssembly(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeDefinition referenceTypeDefinition, out TypeDefinition newTypeDefinition)
        {
            TypeDefinitionHandle typeDefinitionHandle;
            if (!TryMapTypeToNewAssembly(referenceMetadata, newMetadata, referenceTypeDefinition, out typeDefinitionHandle))
            {
                newTypeDefinition = default(TypeDefinition);
                return false;
            }

            newTypeDefinition = newMetadata.GetTypeDefinition(typeDefinitionHandle);
            return true;
        }
    }
}
