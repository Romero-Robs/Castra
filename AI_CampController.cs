using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CampController : MonoBehaviour
{
    //============================Script References===============================
    protected AI_LocationController AI_loc;
    //============================================================================
    //=============Troop count and array for troops======================
    protected Dictionary<int, List<GameObject>> allBuildings = new Dictionary<int, List<GameObject>>();
    //==================================================================
    //==============Resource usage and resource count====================
    protected const int MAX_INFANTRY = 4;
    protected const int MAX_TANK = 2;
    protected const int MAX_PLANE = 2;
    protected int barrackCount = 0;
    protected int factoryCount = 0;
    protected int airportCount = 0;
    //==================================================================
    public CampInfo thisCamp;

    protected virtual void Start() {
        AI_loc = GetComponent<AI_LocationController>();
    }

    public void BuildingGameStart() {
        //Debug.Log("Buying troops");
        if(PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] >= BuyingManager.Instance.buildingCost[0] && barrackCount < 1) {
            barrackCount++;
            PlaceBuilding(PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0], 0, "_Barracks");
        }
        while (PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] > BuyingManager.Instance.unitCost[0]) {
            BuyUnits(0, 0);
        }
    }

    //need to grab all troop camp things from location controller and put it into this script
    virtual public void CalculateTroops() {
        int infTemp = 0, tankTemp = 0, planeTemp = 0, divide;
        //divide resouces equally among the three buildings for different units
        if (PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] / 3 > 0) {
            divide = PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] / 3;
            //Debug.Log("This is how much each building resource gets: " + divide);
            infTemp = divide;

            tankTemp = divide;

            planeTemp = divide;
        }

        //find out how many troops can be made with their split of the resources
        //find out how many barracks can be made based on the max amount of infantry
        int infantry = infTemp / BuyingManager.Instance.unitCost[0];
        int barracks = infantry / MAX_INFANTRY;

        //find out how many tanks can be made with their split of the resources
        //find out how many factories can be made based on the max amount of tanks
        int tank = tankTemp / BuyingManager.Instance.unitCost[1];
        int factory = tank / MAX_TANK;

        //find out how many planes can be made with their split of the resources
        //find out how many airfields can be made based on the max amount of planes
        int plane = planeTemp / BuyingManager.Instance.unitCost[2];
        int airport = plane / MAX_PLANE;

        //continuously make the production buildings needed for the max amount of units wanting to create
        //check if have enough resources remaining to buy the building and the troops after
        while (barrackCount < barracks && infTemp >= BuyingManager.Instance.buildingCost[0]) {
           // Debug.Log("Adding barrack");
            barrackCount++;
            PlaceBuilding(infTemp, 0, "_Barracks");
            infTemp -= BuyingManager.Instance.buildingCost[0];
            //Debug.Log("Infantry money left: " + infTemp);
        }
        while (factoryCount < factory && tankTemp >= BuyingManager.Instance.buildingCost[1]) {
            //Debug.Log("Adding factory");
            factoryCount++;
            PlaceBuilding(tankTemp, 1, "_Factory");
            tankTemp -= BuyingManager.Instance.buildingCost[1];
            //Debug.Log("Tank money left: " + infTemp);
        }
        while (airportCount < airport && planeTemp >= BuyingManager.Instance.buildingCost[2]) {
            //Debug.Log("Adding airport");
            airportCount++;
            PlaceBuilding(planeTemp, 2, "_Airfield");
            planeTemp -= BuyingManager.Instance.buildingCost[2];
            //Debug.Log("Plane money left: " + planeTemp);
        }

        //buy troops based on the amount of resources that are left either buy max amount based on building count
        //or buy as many troops as you can with your money
        if (barrackCount != 0) {
            //Debug.Log("Creating infantry troops");
            for (int i = 0; i < barrackCount; i++) {
                if (infTemp >= (BuyingManager.Instance.unitCost[0] * MAX_INFANTRY)) {
                   // Debug.Log("Can make 4 troops");
                    for (int j = 0; j < MAX_INFANTRY; j++) {
                        BuyUnits(0, Random.Range(0, barrackCount));
                        infTemp -= BuyingManager.Instance.unitCost[0];
                    }
                }
            }
            while (infTemp >= BuyingManager.Instance.unitCost[0]) {
                BuyUnits(0, Random.Range(0, barrackCount));
                infTemp -= BuyingManager.Instance.unitCost[0];
            }
        }

        if (factoryCount != 0) {
            //Debug.Log("Creating tank troops");
            for (int i = 0; i < factoryCount; i++) {
                if (tankTemp >= (BuyingManager.Instance.unitCost[1] * MAX_TANK)) {
                    //Debug.Log("Can make 2 tanks");
                    for (int j = 0; j < MAX_TANK; j++) {
                        BuyUnits(1, i);
                        tankTemp -= BuyingManager.Instance.unitCost[1];
                    }
                }
            }
            while (tankTemp >= BuyingManager.Instance.unitCost[1]) {
                BuyUnits(1, 0);
                tankTemp -= BuyingManager.Instance.unitCost[1];
            }
        }

        if (airportCount != 0) {
            //Debug.Log("Creating plane troops");
            for (int i = 0; i < airportCount; i++) {
                if (planeTemp >= (BuyingManager.Instance.unitCost[2] * MAX_PLANE)) {
                    //Debug.Log("Can make 2 planes");
                    for (int j = 0; j < MAX_PLANE; j++) {
                        BuyUnits(2, i);
                        planeTemp -= BuyingManager.Instance.unitCost[2];
                    }
                }
            }
            while (infTemp >= BuyingManager.Instance.unitCost[2]) {
                BuyUnits(2, 0);
                planeTemp -= BuyingManager.Instance.unitCost[2];
            }
        }
    }
    public void PlaceBuilding(int canAfford, int index, string name) {
        if (canAfford - BuyingManager.Instance.buildingCost[index] >=0 && ResourceManager.Instance.ChangeResource(0, -(BuyingManager.Instance.buildingCost[index]), AI_loc.AI_Num)) {
            if (Resources.Load("Items/Faction" + PersistentGameData.Instance.playerFactions[AI_loc.AI_Num] + name)) {

                GameObject SelectedBuilding = Resources.Load("Items/Faction" + PersistentGameData.Instance.playerFactions[AI_loc.AI_Num] + name) as GameObject;
                GameObject FinalBuilding = Instantiate(SelectedBuilding);
                FinalBuilding.transform.SetParent(thisCamp.transform.Find("Items"), false);
                FinalBuilding.transform.position = new Vector3(FinalBuilding.transform.position.x + Random.Range(-25, 25), FinalBuilding.transform.position.y + 5, FinalBuilding.transform.position.z);
                FinalBuilding.GetComponent<BuildingInfo>().campInfo = thisCamp;

                if (!allBuildings.ContainsKey(index)) {
                    List<GameObject> temp = new List<GameObject>();
                    allBuildings.Add(index, temp);
                    allBuildings[index].Add(FinalBuilding);
                }
                else {
                    allBuildings[index].Add(FinalBuilding);
                }
            }
        }
        
    }

    public void BuyUnits(int index, int buildingIndex) {
        Transform buyButton = allBuildings[index][buildingIndex].GetComponent<BuildingInfo>().buyMenu.transform.GetChild(index);
        buyButton.GetComponent<BuyUnitInfo>().BuyUnit();
    }
}
