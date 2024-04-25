using Core.Constants;
using Core.Helpers;

namespace Core.Models
{
    public class Payload
    {
        public double Load { get; set; }
        public Fuels Fuels { get; set; }
        public List<Powerplant> PowerPlants { get; set; }


        public List<PowerplantModel> GetPowerplantMeritOrder()
        {
            var load = this.Load;
            var meritOrder = new List<PowerplantModel>();

            foreach (var powerplant in this.PowerPlants)
            {
                switch (powerplant.Type.ToLower())
                {
                    case ProjectConstants.WINDTURBINE:
                        var powerPlantToAdd = Mapper.ToPowerplantModel(powerplant, 0, 0);
                        powerPlantToAdd.PMax *= this.Fuels.Wind / (double)100.0;
                        meritOrder.Add(powerPlantToAdd);
                        break;
                    case ProjectConstants.TURBOJET:
                        meritOrder.Add(Mapper.ToPowerplantModel(powerplant, (this.Fuels.Kerosine * load) / powerplant.Efficiency, powerplant.Efficiency * this.Fuels.Kerosine));
                        break;
                    case ProjectConstants.GASFIRED:
                        meritOrder.Add(Mapper.ToPowerplantModel(powerplant, (this.Fuels.Gas * load) / powerplant.Efficiency, powerplant.Efficiency * this.Fuels.Gas));
                        break;
                    default: break;
                }
            }
            return meritOrder.OrderBy(m => m.Merit).ToList();
        }
    }

}