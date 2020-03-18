using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip attackClip, sweepClip, blockClip;

    [SerializeField]
    private AudioClip successfullyBlockClip, getHitClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void AudioEvent_SuccessfullyBlock()
    {
        audioSource.PlayOneShot(successfullyBlockClip);
    }

    public void AudioEvent_Attack()
    {
        audioSource.PlayOneShot(attackClip);
    }

    public void AudioEvent_GetHit()
    {
        audioSource.PlayOneShot(getHitClip);
    }
}
