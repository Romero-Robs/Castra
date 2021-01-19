using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitCombat : MonoBehaviour
{
    public List<GameObject> targetSquads = new List<GameObject>();
    public List<GameObject> unitSquads = new List<GameObject>();
    public List<GameObject> sortedTargets = new List<GameObject>();
    public GameObject attackEffect;
    public GameObject muzzleFlashEffect;
    public GameObject attackEffectSpawn;

    [HideInInspector]
    public bool debug = false;

    public float attackDmg;
    public bool isAttack;

    public float rateOfFire;

    public List<float> attackDmgModifier = new List<float>();

    public UnitController unitController;
    public CombatInfo combatInfo;

    protected virtual void Awake() {
        unitController = gameObject.GetComponent<UnitController>();
    }

    private void Start() {
        if (isAttack) {
            transform.LookAt(combatInfo.defendingUnits.transform);
        }
        else {
            transform.LookAt(combatInfo.attackingUnits.transform);
        }
    }

    //does the actual motions of attack and delay, has nothing to do with tactic
    //changes the health of the enemy based on the attack damage of the unit
    protected virtual IEnumerator Attack(GameObject target) {
        while (target != null && target.GetComponent<UnitController>().isDead == false && target.GetComponent<UnitController>().agent.enabled && !unitController.isDead) {
            if (unitController.unitID == 0) { //only animate on melee unit
                gameObject.GetComponent<UnitController>().unitModel.GetComponent<UnitModel>().animator.GetComponent<Animator>().SetTrigger("Attack");
                yield return new WaitForSeconds(0.3f);//wait to damage until attack animation happens
            }
            SFX_Manager.Instance.ChooseRandomizedEffect(CombatManager.Instance.unitTypes[unitController.unitID] + "Attack", gameObject);
            //SoundEffectManager.Instance.SpawnRandomizedSoundEffect(5, .7f, gameObject.transform);
            //attackEffect.GetComponent<ParticleSystem>().Play();
            if (target != null && target.GetComponent<UnitController>().isDead == false) {
                target.GetComponent<UnitController>().ChangeHealth(attackDmg * CombatManager.Instance.meleeDmgModifier[target.GetComponent<UnitController>().unitID] * attackDmgModifier[target.GetComponent<UnitController>().unitID]);
            }
            //Debug.Log("Unit " + gameObject.GetComponent<UnitController>().unitID + " Attacks unit " + target.GetComponent<UnitController>().unitID + "Deals dmg " + attackDmg * attackDmgModifier[target.GetComponent<UnitController>().unitID]);
            //if (target != null) {
            //    target.GetComponent<UnitController>().ChangeHealth(attackDmg);
            //}
            yield return new WaitForSeconds(rateOfFire);//rate of fire for each unit
            if (debug) {
                Debug.Log("Waiting " + rateOfFire + " second to fire");
            }
        }
        yield return null;
    }

    //the code for the actual calculation of combat, takes the distance of each enemy and makes the unit move towards
    //the closest enemy found
    protected virtual IEnumerator StartFight() {
        //targetSquads.Clear();
        //checks to see who is attacking and assigns both the targetSquad and the unitSquad from the attacking and
        //defending list
        CheckAttack();

        //checks to see if unit is still in combat, keep attacking while still in combat
        while (unitController.inCombat)
        {
            //orders the targets ina a list by who is closest to the current unit
            sortedTargets = targetSquads.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
            if (sortedTargets.Count > 0)
            {
                //move character towards the enemy based on who is first in the target list
                StartCoroutine(unitController.PerformMovement(sortedTargets[0].transform.position));
                //the actual combat happens here where the enemy is less than 2f from the current unit

                //this is where the tactic aspect of the units will happen but the conditions should be in a different area
                if (Vector3.Distance(sortedTargets[0].transform.position, transform.position) < 2.5f)
                {
                    unitController.PerformMovement(transform.position); //stop moving(basically move towards the position you're currently in)
                    transform.LookAt(sortedTargets[0].transform);
                    //attacks the enemy unit that was found closest to current unit
                    StartCoroutine(Attack(sortedTargets[0]));
                    yield return new WaitForSeconds(1.3f);
                }
                //yield return new WaitForSeconds(2f);
            }
            yield return new WaitForSeconds(0.25f);
        }
        yield return null;
    }

    public void ActivateCombat() {
        //unitController.agent.enabled = false;
        if (isAttack)
            transform.position = combatInfo.attackingUnits.transform.position;
        else
            transform.position = combatInfo.defendingUnits.transform.position;

        //NavMesh.SamplePosition
        //unitController.agent.enabled = true;
        unitController.inCombat = true;
        //unitController.agent.speed = 5f;
        StartCoroutine(StartFight());
    }

    public void DeactivateCombat() {
        isAttack = false;
        unitController.inCombat = false;
        unitController.agent.speed = 3.5f;
    }

    public void CombatDeath() {
        if (isAttack) {
            combatInfo.attackingUnitsList.Remove(gameObject);
        }
        else {
            combatInfo.defendingUnitsList.Remove(gameObject);
        }

        //TODO: Update combat info with current unit stats
        combatInfo.UnitDied(this);
    }

    public void CheckAttack()
    {
        if (isAttack)
        {
            targetSquads = combatInfo.defendingUnitsList;
            unitSquads = combatInfo.attackingUnitsList;

            //Debug.Log("attack squad: " + unitSquads.Count);
            //Debug.Log("attack target squad: " + targetSquads.Count);
        }
        else
        {
            targetSquads = combatInfo.attackingUnitsList;
            unitSquads = combatInfo.defendingUnitsList;
            //Debug.Log("defending squad: " + unitSquads.Count);
            //Debug.Log("defend target squad: " + targetSquads.Count);
        }
    }
}
