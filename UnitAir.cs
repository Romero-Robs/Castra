using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitAir : UnitCombat {
    //[HideInInspector]
    //bool debug = false;

    public float healthStart = 300f;

    public float bombSpeed;

    //===================AREA OF EFFECT============================
    public float areaDamage;
    public float closeDamage;
    public float mediumDamage;
    public float farDamage;
    //=============================================================

    List<GameObject> targetHit = new List<GameObject>();
    public GameObject gunner;
    public GameObject gunnerSpawn;

    public int alternateAttackDmg;
    public int planeRateOfFire;
    public int soldierRateOfFire;

    bool turn = false;
    bool attack;

    Vector3 end;
    Vector3 start;

    protected override void Awake() {
        base.Awake();
        unitController.health = healthStart;
    }

    private void Start() {
        end = new Vector3(combatInfo.defendingUnits.transform.position.x, combatInfo.defendingUnits.transform.position.y, combatInfo.defendingUnits.transform.position.z);
        start = new Vector3(combatInfo.attackingUnits.transform.position.x, combatInfo.attackingUnits.transform.position.y, combatInfo.attackingUnits.transform.position.z);
    }

    protected override IEnumerator StartFight() {
        CheckAttack();
        //StartCoroutine(unitController.PerformMovement(start));
        yield return new WaitForSeconds(1f);
        bool _changeDirection = false;
        while (unitController.inCombat) {
            sortedTargets = targetSquads.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
            //Debug.Log("Sorted targets: " + sortedTargets.Count);
            if (sortedTargets.Count > 0) {
                //Debug.Log("Moving back and forth"); 
                //if (sortedTargets[0].GetComponent<UnitMelee>()) {
                //    StartCoroutine(unitController.PerformMovement(sortedTargets[0].transform.position));
                //    if (Vector3.Distance(sortedTargets[0].transform.position, transform.position) < 3f) {
                //        StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)
                //        //attacks the enemy unit that was found closest to current unit
                //        StartAttack(sortedTargets[0]);
                //        yield return new WaitForSeconds(2f);
                //    }
                //}
                //else {
                //StartCoroutine(unitController.PerformMovement(end));
                //do carpet bombs by making it perform movement and go to enemy side and then come back around to unit side
                StartAttack(sortedTargets[0]);
                if (Mathf.Abs(Vector3.Distance(end, this.transform.position)) < 1f) {
                    turn = true;
                    _changeDirection = true;
                }

                if (Mathf.Abs(Vector3.Distance(start, this.transform.position)) < 1f) {
                    turn = false;
                    _changeDirection = true;
                }

                if (this.transform.position != end && !turn && _changeDirection) {
                    _changeDirection = false;
                    StartCoroutine(unitController.PerformMovement(end));
                    StartAttack(sortedTargets[0]);
                    yield return new WaitForSeconds(1f);
                        
                }
                if (this.transform.position != start && turn && _changeDirection) {
                    _changeDirection = false;
                    StartCoroutine(unitController.PerformMovement(start));
                    StartAttack(sortedTargets[0]);
                    yield return new WaitForSeconds(1f);
                }
                //}
            }
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }
    protected override IEnumerator Attack(GameObject target) {
        attack = true;
        while (target != null && target.GetComponent<UnitController>().agent.enabled && !unitController.isDead && target.GetComponent<UnitController>().isDead == false) {
            //then instantiate a new explaosion at the target position taao show effect as if it was bombed
            //!target.GetComponent<UnitAir>() &&
            if (target) {
                if (target.GetComponent<UnitAir>()) {
                    ///Debug.Log("Attacking a plane");
                    //Debug.Log(Mathf.Abs(Vector3.Distance(target.transform.position, transform.position)));
                    if (Mathf.Abs(Vector3.Distance(target.transform.position, this.transform.position)) <= 25f) {
                        GameObject _gunner = Instantiate(gunner);
                        _gunner.transform.position = gunnerSpawn.transform.position;
                        _gunner.transform.rotation = transform.rotation;
                        target.GetComponent<UnitController>().ChangeHealth(alternateAttackDmg * CombatManager.Instance.planeDmgModifier[target.GetComponent<UnitController>().unitID]);
                        Destroy(_gunner, 0.5f);
                    }
                    yield return new WaitForSeconds(planeRateOfFire);
                }
                else {
                    //Debug.Log(Mathf.Abs(Vector3.Distance(target.transform.position, transform.position)));
                    if (Mathf.Abs(Vector3.Distance(target.transform.position, transform.position)) <= 8f) {
                        GameObject _attackEffect = Instantiate(attackEffect);
                        //Debug.Log("creating bomb");
                        _attackEffect.transform.position = attackEffectSpawn.transform.position;
                        //_attackEffect.transform.eulerAngles = new Vector3(0,0,0);
                        _attackEffect.transform.rotation = attackEffectSpawn.transform.rotation;
                        _attackEffect.GetComponent<Rigidbody>().AddForce(attackEffectSpawn.transform.forward * bombSpeed);

                        StartCoroutine(CalculateAttackDamage(target));

                        Destroy(_attackEffect, 1.5f);
                    }
                    yield return new WaitForSeconds(rateOfFire);
                }
            }
        }
        attack = false;
        yield return null;
    }

    IEnumerator CalculateAttackDamage(GameObject target) {
        yield return new WaitForSeconds(1f);

        targetHit.Clear();

        Collider[] hits = Physics.OverlapSphere(target.transform.position, areaDamage);
        for (int i = 0; i < sortedTargets.Count; i++) {
            for (int j = 0; j < hits.Length; j++) {
                if (sortedTargets[i].gameObject == hits[j].gameObject) {
                    targetHit.Add(hits[j].gameObject);
                }
            }
        }

        for (int i = 0; i < targetHit.Count; i++) {
            if (targetHit[i] && target) {

                float _damage = 0f;
                if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= (areaDamage * 0.50f)) {
                    _damage = closeDamage;
                }
                else if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= (areaDamage * 0.75f)) {
                    _damage = mediumDamage;
                }
                else if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= areaDamage) {
                    _damage = farDamage;
                }
                targetHit[i].GetComponent<UnitController>().ChangeHealth(_damage * CombatManager.Instance.planeDmgModifier[target.GetComponent<UnitController>().unitID]);
            }
            else {
                if (i < targetHit.Count) {
                    i++;
                }
            }
        }
    }

    void StartAttack(GameObject target) {
        if (!attack) {
            StartCoroutine(Attack(target));
        }
    }
}
