using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitRanged : UnitCombat
{
    //=================================Variables================================
    public float healthStart = 110f;
    public float healthGameStart = 110f;
    //==========================================================================

    protected override void Awake() {
        base.Awake();
        attackDmg = 15f;
        unitController.health = healthGameStart;
        rateOfFire = 0.75f;
    }

    protected override IEnumerator StartFight() {
        CheckAttack();
        yield return new WaitForSeconds(3f);

        while (unitController.inCombat) {
            sortedTargets = targetSquads.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
            
            if(sortedTargets.Count > 0) {
                StartCoroutine(unitController.PerformMovement(sortedTargets[0].transform.position));
                if (Vector3.Distance(sortedTargets[0].transform.position, transform.position) < 10f) {
                    StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                    transform.LookAt(sortedTargets[0].transform);
                    //attacks the enemy unit that was found closest to current unit
                    StartCoroutine(Attack(sortedTargets[0]));
                    yield return new WaitForSeconds(2f);
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }
}
