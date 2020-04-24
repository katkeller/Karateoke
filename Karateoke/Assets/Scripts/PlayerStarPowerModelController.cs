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
        ParticleSystem hit = Instantiate(hitVFX);
        hit.transform.position = playerLeftHand.transform.position;
        StartCoroutine(WaitThenDestroyVFXObject(hit));
    }

    public void CreateHitVFXFromRightHand()
    {
        ParticleSystem hit = Instantiate(hitVFX);
        hit.transform.position = playerRightHand.transform.position;
        StartCoroutine(WaitThenDestroyVFXObject(hit));
    }

    public void CreateShockWaveVFX()
    {
        //this probably won't work, we'll have to figure out how to put it on the ground under them
        ParticleSystem shock = Instantiate(groundShockVFX);
        groundShockVFX.transform.position = playerTorso.transform.position;
        StartCoroutine(WaitThenDestroyVFXObject(shock));
    }

    #endregion

    private void Start()
    {
        mainPlayerQTEManager = mainPlayerObject.GetComponent<PlayerQTEInput>();
    }
    public void ResetModelsAfterStarPowerMove()
    {
        mainPlayerQTEManager.ResetAfterStarPowerMove();
    }

    private IEnumerator WaitThenDestroyVFXObject(ParticleSystem systemToDestroy)
    {
        //Instead of destroying, we could have an object in the scene that we move around to the point of imapct
        //and just play/pause it? Because destroying doesn't work.
        yield return new WaitForSeconds(1f);
        systemToDestroy.Stop();
        GameObject systemObject = systemToDestroy.GetComponent<GameObject>();
        Destroy(systemObject);
    }
}
