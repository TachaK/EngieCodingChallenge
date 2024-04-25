using Application.Helpers;
using Core.Constants;
using Core.Interface;
using Core.Models;

namespace Application
{
    public class ComputeSolution: IComputeSolution
    {
        public Response Solution(Payload payload)
        {
            var load = payload.Load;
            var solutions = new List<PowerplantResponse>();
            var result = new Response();
            var tempResult = new TemporaryResponseValues();
            var sortedPowerplants = payload.GetPowerplantMeritOrder();

            foreach (var plant in sortedPowerplants)
            {
                //Wind powerplants
                ProcessWindTurbinePlant(plant, load, result, tempResult);
                if (tempResult.ShouldBreak)
                {
                    tempResult.ShouldBreak = false;
                    break;
                }
                // Non-wind powerplants
                else if(!ProjectConstants.WINDTURBINE.Equals(plant.Type.ToLower())
                        && plant.PMax != 0)
                {
                    ProcessGasAndJetFuelPlant(plant, load, result, tempResult);
                    if (tempResult.ShouldBreak)
                    {
                        tempResult.ShouldBreak = false;
                        break;
                    }
                    if (tempResult.ShouldContinue)
                    {
                        tempResult.ShouldContinue = false;
                        continue;
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
                result = ReProcessLoadBalancing(result, tempResult.TotalCapacity, load, sortedPowerplants);
            }
            return result;
        }




        protected Response ReProcessLoadBalancing(Response response, double totalCapacity, double load, List<PowerplantModel> meritOrder)
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

        protected void ProcessWindTurbinePlant(PowerplantModel plant, double load, Response result, TemporaryResponseValues tempResponse)
        {
            if (ProjectConstants.WINDTURBINE.Equals(plant.Type.ToLower())
                && plant.PMax != 0)
            {
                tempResponse.TotalCapacity += plant.PMax.RoundToOneDigit();
                if (tempResponse.TotalCapacity < load)
                    result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                else if (tempResponse.TotalCapacity > load)
                    tempResponse.TotalCapacity -= plant.PMax.RoundToOneDigit();
                else
                {
                    result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                    tempResponse.ShouldBreak = true;
                }
            }
        }

        protected void ProcessGasAndJetFuelPlant(PowerplantModel plant, double load, Response result, TemporaryResponseValues tempResponse)
        {
            tempResponse.TotalCapacity += plant.PMin.RoundToOneDigit();
            if (tempResponse.TotalCapacity == load)
            {
                result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin.RoundToOneDigit()));
                tempResponse.ShouldBreak = true;
            }
            else if (tempResponse.TotalCapacity > load)
            {
                tempResponse.TotalCapacity -= plant.PMin.RoundToOneDigit();
                tempResponse.ShouldContinue = true;
            }
            else
            {   //If PMin added is less than load, check if PMax can be added. If not, find the power between PMin and PMax for this plant
                FindPowerBetweenPMinAndPMax(plant, load, result, tempResponse);
            }
        }

        protected void FindPowerBetweenPMinAndPMax(PowerplantModel plant, double load, Response result, TemporaryResponseValues tempResponse)
        {
            tempResponse.TotalCapacity -= plant.PMin.RoundToOneDigit();
            if (load > tempResponse.TotalCapacity.RoundToOneDigit() + plant.PMax.RoundToOneDigit())
            {
                tempResponse.TotalCapacity += plant.PMax.RoundToOneDigit();
                result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                tempResponse.ShouldContinue = true;
            }
            else if (load == tempResponse.TotalCapacity.RoundToOneDigit() + plant.PMax.RoundToOneDigit())
            {
                result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
                tempResponse.ShouldBreak = true;
            }
            else
            {
                while (tempResponse.TotalCapacity != load)
                {
                    plant.PMin += 0.1.RoundToOneDigit();
                    tempResponse.TotalCapacity += plant.PMin.RoundToOneDigit();
                    if (tempResponse.TotalCapacity.RoundToOneDigit() != load.RoundToOneDigit())
                        tempResponse.TotalCapacity -= plant.PMin.RoundToOneDigit();
                    else
                    {
                        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin.RoundToOneDigit()));
                    }
                };
            }
        }
    }
}