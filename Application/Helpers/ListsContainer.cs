using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public class ListsContainer
    {
        public List<double> PowersList { get; set; }
        public List<double> CostsList { get; set; }
        public List<string> NamesList { get; set; }

        public ListsContainer()
        {
            PowersList = new List<double>();
            CostsList = new List<double>();
            NamesList = new List<string>();
        }

        public ListsContainer GetPowersList(IEnumerable<PowerplantModel> powerplants)
        {
            var powersList = new List<double>();
            var listsContainer = new ListsContainer();
            var powerRangeStep = 0.1;

            foreach (var powerplant in powerplants)
            {
                if (powerplant.Type.ToLower() != "windturbine")
                {
                    var powerRange = (powerplant.PMax - powerplant.PMin) / powerRangeStep;
                    for (int index = 0; index <= powerRange; index++)
                    {
                        var power = powerplant.PMin + (index * powerRangeStep);
                        listsContainer.PowersList.Add(power);
                        listsContainer.CostsList.Add(power * powerplant.Cost);
                        listsContainer.NamesList.Add(powerplant.Name);
                    }
                }
            }

            return listsContainer;
        }
    }


}
