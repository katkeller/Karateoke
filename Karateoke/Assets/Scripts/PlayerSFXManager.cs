using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This script controls sound effects and visual effects for the players' various combat moves.
/// </summary>
public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera mainCamera;

    [SerializeField]
    private PlayerPhysicalSFXManager physicalSFXManager;

    [SerializeField]
    private float cameraShakeDuration = 0.2f, cameraShakeAmplitude = 1.0f, cameraShakeFrequency = 2.0f;

    [SerializeField]
    private List<AudioClip> attackClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> getHitClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> sweepAttackClips = new List<AudioClip>();

    [SerializeField]
    private AudioClip successfullyBlockClip;

    [SerializeField]
    private ParticleSystem kickTracer, blockParticles, sweepTracer, fallParticles;

    private AudioSource audioSource;
    private CinemachineBasicMultiChannelPerlin mainCameraNoise;

    private int attackClipCurrentIndex = 0;
    private int getHitClipCurrentIndex = 0;
    private int sweepAttckClipCountIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainCameraNoise = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void AudioEvent_Attack()
    {
        audioSource.PlayOneShot(attackClips[attackClipCurrentIndex]);
        attackClipCurrentIndex = SetNextAudioClipIndex(attackClipCurrentIndex, attackClips.Count);
    }

    public void AudioEvent_GetHit()
    {
        audioSource.PlayOneShot(getHitClips[getHitClipCurrentIndex]);
        StartCoroutine(ApplyCameraShake());
        getHitClipCurrentIndex = SetNextAudioClipIndex(getHitClipCurrentIndex, getHitClips.Count);
    }

    public void AudioEvent_KickWoosh()
    {
        physicalSFXManager.PlayKickWooshSFX();
    }

    public void AudioEvent_HitLands()
    {
        physicalSFXManager.PlayHitLandsSFX();
    }

    public void AudioEvent_SuccesfulBlock()
    {
        physicalSFXManager.PlaySuccessfulBlockSFX();
    }

    public void AudioEvent_Sweep()
    {
        physicalSFXManager.PlaySweepSFX();

        audioSource.PlayOneShot(sweepAttackClips[sweepAttckClipCountIndex]);
        sweepAttckClipCountIndex = SetNextAudioClipIndex(sweepAttckClipCountIndex, sweepAttackClips.Count);
    }

    public void AudioEvent_Fall()
    {
        physicalSFXManager.PlayFallSFX();

        audioSource.PlayOneShot(getHitClips[getHitClipCurrentIndex]);
        getHitClipCurrentIndex = SetNextAudioClipIndex(getHitClipCurrentIndex, getHitClips.Count);
        StartCoroutine(ApplyCameraShake());
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
    }

    public void ActivateBlockVFXAndSFX()
    {
        //audioSource.PlayOneShot(successfullyBlockClip);
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

    private int SetNextAudioClipIndex(int currentIndex, int lengthOfClipList)
    {
        currentIndex++;
        if (currentIndex >= lengthOfClipList)
        {
            currentIndex = 0;
        }
        return currentIndex;
    }
}
