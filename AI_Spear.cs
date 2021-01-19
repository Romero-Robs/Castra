using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Spear : AI_CampController
{

    bool tankChosen = false;
    bool planeChosen = false;
    public float infantryPercent = 0.5f;
    new void Start() {
        base.Start();
    }
    public override void CalculateTroops() {
        int infTemp = 0, tankPlaneTemp = 0, divide;
        //divide resouces equally among the three buildings for different units
        if (PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] * infantryPercent > 0)
        {
            divide = (int)(PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] * infantryPercent);
            //Debug.Log("This is how much each building resource gets: " + divide);
            infTemp = divide;
            tankPlaneTemp = PersistentGameData.Instance.playerResources[AI_loc.AI_Num][0] - infTemp;
        }

        int chosenUnit = Random.Range(1, 3);
        //infantry now has half the money of my resources the AI will spend all of that money on the infantry

        //find out how many troops can be made with their split of the resources
        //find out how many barracks can be made based on the max amount of infantry
        int infantry = infTemp / BuyingManager.Instance.unitCost[0];
        //Debug.Log("can buy this many infantry: " + infantry);
        int barracks = infantry / MAX_INFANTRY;

        //find out how many tanks can be made with their split of the resources
        //find out how many factories can be made based on the max amount of tanks
        if (chosenUnit == 1)
        {
            tankChosen = true;
            planeChosen = false;
        }
        else
        {
            planeChosen = true;
            tankChosen = false;
        }
        while (barrackCount < barracks && infTemp >= BuyingManager.Instance.buildingCost[0]) {
            //Debug.Log("Adding barrack");
            barrackCount++;
            PlaceBuilding(infTemp, 0, "_Barracks");
            infTemp -= BuyingManager.Instance.buildingCost[0];
            //Debug.Log("Infantry money left: " + infTemp);
        }
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

        if (tankChosen) {
            int tank = tankPlaneTemp / BuyingManager.Instance.unitCost[1];
            int factory = tank / MAX_TANK;

            while (factoryCount < factory && tankPlaneTemp >= BuyingManager.Instance.buildingCost[1]) {
                //Debug.Log("Adding factory");
                factoryCount++;
                PlaceBuilding(tankPlaneTemp, 1, "_Factory");
                tankPlaneTemp -= BuyingManager.Instance.buildingCost[1];
                //Debug.Log("Tank money left: " + infTemp);
            }

            if (factoryCount != 0) {
                //Debug.Log("Creating tank troops");
                for (int i = 0; i < factoryCount; i++) {
                    if (tankPlaneTemp >= (BuyingManager.Instance.unitCost[1] * MAX_TANK)) {
                        //Debug.Log("Can make 2 tanks");
                        for (int j = 0; j < MAX_TANK; j++) {
                            BuyUnits(1, i);
                            tankPlaneTemp -= BuyingManager.Instance.unitCost[1];
                        }
                    }
                }
                while (tankPlaneTemp >= BuyingManager.Instance.unitCost[1]) {
                    BuyUnits(1, 0);
                    tankPlaneTemp -= BuyingManager.Instance.unitCost[1];
                }
            }
        }
        if (planeChosen) {
            int plane = tankPlaneTemp / BuyingManager.Instance.unitCost[2];
            int airport = plane / MAX_PLANE;

            while (airportCount < airport && tankPlaneTemp >= BuyingManager.Instance.buildingCost[2]) {
                //Debug.Log("Adding airport");
                airportCount++;
                PlaceBuilding(tankPlaneTemp, 2, "_Airfield");
                tankPlaneTemp -= BuyingManager.Instance.buildingCost[2];
                //Debug.Log("Plane money left: " + planeTemp);
            }

            if (airportCount != 0) {
                //Debug.Log("Creating plane troops");
                for (int i = 0; i < airportCount; i++) {
                    if (tankPlaneTemp >= (BuyingManager.Instance.unitCost[2] * MAX_PLANE)) {
                        //Debug.Log("Can make 2 planes");
                        for (int j = 0; j < MAX_PLANE; j++) {
                            BuyUnits(2, i);
                            tankPlaneTemp -= BuyingManager.Instance.unitCost[2];
                        }
                    }
                }
                while (infTemp >= BuyingManager.Instance.unitCost[2]) {
                    BuyUnits(2, 0);
                    tankPlaneTemp -= BuyingManager.Instance.unitCost[2];
                }
            }
        }
    }
}
