using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI player1ActionText;

    void Update()
    {
        if (Input.GetButtonDown("Player1Attack"))
        {
            player1ActionText.text = "Player 1 attacks!";
        }
    }
}
