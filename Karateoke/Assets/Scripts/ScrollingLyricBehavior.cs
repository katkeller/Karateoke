using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrollingLyricBehavior : MonoBehaviour
{
    [SerializeField]
    private Color32 activatedColor;

    [SerializeField]
    private GameObject trail;

    [SerializeField]
    private float secondsBeforeDestroy = 1.0f;

    private TextMeshPro text;

    private void Start()
    {
        text = GetComponent<TextMeshPro>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LyricLine"))
        {
            text.color = activatedColor;
            StartCoroutine(WaitThenDestroy());
        }

        if (collision.CompareTag("TrailTrigger"))
        {
            trail.SetActive(true);

            //GameObject particles = Instantiate(trail);
            //particles.transform.SetParent(this.transform);
            //particles.transform.position = new Vector3(0, 0);
        }
    }

    IEnumerator WaitThenDestroy()
    {
        yield return new WaitForSeconds(secondsBeforeDestroy);
        Destroy(this.gameObject);
        Debug.Log($"{this.name} should be destroyed");
    }
}
