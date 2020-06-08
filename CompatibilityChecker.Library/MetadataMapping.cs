namespace CompatibilityChecker.Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection.Metadata;

    internal class MetadataMapping
    {
        private readonly MetadataReader _sourceMetadata;
        private readonly MetadataReader _targetMetadata;

        private readonly ConcurrentDictionary<AssemblyDefinitionHandle, Mapping<AssemblyDefinitionHandle>> _assemblyDefinitions =
            new ConcurrentDictionary<AssemblyDefinitionHandle, Mapping<AssemblyDefinitionHandle>>();

        private readonly ConcurrentDictionary<AssemblyFileHandle, Mapping<AssemblyFileHandle>> _assemblyFiles =
            new ConcurrentDictionary<AssemblyFileHandle, Mapping<AssemblyFileHandle>>();

        private readonly ConcurrentDictionary<AssemblyReferenceHandle, Mapping<AssemblyReferenceHandle>> _assemblyReferences =
            new ConcurrentDictionary<AssemblyReferenceHandle, Mapping<AssemblyReferenceHandle>>();

        private readonly ConcurrentDictionary<ConstantHandle, Mapping<ConstantHandle>> _constants =
            new ConcurrentDictionary<ConstantHandle, Mapping<ConstantHandle>>();

        private readonly ConcurrentDictionary<CustomAttributeHandle, Mapping<CustomAttributeHandle>> _customAttributes =
            new ConcurrentDictionary<CustomAttributeHandle, Mapping<CustomAttributeHandle>>();

        private readonly ConcurrentDictionary<DeclarativeSecurityAttributeHandle, Mapping<DeclarativeSecurityAttributeHandle>> _declarativeSecurityAttributes =
            new ConcurrentDictionary<DeclarativeSecurityAttributeHandle, Mapping<DeclarativeSecurityAttributeHandle>>();

        private readonly ConcurrentDictionary<EventDefinitionHandle, Mapping<EventDefinitionHandle>> _eventDefinitions =
            new ConcurrentDictionary<EventDefinitionHandle, Mapping<EventDefinitionHandle>>();

        private readonly ConcurrentDictionary<ExportedTypeHandle, Mapping<ExportedTypeHandle>> _exportedTypes =
            new ConcurrentDictionary<ExportedTypeHandle, Mapping<ExportedTypeHandle>>();

        private readonly ConcurrentDictionary<FieldDefinitionHandle, Mapping<FieldDefinitionHandle>> _fieldDefinitions =
            new ConcurrentDictionary<FieldDefinitionHandle, Mapping<FieldDefinitionHandle>>();

        private readonly ConcurrentDictionary<GenericParameterHandle, Mapping<GenericParameterHandle>> _genericParameters =
            new ConcurrentDictionary<GenericParameterHandle, Mapping<GenericParameterHandle>>();

        private readonly ConcurrentDictionary<GenericParameterConstraintHandle, Mapping<GenericParameterConstraintHandle>> _genericParameterConstraints =
            new ConcurrentDictionary<GenericParameterConstraintHandle, Mapping<GenericParameterConstraintHandle>>();

        private readonly ConcurrentDictionary<InterfaceImplementationHandle, Mapping<InterfaceImplementationHandle>> _interfaceImplementations =
            new ConcurrentDictionary<InterfaceImplementationHandle, Mapping<InterfaceImplementationHandle>>();

        private readonly ConcurrentDictionary<ManifestResourceHandle, Mapping<ManifestResourceHandle>> _manifestResources =
            new ConcurrentDictionary<ManifestResourceHandle, Mapping<ManifestResourceHandle>>();

        private readonly ConcurrentDictionary<MemberReferenceHandle, Mapping<MemberReferenceHandle>> _memberReferences =
            new ConcurrentDictionary<MemberReferenceHandle, Mapping<MemberReferenceHandle>>();

        private readonly ConcurrentDictionary<MethodDefinitionHandle, Mapping<MethodDefinitionHandle>> _methodDefinitions =
            new ConcurrentDictionary<MethodDefinitionHandle, Mapping<MethodDefinitionHandle>>();

        private readonly ConcurrentDictionary<MethodImplementationHandle, Mapping<MethodImplementationHandle>> _methodImplementations =
            new ConcurrentDictionary<MethodImplementationHandle, Mapping<MethodImplementationHandle>>();

        private readonly ConcurrentDictionary<MethodSpecificationHandle, Mapping<MethodSpecificationHandle>> _methodSpecifications =
            new ConcurrentDictionary<MethodSpecificationHandle, Mapping<MethodSpecificationHandle>>();

        private readonly ConcurrentDictionary<ModuleDefinitionHandle, Mapping<ModuleDefinitionHandle>> _moduleDefinitions =
            new ConcurrentDictionary<ModuleDefinitionHandle, Mapping<ModuleDefinitionHandle>>();

        private readonly ConcurrentDictionary<ModuleReferenceHandle, Mapping<ModuleReferenceHandle>> _moduleReferences =
            new ConcurrentDictionary<ModuleReferenceHandle, Mapping<ModuleReferenceHandle>>();

        private readonly ConcurrentDictionary<NamespaceDefinitionHandle, Mapping<NamespaceDefinitionHandle>> _namespaceDefinitions =
            new ConcurrentDictionary<NamespaceDefinitionHandle, Mapping<NamespaceDefinitionHandle>>();

        private readonly ConcurrentDictionary<ParameterHandle, Mapping<ParameterHandle>> _parameters =
            new ConcurrentDictionary<ParameterHandle, Mapping<ParameterHandle>>();

        private readonly ConcurrentDictionary<PropertyDefinitionHandle, Mapping<PropertyDefinitionHandle>> _propertyDefinitions =
            new ConcurrentDictionary<PropertyDefinitionHandle, Mapping<PropertyDefinitionHandle>>();

        private readonly ConcurrentDictionary<StandaloneSignatureHandle, Mapping<StandaloneSignatureHandle>> _standaloneSignatures =
            new ConcurrentDictionary<StandaloneSignatureHandle, Mapping<StandaloneSignatureHandle>>();

        private readonly ConcurrentDictionary<TypeDefinitionHandle, Mapping<TypeDefinitionHandle>> _typeDefinitions =
            new ConcurrentDictionary<TypeDefinitionHandle, Mapping<TypeDefinitionHandle>>();

        private readonly ConcurrentDictionary<TypeReferenceHandle, Mapping<TypeReferenceHandle>> _typeReferences =
            new ConcurrentDictionary<TypeReferenceHandle, Mapping<TypeReferenceHandle>>();

        private readonly ConcurrentDictionary<TypeSpecificationHandle, Mapping<TypeSpecificationHandle>> _typeSpecifications =
            new ConcurrentDictionary<TypeSpecificationHandle, Mapping<TypeSpecificationHandle>>();

        public MetadataMapping(MetadataReader sourceMetadata, MetadataReader targetMetadata)
        {
            if (sourceMetadata == null)
                throw new ArgumentNullException("sourceMetadata");
            if (targetMetadata == null)
                throw new ArgumentNullException("targetMetadata");

            _sourceMetadata = sourceMetadata;
            _targetMetadata = targetMetadata;
        }

        public Mapping<Handle> MapHandle(Handle handle)
        {
            if (handle.IsNil)
                return new Mapping<Handle>();

            switch (handle.Kind)
            {
                case HandleKind.ModuleDefinition:
                    return (Mapping<Handle>)MapModuleDefinition((ModuleDefinitionHandle)handle);

                case HandleKind.TypeReference:
                    return (Mapping<Handle>)MapTypeReference((TypeReferenceHandle)handle);

                case HandleKind.TypeDefinition:
                    return (Mapping<Handle>)MapTypeDefinition((TypeDefinitionHandle)handle);

                case HandleKind.FieldDefinition:
                    return (Mapping<Handle>)MapFieldDefinition((FieldDefinitionHandle)handle);

                case HandleKind.MethodDefinition:
                    return (Mapping<Handle>)MapMethodDefinition((MethodDefinitionHandle)handle);

                case HandleKind.Parameter:
                    return (Mapping<Handle>)MapParameter((ParameterHandle)handle);

                case HandleKind.InterfaceImplementation:
                    return (Mapping<Handle>)MapInterfaceImplementation((InterfaceImplementationHandle)handle);

                case HandleKind.MemberReference:
                    return (Mapping<Handle>)MapMemberReference((MemberReferenceHandle)handle);

                case HandleKind.Constant:
                    return (Mapping<Handle>)MapConstant((ConstantHandle)handle);

                case HandleKind.CustomAttribute:
                    return (Mapping<Handle>)MapCustomAttribute((CustomAttributeHandle)handle);

                case HandleKind.DeclarativeSecurityAttribute:
                    return (Mapping<Handle>)MapDeclarativeSecurityAttribute((DeclarativeSecurityAttributeHandle)handle);

                case HandleKind.StandaloneSignature:
                    return (Mapping<Handle>)MapStandaloneSignature((StandaloneSignatureHandle)handle);

                case HandleKind.EventDefinition:
                    return (Mapping<Handle>)MapEventDefinition((EventDefinitionHandle)handle);

                case HandleKind.PropertyDefinition:
                    return (Mapping<Handle>)MapPropertyDefinition((PropertyDefinitionHandle)handle);

                case HandleKind.MethodImplementation:
                    return (Mapping<Handle>)MapMethodImplementation((MethodImplementationHandle)handle);

                case HandleKind.ModuleReference:
                    return (Mapping<Handle>)MapModuleReference((ModuleReferenceHandle)handle);

                case HandleKind.TypeSpecification:
                    return (Mapping<Handle>)MapTypeSpecification((TypeSpecificationHandle)handle);

                case HandleKind.AssemblyDefinition:
                    return (Mapping<Handle>)MapAssemblyDefinition((AssemblyDefinitionHandle)handle);

                case HandleKind.AssemblyFile:
                    return (Mapping<Handle>)MapAssemblyFile((AssemblyFileHandle)handle);

                case HandleKind.AssemblyReference:
                    return (Mapping<Handle>)MapAssemblyReference((AssemblyReferenceHandle)handle);

                case HandleKind.ExportedType:
                    return (Mapping<Handle>)MapExportedType((ExportedTypeHandle)handle);

                case HandleKind.GenericParameter:
                    return (Mapping<Handle>)MapGenericParameter((GenericParameterHandle)handle);

                case HandleKind.MethodSpecification:
                    return (Mapping<Handle>)MapMethodSpecification((MethodSpecificationHandle)handle);

                case HandleKind.GenericParameterConstraint:
                    return (Mapping<Handle>)MapGenericParameterConstraint((GenericParameterConstraintHandle)handle);

                case HandleKind.ManifestResource:
                    return (Mapping<Handle>)MapManifestResource((ManifestResourceHandle)handle);

                case HandleKind.NamespaceDefinition:
                    return (Mapping<Handle>)MapNamespaceDefinition((NamespaceDefinitionHandle)handle);

                default:
                    throw new NotSupportedException(string.Format("Mapping '{0}' handles between assemblies is not supported.", handle.Kind));
            }
        }

        public Mapping<AssemblyDefinitionHandle> MapAssemblyDefinition(AssemblyDefinitionHandle handle)
        {
            return _assemblyDefinitions.GetOrAdd(handle, MapAssemblyDefinitionImpl);
        }

        public Mapping<AssemblyFileHandle> MapAssemblyFile(AssemblyFileHandle handle)
        {
            return _assemblyFiles.GetOrAdd(handle, MapAssemblyFileImpl);
        }

        public Mapping<AssemblyReferenceHandle> MapAssemblyReference(AssemblyReferenceHandle handle)
        {
            return _assemblyReferences.GetOrAdd(handle, MapAssemblyReferenceImpl);
        }

        public Mapping<ConstantHandle> MapConstant(ConstantHandle handle)
        {
            return _constants.GetOrAdd(handle, MapConstantImpl);
        }

        public Mapping<CustomAttributeHandle> MapCustomAttribute(CustomAttributeHandle handle)
        {
            return _customAttributes.GetOrAdd(handle, MapCustomAttributeImpl);
        }

        public Mapping<DeclarativeSecurityAttributeHandle> MapDeclarativeSecurityAttribute(DeclarativeSecurityAttributeHandle handle)
        {
            return _declarativeSecurityAttributes.GetOrAdd(handle, MapDeclarativeSecurityAttributeImpl);
        }

        public Mapping<EventDefinitionHandle> MapEventDefinition(EventDefinitionHandle handle)
        {
            return _eventDefinitions.GetOrAdd(handle, MapEventDefinitionImpl);
        }

        public Mapping<ExportedTypeHandle> MapExportedType(ExportedTypeHandle handle)
        {
            return _exportedTypes.GetOrAdd(handle, MapExportedTypeImpl);
        }

        public Mapping<FieldDefinitionHandle> MapFieldDefinition(FieldDefinitionHandle handle)
        {
            return _fieldDefinitions.GetOrAdd(handle, MapFieldDefinitionImpl);
        }

        public Mapping<GenericParameterHandle> MapGenericParameter(GenericParameterHandle handle)
        {
            return _genericParameters.GetOrAdd(handle, MapGenericParameterImpl);
        }

        public Mapping<GenericParameterConstraintHandle> MapGenericParameterConstraint(GenericParameterConstraintHandle handle)
        {
            return _genericParameterConstraints.GetOrAdd(handle, MapGenericParameterConstraintImpl);
        }

        public Mapping<InterfaceImplementationHandle> MapInterfaceImplementation(InterfaceImplementationHandle handle)
        {
            return _interfaceImplementations.GetOrAdd(handle, MapInterfaceImplementationImpl);
        }

        public Mapping<ManifestResourceHandle> MapManifestResource(ManifestResourceHandle handle)
        {
            return _manifestResources.GetOrAdd(handle, MapManifestResourceImpl);
        }

        public Mapping<MemberReferenceHandle> MapMemberReference(MemberReferenceHandle handle)
        {
            return _memberReferences.GetOrAdd(handle, MapMemberReferenceImpl);
        }

        public Mapping<MethodDefinitionHandle> MapMethodDefinition(MethodDefinitionHandle handle)
        {
            return _methodDefinitions.GetOrAdd(handle, MapMethodDefinitionImpl);
        }

        public Mapping<MethodImplementationHandle> MapMethodImplementation(MethodImplementationHandle handle)
        {
            return _methodImplementations.GetOrAdd(handle, MapMethodImplementationImpl);
        }

        public Mapping<MethodSpecificationHandle> MapMethodSpecification(MethodSpecificationHandle handle)
        {
            return _methodSpecifications.GetOrAdd(handle, MapMethodSpecificationImpl);
        }

        public Mapping<ModuleDefinitionHandle> MapModuleDefinition(ModuleDefinitionHandle handle)
        {
            return _moduleDefinitions.GetOrAdd(handle, MapModuleDefinitionImpl);
        }

        public Mapping<ModuleReferenceHandle> MapModuleReference(ModuleReferenceHandle handle)
        {
            return _moduleReferences.GetOrAdd(handle, MapModuleReferenceImpl);
        }

        public Mapping<NamespaceDefinitionHandle> MapNamespaceDefinition(NamespaceDefinitionHandle handle)
        {
            return _namespaceDefinitions.GetOrAdd(handle, MapNamespaceDefinitionImpl);
        }

        public Mapping<ParameterHandle> MapParameter(ParameterHandle handle)
        {
            return _parameters.GetOrAdd(handle, MapParameterImpl);
        }

        public Mapping<PropertyDefinitionHandle> MapPropertyDefinition(PropertyDefinitionHandle handle)
        {
            return _propertyDefinitions.GetOrAdd(handle, MapPropertyDefinitionImpl);
        }

        public Mapping<StandaloneSignatureHandle> MapStandaloneSignature(StandaloneSignatureHandle handle)
        {
            return _standaloneSignatures.GetOrAdd(handle, MapStandaloneSignatureImpl);
        }

        public Mapping<TypeDefinitionHandle> MapTypeDefinition(TypeDefinitionHandle handle)
        {
            return _typeDefinitions.GetOrAdd(handle, MapTypeDefinitionImpl);
        }

        public Mapping<TypeReferenceHandle> MapTypeReference(TypeReferenceHandle handle)
        {
            return _typeReferences.GetOrAdd(handle, MapTypeReferenceImpl);
        }

        public Mapping<TypeSpecificationHandle> MapTypeSpecification(TypeSpecificationHandle handle)
        {
            return _typeSpecifications.GetOrAdd(handle, MapTypeSpecificationImpl);
        }

        private Mapping<AssemblyDefinitionHandle> MapAssemblyDefinitionImpl(AssemblyDefinitionHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<AssemblyFileHandle> MapAssemblyFileImpl(AssemblyFileHandle handle)
        {
            AssemblyFile assemblyFile = _sourceMetadata.GetAssemblyFile(handle);
            string name = _sourceMetadata.GetString(assemblyFile.Name);
            foreach (var targetHandle in _targetMetadata.AssemblyFiles)
            {
                AssemblyFile target = _targetMetadata.GetAssemblyFile(targetHandle);
                if (!_targetMetadata.StringComparer.Equals(target.Name, name))
                    continue;

                return new Mapping<AssemblyFileHandle>(targetHandle);
            }

            return new Mapping<AssemblyFileHandle>();
        }

        private Mapping<AssemblyReferenceHandle> MapAssemblyReferenceImpl(AssemblyReferenceHandle handle)
        {
            AssemblyReference assemblyReference = _sourceMetadata.GetAssemblyReference(handle);
            string name = _sourceMetadata.GetString(assemblyReference.Name);
            string culture = _sourceMetadata.GetString(assemblyReference.Culture);
            ImmutableArray<byte> publicKeyOrToken = _sourceMetadata.GetBlobContent(assemblyReference.PublicKeyOrToken);
            foreach (var targetHandle in _targetMetadata.AssemblyReferences)
            {
                AssemblyReference target = _targetMetadata.GetAssemblyReference(targetHandle);
                if (!_targetMetadata.StringComparer.Equals(target.Name, name))
                    continue;

                if (!_targetMetadata.StringComparer.Equals(target.Culture, culture))
                    continue;

                if (!publicKeyOrToken.IsDefaultOrEmpty)
                {
                    if (target.Version != assemblyReference.Version)
                        continue;

                    ImmutableArray<byte> targetPublicKeyOrToken = _targetMetadata.GetBlobContent(target.PublicKeyOrToken);
                    if (!targetPublicKeyOrToken.SequenceEqual(targetPublicKeyOrToken))
                        continue;
                }

                return new Mapping<AssemblyReferenceHandle>(targetHandle);
            }

            return new Mapping<AssemblyReferenceHandle>();
        }

        private Mapping<ConstantHandle> MapConstantImpl(ConstantHandle handle)
        {
            Constant constant = _sourceMetadata.GetConstant(handle);
            Mapping<Handle> parent = MapHandle(constant.Parent);
            if (parent.Target.IsNil)
                return new Mapping<ConstantHandle>();

            ConstantHandle targetHandle;
            switch (parent.Target.Kind)
            {
                case HandleKind.Parameter:
                    Parameter targetParameter = _targetMetadata.GetParameter((ParameterHandle)parent.Target);
                    targetHandle = targetParameter.GetDefaultValue();
                    break;

                case HandleKind.FieldDefinition:
                    FieldDefinition targetFieldDefinition = _targetMetadata.GetFieldDefinition((FieldDefinitionHandle)parent.Target);
                    targetHandle = targetFieldDefinition.GetDefaultValue();
                    break;

                case HandleKind.PropertyDefinition:
                    PropertyDefinition targetPropertyDefinition = _targetMetadata.GetPropertyDefinition((PropertyDefinitionHandle)parent.Target);
                    targetHandle = targetPropertyDefinition.GetDefaultValue();
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (targetHandle.IsNil)
                return new Mapping<ConstantHandle>();

            Constant targetConstant = _targetMetadata.GetConstant(targetHandle);
            if (constant.TypeCode != targetConstant.TypeCode)
            {
                var candidateTargets = ImmutableArray.Create(targetHandle);
                var candidateReasons = ImmutableArray.Create("Mapped constant has a different type.");
                return new Mapping<ConstantHandle>(candidateTargets, candidateReasons);
            }

            ImmutableArray<byte> sourceContent = _sourceMetadata.GetBlobContent(constant.Value);
            ImmutableArray<byte> targetContent = _targetMetadata.GetBlobContent(targetConstant.Value);
            if (!sourceContent.SequenceEqual(targetContent))
            {
                var candidateTargets = ImmutableArray.Create(targetHandle);
                var candidateReasons = ImmutableArray.Create("Mapped constant has a different value.");
                return new Mapping<ConstantHandle>(candidateTargets, candidateReasons);
            }

            return new Mapping<ConstantHandle>(targetHandle);
        }

        private Mapping<CustomAttributeHandle> MapCustomAttributeImpl(CustomAttributeHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<DeclarativeSecurityAttributeHandle> MapDeclarativeSecurityAttributeImpl(DeclarativeSecurityAttributeHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<EventDefinitionHandle> MapEventDefinitionImpl(EventDefinitionHandle handle)
        {
            EventDefinition eventDefinition = _sourceMetadata.GetEventDefinition(handle);
            EventAccessors accessors = eventDefinition.GetAccessors();

            // events always have an adder method, so use that to find the declaring type
            MethodDefinition adderMethodDefinition = _sourceMetadata.GetMethodDefinition(accessors.Adder);
            Mapping<TypeDefinitionHandle> declaringTypeMapping = MapTypeDefinition(adderMethodDefinition.GetDeclaringType());
            if (declaringTypeMapping.Target.IsNil)
                return new Mapping<EventDefinitionHandle>();

            // Make sure each of the accessors maps successfully. Only the raiser is optional.
            Mapping<MethodDefinitionHandle> adderMethodDefinitionMapping = MapMethodDefinition(accessors.Adder);
            if (adderMethodDefinitionMapping.Target.IsNil)
                return new Mapping<EventDefinitionHandle>();

            Mapping<MethodDefinitionHandle> removerMethodDefinitionMapping = MapMethodDefinition(accessors.Remover);
            if (removerMethodDefinitionMapping.Target.IsNil)
                return new Mapping<EventDefinitionHandle>();

            Mapping<MethodDefinitionHandle> raiserMethodDefinitionMapping = default(Mapping<MethodDefinitionHandle>);
            if (!accessors.Raiser.IsNil)
            {
                raiserMethodDefinitionMapping = MapMethodDefinition(accessors.Raiser);
                if (raiserMethodDefinitionMapping.Target.IsNil)
                    return new Mapping<EventDefinitionHandle>();
            }

            // locate the target event by name
            string eventName = _sourceMetadata.GetString(eventDefinition.Name);
            EventDefinitionHandle targetEventDefinitionHandle = default(EventDefinitionHandle);
            EventDefinition targetEventDefinition = default(EventDefinition);
            foreach (var targetHandle in _targetMetadata.EventDefinitions)
            {
                targetEventDefinition = _targetMetadata.GetEventDefinition(targetHandle);
                MethodDefinition targetAdderMethodDefinition = _targetMetadata.GetMethodDefinition(targetEventDefinition.GetAccessors().Adder);
                if (targetAdderMethodDefinition.GetDeclaringType() != declaringTypeMapping.Target)
                    continue;

                if (!_targetMetadata.StringComparer.Equals(targetEventDefinition.Name, eventName))
                    continue;

                targetEventDefinitionHandle = targetHandle;
                break;
            }

            if (targetEventDefinitionHandle.IsNil)
                return new Mapping<EventDefinitionHandle>();

            EventAccessors targetAccessors = targetEventDefinition.GetAccessors();
            if (targetAccessors.Adder != adderMethodDefinitionMapping.Target)
                return new Mapping<EventDefinitionHandle>();

            if (targetAccessors.Remover != removerMethodDefinitionMapping.Target)
                return new Mapping<EventDefinitionHandle>();

            if (!accessors.Raiser.IsNil)
            {
                if (targetAccessors.Raiser != raiserMethodDefinitionMapping.Target)
                    return new Mapping<EventDefinitionHandle>();
            }

            return new Mapping<EventDefinitionHandle>(targetEventDefinitionHandle);
        }

        private Mapping<ExportedTypeHandle> MapExportedTypeImpl(ExportedTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<FieldDefinitionHandle> MapFieldDefinitionImpl(FieldDefinitionHandle handle)
        {
            FieldDefinition fieldDefinition = _sourceMetadata.GetFieldDefinition(handle);

            // Map the parent
            Mapping<TypeDefinitionHandle> declaringTypeMapping = MapTypeDefinition(fieldDefinition.GetDeclaringType());
            if (declaringTypeMapping.Target.IsNil)
                return new Mapping<FieldDefinitionHandle>();

            string fieldName = _sourceMetadata.GetString(fieldDefinition.Name);

            TypeDefinition targetDeclaringType = _targetMetadata.GetTypeDefinition(declaringTypeMapping.Target);
            foreach (var targetHandle in targetDeclaringType.GetFields())
            {
                var targetField = _targetMetadata.GetFieldDefinition(targetHandle);
                if (!_targetMetadata.StringComparer.Equals(targetField.Name, fieldName))
                    continue;

                // The name matches. If the signature matches, return in Target; otherwise, return in CandidateTargets.
                FieldSignature sourceSignature = _sourceMetadata.GetSignature(fieldDefinition);
                FieldSignature targetSignature = _targetMetadata.GetSignature(targetField);
                string candidateReason = CompareFieldSignatures(sourceSignature, targetSignature);
                if (candidateReason == null)
                    return new Mapping<FieldDefinitionHandle>(targetHandle);

                var candidateTargets = ImmutableArray.Create(targetHandle);
                var candidateReasons = ImmutableArray.Create(candidateReason);
                return new Mapping<FieldDefinitionHandle>(candidateTargets, candidateReasons);
            }

            // No field with this name was located.
            return new Mapping<FieldDefinitionHandle>();
        }

        private string CompareFieldSignatures(FieldSignature sourceSignature, FieldSignature targetSignature)
        {
            if (!sourceSignature.CustomModifiers.IsEmpty || !targetSignature.CustomModifiers.IsEmpty)
                throw new NotImplementedException();

            return CompareTypeSignatures(sourceSignature.Type, targetSignature.Type);
        }

        private string CompareTypeSignatures(TypeSignature sourceSignature, TypeSignature targetSignature)
        {
            if (sourceSignature.TypeCode != targetSignature.TypeCode)
                return "Type mismatch";

            switch (sourceSignature.TypeCode)
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
                    return null;

                case SignatureTypeCode.Object:
                case SignatureTypeCode.String:
                    return null;

                case SignatureTypeCode.Array:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", sourceSignature.TypeCode));

                case SignatureTypeCode.FunctionPointer:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", sourceSignature.TypeCode));

                case SignatureTypeCode.GenericTypeInstance:
                    if (!IsSameHandle(sourceSignature.TypeHandle, targetSignature.TypeHandle))
                        return "Unbound generic type does not match.";

                    ImmutableArray<TypeSignature> sourceGenericArguments = sourceSignature.GenericTypeArguments;
                    ImmutableArray<TypeSignature> targetGenericArguments = targetSignature.GenericTypeArguments;
                    if (sourceGenericArguments.Length != targetGenericArguments.Length)
                        return "Generic arity does not match.";

                    for (int i = 0; i < sourceGenericArguments.Length; i++)
                    {
                        string genericParameterResult = CompareTypeSignatures(sourceGenericArguments[i], targetGenericArguments[i]);
                        if (genericParameterResult != null)
                            return string.Format("Generic parameter {0} does not match: {1}", i, genericParameterResult);
                    }

                    return null;

                case SignatureTypeCode.GenericMethodParameter:
                case SignatureTypeCode.GenericTypeParameter:
                    if (sourceSignature.GenericParameterIndex != targetSignature.GenericParameterIndex)
                        return "Generic parameter index differs.";

                    return null;

                case SignatureTypeCode.TypeHandle:
                    //throw new NotImplementedException(string.Format("{0} is not yet implemented.", sourceSignature.TypeCode));
                    Handle referenceTypeHandle = sourceSignature.TypeHandle;
                    Handle newTypeHandle = targetSignature.TypeHandle;
                    return IsSameHandle(referenceTypeHandle, newTypeHandle) ? null : "Type handle mismatch";

                case SignatureTypeCode.Pointer:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", sourceSignature.TypeCode));

                case SignatureTypeCode.SZArray:
                    if (!sourceSignature.CustomModifiers.IsEmpty || !targetSignature.CustomModifiers.IsEmpty)
                        throw new NotImplementedException();

                    string szArrayResult = CompareTypeSignatures(sourceSignature.ElementType, targetSignature.ElementType);
                    if (szArrayResult != null)
                        szArrayResult = string.Format("SZArray element type mismatch: {0}", szArrayResult);

                    return szArrayResult;

                default:
                    throw new InvalidOperationException("Invalid signature type code.");
            }
        }

        private bool IsSameHandle(Handle sourceHandle, Handle targetHandle)
        {
            if (sourceHandle.IsNil != targetHandle.IsNil)
                return false;

            if (sourceHandle.IsNil)
                return true;

            Mapping<Handle> mappedTarget = MapHandle(sourceHandle);
            if (mappedTarget.Target.IsNil)
                return false;

            return mappedTarget.Target == targetHandle;
        }

        private Mapping<GenericParameterHandle> MapGenericParameterImpl(GenericParameterHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<GenericParameterConstraintHandle> MapGenericParameterConstraintImpl(GenericParameterConstraintHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<InterfaceImplementationHandle> MapInterfaceImplementationImpl(InterfaceImplementationHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<ManifestResourceHandle> MapManifestResourceImpl(ManifestResourceHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<MemberReferenceHandle> MapMemberReferenceImpl(MemberReferenceHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<MethodDefinitionHandle> MapMethodDefinitionImpl(MethodDefinitionHandle handle)
        {
            MetadataReader referenceMetadata = _sourceMetadata;
            MetadataReader newMetadata = _targetMetadata;
            MethodDefinition referenceMethodDefinition = referenceMetadata.GetMethodDefinition(handle);

            string referenceName = referenceMetadata.GetString(referenceMethodDefinition.Name);
            
            foreach (var MethodDefinitionHandle in newMetadata.MethodDefinitions)
            {
                var MethodDefinition = newMetadata.GetMethodDefinition(MethodDefinitionHandle);

                if (!newMetadata.StringComparer.Equals(MethodDefinition.Name, referenceName))
                    continue;
                
                if (!MethodDefinition.GetDeclaringType().IsNil)
                {
                    if (referenceMethodDefinition.GetDeclaringType().IsNil)
                        continue;

                    Mapping<TypeDefinitionHandle> newDeclaringTypeDefinitionHandle = MapTypeDefinition(referenceMethodDefinition.GetDeclaringType());
                    if (newDeclaringTypeDefinitionHandle.Target.IsNil)
                        continue;

                    if (newDeclaringTypeDefinitionHandle.Target != MethodDefinition.GetDeclaringType())
                        continue;
                }

                return new Mapping<MethodDefinitionHandle>(MethodDefinitionHandle);
            }

            return new Mapping<MethodDefinitionHandle>();
        }

        private Mapping<MethodImplementationHandle> MapMethodImplementationImpl(MethodImplementationHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<MethodSpecificationHandle> MapMethodSpecificationImpl(MethodSpecificationHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<ModuleDefinitionHandle> MapModuleDefinitionImpl(ModuleDefinitionHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<ModuleReferenceHandle> MapModuleReferenceImpl(ModuleReferenceHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<NamespaceDefinitionHandle> MapNamespaceDefinitionImpl(NamespaceDefinitionHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<ParameterHandle> MapParameterImpl(ParameterHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<PropertyDefinitionHandle> MapPropertyDefinitionImpl(PropertyDefinitionHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<StandaloneSignatureHandle> MapStandaloneSignatureImpl(StandaloneSignatureHandle handle)
        {
            throw new NotImplementedException();
        }

        private Mapping<TypeDefinitionHandle> MapTypeDefinitionImpl(TypeDefinitionHandle handle)
        {
            MetadataReader referenceMetadata = _sourceMetadata;
            MetadataReader newMetadata = _targetMetadata;
            TypeDefinition referenceTypeDefinition = referenceMetadata.GetTypeDefinition(handle);

            string referenceName = referenceMetadata.GetString(referenceTypeDefinition.Name);
            string referenceNamespace = referenceMetadata.GetString(referenceTypeDefinition.Namespace);

            foreach (var typeDefinitionHandle in newMetadata.TypeDefinitions)
            {
                var typeDefinition = newMetadata.GetTypeDefinition(typeDefinitionHandle);

                if (!newMetadata.StringComparer.Equals(typeDefinition.Name, referenceName))
                    continue;

                if (!newMetadata.StringComparer.Equals(typeDefinition.Namespace, referenceNamespace))
                    continue;

                if (!typeDefinition.GetDeclaringType().IsNil)
                {
                    if (referenceTypeDefinition.GetDeclaringType().IsNil)
                        continue;

                    Mapping<TypeDefinitionHandle> newDeclaringTypeDefinitionHandle = MapTypeDefinition(referenceTypeDefinition.GetDeclaringType());
                    if (newDeclaringTypeDefinitionHandle.Target.IsNil)
                        continue;

                    if (newDeclaringTypeDefinitionHandle.Target != typeDefinition.GetDeclaringType())
                        continue;
                }

                return new Mapping<TypeDefinitionHandle>(typeDefinitionHandle);
            }

            return new Mapping<TypeDefinitionHandle>();
        }

        private Mapping<TypeReferenceHandle> MapTypeReferenceImpl(TypeReferenceHandle handle)
        {
            MetadataReader referenceMetadata = _sourceMetadata;
            MetadataReader newMetadata = _targetMetadata;
            TypeReference referenceTypeReference = referenceMetadata.GetTypeReference(handle);

            string referenceName = referenceMetadata.GetString(referenceTypeReference.Name);
            string referenceNamespace = referenceMetadata.GetString(referenceTypeReference.Namespace);

            foreach (var typeReferenceHandle in newMetadata.TypeReferences)
            {
                var typeReference = newMetadata.GetTypeReference(typeReferenceHandle);

                if (!newMetadata.StringComparer.Equals(typeReference.Name, referenceName))
                    continue;

                if (!newMetadata.StringComparer.Equals(typeReference.Namespace, referenceNamespace))
                    continue;                

                return new Mapping<TypeReferenceHandle>(typeReferenceHandle);
            }

            return new Mapping<TypeReferenceHandle>();
        }

        private Mapping<TypeSpecificationHandle> MapTypeSpecificationImpl(TypeSpecificationHandle handle)
        {
            throw new NotImplementedException();
        }
    }
}
