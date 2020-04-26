using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTestingScript : MonoBehaviour
{
    private CombatManager combatManager;
    private StarPowerQTE spQTE;
    private bool starPowerIsHappening;

    public static event Action EndOfPhrase;

    void Start()
    {
        combatManager = GetComponent<CombatManager>();
        spQTE = GetComponent<StarPowerQTE>();
        combatManager.StartAnimations();
        //StartCoroutine(WaitThenTriggerEndOfPhrase());
    }

    void Update()
    {
        if (Input.GetButtonDown("TestStarPowerQTE") && !starPowerIsHappening)
        {
            starPowerIsHappening = true;

            spQTE.StartQTE(0);
        }
    }

    IEnumerator WaitThenTriggerEndOfPhrase()
    {
        yield return new WaitForSeconds(10);
        EndOfPhrase?.Invoke();
        Debug.Log("Debug phrase end has been triggered from combat test script.");
        StartCoroutine(WaitThenTriggerEndOfPhrase());
    }
}
