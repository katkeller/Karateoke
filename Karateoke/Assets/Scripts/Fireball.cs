using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    //[Tooltip("If this is not checked, the firball will move right.")]
    //[SerializeField]
    //bool shouldMoveLeft = false;
    [SerializeField]
    private float speedMultiplier = 5f;

    [SerializeField]
    private ParticleSystem explosionParticleSystem;

    [SerializeField]
    private ParticleSystem mainParticleSystem;

    private bool shouldMove = false;
    private Vector3 target;

    public void StartMoving(Vector3 targetToMoveTowards)
    {
        target = targetToMoveTowards;
        shouldMove = true;
        Debug.Log($"{this.name} should be moving.");
    }

    private void Update()
    {
        //timeElapsed += Time.deltaTime;
        if (shouldMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speedMultiplier);
        }

        if (transform.position == target)
        {
            //StartCoroutine(CreateExplosionThenDestroyObjects());
            Destroy(this.gameObject);
        }
    }

    private IEnumerator CreateExplosionThenDestroyObjects()
    {
        ParticleSystem explosion = Instantiate(explosionParticleSystem);
        explosion.transform.position = this.transform.position;
        explosion.Play();

        mainParticleSystem.Stop();

        yield return new WaitForSeconds(0.25f);

        Debug.Log($"{this.name} and explosion should be destroyed");
        Destroy(explosion.gameObject);
        Destroy(this.gameObject);
    }
}
