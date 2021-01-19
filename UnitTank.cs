using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitTank : UnitCombat
{
    public float healthStart = 400f;

    //===================AREA OF EFFECT============================
    public float areaDamage;
    public float closeDamage;
    public float mediumDamage;
    public float farDamage;
    //=============================================================

    List<GameObject> targetHit = new List<GameObject>();
    bool attack;

    protected override void Awake()
    {
        base.Awake();
        unitController.health = healthStart;
    }

    protected override IEnumerator StartFight()
    {
        CheckAttack();
        yield return new WaitForSeconds(3f);

        while (unitController.inCombat)
        {
            sortedTargets = targetSquads.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();

            if (sortedTargets.Count > 0)
            {
                StartCoroutine(unitController.PerformMovement(sortedTargets[0].transform.position));
                if (Vector3.Distance(sortedTargets[0].transform.position, transform.position) < 25f)
                {
                    StartCoroutine(unitController.PerformMovement(transform.position)); //stop moving(basically move towards the position you're currently in)

                    Vector3 direction = (sortedTargets[0].transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));    // flattens the vector3
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * .1f);

                    //transform.LookAt(sortedTargets[0].transform);

                    //attacks the enemy unit that was found closest to current unit
                    if (attack == false) {
                        StartCoroutine(Attack(sortedTargets[0]));
                    }
                    yield return new WaitForSeconds(2f);
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }

    protected override IEnumerator Attack(GameObject target) {
        attack = true;
        while (target != null && target.GetComponent<UnitController>().agent.enabled && !unitController.isDead && target.GetComponent<UnitController>().isDead == false) {
            GameObject _muzzleEffect = Instantiate(muzzleFlashEffect, attackEffectSpawn.transform);
            Destroy(_muzzleEffect, 0.5f);
            yield return new WaitForSeconds(0.25f);
            SFX_Manager.Instance.ChooseRandomizedEffect("tankAttack", gameObject);
            gameObject.transform.GetChild(0).GetComponent<Animator>().SetTrigger("TankShot");

            GameObject _attackEffect = Instantiate(attackEffect);
            _attackEffect.transform.position = attackEffectSpawn.transform.position;
            _attackEffect.transform.rotation = transform.rotation;

            _attackEffect.transform.LookAt(target.transform.GetChild(0));
            _attackEffect.GetComponent<TankProjectile>().target = target.transform.GetChild(0);
            //_attackEffect.GetComponent<Rigidbody>().AddRelativeForce(transform.forward * 1000f);
            Destroy(_attackEffect, 5f);
            if (target) {
                //SoundEffectManager.Instance.SpawnRandomizedSoundEffect(5, .7f, gameObject.transform);
                //attackEffect.GetComponent<ParticleSystem>().Play();
                //target.GetComponent<UnitController>().ChangeHealth(attackDmg);

                //when getting the target to hit see what other colliders got hit with the area damage
                //for all sorted targets that are within the collider array damage based on distance from
                //the main target
                targetHit.Clear();

                Collider[] hits = Physics.OverlapSphere(target.transform.position, areaDamage);
                for(int i = 0; i < sortedTargets.Count; i++) {
                    for (int j = 0; j < hits.Length; j++) {
                        if (sortedTargets[i].gameObject == hits[j].gameObject) {
                            targetHit.Add(hits[j].gameObject);
                        }
                    }
                }

                for (int i = 0; i < targetHit.Count; i++) {
                    if (debug) {
                        //Debug.Log("Attacking in the for loop");
                        Debug.Log("targets: " + targetHit.Count);
                    }
                    if (targetHit[i] && target) {
                        if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= (areaDamage * 0.50f)) {
                            if (debug) {
                                Debug.Log(Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)));
                                Debug.Log(targetHit[i].name + "hit with " + closeDamage);
                            }
                            //Apply Damage
                            targetHit[i].GetComponent<UnitController>().ChangeHealth(closeDamage * CombatManager.Instance.tankDmgModifier[target.GetComponent<UnitController>().unitID]);
                            //Destroy(_attackEffect);
                        }
                        else if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= (areaDamage * 0.75f)) {
                            if (debug) {
                                Debug.Log(Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)));
                                Debug.Log(targetHit[i].name + "hit with " + mediumDamage);
                            }
                            //Apply Damage
                            targetHit[i].GetComponent<UnitController>().ChangeHealth(mediumDamage * CombatManager.Instance.tankDmgModifier[target.GetComponent<UnitController>().unitID]);
                            //Destroy(_attackEffect);
                        }
                        else if (Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)) <= areaDamage) {
                            if (debug) {
                                Debug.Log(Mathf.Abs(Vector3.Distance(targetHit[i].transform.position, target.transform.position)));
                                Debug.Log(targetHit[i].name + "hit with " + farDamage);
                            }
                            //Apply Damage
                            targetHit[i].GetComponent<UnitController>().ChangeHealth(farDamage * CombatManager.Instance.tankDmgModifier[target.GetComponent<UnitController>().unitID]);
                            //Destroy(_attackEffect);
                        }
                        if (debug) {
                            Debug.Log("Waiting " + rateOfFire + " second to fire");
                        }
                    }
                    else {
                        if (i < targetHit.Count) {
                            i++;
                        }
                    }
                }
                yield return new WaitForSeconds(rateOfFire);
            }
        }
        attack = false;
        yield return null;
    }
}
