using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStarPowerModelController : MonoBehaviour
{
    [SerializeField]
    private GameObject mainPlayerObject;

    private PlayerQTEInput mainPlayerQTEManager;


    private void Start()
    {
        mainPlayerQTEManager = mainPlayerObject.GetComponent<PlayerQTEInput>();
    }
    public void ResetModelsAfterStarPowerMove()
    {
        mainPlayerQTEManager.ResetAfterStarPowerMove();
    }
}
