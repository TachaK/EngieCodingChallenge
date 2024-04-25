using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.Models
{
    public class Fuels
    {

        [JsonPropertyName("gas(euro/MWh)")]
        public double Gas { get; set; }

        [JsonPropertyName("kerosine(euro/MWh)")]
        public double Kerosine { get; set; }

        [JsonPropertyName("co2(euro/ton)")]
        public double CO2 { get; set; }

        [JsonPropertyName("wind(%)")]
        public double Wind { get; set; }
    }

    //    private double gasEuroPerMwh;
    //    private double kerosineEuroPerMwh;
    //    private double co2EuroPerTon;
    //    private double windPercent;

    //    public double Load { get; set; }

    //    public Dictionary<string, double> Fuels
    //    {
    //        get
    //        {
    //            return new Dictionary<string, double>()
    //{
    //  { "gas(euro/MWh)", gasEuroPerMwh },
    //  { "kerosine(euro/MWh)", kerosineEuroPerMwh },
    //  { "co2(euro/ton)", co2EuroPerTon },
    //  { "wind(%)", windPercent }
    //};
    //        }
    //    }

    //    // Public setter methods for private fields (optional)
    //    public void SetGasEuroPerMwh(double value) { gasEuroPerMwh = value; }
    //    public void SetkerosineEuroPerMwh(double value) { kerosineEuroPerMwh = value; }
    //    public void Setco2EuroPerTon(double value) { co2EuroPerTon = value; }
    //    public void SetwindPercent(double value) { windPercent = value; }
    //    public double Gas { get; set; }
    //    public double Kerosine { get; set; }
    //    public double Co2 { get; set; }
    //    public double Wind { get; set; }
    //}
}
