using System.Drawing;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class Flow
    {
        public bool IsSelected { get; set; }
        public Color? Color { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
