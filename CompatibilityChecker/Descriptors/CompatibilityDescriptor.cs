namespace CompatibilityChecker.Descriptors
{
    using System;

    internal abstract class CompatibilityDescriptor
    {
        protected CompatibilityDescriptor(string ruleId, string title, string messageFormat, string category, Severity defaultSeverity, string description = null)
        {
            if (ruleId == null)
                throw new ArgumentNullException("ruleId");
            if (title == null)
                throw new ArgumentNullException("title");
            if (messageFormat == null)
                throw new ArgumentNullException("messageFormat");
            if (category == null)
                throw new ArgumentNullException("category");

            RuleId = ruleId;
            Title = title;
            MessageFormat = messageFormat;
            Category = category;
            DefaultSeverity = defaultSeverity;
            Description = description;
        }

        public string RuleId {
            get;
        }

        public string Title {
            get;
        }

        public string MessageFormat {
            get;
        }

        public string Category {
            get;
        }

        public Severity DefaultSeverity {
            get;
        }

        public string Description {
            get;
        }
    }
}
