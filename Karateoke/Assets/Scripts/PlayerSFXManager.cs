using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera mainCamera;

    [SerializeField]
    private float cameraShakeDuration = 0.2f, cameraShakeAmplitude = 1.0f, cameraShakeFrequency = 2.0f;

    [SerializeField]
    private AudioClip attackClip, sweepClip, fallClip;

    [SerializeField]
    private AudioClip successfullyBlockClip;

    [SerializeField]
    private AudioClip[] getHitClips = new AudioClip[6];

    [SerializeField]
    private ParticleSystem kickTracer, blockParticles, sweepTracer, fallParticles;

    private AudioSource audioSource;
    private CinemachineBasicMultiChannelPerlin mainCameraNoise;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainCameraNoise = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void AudioEvent_Attack()
    {
        audioSource.PlayOneShot(attackClip);
    }

    public void AudioEvent_GetHit()
    {
        var index = UnityEngine.Random.Range(0, 5);
        var getHitClip = getHitClips[index];
        audioSource.PlayOneShot(getHitClip);
        StartCoroutine(ApplyCameraShake());
    }

    public void AudioEvent_Sweep()
    {
        audioSource.PlayOneShot(sweepClip);
    }

    public void ActivateSweepVFX()
    {
        sweepTracer.Play();
        StartCoroutine(WaitThenStopVFX(1f, sweepTracer));
    }

    public void ActivateKickVFX()
    {
        kickTracer.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(0.15f, kickTracer));
    }

    public void ActivateFallVFX()
    {
        fallParticles.Play();
        audioSource.PlayOneShot(fallClip);
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

    private IEnumerator ApplyCameraShake()
    {
        mainCameraNoise.m_AmplitudeGain = cameraShakeAmplitude;
        mainCameraNoise.m_FrequencyGain = cameraShakeFrequency;

        yield return new WaitForSeconds(cameraShakeDuration);

        mainCameraNoise.m_AmplitudeGain = 0;
        mainCameraNoise.m_FrequencyGain = 0;

    }
}
