using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is a simple trigger that sits on the lyric timing bar and triggers an event to signal the end of a phrase.
/// </summary>
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
