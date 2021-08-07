namespace CompatibilityChecker.Library
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct ArrayShapeSignature
    {
        private readonly BlobReader reader;

        public ArrayShapeSignature(BlobReader blobReader)
        {
            reader = blobReader;
        }

        public int Rank
        {
            get
            {
                return reader.ReadCompressedInteger();
            }
        }

        public ImmutableArray<int> Lengths
        {
            get
            {
                var reader = this.reader;

                // rank
                reader.ReadCompressedInteger();

                // sizes
                int numSizes = reader.ReadCompressedInteger();
                var builder = ImmutableArray.CreateBuilder<int>(numSizes);
                for (int i = 0; i < numSizes; i++)
                {
                    builder.Add(reader.ReadCompressedInteger());
                }

                return builder.ToImmutable();
            }
        }

        public ImmutableArray<int> LowerBounds
        {
            get
            {
                var reader = this.reader;

                // rank
                reader.ReadCompressedInteger();

                // sizes
                int numSizes = reader.ReadCompressedInteger();
                for (int i = 0; i < numSizes; i++)
                {
                    reader.ReadCompressedInteger();
                }

                // bounds
                int numBounds = reader.ReadCompressedInteger();
                var builder = ImmutableArray.CreateBuilder<int>(numBounds);
                for (int i = 0; i < numBounds; i++)
                {
                    builder.Add(reader.ReadCompressedSignedInteger());
                }

                return builder.ToImmutable();
            }
        }

        public BlobReader Skip()
        {
            var reader = this.reader;

            // rank
            reader.ReadCompressedInteger();

            // sizes
            int numSizes = reader.ReadCompressedInteger();
            for (int i = 0; i < numSizes; i++)
            {
                reader.ReadCompressedInteger();
            }

            // bounds
            int numBounds = reader.ReadCompressedInteger();
            for (int i = 0; i < numBounds; i++)
            {
                reader.ReadCompressedSignedInteger();
            }

            return reader;
        }
    }
}
