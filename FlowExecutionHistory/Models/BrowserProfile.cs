namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class BrowserProfile
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public override string ToString() => this.Name;
    }
}
