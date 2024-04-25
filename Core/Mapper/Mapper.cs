using Core.Models;

namespace Core.Helpers
{
    public static class Mapper
    {
        public static PowerplantModel ToPowerplantModel(Powerplant powerplant, double merit, double cost)
        {
            var powerplantModel = new PowerplantModel
            {
                Name = powerplant.Name,
                Type = powerplant.Type,
                Efficiency = powerplant.Efficiency,
                PMin = powerplant.PMin,
                PMax = powerplant.PMax,
                Merit = merit,
                Cost = cost
            };
            return powerplantModel;
        }
    }
}
