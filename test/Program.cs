// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.XPath;
using Core.Models;

var filePath = "..\\..\\..\\..\\Core\\Data\\payload3.json";
string jsonFileContents = File.ReadAllText(filePath, Encoding.UTF8);
var payload = JsonConvert.DeserializeObject<Payload>(jsonFileContents);

var solution = Solution(payload);

var jsonResult = JsonConvert.SerializeObject(solution, Formatting.Indented);

Console.WriteLine(jsonResult);


// non-recursive method
static Response Solution(Payload payload)
{
    var load = payload.Load;
    var sortedPowerplants = GetPowerplantMeritOrder(payload);

    var listsContainer = GetPowersList(sortedPowerplants);

    var powersList = listsContainer.PowersList;
    var costsList = listsContainer.CostsList;
    var namesList = listsContainer.NamesList;

    var totalCapacity = 0.0;
    var cost = 0.0;

    var solutions = new List<PowerplantResponse>();
    var result = new Response();

    foreach(var plant in sortedPowerplants)
    {
        //Wind powerplants
        if ("windturbine".Equals(plant.Type.ToLower())
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
        else
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
            {
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
                    do
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
                    }
                    while (totalCapacity != load);
                }
            }
        }
    }

    return result;

}

static IEnumerable<PowerplantModel> GetPowerplantMeritOrder(Payload payload)
{
    var load = payload.Load;    
    var meritOrder = new List<PowerplantModel>();

    foreach (var powerplant in payload.PowerPlants)
    {
        switch (powerplant.Type.ToLower())
        {
            case "windturbine":
                var powerPlantToAdd = ToPowerplantModel(powerplant, 0, 0);
                powerPlantToAdd.PMax *= payload.Fuels.Wind/(double)100.0;
                meritOrder.Add(powerPlantToAdd);
                break;
            case "turbojet":
                meritOrder.Add(ToPowerplantModel(powerplant, (payload.Fuels.Kerosine * load)/ powerplant.Efficiency, powerplant.Efficiency * payload.Fuels.Kerosine));
                break;
            case "gasfired":
                meritOrder.Add(ToPowerplantModel(powerplant, (payload.Fuels.Gas * load)/ powerplant.Efficiency, powerplant.Efficiency * payload.Fuels.Gas));
                break;
            default: break;
        }
    }
    return meritOrder.OrderBy(m => m.Merit);
}

static PowerplantModel ToPowerplantModel(Powerplant powerplant, double merit, double cost)
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

static ListsContainer GetPowersList(IEnumerable<PowerplantModel> powerplants)
{
    var powersList = new List<double>();
    var listsContainer = new ListsContainer();
    var powerRangeStep = 0.1;

    foreach (var powerplant in powerplants)
    {
        if(powerplant.Type.ToLower() != "windturbine")
        {
            var powerRange = (powerplant.PMax - powerplant.PMin)/powerRangeStep;
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

//static double ProcessWindPlants(PowerplantModel plant, double load, double totalCapacity, Response result)
//{
//    totalCapacity += plant.PMax.RoundToOneDigit();
//    if (totalCapacity < load)
//        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
//    else if (totalCapacity > load)
//        totalCapacity -= plant.PMax.RoundToOneDigit();
//    else
//    {
//        result.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax.RoundToOneDigit()));
//        break;
//    }
//}

public static class DoubleExtensions
{
    public static double RoundToOneDigit(this double value)
    {
        return Math.Round(value, 1);
    }
}

public class ListsContainer{
    public List<double> PowersList { get; set; }
    public List<double> CostsList { get; set; }
    public List<string> NamesList { get; set; }

    public ListsContainer()
    {
        PowersList= new List<double>();
        CostsList= new List<double>();
        NamesList= new List<string>();
    }
}



//Solution(payload);

//Response KnapsackEnergyLoad(int capacity, List<Powerplant> powerplant)
//{
//    var response = new Response();
//    // Base case
//    if (capacity <= 0)
//        var powerplantnull = new PowerplantResponse(0,);
//    return (0, new List<EnergyProducer>());
//    int minCost = int.MaxValue;
//    List<Powerplant> bestCombination = new List<Powerplant>();

//    foreach (var producer in producers)
//    {
//        int cost = producer.Cost;
//        int effectiveness = producer.Effectiveness;

//        if (cost > capacity)
//            continue;

//        int remainingCapacity = capacity - cost;
//        var (remainingEffectiveness, remainingCombination) = KnapsackEnergyLoad(remainingCapacity, producers);

//        int totalCost = cost + remainingEffectiveness;
//        int totalEffectiveness = effectiveness + remainingCombination.Sum(p => p.Effectiveness);

//        if (totalCost < minCost)
//        {
//            minCost = totalCost;
//            bestCombination = new List<EnergyProducer> { producer };
//            bestCombination.AddRange(remainingCombination);
//        }
//    }

//    return (minCost, bestCombination);
//}

//Response KnapsackEnergyLoad(int capacity, List<Powerplant> powerPlants, List<double> windEfficiency, List<double> jetcost, List<double> gascost)
//{
//    var response = new Response
//    {
//        PowerplantResponses= new List<PowerplantResponse>()
//    };

//    KnapsackEnergyLoadHelper(capacity, powerPlants, response.PowerplantResponses, windEfficiency, jetcost, gascost);

//    return response;
//}


//int KnapsackEnergyLoadHelper(double load, List<PowerplantModel> powerPlants, List<PowerplantResponse> result)
//{
//    if (load <= 0 || powerPlants.Count == 0)
//        return 0;

//    double minCost = double.MaxValue;
//    double capacity = 0;

//    List<PowerplantResponse> bestCombination = new List<PowerplantResponse>();

//    foreach (var plant in powerPlants)
//    {

//        if (plant.PMax <= load)
//        {
//            var remainingPowerPlants = powerPlants.Where(p => p != plant).ToList();
//            int remainingCapacity = load - plant.PMax;
//            int remainingPower = KnapsackEnergyLoadHelper(remainingCapacity, remainingPowerPlants, result, windEfficiency, jetcost, gascost);
//            int powerProduced = plant.PMax;
//            int totalPowerProduced = powerProduced + remainingPower;

//            if (totalPowerProduced < minCost)
//            {
//                minCost = totalPowerProduced;
//                bestCombination = new List<PowerplantResponse>
//                    {
//                        new PowerplantResponse ( plant.Name, powerProduced )
//                    };
//                bestCombination.AddRange(result);
//            }
//        }
//    }

//    //result.Clear();
//    result.AddRange(bestCombination);

//    return minCost;
//}

//static void KnapsackEnergyLoader(int n, int c)
//{

//}





