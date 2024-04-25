using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Powerplant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Efficiency { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }
    }
}
