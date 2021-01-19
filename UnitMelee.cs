using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMelee : UnitCombat
{
    //=================================Variables================================
    public float healthStart = 275f;
    //==========================================================================

    protected override void Awake()
    {
        base.Awake();
        unitController.health = healthStart;
    }

    protected override IEnumerator StartFight() {
        CheckAttack();

        yield return new WaitForSeconds(3f);

        //checks to see if unit is still in combat, keep attacking while still in combat
        while (unitController.inCombat) {
            //orders the targets ina a list by who is closest to the current unit
            sortedTargets = targetSquads.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
            if (sortedTargets.Count > 0) {
                //==============================================tactic for later==========================================================
                //move character towards the enemy based on who is first in the target list
                //the actual combat happens here where the enemy is less than 2f from the current unit

                //this is where the tactic aspect of the units will happen but the conditions should be in a different area
                //make sure there are an even amount of pairs that can be used on the enemy units

                //ignore tactics for now
                //if((unitSquads.Count % targetSquads.Count) == 0 && unitSquads.Count != targetSquads.Count)
                //{
                //    if (debug) {
                //        Debug.Log("Even number of pairs for the enemy");
                //    }

                //    int size = unitSquads.Count;
                //    int squad = (unitSquads.Count / targetSquads.Count) - 1;
                //    int start = 0;

                //    while(start < size) {
                //        if (debug) {
                //            Debug.Log("size: " + size);
                //            Debug.Log("start: " + start);
                //            Debug.Log("squad: " + squad);
                //        }
                //        for (int i = start; i <= squad; i++) {
                //            if (debug) {
                //                Debug.Log(unitSquads[i]);
                //                Debug.Log(unitSquads[i].gameObject.GetComponent<UnitMelee>().sortedTargets.Count);
                //            }
                //            //CopyList(unitSquads[i].gameObject.GetComponent<UnitMelee>().sortedTargets, this.sortedTargets);
                //        }
                //        start += unitSquads.Count / targetSquads.Count;
                //        squad += unitSquads.Count / targetSquads.Count;
                //    }
                //}
                //else 
                //{
                //==================================================================================================================
                if (!sortedTargets[0].gameObject.GetComponent<UnitAir>()) {
                    StartCoroutine(unitController.PerformMovement(sortedTargets[0].transform.position));
                    if (Vector3.Distance(sortedTargets[0].transform.position, transform.position) < 2f) {
                        StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                        transform.LookAt(sortedTargets[0].transform);
                        //attacks the enemy unit that was found closest to current 
                        StartCoroutine(Attack(sortedTargets[0]));
                        yield return new WaitForSeconds(1.3f);
                    }
                }
                else {
                    if(sortedTargets[0].gameObject.GetComponent<UnitAir>() && sortedTargets.Count == 1) {
                        Vector3 position = (combatInfo.defendingUnits.transform.position + combatInfo.attackingUnits.transform.position) / 2;
                        StartCoroutine(unitController.PerformMovement(position));
                        if (Vector3.Distance(position, transform.position) < 2f) {
                            StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                            yield return new WaitForSeconds(1.3f);
                        }
                    }
                    else if(!sortedTargets[1].gameObject.GetComponent<UnitAir>()) {
                        Debug.Log("Attacking: " + sortedTargets[1].name);
                        StartCoroutine(unitController.PerformMovement(sortedTargets[1].transform.position));
                        if (Vector3.Distance(sortedTargets[1].transform.position, transform.position) < 2f) {
                            StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                            transform.LookAt(sortedTargets[1].transform);
                            //attacks the enemy unit that was found closest to current 
                            StartCoroutine(Attack(sortedTargets[1]));
                            yield return new WaitForSeconds(1.3f);
                        }
                    }
                    else {
                        Vector3 position = (combatInfo.defendingUnits.transform.position + combatInfo.attackingUnits.transform.position) / 2;
                        StartCoroutine(unitController.PerformMovement(position));
                        if (Vector3.Distance(position, transform.position) < 2f) {
                            StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                            yield return new WaitForSeconds(1.3f);
                        }
                    }
                }
                //}
            }
            yield return new WaitForSeconds(0.25f);
        }
        yield return null;
    }

    void CopyList(List<GameObject> dest, List<GameObject> src) {
        //Debug.Log("Copying list");
        for (int i = 0; i < src.Count; i++) {
            dest[i] = src[i];
        }
    }
}
