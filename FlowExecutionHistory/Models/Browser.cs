using System.Collections.Generic;
using McTools.Xrm.Connection;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class Browser
    {
        public string Name { get; set; }
        public BrowserEnum Type { get; set; }
        public List<BrowserProfile> Profiles { get; set; }

        public string Executable { get; set; }
        public override string ToString() => Name;

    }
}
