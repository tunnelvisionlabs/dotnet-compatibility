namespace CompatibilityChecker.Library
{
    using System.Reflection.Metadata;

    public static class MetadataReaderExtensions
    {
        public static FieldSignature GetSignature(this MetadataReader metadataReader, FieldDefinition fieldDefinition)
        {
            return new FieldSignature(metadataReader.GetBlobReader(fieldDefinition.Signature));
        }

        public static MethodSignature GetSignature(this MetadataReader metadataReader, MethodDefinition methodDefinition)
        {
            return new MethodSignature(metadataReader.GetBlobReader(methodDefinition.Signature));
        }

        public static PropertySignature GetSignature(this MetadataReader metadataReader, PropertyDefinition propertyDefinition)
        {
            return new PropertySignature(metadataReader.GetBlobReader(propertyDefinition.Signature));
        }

        public static TypeSpecificationSignature GetSignature(this MetadataReader metadataReader, TypeSpecification typeSpecification)
        {
            return new TypeSpecificationSignature(metadataReader.GetBlobReader(typeSpecification.Signature));
        }
    }
}
