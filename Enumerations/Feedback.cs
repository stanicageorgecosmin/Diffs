using System.Collections.Generic;

namespace BusinessSupport
{
    public class Feedback
    {
        public bool OperationSucceeded { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class CalculatedDiffsFeedback : Feedback
    {
        public string LeftPartSize { get; set; }

        public string RightPartSize { get; set; }

        public bool PartsAreEqual { get; set; }

        public IList<DiffsMetadata> DiffsMetadata { get; set; }
    }
}
