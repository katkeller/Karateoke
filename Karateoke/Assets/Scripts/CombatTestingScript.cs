using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTestingScript : MonoBehaviour
{
    public static event Action EndOfPhrase;

    void Start()
    {
        StartCoroutine(WaitThenTriggerEndOfPhrase());
    }

    void Update()
    {
        
    }

    IEnumerator WaitThenTriggerEndOfPhrase()
    {
        yield return new WaitForSeconds(10);
        EndOfPhrase?.Invoke();
        Debug.Log("Debug phrase end has been triggered from combat test script.");
        StartCoroutine(WaitThenTriggerEndOfPhrase());
    }
}
