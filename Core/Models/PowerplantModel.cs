
namespace Core.Models
{
    public class PowerplantModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Efficiency { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }
        public double Merit { get; set; }
        public double Cost { get; set; }
    }
}
