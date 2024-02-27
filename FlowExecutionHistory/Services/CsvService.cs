using Fic.XTB.FlowExecutionHistory.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public static class CsvService
    {
        public static void ExportToCsv(List<FlowRun> flowRuns, string filename)
        {
            var sw = new StreamWriter(filename, false, Encoding.UTF8);

            sw.WriteLine("Id;Flow Name;Status;Start Date;Duration;Url;Error");

            foreach (var flowRun in flowRuns)
            {
                sw.WriteLine($"{flowRun.Id};{flowRun.Flow.Name};{flowRun.Status};{flowRun.StartDate};{flowRun.DurationInMilliseconds};{flowRun.Url};{flowRun.Error?.Details}");
            }

            sw.Close();
        }
    }
}
