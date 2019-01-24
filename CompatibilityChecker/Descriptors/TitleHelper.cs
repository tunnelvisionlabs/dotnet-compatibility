namespace CompatibilityChecker.Descriptors
{
    using System.Text;

    internal static class TitleHelper
    {
        internal static string GenerateTitle(string ruleId)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in ruleId)
            {
                if (builder.Length == 0)
                {
                    builder.Append(c);
                    continue;
                }

                if (char.IsUpper(c))
                {
                    builder.Append(' ');
                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
