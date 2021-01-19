using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI_LocationController : MonoBehaviour
{
    //============================Script References===============================
    protected Location capital;
    //public GameObject AI_camp;
    //============================================================================

    //============================Lists References===============================
    //front line locations may be the locations nearest other enemy locations(player locations)
    protected List<Location> myFrontLineLocations = new List<Location>();
    //list of all the locations attacking my locations?
    protected List<LocationInfo> attackFrom = new List<LocationInfo>();
    //list of all locations that my locations may decide to attack based on set conditions
    protected List<LocationInfo> attackTarget = new List<LocationInfo>();
    //list of midline territories
    protected List<Location> midLineLocations = new List<Location>();
    //storing info of previous day
    protected Dictionary<Location, Location> prevData = new Dictionary<Location, Location>();
    //============================================================================

    public float capitalUnitPercent = 0.75f;
    public float midLineUnitPercent = 0.75f;
    public float frontLineUnitPercent = 0.65f;
    public int AI_Num;
    int[] currentUnit;

    const int INFANTRY = 0;
    const int TANK = 1;
    const int PLANE = 2;
    bool beginnerDone = false;

    int prevDay;
    int hours;
    int first, second, third, fourth;

    //is the actual location of my capital
    int random;
    bool freeLocation;

    //the decisions that the AI will make throughout the run of the game depending
    //on the choices and updated informatin of the map

    //create the different behaviors the AI can adopt based on the information of the map and
    //its current control over the map
    virtual protected void Start() {

        for (int i = 2; i < PersistentGameData.Instance.playerCount; i++) {
            int rand = UnityEngine.Random.Range(0, 2);
            if (rand == 0) {
                //Debug.Log("Choosing regular camp");
                gameObject.AddComponent<AI_CampController>();
            }
            else {
                //Debug.Log("Choosing spear camp");
                gameObject.AddComponent<AI_Spear>();
            }
        }
        //AI_camp = GetComponent<AI_CampController>();
    }

    public void PickCapital() {
        //update the map info to be pulling from
        Locations.Instance.UpdatePlayerLocations();
        //choose random location to place capital
        //If no free location is found, there's no backup plan!!
        freeLocation = false;
        while (!freeLocation) {
            CheckLocation();
        }

        //======================================================================
        //dumb way to stop crash
        //if (AI_Num <= 3) {
        //    while (!freeLocation) {
        //        CheckLocation();
        //    }
        //}
        //else {
        //    while (!freeLocation) {
        //        random = (int)(Random.Range(1.0f, 22.0f));
        //        freeLocation = true;
        //        for (int i = 0; i < Locations.Instance.activeMap.Count; i++) {
        //            if (Locations.Instance.activeMap[random].playerNum != 0) {
        //                freeLocation = false;
        //            }
        //        }
        //    }
        //}
        //======================================================================

        capital = Locations.Instance.activeMap[random];
        LocationManager.Instance.PlaceCapital(capital.location.GetComponent<LocationInfo>(), AI_Num);
    }

    void CheckLocation() {
        random = (int)(UnityEngine.Random.Range(1.0f, 22.0f));
        freeLocation = true;
        //checked to see if that location or any surrounding locations is currently owned by an enemy
        for (int i = 0; i < Locations.Instance.activeMap.Count; i++) {
            if (Locations.Instance.activeMap[random].playerNum != 0) {
                freeLocation = false;
            }
            for (int j = 0; j < Locations.Instance.activeMap[random].neighbors.Count; j++) {
                if (Locations.Instance.activeMap[random].neighbors[j].playerNum != 0) {
                    freeLocation = false;
                }
            }
        }
    }

    private void Update() {
        if (Calendar.Instance.day != prevDay && MatchOutcome.Instance.defeatedPlayers[AI_Num - 1] == false) {
            prevDay = Calendar.Instance.day;
            MainAILoop();
        }

        //if (Calendar.Instance.hour != hours) {
        //    hours = Calendar.Instance.hour;
        //    HourlyTask();
        //}
    }

    //decide where to place troops when barracks are placede in capitals and decide
    //what should be reinforced 
    void MainAILoop() {

        int[] time = new int[4];
        for (int i = 0; i < time.Length; i++) {
            time[i] = (int)UnityEngine.Random.Range(9f, 20f);
        }

        Array.Sort(time);
        first = time[0];
        //Debug.Log(first);
        second = time[1];
        //Debug.Log(second);
        third = time[2];
        //Debug.Log(third);
        fourth = time[3];
        //Debug.Log(fourth);

        UpdateMap();

        //set it up sp it choose at what hours to do certain objectives
        //FrontlineAttackTroop();
        //yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 8f));
        //GetCapitalTroops();
        //yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        //MoveMidlineTroop();
        //yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        //FrontlineAttackTroop();
        //yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 8f));
        //MoveCapitalTroop();
    }

    void HourlyTask() { 
        //Debug.Log("hour: " + hours);
        if (hours == first) {
            FrontlineAttackTroop();
        }
        if (hours == second && capital.playerNum == AI_Num) {
            GetCapitalTroops();
        }
        if (hours == third) {
            MoveMidlineTroop();
        }
        if (hours == fourth && capital.playerNum == AI_Num) {
            MoveCapitalTroop();
            //Ready the AI up
            PlayerListManager.Instance.PlayerReady(AI_Num - 1, true);
        }
    }

    void UpdateMap() {
        //updating the map
        Locations.Instance.UpdatePlayerLocations();
        //after updating map update list of all territories in the 
        MakeFrontLine();
        //after updating the map update list of all enemies near my updated
        MakeAttackList();
        //after updating the map update list of all midline territories
        MidLineTerritories();

        //for (int i = 0; i < Locations.Instance.playerLocations[AI_Num].Count; i++)
        //{
        //    Debug.Log("My Locations: " + Locations.Instance.playerLocations[AI_Num][i].locationName);
        //}
        //for (int i = 0; i < myFrontLineLocations.Count; i++) {
        //    Debug.Log("My frontLines: " + myFrontLineLocations[i].locationName);
        //}
        //for (int i = 0; i < midLineLocations.Count; i++) {
        //    Debug.Log("My midLines: " + midLineLocations[i].locationName);
        //}
        //for (int i = 0; i < attackTarget.Count; i++) {
        //    Debug.Log("My enemies: " + attackTarget[i].thisLocation.locationName);
        //}
    }

    //==========================GIVE TROOPS TO THE CAPITAL AT THE START OF THE DAY======================================
    void GetCapitalTroops() {
        if (PersistentGameData.Instance.playerResources[AI_Num][0] < 300 && !beginnerDone) {
            gameObject.GetComponent<AI_CampController>().BuildingGameStart();

        }
        else {
            gameObject.GetComponent<AI_CampController>().CalculateTroops();
            beginnerDone = true;
        }
        //if (gameObject.GetComponent<AI_CampController>()) {
        //    if (PersistentGameData.Instance.playerResources[AI_Num][0] < 300 && !beginnerDone) {
        //        gameObject.GetComponent<AI_CampController>().BuildingGameStart();

        //    }
        //    else {
        //        gameObject.GetComponent<AI_CampController>().CalculateTroops();
        //        beginnerDone = true;
        //    }
        //}
        //if (gameObject.GetComponent<AI_Spear>()) {
        //    if (PersistentGameData.Instance.playerResources[AI_Num][0] < 300 && !beginnerDone) {
        //        gameObject.GetComponent<AI_Spear>().BuildingGameStart();

        //    }
        //    else {
        //        gameObject.GetComponent<AI_Spear>().CalculateTroops();
        //        beginnerDone = true;
        //    }
        //}
       
        //capital.location.GetComponent<LocationInfo>().UpdateDisplay();
    }
    //==================================================================================================================

    //==========================LOOK FRO FIRST NEIGHBOR WITH A FRONTLINE===================================================
    Location SearchNeighbor(Location root){
        Location saved = FindFrontline(root);
        if (saved != null) {
            Debug.Log("Found first frontline: " + saved);
            return saved;
        }
        else
        {
            Debug.Log("Going into else into neighbors");
            for(int i = 0; i < root.neighbors.Count; i++)
            {
                return SearchNeighbor(root.neighbors[i]);
            }
        }
        return null;
    }

    Location FindFrontline(Location loc) {
        for(int i = 0; i < myFrontLineLocations.Count; i++) {
            if (loc.neighbors.Contains(myFrontLineLocations[i])) {
                //Debug.Log("Found a frontline: " + loc.neighbors[i]);
                return loc.neighbors[loc.neighbors.IndexOf(myFrontLineLocations[i])];
            }
        }
        return null;
    }
    //=====================================================================================================================

    //======================UPDATES MAP AND MOVES MY CURRENT TROOPS FROM CAPITAL=========================================
    virtual protected void MoveCapitalTroop() {

        Location dest = MidlineToFrontTroop(capital);
       // Debug.Log("Looking for frontline and found: " + SearchNeighbor(capital).locationName);
        if((int)(capital.supply * capitalUnitPercent) > 0){
            MoveTroops(capital.unitData, capital.supply - (int)(capital.supply * capitalUnitPercent), capital.supply, capital, dest);//capital.neighbors[location]
        }
       
    }
    //===================================================================================================================

    //======================UPDATES MAP AND MOVES MY CURRENT TROOPS FROM MIDLINES=========================================
    virtual protected void MoveMidlineTroop() {

        if(midLineLocations.Count > 0) {
            Location dest;
            for (int i = 0; i < midLineLocations.Count; i++) {

                dest = MidlineToFrontTroop(midLineLocations[i]);
                if ((int)(midLineLocations[i].supply * midLineUnitPercent) > 0 && dest.locationID != capital.locationID) {
                    MoveTroops(midLineLocations[i].unitData, midLineLocations[i].supply - (int)(midLineLocations[i].supply * midLineUnitPercent), midLineLocations[i].supply, midLineLocations[i], dest);//midLineLocations[mLocation].neighbors[nLocation]
                }
            }
        }
    }

    virtual protected Location MidlineToFrontTroop(Location midline) {
        //looop through all the neighbors of the midline, and loop through all frontlines find the lowest frontline, give troop
        //else give to a frontline regardless, last case is give to the lowest neighbor
        List<Location> allFrontNeighbors = new List<Location>();
        int index = 0; 
        int lowestFront;
        for (int i = 0; i < myFrontLineLocations.Count; i++) {
            //Debug.Log("frontline" + myFrontLineLocations[i].locationName);
            if (midline.neighbors.Contains(myFrontLineLocations[i])) {
                //Debug.Log("frontline near midline" + myFrontLineLocations[i].locationName);
                allFrontNeighbors.Add(myFrontLineLocations[i]);
            }
        }

        if (allFrontNeighbors.Count > 0) {
            Location selectedFront;
            lowestFront = allFrontNeighbors[0].supply;
            for (int i = 0; i < allFrontNeighbors.Count; i++) {
                if (lowestFront > allFrontNeighbors[i].supply) {
                    lowestFront = allFrontNeighbors[i].supply;
                    index = i;
                }
            }
            selectedFront = allFrontNeighbors[index];
            //Debug.Log("Giving to my frontline first");
            //Debug.Log("Found lowest frontline: " + myFrontLineLocations[index].locationName);
            return selectedFront;
        }
        else {
            //Debug.Log("Going to give to midline instead, no frontline");
            return MidlineToMidline(midline);
        }
    }

    virtual protected Location MidlineToMidline(Location midline) {
        int lowestMid = midline.neighbors[0].supply;
        int index = 0;
        for(int i = 0; i < midline.neighbors.Count; i++) {
            if(lowestMid > midline.neighbors[i].supply) {
                index = i;
                lowestMid = midline.neighbors[i].supply;
            }
        }
        //Debug.Log("Found lowest midline: " + midline.neighbors[index].locationName);
        return midline.neighbors[index];
    }
    //===================================================================================================================

    //======================UPDATES MAP AND MOVES MY CURRENT TROOPS FROM CAPITAL=========================================
    virtual protected void FrontlineAttackTroop() {
        for(int i = 0; i < myFrontLineLocations.Count; i++) {
            for(int j = 0; j < myFrontLineLocations[i].neighbors.Count; j++) {
                if (attackTarget.Contains(myFrontLineLocations[i].neighbors[j].location.GetComponent<LocationInfo>()) && (int)(myFrontLineLocations[i].supply * frontLineUnitPercent) > myFrontLineLocations[i].neighbors[j].supply) {
                    //Check if dest is not currently in combat
                    if (myFrontLineLocations[i].neighbors[j].location.GetComponent<LocationInfo>().moveStates[2] == false) { 
                        MoveTroops(myFrontLineLocations[i].unitData, myFrontLineLocations[i].supply - (int)(myFrontLineLocations[i].supply * frontLineUnitPercent), myFrontLineLocations[i].supply, myFrontLineLocations[i], myFrontLineLocations[i].neighbors[j]);
                    }
                }
            }
        }
    }
    //===================================================================================================================

    //======================UPDATES MAP AND UPDATES MY CURRENT FRONTLINES=========================================
    void MakeFrontLine() {
        //clear frontlines
        myFrontLineLocations.Clear();
        //search for all locations that may be the frontlines
        for (int i = 0; i < Locations.Instance.playerLocations[AI_Num].Count; i++) {
            FindMatch(Locations.Instance.playerLocations[AI_Num][i]);
        }
    }

    void FindMatch(Location loc) {
        //loop through the neighbors
        //location.GetComponent<LocationInfo>().thisLocation.adjacencyData.Length
        for (int i = 0; i < loc.neighbors.Count; i++) {
            for(int j = 1; j < Locations.Instance.activeMap.Count + 1; j++) {
                //Debug.Log(Locations.Instance.activeMap[j].locationID - 1);
                if (loc.neighbors.Contains(Locations.Instance.activeMap[j]) && 
                    Locations.Instance.activeMap[j].playerNum != loc.playerNum) {
                    //add into the list
                    //Locations.Instance.activeMap[j].playerNum != 0 this is to ignore the neutral territories
                    myFrontLineLocations.Add(loc);
                }
            }
        }
    }
    //================================================================================================================

    //======================UPDATES MAP AND UPDATES MY CURRENT MIDLINE LOCATIONS=========================================
    void MidLineTerritories() {
        //clear the midlines with each update
        midLineLocations.Clear();
        //compare all AI current location with the frontlines and if they are not the same and is not a capital add it into the midline list
        for (int i = 0; i < Locations.Instance.playerLocations[AI_Num].Count; i++) {
            if (!myFrontLineLocations.Contains(Locations.Instance.playerLocations[AI_Num][i]) &&
                Locations.Instance.playerLocations[AI_Num][i].locationID != random) {
                //add the locastion into the list
                midLineLocations.Add(Locations.Instance.playerLocations[AI_Num][i]);
            }
        }
    }
    //====================================================================================================================

    //======================UPDATES MAP AND UPDATES MY CURRENT ENEMIES LIST=========================================
    void MakeAttackList() {
        //clear list to update
        attackTarget.Clear();
        //check if the neighbor attack num is higher or lower than frontline units
        //if higher request a neighboring ally to supply thsome amount of units
        for (int i = 0; i < myFrontLineLocations.Count; i++) {
            for (int j = 0; j < myFrontLineLocations[i].adjacencyData.Length; j++) {
                if (myFrontLineLocations[i].adjacencyData[j] == 1 && Locations.Instance.activeMap[j + 1].playerNum != AI_Num) {
                    //Debug.Log("To:" + myFrontLineLocations[i].locationName + " From: " + Locations.Instance.activeMap[j + 1].locationName);
                    //attack the territory
                    attackTarget.Add(Locations.Instance.activeMap[j + 1].location.GetComponent<LocationInfo>());
                }
            }
        }
    }
    //==============================================================================================================

    //======================UPDATES MAP AND MOVES MY UNITS AROUND WITHIN MY TERRITORIES=========================================
    //script to try and move the units to another ally territory, use as reference only
    protected void MoveTroops(int[] _maxUnits, int supply, int total, Location src, Location dest) {
        //create clones of amount of units given
        currentUnit = (int[])_maxUnits.Clone();

        if (src.location.GetComponent<LocationInfo>().moveStates[0] && dest.location.GetComponent<LocationInfo>().moveStates[0]) {
            if (!src.location.GetComponent<LocationInfo>().moveStates[2] && !dest.location.GetComponent<LocationInfo>().moveStates[2]) {
                //decide which neighbors get what amount of troops
                //pass in the locations of the neighbors that will be sent the troops
                //Debug.Log("Passing " + supply + " troops from: " + src.locationName + " to " + dest.locationName);
                LocationManager.Instance.MoveUnits(CalcSupply(currentUnit, supply, total), src.location.GetComponent<LocationInfo>(), dest.location.GetComponent<LocationInfo>());
            }
        }
    }

    int[] CalcSupply(int[] unit, int sup, int totalSup) {

        //Debug.Log("supply: " + sup);
        int total = totalSup;
        int[] moveUnits = new int[6];

        while (total > sup) {
           // Debug.Log("Total: " + total);
            if (total - (PLANE + 1) >= sup && unit[PLANE] != 0) {
                moveUnits[PLANE]++;
                total -= (PLANE + 1);
                //Debug.Log("Total after plane: " + total);
            }
            if (total - (TANK + 1) >= sup && unit[TANK] != 0) {
                moveUnits[TANK]++;
                total -= (TANK + 1);
                //Debug.Log("Total after tank: " + total);
            }
            if (total - (INFANTRY + 1) >= sup && unit[INFANTRY] != 0) {
                moveUnits[INFANTRY]++;
                total -= (INFANTRY + 1);
                //Debug.Log("Total after infantry: " + total);
            }
        }

        //return moveUnits;
        return moveUnits;
    }
    //==========================================================================================================================
}
