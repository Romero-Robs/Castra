using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_LocationManager : MonoBehaviour {
    public static AI_LocationManager Instance { get; private set; }

    public GameObject locationAI;
    //each AI will have their own location controller script,. not sharede globally
    //public List<AI_LocationController> locationAIs = new List<AI_LocationController>();
    public List<GameObject> locationAIs = new List<GameObject>();
    void Awake() {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    void Start() {
        InitAI();
    }

    void SetDifficulty(GameObject AI) {
        if(PersistentGameData.Instance.difficulty == 0){
            AI.AddComponent<AI_Passive>();
        }
        else if (PersistentGameData.Instance.difficulty == 1) {
            AI.AddComponent<AI_LocationController>();
        }
        else if(PersistentGameData.Instance.difficulty == 2){
            AI.AddComponent<AI_Aggressive>();
        }
    }

    void InitAI() {
        //loop through player list to determine what is AI and what is player
        for (int i = 1; i < PersistentGameData.Instance.playerCount; i++) {
            //if the current index is not a player it will instantiate the location AI prefab
            if (i != PersistentGameData.Instance.playerNum) {
                GameObject newLocationAI = Instantiate(locationAI, transform);
                SetDifficulty(newLocationAI);
                if (newLocationAI.GetComponent<AI_LocationController>() != null)
                {
                    newLocationAI.GetComponent<AI_LocationController>().AI_Num = i;
                }
                locationAIs.Add(newLocationAI);
            }
        }
    }

    IEnumerator PlaceCapitals() {
        for(int i = 0; i < locationAIs.Count; i++) {
            //Debug
            if (DebugManager.Instance.debugCapital == false) {
                yield return new WaitForSeconds(2f);
            }

            if (locationAIs[i].GetComponent<AI_LocationController>() != null){
                locationAIs[i].GetComponent<AI_LocationController>().PickCapital();
            }
        }
        Calendar.Instance.StartClock();
    }

    public void AIDefeated(int _AI_Num) {
        //locationAIs[_AI_Num - 2]
        PlayerListManager.Instance.RemovePlayer(_AI_Num);
    }
}
