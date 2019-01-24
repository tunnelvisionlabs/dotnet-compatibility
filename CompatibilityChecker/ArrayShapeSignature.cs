namespace CompatibilityChecker
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    public struct ArrayShapeSignature
    {
        private readonly BlobReader _reader;

        public ArrayShapeSignature(BlobReader blobReader)
        {
            _reader = blobReader;
        }

        public int Rank {
            get {
                return _reader.ReadCompressedInteger();
            }
        }

        public ImmutableArray<int> Lengths {
            get {
                BlobReader reader = _reader;

                // rank
                reader.ReadCompressedInteger();

                // sizes
                int numSizes = reader.ReadCompressedInteger();
                var builder = ImmutableArray.CreateBuilder<int>(numSizes);
                for (int i = 0; i < numSizes; i++)
                    builder.Add(reader.ReadCompressedInteger());

                return builder.ToImmutable();
            }
        }

        public ImmutableArray<int> LowerBounds {
            get {
                BlobReader reader = _reader;

                // rank
                reader.ReadCompressedInteger();

                // sizes
                int numSizes = reader.ReadCompressedInteger();
                for (int i = 0; i < numSizes; i++)
                    reader.ReadCompressedInteger();

                // bounds
                int numBounds = reader.ReadCompressedInteger();
                var builder = ImmutableArray.CreateBuilder<int>(numBounds);
                for (int i = 0; i < numBounds; i++)
                    builder.Add(reader.ReadCompressedSignedInteger());

                return builder.ToImmutable();
            }
        }

        public BlobReader Skip()
        {
            BlobReader reader = _reader;

            // rank
            reader.ReadCompressedInteger();

            // sizes
            int numSizes = reader.ReadCompressedInteger();
            for (int i = 0; i < numSizes; i++)
                reader.ReadCompressedInteger();

            // bounds
            int numBounds = reader.ReadCompressedInteger();
            for (int i = 0; i < numBounds; i++)
                reader.ReadCompressedSignedInteger();

            return reader;
        }
    }
}
