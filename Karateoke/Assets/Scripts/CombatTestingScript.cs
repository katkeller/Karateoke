using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTestingScript : MonoBehaviour
{
    private CombatManager combatManager;

    public static event Action EndOfPhrase;

    void Start()
    {
        combatManager = GetComponent<CombatManager>();
        combatManager.StartAnimations();
        StartCoroutine(WaitThenTriggerEndOfPhrase());
    }

    void Update()
    {
        
    }

    IEnumerator WaitThenTriggerEndOfPhrase()
    {
        yield return new WaitForSeconds(8);
        EndOfPhrase?.Invoke();
        Debug.Log("Debug phrase end has been triggered from combat test script.");
        StartCoroutine(WaitThenTriggerEndOfPhrase());
    }
}
