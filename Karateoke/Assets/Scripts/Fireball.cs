using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField]
    private float speedMultiplier = 5f;

    [SerializeField]
    private GameObject explosionParticleSystemObject;

    [SerializeField]
    private ParticleSystem mainParticleSystem;

    private bool shouldMove = false;
    private bool hasCreatedExplosion = false;
    private Vector3 target;

    public void StartMoving(Vector3 targetToMoveTowards)
    {
        target = targetToMoveTowards;
        shouldMove = true;
        Debug.Log($"{this.name} should be moving.");
    }

    private void Update()
    {
        if (shouldMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speedMultiplier);
        }

        if (transform.position == target && !hasCreatedExplosion)
        {
            hasCreatedExplosion = true;
            StartCoroutine(CreateExplosionThenDestroyObjects());
        }
    }

    private IEnumerator CreateExplosionThenDestroyObjects()
    {
        GameObject explosion = Instantiate(explosionParticleSystemObject) as GameObject;
        explosion.transform.position = this.transform.position;

        mainParticleSystem.Stop();

        yield return new WaitForSeconds(0.25f);

        Debug.Log($"{this.name} and explosion should be destroyed");
        Destroy(explosion.gameObject);
        Destroy(this.gameObject);
    }
}
