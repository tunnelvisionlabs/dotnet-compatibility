namespace CompatibilityChecker
{
    using CompatibilityChecker.Descriptors;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;

    public class Analyzer
    {
        private readonly PEReader _referenceAssembly;
        private readonly PEReader _newAssembly;
        private readonly IMessageLogger _logger;

        private MetadataReader _referenceMetadata;
        private MetadataReader _newMetadata;

        private MetadataMapping _referenceToNewMapping;
        private MetadataMapping _newToReferenceMapping;

        public Analyzer(PEReader referenceAssembly, PEReader newAssembly, IMessageLogger logger)
        {
            _referenceAssembly = referenceAssembly;
            _newAssembly = newAssembly;
            _logger = logger ?? new ConsoleMessageLogger();
        }

        public void Run()
        {
            _referenceMetadata = _referenceAssembly.GetMetadataReader();
            _newMetadata = _newAssembly.GetMetadataReader();
            var referenceMetadata = _referenceMetadata;
            var newMetadata = _newMetadata;

            _referenceToNewMapping = new MetadataMapping(referenceMetadata, newMetadata);
            _newToReferenceMapping = new MetadataMapping(newMetadata, referenceMetadata);

            CheckAssemblyProperties(referenceMetadata, newMetadata);

            foreach (var typeDefinitionHandle in referenceMetadata.TypeDefinitions)
            {
                TypeDefinition typeDefinition = referenceMetadata.GetTypeDefinition(typeDefinitionHandle);

                if (!IsPubliclyVisible(referenceMetadata, typeDefinition))
                    continue;

                if (IsMarkedPreliminary(referenceMetadata, typeDefinition))
                    continue;

                Mapping<TypeDefinitionHandle> typeMapping = _referenceToNewMapping.MapTypeDefinition(typeDefinitionHandle);

                // make sure the type still exists
                if (typeMapping.Target.IsNil)
                {
                    _logger.Report(TypeMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition)));
                    continue;
                }

                TypeDefinition newTypeDefinition = newMetadata.GetTypeDefinition(typeMapping.Target);
                if ((typeDefinition.Attributes & TypeAttributes.Sealed) == 0)
                {
                    if ((newTypeDefinition.Attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed
                        && HasVisibleConstructors(newMetadata, newTypeDefinition))
                    {
                        _logger.Report(SealedMustNotBeAddedToType.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition)));
                    }
                }

                if ((typeDefinition.Attributes & TypeAttributes.Abstract) == 0)
                {
                    if ((newTypeDefinition.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract
                        && HasVisibleConstructors(newMetadata, newTypeDefinition))
                    {
                        _logger.Report(AbstractMustNotBeAddedToType.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition)));
                    }
                }

                TypeAttributes uncheckedAttributesMask = ~(TypeAttributes.Sealed | TypeAttributes.Abstract);
                if ((typeDefinition.Attributes & uncheckedAttributesMask) != (newTypeDefinition.Attributes & uncheckedAttributesMask))
                    _logger.Report(PublicAttributesMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition)));

                if (IsMarkedPreliminary(newMetadata, newTypeDefinition))
                    _logger.Report(TypeMustNotBeMadePreliminaryFromStable.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition)));

                // check base type
                Handle baseTypeHandle = typeDefinition.BaseType;
                if (!baseTypeHandle.IsNil)
                {
                    Handle newBaseTypeHandle = newTypeDefinition.BaseType;
                    if (newBaseTypeHandle.IsNil)
                        _logger.Report(BaseTypeMustNotChange.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition))); // throw new NotImplementedException("Base type changed.");

                    if (baseTypeHandle.Kind != newBaseTypeHandle.Kind)
                        _logger.Report(BaseTypeMustNotChange.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition))); // throw new NotImplementedException("Base type changed.");

                    switch (baseTypeHandle.Kind)
                    {
                        case HandleKind.TypeDefinition:
                            CheckBaseType(referenceMetadata, newMetadata, (TypeDefinitionHandle)baseTypeHandle, (TypeDefinitionHandle)newBaseTypeHandle);
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
                        _logger.Report(ImplementedInterfaceMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition), GetMetadataName(referenceMetadata, interfaceImplementation))); // throw new NotImplementedException("Implemented interface was removed from a type.");
                }

                // check fields
                foreach (var fieldDefinitionHandle in typeDefinition.GetFields())
                {
                    var fieldDefinition = referenceMetadata.GetFieldDefinition(fieldDefinitionHandle);
                    if (!IsPubliclyVisible(referenceMetadata, fieldDefinition))
                        continue;

                    Mapping<FieldDefinitionHandle> fieldMapping = _referenceToNewMapping.MapFieldDefinition(fieldDefinitionHandle);
                    if (fieldMapping.Target.IsNil)
                    {
                        if (fieldMapping.CandidateTargets.IsDefaultOrEmpty)
                            _logger.Report(FieldMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, fieldDefinition))); //throw new NotImplementedException(string.Format("Publicly-visible field '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, fieldDefinition)));

                        //throw new NotImplementedException();
                    }

                    FieldDefinition newFieldDefinition = newMetadata.GetFieldDefinition(fieldMapping.Target);
                    if (fieldDefinition.Attributes != newFieldDefinition.Attributes)
                        _logger.Report(FieldAttributesMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, fieldDefinition))); // throw new NotImplementedException("Attributes of publicly-visible field changed.");

                    if (!IsSameFieldSignature(referenceMetadata, newMetadata, referenceMetadata.GetSignature(fieldDefinition), newMetadata.GetSignature(newFieldDefinition)))
                        _logger.Report(OtherError.CreateMessage("Signature of publicly-accessible field changed.")); // throw new NotImplementedException("Signature of publicly-accessible field changed.");

                    if (!fieldDefinition.GetDefaultValue().IsNil)
                    {
                        if (newFieldDefinition.GetDefaultValue().IsNil)
                            _logger.Report(OtherError.CreateMessage("Constant value of a field was removed.")); // throw new NotImplementedException("Constant value of a field was removed.");

                        Constant constant = referenceMetadata.GetConstant(fieldDefinition.GetDefaultValue());
                        Constant newConstant = newMetadata.GetConstant(newFieldDefinition.GetDefaultValue());
                        if (constant.TypeCode != newConstant.TypeCode)
                            _logger.Report(OtherError.CreateMessage("Constant value's type of a field changed.")); // throw new NotImplementedException("Constant value of a field changed.");

                        var referenceValue = referenceMetadata.GetBlobContent(constant.Value);
                        var newValue = referenceMetadata.GetBlobContent(constant.Value);
                        if (!referenceValue.SequenceEqual(newValue))
                            _logger.Report(OtherError.CreateMessage("Constant value of a field changed.")); // throw new NotImplementedException("Constant value of a field changed.");
                    }
                }
                

                // check methods
                foreach (var methodDefinition in typeDefinition.GetMethods().Select(referenceMetadata.GetMethodDefinition))
                {
                    if (!IsPubliclyVisible(referenceMetadata, methodDefinition))
                        continue;

                    string referenceName = referenceMetadata.GetString(methodDefinition.Name);

                    List<MethodDefinition> newMethodDefinitions = new List<MethodDefinition>();
                    foreach (var newMethodDefinition in newTypeDefinition.GetMethods().Select(newMetadata.GetMethodDefinition))
                    {
                        string newName = newMetadata.GetString(newMethodDefinition.Name);
                        if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                            continue;

                        // filter on number of generic parameters
                        if (methodDefinition.GetGenericParameters().Count != newMethodDefinition.GetGenericParameters().Count)
                            continue;

                        // filter on number of parameters
                        if (methodDefinition.GetParameters().Count != newMethodDefinition.GetParameters().Count)
                            continue;

                        newMethodDefinitions.Add(newMethodDefinition);
                    }

                    if (newMethodDefinitions.Count == 0)
                    {
                        _logger.Report(MethodMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, methodDefinition))); //throw new NotImplementedException(string.Format("Publicly-visible method '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, methodDefinition)));
                        continue;
                    }

                    bool foundMethodDefinition = false;
                    foreach (var newMethodDefinition in newMethodDefinitions)
                    {
                        MethodSignature referenceSignatureReader = referenceMetadata.GetSignature(methodDefinition);
                        MethodSignature newSignatureReader = newMetadata.GetSignature(newMethodDefinition);
                        if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceSignatureReader, newSignatureReader))
                            continue;

                        if (methodDefinition.Attributes != newMethodDefinition.Attributes)
                            _logger.Report(MethodAttributesMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, methodDefinition))); // throw new NotImplementedException("Attributes of publicly-visible method changed.");

                        foundMethodDefinition = true;
                        break;
                    }

                    if (!foundMethodDefinition)
                        _logger.Report(MethodMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, methodDefinition))); //throw new NotImplementedException(string.Format("Publicly-visible method '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, methodDefinition)));
                }

                // If the type is an interface, additionally make sure the number of methods did not change.
                if ((typeDefinition.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
                {
                    if (typeDefinition.GetMethods().Count < newTypeDefinition.GetMethods().Count)
                    {
                        List<MethodDefinition> addedMethods = new List<MethodDefinition>();
                        foreach (var newMethodDefinition in newTypeDefinition.GetMethods().Select(newMetadata.GetMethodDefinition))
                        { 
                            string newName = newMetadata.GetString(newMethodDefinition.Name);

                            List<MethodDefinition> methodDefinitions = new List<MethodDefinition>();
                            foreach (var methodDefinition in typeDefinition.GetMethods().Select(referenceMetadata.GetMethodDefinition))
                            {
                                string referenceName = referenceMetadata.GetString(methodDefinition.Name);
                                if (!string.Equals(newName, referenceName, StringComparison.Ordinal))
                                    continue;

                                // filter on number of generic parameters
                                if (methodDefinition.GetGenericParameters().Count != newMethodDefinition.GetGenericParameters().Count)
                                    continue;

                                // filter on number of parameters
                                if (methodDefinition.GetParameters().Count != newMethodDefinition.GetParameters().Count)
                                    continue;

                                methodDefinitions.Add(methodDefinition);
                            }
                            if (methodDefinitions.Count == 0)
                            {
                                addedMethods.Add(newMethodDefinition);
                                continue;
                            }
                        }
                        foreach(var addedMethod in addedMethods)
                            _logger.Report(MethodMustNotBeAddedToInterface.CreateMessage(GetMetadataName(referenceMetadata, typeDefinition), GetMetadataName(newMetadata, addedMethod))); // throw new NotImplementedException("Method was added to an interface.");
                    }
                }

                // check events
                foreach (var eventDefinitionHandle in typeDefinition.GetEvents())
                {
                    var eventDefinition = referenceMetadata.GetEventDefinition(eventDefinitionHandle);
                    if (!IsPubliclyVisible(referenceMetadata, eventDefinition))
                        continue;

                    Mapping<EventDefinitionHandle> eventDefinitionMapping = _referenceToNewMapping.MapEventDefinition(eventDefinitionHandle);
                    if (eventDefinitionMapping.Target.IsNil)
                        _logger.Report(EventMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); //throw new NotImplementedException(string.Format("Publicly-visible event '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, eventDefinition, typeDefinition)));

                    EventDefinition newEventDefinition = newMetadata.GetEventDefinition(eventDefinitionMapping.Target);
                    if (eventDefinition.Attributes != newEventDefinition.Attributes)
                        _logger.Report(EventAttributesMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); // throw new NotImplementedException("Attributes of publicly-visible event changed.");

                    if (!IsSameType(referenceMetadata, newMetadata, eventDefinition.Type, newEventDefinition.Type))
                        _logger.Report(EventSignatureMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); // throw new NotImplementedException("Signature of publicly-visible event changed.");

                    EventAccessors eventAccessors = eventDefinition.GetAccessors();

                    if (!eventAccessors.Adder.IsNil)
                    {
                        MethodDefinition referenceAdderMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Adder);
                        if (IsPubliclyVisible(referenceMetadata, referenceAdderMethodDefinition))
                        {
                            EventAccessors newEventAccessors = newEventDefinition.GetAccessors();
                            if (newEventAccessors.Adder.IsNil)
                                _logger.Report(EventAdderMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); // throw new NotImplementedException("Event adder was removed.");

                            MethodDefinition newAdderMethodDefinition = newMetadata.GetMethodDefinition(newEventAccessors.Adder);

                            string referenceAdderName = referenceMetadata.GetString(referenceAdderMethodDefinition.Name);
                            string newAdderName = newMetadata.GetString(newAdderMethodDefinition.Name);
                            if (!string.Equals(referenceAdderName, newAdderName, StringComparison.Ordinal))
                                _logger.Report(OtherError.CreateMessage("Name of event adder changed.")); // throw new NotImplementedException("Signature of event adder changed.");

                            MethodSignature referenceSignatureReader = referenceMetadata.GetSignature(referenceAdderMethodDefinition);
                            MethodSignature newSignatureReader = newMetadata.GetSignature(newAdderMethodDefinition);
                            if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceSignatureReader, newSignatureReader))
                                _logger.Report(OtherError.CreateMessage("Signature of event adder changed.")); // throw new NotImplementedException("Signature of event adder changed.");
                        }
                    }

                    if (!eventAccessors.Remover.IsNil)
                    {
                        MethodDefinition referenceRemoverMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Remover);
                        if (IsPubliclyVisible(referenceMetadata, referenceRemoverMethodDefinition))
                        {
                            EventAccessors newEventAccessors = newEventDefinition.GetAccessors();
                            if (newEventAccessors.Remover.IsNil)
                                _logger.Report(EventRemoverMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); // throw new NotImplementedException("Event remover was removed.");

                            MethodDefinition newRemoverMethodDefinition = newMetadata.GetMethodDefinition(newEventAccessors.Remover);

                            string referenceRemoverName = referenceMetadata.GetString(referenceRemoverMethodDefinition.Name);
                            string newRemoverName = newMetadata.GetString(newRemoverMethodDefinition.Name);
                            if (!string.Equals(referenceRemoverName, newRemoverName, StringComparison.Ordinal))
                                _logger.Report(OtherError.CreateMessage("Name of event remover changed.")); // throw new NotImplementedException("Signature of event remover changed.");

                            MethodSignature referenceSignatureReader = referenceMetadata.GetSignature(referenceRemoverMethodDefinition);
                            MethodSignature newSignatureReader = newMetadata.GetSignature(newRemoverMethodDefinition);
                            if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceSignatureReader, newSignatureReader))
                                _logger.Report(OtherError.CreateMessage("Signature of event remover changed.")); // throw new NotImplementedException("Signature of event remover changed.");
                        }
                    }

                    if (!eventAccessors.Raiser.IsNil)
                    {
                        MethodDefinition referenceRaiserMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Raiser);
                        if (IsPubliclyVisible(referenceMetadata, referenceRaiserMethodDefinition))
                        {
                            EventAccessors newEventAccessors = newEventDefinition.GetAccessors();
                            if (newEventAccessors.Raiser.IsNil)
                                _logger.Report(EventRaiserMustNotBeRemoved.CreateMessage(GetMetadataName(referenceMetadata, eventDefinition, typeDefinition))); // throw new NotImplementedException("Event raiser was removed.");

                            MethodDefinition newRaiserMethodDefinition = newMetadata.GetMethodDefinition(newEventAccessors.Raiser);

                            string referenceRaiserName = referenceMetadata.GetString(referenceRaiserMethodDefinition.Name);
                            string newRaiserName = newMetadata.GetString(newRaiserMethodDefinition.Name);
                            if (!string.Equals(referenceRaiserName, newRaiserName, StringComparison.Ordinal))
                                _logger.Report(OtherError.CreateMessage("Name of event raiser changed.")); // throw new NotImplementedException("Signature of event raiser changed.");

                            MethodSignature referenceSignatureReader = referenceMetadata.GetSignature(referenceRaiserMethodDefinition);
                            MethodSignature newSignatureReader = newMetadata.GetSignature(newRaiserMethodDefinition);
                            if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceSignatureReader, newSignatureReader))
                                _logger.Report(OtherError.CreateMessage("Signature of event raiser changed.")); // throw new NotImplementedException("Signature of event raiser changed.");
                        }
                    }
                }

                // check properties
                foreach (var propertyDefinition in typeDefinition.GetProperties().Select(referenceMetadata.GetPropertyDefinition))
                {
                    if (!IsPubliclyVisible(referenceMetadata, propertyDefinition))
                        continue;

                    string referenceName = referenceMetadata.GetString(propertyDefinition.Name);

                    PropertyDefinitionHandle newPropertyDefinitionHandle = default(PropertyDefinitionHandle);
                    foreach (var propertyDefinitionHandle in newTypeDefinition.GetProperties())
                    {
                        string newName = newMetadata.GetString(newMetadata.GetPropertyDefinition(propertyDefinitionHandle).Name);
                        if (string.Equals(referenceName, newName, StringComparison.Ordinal))
                        {
                            newPropertyDefinitionHandle = propertyDefinitionHandle;
                            break;
                        }
                    }

                    if (newPropertyDefinitionHandle.IsNil)
                        _logger.Report(PropertyMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, propertyDefinition, typeDefinition))); //throw new NotImplementedException(string.Format("Publicly-visible property '{0}' was renamed or removed.", GetMetadataName(referenceMetadata, propertyDefinition, typeDefinition)));

                    PropertyDefinition newPropertyDefinition = newMetadata.GetPropertyDefinition(newPropertyDefinitionHandle);
                    if (propertyDefinition.Attributes != newPropertyDefinition.Attributes)
                        _logger.Report(PropertyAttributesMustNotBeChanged.CreateMessage(GetMetadataName(referenceMetadata, propertyDefinition, typeDefinition))); // throw new NotImplementedException("Attributes of publicly-visible property changed.");

                    PropertySignature referenceSignature = referenceMetadata.GetSignature(propertyDefinition);
                    PropertySignature newSignature = newMetadata.GetSignature(newPropertyDefinition);
                    if (!IsSamePropertySignature(referenceMetadata, newMetadata, referenceSignature, newSignature))
                        _logger.Report(OtherError.CreateMessage("Signature of publicly-visible property changed.")); // throw new NotImplementedException("Signature of publicly-visible property changed.");

                    PropertyAccessors propertyAccessors = propertyDefinition.GetAccessors();
                    if (!propertyAccessors.Getter.IsNil)
                    {
                        MethodDefinition referenceGetterMethodDefinition = referenceMetadata.GetMethodDefinition(propertyAccessors.Getter);
                        if (IsPubliclyVisible(referenceMetadata, referenceGetterMethodDefinition))
                        {
                            PropertyAccessors newPropertyAccessors = newPropertyDefinition.GetAccessors();
                            if (newPropertyAccessors.Getter.IsNil)
                                _logger.Report(OtherError.CreateMessage("Property getter was removed.")); // throw new NotImplementedException("Property getter was removed.");

                            MethodDefinition newGetterMethodDefinition = newMetadata.GetMethodDefinition(newPropertyAccessors.Getter);

                            string referenceGetterName = referenceMetadata.GetString(referenceGetterMethodDefinition.Name);
                            string newGetterName = newMetadata.GetString(newGetterMethodDefinition.Name);
                            if (!string.Equals(referenceGetterName, newGetterName, StringComparison.Ordinal))
                                _logger.Report(OtherError.CreateMessage("Name of property getter changed.")); // throw new NotImplementedException("Signature of property getter changed.");

                            MethodSignature referenceAccessorSignatureReader = referenceMetadata.GetSignature(referenceGetterMethodDefinition);
                            MethodSignature newAccessorSignatureReader = newMetadata.GetSignature(newGetterMethodDefinition);
                            if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceAccessorSignatureReader, newAccessorSignatureReader))
                                _logger.Report(OtherError.CreateMessage("Signature of property getter changed.")); // throw new NotImplementedException("Signature of property getter changed.");
                        }
                    }

                    if (!propertyAccessors.Setter.IsNil)
                    {
                        MethodDefinition referenceSetterMethodDefinition = referenceMetadata.GetMethodDefinition(propertyAccessors.Setter);
                        if (IsPubliclyVisible(referenceMetadata, referenceSetterMethodDefinition))
                        {
                            PropertyAccessors newPropertyAccessors = newPropertyDefinition.GetAccessors();
                            if (newPropertyAccessors.Setter.IsNil)
                                _logger.Report(OtherError.CreateMessage("Property setter was removed.")); // throw new NotImplementedException("Property setter was removed.");

                            MethodDefinition newSetterMethodDefinition = newMetadata.GetMethodDefinition(newPropertyAccessors.Setter);

                            string referenceSetterName = referenceMetadata.GetString(referenceSetterMethodDefinition.Name);
                            string newSetterName = newMetadata.GetString(newSetterMethodDefinition.Name);
                            if (!string.Equals(referenceSetterName, newSetterName, StringComparison.Ordinal))
                                _logger.Report(OtherError.CreateMessage("Name of property setter changed.")); // throw new NotImplementedException("Signature of property setter changed.");

                            MethodSignature referenceAccessorSignatureReader = referenceMetadata.GetSignature(referenceSetterMethodDefinition);
                            MethodSignature newAccessorSignatureReader = newMetadata.GetSignature(newSetterMethodDefinition);
                            if (!IsSameMethodSignature(referenceMetadata, newMetadata, referenceAccessorSignatureReader, newAccessorSignatureReader))
                                _logger.Report(OtherError.CreateMessage("Signature of property setter changed.")); // throw new NotImplementedException("Signature of property setter changed.");
                        }
                    }
                }
            }
        }

        private void CheckAssemblyProperties(MetadataReader referenceMetadata, MetadataReader newMetadata)
        {
            AssemblyDefinition referenceAssemblyDefinition = referenceMetadata.GetAssemblyDefinition();
            AssemblyDefinition newAssemblyDefinition = newMetadata.GetAssemblyDefinition();

            string referenceName = referenceMetadata.GetString(referenceAssemblyDefinition.Name);
            string newName = newMetadata.GetString(newAssemblyDefinition.Name);
            if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                _logger.Report(AssemblyNameMustNotBeChanged.CreateMessage(referenceName));

            string referenceCulture = referenceMetadata.GetString(referenceAssemblyDefinition.Culture);
            string newCulture = referenceMetadata.GetString(newAssemblyDefinition.Culture);
            if (!string.Equals(referenceCulture, newCulture, StringComparison.Ordinal))
                _logger.Report(OtherError.CreateMessage("Assembly culture changed.")); // throw new NotImplementedException("Assembly culture changed.");

            if (!referenceAssemblyDefinition.PublicKey.IsNil)
            {
                // adding a public key is supported, but removing or changing it is not.
                var referencePublicKey = referenceMetadata.GetBlobContent(referenceAssemblyDefinition.PublicKey);
                var newPublicKey = newMetadata.GetBlobContent(newAssemblyDefinition.PublicKey);
                if (!referencePublicKey.SequenceEqual(newPublicKey))
                    _logger.Report(PublicKeyMustNotBeChanged.CreateMessage(referenceName));
            }
        }

        private bool HasVisibleConstructors(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            // for now, assume that the type has publicly-visible constructors.
            return true;
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

        private string GetMetadataName(MetadataReader metadataReader, InterfaceImplementation interfaceImplementation)
        {            
            var interfaceNamespaceName = "Unknown";
            var interfaceName = "Interface";
            if (interfaceImplementation.Interface.Kind == HandleKind.TypeDefinition)
            {
                TypeDefinition interfaceImplementationTypeDefinition = metadataReader.GetTypeDefinition((TypeDefinitionHandle)interfaceImplementation.Interface);

                interfaceNamespaceName = metadataReader.GetString(interfaceImplementationTypeDefinition.Namespace);
                interfaceName = metadataReader.GetString(interfaceImplementationTypeDefinition.Name);
            }
            return string.Format("{0}.{1}", interfaceNamespaceName, interfaceName);
        }

        private string GetMetadataName(MetadataReader metadataReader, FieldDefinition fieldDefinition)
        {
            TypeDefinition declaringTypeDefinition = metadataReader.GetTypeDefinition(fieldDefinition.GetDeclaringType());
            string typeName = GetMetadataName(metadataReader, declaringTypeDefinition);
            string fieldName = metadataReader.GetString(fieldDefinition.Name);
            return string.Format("{0}.{1}", typeName, fieldName);
        }

        private string GetMetadataName(MetadataReader metadataReader, MethodDefinition methodDefinition)
        {
            TypeDefinition declaringTypeDefinition = metadataReader.GetTypeDefinition(methodDefinition.GetDeclaringType());
            string typeName = GetMetadataName(metadataReader, declaringTypeDefinition);
            string methodName = metadataReader.GetString(methodDefinition.Name);
            return string.Format("{0}.{1}", typeName, methodName);
        }

        private string GetMetadataName(MetadataReader metadataReader, EventDefinition eventDefinition, TypeDefinition declaringTypeDefinition)
        {
            string typeName = GetMetadataName(metadataReader, declaringTypeDefinition);
            string eventName = metadataReader.GetString(eventDefinition.Name);
            return string.Format("{0}.{1}", typeName, eventName);
        }

        private string GetMetadataName(MetadataReader metadataReader, PropertyDefinition propertyDefinition, TypeDefinition declaringTypeDefinition)
        {
            string typeName = GetMetadataName(metadataReader, declaringTypeDefinition);
            string propertyName = metadataReader.GetString(propertyDefinition.Name);
            return string.Format("{0}.{1}", typeName, propertyName);
        }

        private void CheckBaseType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeDefinitionHandle referenceBaseTypeHandle, TypeDefinitionHandle newBaseTypeDefinitionHandle)
        {
            Mapping<TypeDefinitionHandle> mappedTypeDefinitionHandle = _referenceToNewMapping.MapTypeDefinition(referenceBaseTypeHandle);
            if (mappedTypeDefinitionHandle.Target.IsNil)
                _logger.Report(OtherError.CreateMessage("Base type no longer in assembly.")); // throw new NotImplementedException("Base type no longer in assembly.");

            if (mappedTypeDefinitionHandle.Target != newBaseTypeDefinitionHandle)
                _logger.Report(OtherError.CreateMessage("Base type changed.")); // throw new NotImplementedException("Base type changed.");
        }

        private void CheckBaseType(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeReference referenceBaseTypeReference, TypeReference newBaseTypeReference)
        {
            CheckResolutionScope(referenceMetadata, newMetadata, referenceBaseTypeReference.ResolutionScope, newBaseTypeReference.ResolutionScope);

            string referenceName = referenceMetadata.GetString(referenceBaseTypeReference.Name);
            string newName = newMetadata.GetString(newBaseTypeReference.Name);
            if (!string.Equals(referenceName, newName, StringComparison.Ordinal))
                _logger.Report(OtherError.CreateMessage("Base type changed.")); // throw new NotImplementedException("Base type changed.");

            string referenceNamespace = referenceMetadata.GetString(referenceBaseTypeReference.Namespace);
            string newNamespace = newMetadata.GetString(newBaseTypeReference.Namespace);
            if (!string.Equals(referenceNamespace, newNamespace, StringComparison.Ordinal))
                _logger.Report(OtherError.CreateMessage("Base type changed.")); // throw new NotImplementedException("Base type changed.");
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
                    Mapping<TypeDefinitionHandle> mappedTypeDefinitionHandle = _referenceToNewMapping.MapTypeDefinition((TypeDefinitionHandle)referenceTypeHandle);
                    if (mappedTypeDefinitionHandle.Target.IsNil)
                        return false;

                    return mappedTypeDefinitionHandle.Target == (TypeDefinitionHandle)newTypeHandle;

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
            TypeSpecificationSignature referenceSignature = referenceMetadata.GetSignature(referenceTypeSpecification);
            TypeSpecificationSignature newSignature = newMetadata.GetSignature(newTypeSpecification);

            SignatureTypeCode referenceTypeCode = referenceSignature.TypeCode;
            SignatureTypeCode newTypeCode = newSignature.TypeCode;
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
                    if (!IsSameType(referenceMetadata, newMetadata, referenceSignature.TypeHandle, newSignature.TypeHandle))
                        return false;

                    ImmutableArray<TypeSignature> referenceGenericArguments = referenceSignature.GenericTypeArguments;
                    ImmutableArray<TypeSignature> newGenericArguments = newSignature.GenericTypeArguments;
                    if (referenceGenericArguments.Length != newGenericArguments.Length)
                        return false;

                    for (int i = 0; i < referenceGenericArguments.Length; i++)
                    {
                        if (!IsSameTypeSignature(referenceMetadata, newMetadata, referenceGenericArguments[i], newGenericArguments[i]))
                            return false;
                    }

                    return true;

                default:
                    return false;
            }
        }

        private bool IsSameFieldSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, FieldSignature referenceSignature, FieldSignature newSignature)
        {
            if (!referenceSignature.CustomModifiers.IsEmpty || !newSignature.CustomModifiers.IsEmpty)
                throw new NotImplementedException();

            return IsSameTypeSignature(referenceMetadata, newMetadata, referenceSignature.Type, newSignature.Type);
        }

        private bool IsSameMethodSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, MethodSignature referenceSignatureReader, MethodSignature newSignatureReader)
        {
            SignatureHeader referenceHeader = referenceSignatureReader.Header;
            SignatureHeader newHeader = newSignatureReader.Header;
            if (referenceHeader.Kind != SignatureKind.Method || newHeader.Kind != SignatureKind.Method)
                throw new InvalidOperationException("Expected method signatures.");

            if (referenceHeader.RawValue != newHeader.RawValue)
                return false;

            if (referenceHeader.IsGeneric)
            {
                if (referenceSignatureReader.GenericParameterCount != newSignatureReader.GenericParameterCount)
                    return false;
            }

            if (!IsSameReturnTypeSignature(referenceMetadata, newMetadata, referenceSignatureReader.ReturnType, newSignatureReader.ReturnType))
                return false;

            var referenceParameters = referenceSignatureReader.Parameters;
            var newParameters = newSignatureReader.Parameters;
            if (referenceParameters.Length != newParameters.Length)
                return false;

            for (int i = 0; i < referenceParameters.Length; i++)
            {
                if (!IsSameParameterSignature(referenceMetadata, newMetadata, referenceParameters[i], newParameters[i]))
                    return false;
            }

            return true;
        }

        private bool IsSamePropertySignature(MetadataReader referenceMetadata, MetadataReader newMetadata, PropertySignature referenceSignatureReader, PropertySignature newSignatureReader)
        {
            SignatureHeader referenceHeader = referenceSignatureReader.Header;
            SignatureHeader newHeader = newSignatureReader.Header;
            if (referenceHeader.Kind != SignatureKind.Property || newHeader.Kind != SignatureKind.Property)
                throw new InvalidOperationException("Expected property signatures.");

            if (referenceHeader.IsInstance != newHeader.IsInstance)
                return false;

            ImmutableArray<ParameterSignature> referenceParameters = referenceSignatureReader.Parameters;
            ImmutableArray<ParameterSignature> newParameters = newSignatureReader.Parameters;
            if (referenceParameters.Length != newParameters.Length)
                return false;

            if (!referenceSignatureReader.CustomModifiers.IsEmpty || !newSignatureReader.CustomModifiers.IsEmpty)
                throw new NotImplementedException();

            if (!IsSameTypeSignature(referenceMetadata, newMetadata, referenceSignatureReader.PropertyType, newSignatureReader.PropertyType))
                return false;

            for (int i = 0; i < referenceParameters.Length; i++)
            {
                if (!IsSameParameterSignature(referenceMetadata, newMetadata, referenceParameters[i], newParameters[i]))
                    return false;
            }

            return true;
        }

        private bool IsSameParameterSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, ParameterSignature referenceSignatureReader, ParameterSignature newSignatureReader)
        {
            if (!referenceSignatureReader.CustomModifiers.IsEmpty || !newSignatureReader.CustomModifiers.IsEmpty)
                throw new NotImplementedException();

            if (referenceSignatureReader.TypeCode != newSignatureReader.TypeCode)
                return false;

            switch (referenceSignatureReader.TypeCode)
            {
                case SignatureTypeCode.TypedReference:
                    return true;

                default:
                    if (referenceSignatureReader.IsByRef != newSignatureReader.IsByRef)
                        return false;

                    return IsSameTypeSignature(referenceMetadata, newMetadata, referenceSignatureReader.Type, newSignatureReader.Type);
            }
        }

        private bool IsSameReturnTypeSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, ReturnTypeSignature referenceSignatureReader, ReturnTypeSignature newSignatureReader)
        {
            if (!referenceSignatureReader.CustomModifiers.IsEmpty || !newSignatureReader.CustomModifiers.IsEmpty)
                throw new NotImplementedException();

            if (referenceSignatureReader.TypeCode != newSignatureReader.TypeCode)
                return false;

            switch (referenceSignatureReader.TypeCode)
            {
                case SignatureTypeCode.TypedReference:
                case SignatureTypeCode.Void:
                    return true;

                default:
                    if (referenceSignatureReader.IsByRef != newSignatureReader.IsByRef)
                        return false;

                    return IsSameTypeSignature(referenceMetadata, newMetadata, referenceSignatureReader.Type, newSignatureReader.Type);
            }
        }

        private bool IsSameTypeSignature(MetadataReader referenceMetadata, MetadataReader newMetadata, TypeSignature referenceSignature, TypeSignature newSignature)
        {
            if (referenceSignature.TypeCode != newSignature.TypeCode)
                return false;

            switch (referenceSignature.TypeCode)
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
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceSignature.TypeCode));

                case SignatureTypeCode.FunctionPointer:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceSignature.TypeCode));

                case SignatureTypeCode.GenericTypeInstance:
                    if (!IsSameType(referenceMetadata, newMetadata, referenceSignature.TypeHandle, newSignature.TypeHandle))
                        return false;

                    ImmutableArray<TypeSignature> referenceGenericArguments = referenceSignature.GenericTypeArguments;
                    ImmutableArray<TypeSignature> newGenericArguments = newSignature.GenericTypeArguments;
                    if (referenceGenericArguments.Length != newGenericArguments.Length)
                        return false;

                    for (int i = 0; i < referenceGenericArguments.Length; i++)
                    {
                        if (!IsSameTypeSignature(referenceMetadata, newMetadata, referenceGenericArguments[i], newGenericArguments[i]))
                            return false;
                    }

                    return true;

                case SignatureTypeCode.GenericMethodParameter:
                case SignatureTypeCode.GenericTypeParameter:
                    return referenceSignature.GenericParameterIndex == newSignature.GenericParameterIndex;

                case SignatureTypeCode.TypeHandle:
                    Handle referenceTypeHandle = referenceSignature.TypeHandle;
                    Handle newTypeHandle = newSignature.TypeHandle;
                    return IsSameType(referenceMetadata, newMetadata, referenceTypeHandle, newTypeHandle);

                case SignatureTypeCode.Pointer:
                    throw new NotImplementedException(string.Format("{0} is not yet implemented.", referenceSignature.TypeCode));

                case SignatureTypeCode.SZArray:
                    if (!referenceSignature.CustomModifiers.IsEmpty || !newSignature.CustomModifiers.IsEmpty)
                        throw new NotImplementedException();

                    return IsSameTypeSignature(referenceMetadata, newMetadata, referenceSignature.ElementType, newSignature.ElementType);

                default:
                    throw new InvalidOperationException("Invalid signature type code.");
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
                _logger.Report(OtherError.CreateMessage("ResolutionScope changed.")); // throw new NotImplementedException("ResolutionScope changed.");

            if (referenceResolutionScope.Kind != newResolutionScope.Kind)
                _logger.Report(OtherError.CreateMessage("ResolutionScope changed.")); // throw new NotImplementedException("ResolutionScope changed.");

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
                _logger.Report(OtherError.CreateMessage("ResolutionScope assembly reference changed.")); // throw new NotImplementedException("ResolutionScope assembly reference changed.");
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

        private bool IsPubliclyVisible(MetadataReader referenceMetadata, MethodDefinition methodDefinition, bool checkDeclaringType = false)
        {
            switch (methodDefinition.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Public:
                case MethodAttributes.Family:
                case MethodAttributes.FamORAssem:
                    break;

                case MethodAttributes.FamANDAssem:
                case MethodAttributes.Assembly:
                case MethodAttributes.Private:
                case MethodAttributes.PrivateScope:
                default:
                    return false;
            }

            if (checkDeclaringType)
            {
                TypeDefinition declaringTypeDefinition = referenceMetadata.GetTypeDefinition(methodDefinition.GetDeclaringType());
                if (!IsPubliclyVisible(referenceMetadata, declaringTypeDefinition))
                    return false;
            }

            return true;
        }

        private bool IsPubliclyVisible(MetadataReader referenceMetadata, EventDefinition eventDefinition, bool checkDeclaringType = false)
        {
            EventAccessors eventAccessors = eventDefinition.GetAccessors();
            if (!eventAccessors.Adder.IsNil)
            {
                MethodDefinition adderMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Adder);
                if (IsPubliclyVisible(referenceMetadata, adderMethodDefinition, checkDeclaringType))
                    return true;
            }

            if (!eventAccessors.Remover.IsNil)
            {
                MethodDefinition removerMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Remover);
                if (IsPubliclyVisible(referenceMetadata, removerMethodDefinition, checkDeclaringType))
                    return true;
            }

            if (!eventAccessors.Raiser.IsNil)
            {
                MethodDefinition raiserMethodDefinition = referenceMetadata.GetMethodDefinition(eventAccessors.Raiser);
                if (IsPubliclyVisible(referenceMetadata, raiserMethodDefinition, checkDeclaringType))
                    return true;
            }

            return false;
        }

        private bool IsPubliclyVisible(MetadataReader referenceMetadata, PropertyDefinition propertyDefinition, bool checkDeclaringType = false)
        {
            PropertyAccessors propertyAccessors = propertyDefinition.GetAccessors();
            if (!propertyAccessors.Getter.IsNil)
            {
                MethodDefinition getterMethodDefinition = referenceMetadata.GetMethodDefinition(propertyAccessors.Getter);
                if (IsPubliclyVisible(referenceMetadata, getterMethodDefinition, checkDeclaringType))
                    return true;
            }

            if (!propertyAccessors.Setter.IsNil)
            {
                MethodDefinition setterMethodDefinition = referenceMetadata.GetMethodDefinition(propertyAccessors.Setter);
                if (IsPubliclyVisible(referenceMetadata, setterMethodDefinition, checkDeclaringType))
                    return true;
            }

            return false;
        }

        private bool IsMarkedPreliminary(MetadataReader metadataReader, TypeDefinition typeDefinition)
        {
            return false;
        }
    }
}
