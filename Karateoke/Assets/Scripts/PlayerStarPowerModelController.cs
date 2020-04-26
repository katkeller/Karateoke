using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStarPowerModelController : MonoBehaviour
{
    [SerializeField]
    private GameObject mainPlayerObject;

    [SerializeField]
    private GameObject playerLeftHand, playerRightHand, playerTorso;

    [SerializeField]
    private ParticleSystem hitVFX, groundShockVFX;

    [Tooltip("This is most likely the main camera.")]
    [SerializeField]
    private GameObject cinemachineBrainObject;

    [SerializeField]
    private float cameraShakeDuration = 0.2f, cameraShakeAmplitude = 2.0f, cameraShakeFrequency = 2.0f;

    private CinemachineBrain cinemachineBrain;

    private PlayerQTEInput mainPlayerQTEManager;

    #region Animation Events

    public void CreateHitVFXFromLeftHand()
    {
        // should add SFX here as well
        hitVFX.transform.position = playerLeftHand.transform.position;
        hitVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(hitVFX));
        var perlin = GetActiveVirtualCameraPerlin();
        StartCoroutine(ApplyCameraShake(perlin));
    }

    public void CreateHitVFXFromRightHand()
    {
        hitVFX.transform.position = playerRightHand.transform.position;
        hitVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(hitVFX));
        var perlin = GetActiveVirtualCameraPerlin();
        StartCoroutine(ApplyCameraShake(perlin));
    }

    public void CreateShockWaveVFX()
    {
        groundShockVFX.transform.position = playerTorso.transform.position;
        groundShockVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(groundShockVFX));
        var perlin = GetActiveVirtualCameraPerlin();
        StartCoroutine(ApplyCameraShake(perlin, extraSeconds: 0.25f));
    }

    #endregion

    private void Start()
    {
        mainPlayerQTEManager = mainPlayerObject.GetComponent<PlayerQTEInput>();
        cinemachineBrain = cinemachineBrainObject.GetComponent<CinemachineBrain>();
        groundShockVFX.Stop(withChildren: true);
        hitVFX.Stop(withChildren: true);
    }
    public void ResetModelsAfterStarPowerMove()
    {
        mainPlayerQTEManager.ResetAfterStarPowerMove();
    }

    private CinemachineBasicMultiChannelPerlin GetActiveVirtualCameraPerlin()
    {
        var activeCameraObject = cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject;
        //CinemachineVirtualCamera activeCamera = activeCameraObject.GetComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineVirtualCamera activeCamera = activeCameraObject.GetComponent<CinemachineVirtualCamera>();
        var perlin = activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            return perlin;
        }

        Debug.Log("Perlin was null.");
        return null;
    }

    private IEnumerator WaitThenStopVFX(ParticleSystem systemToStop)
    {
        yield return new WaitForSeconds(0.5f);
        systemToStop.Stop(withChildren: true);
    }

    private IEnumerator ApplyCameraShake(CinemachineBasicMultiChannelPerlin perlin, float extraSeconds = 0.0f)
    {
        perlin.m_AmplitudeGain = cameraShakeAmplitude;
        perlin.m_FrequencyGain = cameraShakeFrequency;

        var seconds = cameraShakeDuration + extraSeconds;
        yield return new WaitForSeconds(seconds);

        perlin.m_AmplitudeGain = 0;
        perlin.m_FrequencyGain = 0;
    }
}
