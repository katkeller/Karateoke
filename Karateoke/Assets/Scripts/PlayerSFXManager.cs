using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip attackClip, sweepClip, blockClip;

    [SerializeField]
    private AudioClip successfullyBlockClip, getHitClip;

    [SerializeField]
    private ParticleSystem kickTracer, blockParticles;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void AudioEvent_Attack()
    {
        audioSource.PlayOneShot(attackClip);
    }

    public void AudioEvent_GetHit()
    {
        audioSource.PlayOneShot(getHitClip);
    }

    public void ActivateKickVFX()
    {
        //Add woosh SFX here as well
        kickTracer.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(0.15f, kickTracer));
    }

    public void ActivateBlockVFXAndSFX()
    {
        audioSource.PlayOneShot(successfullyBlockClip);
        blockParticles.Play();
        StartCoroutine(WaitThenStopVFX(0.5f, blockParticles));
    }

    private IEnumerator WaitThenStopVFX(float secondsToWait, ParticleSystem systemToStop)
    {
        yield return new WaitForSeconds(secondsToWait);
        systemToStop.Stop(withChildren: true);
    }
}
