using Application.Helpers;
using Core.Constants;
using Core.Interface;
using Core.Models;
using Microsoft.VisualBasic;

namespace Application
{
    public class ComputeSolution: IComputeSolution
    {
        public Response Solution(Payload payload)
        {
            var load = payload.Load;
            var sortedPowerplants = payload.GetPowerplantMeritOrder();

            var listsContainer = new ListsContainer().GetPowersList(sortedPowerplants);

            var powersList = listsContainer.PowersList;
            var costsList = listsContainer.CostsList;
            var namesList = listsContainer.NamesList;

            var totalCapacity = 0.0;
            var cost = 0.0;

            var solutions = new List<PowerplantResponse>();
            var result = new Response();

            foreach (var plant in sortedPowerplants)
            {
                //Wind powerplants
                if (ProjectConstants.WINDTURBINE.Equals(plant.Type.ToLower())
                    && plant.PMax != 0)
                {
                    totalCapacity += plant.PMax.RoundToOneDigit();
                    if (totalCapacity < load)
                        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                    else if (totalCapacity > load)
                        totalCapacity -= plant.PMax.RoundToOneDigit();
                    else
                    {
                        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                        break;
                    }
                }

                // Non-wind powerplants
                else if(!ProjectConstants.WINDTURBINE.Equals(plant.Type.ToLower())
                        && plant.PMax != 0)
                {
                    totalCapacity += plant.PMin.RoundToOneDigit();
                    if (totalCapacity == load)
                    {
                        cost += plant.Cost.RoundToOneDigit() * plant.PMin.RoundToOneDigit();
                        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin.RoundToOneDigit()));
                        break;
                    }
                    else if (totalCapacity > load)
                    {
                        totalCapacity -= plant.PMin.RoundToOneDigit();
                        continue;
                    }
                    else
                    {   //If PMin added is less than load, check if PMax can be added. If not, find the power between PMin and PMax for this plant
                        totalCapacity -= plant.PMin;
                        if (load > totalCapacity.RoundToOneDigit() + plant.PMax.RoundToOneDigit())
                        {
                            totalCapacity += plant.PMax.RoundToOneDigit();
                            cost += plant.Cost * plant.PMax.RoundToOneDigit();
                            result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                            continue;
                        }
                        else if (load == totalCapacity.RoundToOneDigit() + plant.PMax.RoundToOneDigit())
                        {
                            cost += plant.Cost.RoundToOneDigit() * plant.PMax.RoundToOneDigit();
                            result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                            break;
                        }
                        else
                        {
                            while (totalCapacity != load)
                            {
                                plant.PMin += 0.1.RoundToOneDigit();
                                totalCapacity += plant.PMin.RoundToOneDigit();
                                if (totalCapacity.RoundToOneDigit() != load.RoundToOneDigit())
                                    totalCapacity -= plant.PMin.RoundToOneDigit();
                                else
                                {
                                    cost += plant.Cost * plant.PMin.RoundToOneDigit();
                                    result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin.RoundToOneDigit()));
                                }
                            };
                        }
                    }
                }
            }

            Func<Response,double, bool> isLoadReached = (result, load) =>
            {
                var computedLoad = result.PowerplantResponses.Sum(plant => plant.Power);
                return computedLoad.RoundToOneDigit() == load.RoundToOneDigit();
            };

            if (!isLoadReached(result, load))
            {
                result = ReProcessLoadBalancing(result, ref totalCapacity, load, sortedPowerplants);
            }
            return result;

        }

        protected Response ReProcessLoadBalancing(Response response, ref double totalCapacity, double load, List<PowerplantModel> meritOrder)
        {
            var unusedPowerplant = GetUnusedPlantInMeritOrder(response, meritOrder);
            var mostUsedPowerplant = GetMostPoweredPlant(response);

            mostUsedPowerplant.Power -= unusedPowerplant.PMin;
            var powerStillNeeded = load - (totalCapacity- unusedPowerplant.PMin);

            var powerPlantToAdd = new PowerplantResponse(unusedPowerplant.Name, powerStillNeeded);

            response.PowerplantResponses.Add(powerPlantToAdd);

            return response;
        }

        protected PowerplantResponse? GetMostPoweredPlant(Response response)
        {
            var power = 0.0;
            PowerplantResponse? mostPoweredPlant = null;

            var maxPower = response.PowerplantResponses.Max(p => p.Power);
            mostPoweredPlant = response.PowerplantResponses.First(item => item.Power == maxPower);

            return mostPoweredPlant;
        }

        protected PowerplantModel? GetUnusedPlantInMeritOrder(Response response, List<PowerplantModel> meritOrder)
        {
            var found = false;
            var index = 0;
            PowerplantModel plantName = null;
            while(!found && index < meritOrder.Count())
            {
                if (meritOrder[index].PMax != 0)
                {
                    found = !response.PowerplantResponses.Any(p => p.Name == meritOrder[index].Name);
                    plantName = found ? meritOrder[index] : null;
                }
                index++;
            }
            return plantName;
        }
    }
}