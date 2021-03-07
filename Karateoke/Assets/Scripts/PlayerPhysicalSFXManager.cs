using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicalSFXManager : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> kickWooshClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> hitLandsClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> blockClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> sweepClips = new List<AudioClip>();

    [SerializeField]
    private List<AudioClip> fallClips = new List<AudioClip>();

    private AudioSource audioSource;

    private int kickWooshClipCurrentIndex = 0;
    private int hitLandsClipCurrentIndex = 0;
    private int blockClipCurrentIndex = 0;
    private int sweepClipCurrentIndex = 0;
    private int fallClipCurrentIndex = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayKickWooshSFX()
    {
        audioSource.PlayOneShot(kickWooshClips[kickWooshClipCurrentIndex]);
        kickWooshClipCurrentIndex = SetNextAudioClipIndex(kickWooshClipCurrentIndex, kickWooshClips.Count);
    }

    public void PlayHitLandsSFX()
    {
        audioSource.PlayOneShot(hitLandsClips[hitLandsClipCurrentIndex]);
        hitLandsClipCurrentIndex = SetNextAudioClipIndex(hitLandsClipCurrentIndex, hitLandsClips.Count);
    }

    public void PlaySuccessfulBlockSFX()
    {
        audioSource.PlayOneShot(blockClips[blockClipCurrentIndex]);
        blockClipCurrentIndex = SetNextAudioClipIndex(blockClipCurrentIndex, blockClips.Count);
    }

    public void PlaySweepSFX()
    {
        audioSource.PlayOneShot(sweepClips[sweepClipCurrentIndex]);
        sweepClipCurrentIndex = SetNextAudioClipIndex(sweepClipCurrentIndex, sweepClips.Count);
    }

    public void PlayFallSFX()
    {
        audioSource.PlayOneShot(fallClips[fallClipCurrentIndex]);
        fallClipCurrentIndex = SetNextAudioClipIndex(fallClipCurrentIndex, fallClips.Count);
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
