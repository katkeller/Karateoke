using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    //[Tooltip("If this is not checked, the firball will move right.")]
    //[SerializeField]
    //bool shouldMoveLeft = false;

    [SerializeField]
    private Vector3 target;

    [SerializeField]
    private float speedMultiplier;

    //private float timeElapsed;


    private void Update()
    {
        //timeElapsed += Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speedMultiplier);
        
        if (transform.position == target)
        {
            Destroy(this.gameObject);
            Debug.Log($"{this.name} should be destroyed");
        }
    }
}
