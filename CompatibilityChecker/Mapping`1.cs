namespace CompatibilityChecker
{
    using System.Collections.Immutable;
    using System.Reflection.Metadata;

    internal struct Mapping<THandle>
    {
        public Mapping(THandle target)
            : this()
        {
            Target = target;
        }

        public Mapping(ImmutableArray<THandle> candidateTargets, ImmutableArray<string> candidateReasons)
            : this()
        {
            CandidateTargets = candidateTargets;
            CandidateReasons = candidateReasons;
        }

        public Mapping(THandle target, ImmutableArray<THandle> candidateTargets, ImmutableArray<string> candidateReasons)
        {
            Target = target;
            CandidateTargets = candidateTargets;
            CandidateReasons = candidateReasons;
        }

        public ImmutableArray<THandle> CandidateTargets
        {
            get;
        }

        public ImmutableArray<string> CandidateReasons
        {
            get;
        }

        public THandle Target
        {
            get;
        }

        public static explicit operator Mapping<Handle>(Mapping<THandle> mapping)
        {
            Handle target = ConvertToHandle(mapping.Target);
            ImmutableArray<Handle> candidateTargets = default(ImmutableArray<Handle>);
            ImmutableArray<string> candidateReasons = mapping.CandidateReasons;

            if (!mapping.CandidateTargets.IsDefault)
            {
                if (mapping.CandidateTargets.IsEmpty)
                {
                    candidateTargets = ImmutableArray<Handle>.Empty;
                }
                else
                {
                    var builder = ImmutableArray.CreateBuilder<Handle>(mapping.CandidateTargets.Length);
                    foreach (var handle in mapping.CandidateTargets)
                        builder.Add(ConvertToHandle(handle));
                }
            }

            return new Mapping<Handle>(target, candidateTargets, candidateReasons);
        }

        public static explicit operator Mapping<THandle>(Mapping<Handle> mapping)
        {
            THandle target = ConvertToHandle<THandle>(mapping.Target);
            ImmutableArray<THandle> candidateTargets = default(ImmutableArray<THandle>);
            ImmutableArray<string> candidateReasons = mapping.CandidateReasons;

            if (!mapping.CandidateTargets.IsDefault)
            {
                if (mapping.CandidateTargets.IsEmpty)
                {
                    candidateTargets = ImmutableArray<THandle>.Empty;
                }
                else
                {
                    var builder = ImmutableArray.CreateBuilder<THandle>(mapping.CandidateTargets.Length);
                    foreach (var handle in mapping.CandidateTargets)
                        builder.Add(ConvertToHandle<THandle>(handle));
                }
            }

            return new Mapping<THandle>(target, candidateTargets, candidateReasons);
        }

        private static Handle ConvertToHandle(dynamic handle)
        {
            if (handle.IsNil)
                return default(Handle);

            return (Handle)handle;
        }

        private static T ConvertToHandle<T>(dynamic handle)
        {
            if (handle.IsNil)
                return default(T);

            return (T)handle;
        }
    }
}
