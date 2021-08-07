using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompatibilityChecker.Library
{
    internal class BasicListingStatistics : IReportStatistics
    {
        private (int error, int warning, int information, int disabled) severityCounts;

        (int error, int warning, int information, int disabled) IReportStatistics.SeverityCounts => severityCounts;

        public BasicListingStatistics(int severityError, int severityWarning, int severityInformation, int severityDisabled)
        {
            severityCounts = (error: severityError, warning: severityWarning, information: severityInformation, disabled: severityDisabled);
        }
    }
}
