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

    private PlayerQTEInput mainPlayerQTEManager;

    #region Animation Events

    public void CreateHitVFXFromLeftHand()
    {
        // should add SFX here as well
        hitVFX.transform.position = playerLeftHand.transform.position;
        hitVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(hitVFX));
    }

    public void CreateHitVFXFromRightHand()
    {
        hitVFX.transform.position = playerRightHand.transform.position;
        hitVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(hitVFX));
    }

    public void CreateShockWaveVFX()
    {
        groundShockVFX.transform.position = playerTorso.transform.position;
        groundShockVFX.Play(withChildren: true);
        StartCoroutine(WaitThenStopVFX(groundShockVFX));
    }

    #endregion

    private void Start()
    {
        mainPlayerQTEManager = mainPlayerObject.GetComponent<PlayerQTEInput>();
        groundShockVFX.Stop(withChildren: true);
        hitVFX.Stop(withChildren: true);
    }
    public void ResetModelsAfterStarPowerMove()
    {
        mainPlayerQTEManager.ResetAfterStarPowerMove();
    }

    private IEnumerator WaitThenStopVFX(ParticleSystem systemToStop)
    {
        yield return new WaitForSeconds(0.5f);
        systemToStop.Stop(withChildren: true);
    }
}
