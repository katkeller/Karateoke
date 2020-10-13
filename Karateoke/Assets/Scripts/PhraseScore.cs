using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This object allows us to easily store info about each phrase win.
/// </summary>
public class PhraseScore : MonoBehaviour
{
    public int IndexOfWinner { get; set; }
    public int IndexOfLoser { get; set; }
    public float ScoreDisparity { get; set; }
}
