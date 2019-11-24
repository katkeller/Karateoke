using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhraseEndTrigger : MonoBehaviour
{
    public static event Action EndOfPhrase;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PhraseEnd"))
        {
            EndOfPhrase?.Invoke();
            Debug.Log("A phrase end lyric has entered the phrase end trigger!");
        }
    }
}
