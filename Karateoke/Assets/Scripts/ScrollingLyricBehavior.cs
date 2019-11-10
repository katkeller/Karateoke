using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrollingLyricBehavior : MonoBehaviour
{
    [SerializeField]
    private Color32 activatedColor;

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
        }
    }
}
