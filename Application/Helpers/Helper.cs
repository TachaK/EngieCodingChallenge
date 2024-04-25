using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class Helper
    {
        public static List<PowerplantModel> GetPowerplantMeritOrder(Payload payload)
        {
            var load = payload.Load;
            var meritOrder = new List<PowerplantModel>();

            foreach (var powerplant in payload.PowerPlants)
            {
                switch (powerplant.Type.ToLower())
                {
                    case "windturbine":
                        var powerPlantToAdd = Mapper.ToPowerplantModel(powerplant, 0, 0);
                        powerPlantToAdd.PMax *= payload.Fuels.Wind / (double)100.0;
                        meritOrder.Add(powerPlantToAdd);
                        break;
                    case "turbojet":
                        meritOrder.Add(Mapper.ToPowerplantModel(powerplant, (payload.Fuels.Kerosine * load) / powerplant.Efficiency, powerplant.Efficiency * payload.Fuels.Kerosine));
                        break;
                    case "gasfired":
                        meritOrder.Add(Mapper.ToPowerplantModel(powerplant, (payload.Fuels.Gas * load) / powerplant.Efficiency, powerplant.Efficiency * payload.Fuels.Gas));
                        break;
                    default: break;
                }
            }
            return meritOrder.OrderBy(m => m.Merit).ToList();
        }


        public static double RoundToOneDigit(this double value)
        {
            return Math.Round(value, 1);
        }
        
    }
}
