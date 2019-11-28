using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPortraits : MonoBehaviour
{
    [SerializeField]
    private Image player1Portrait, player2Portrait;

    [SerializeField]
    private Sprite player1NoChoicePortrait, player1ChoiceMadePortrait, player2NoChoicePortrait, player2ChoiceMadePortrait;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
