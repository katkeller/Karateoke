using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private Animator p1Animator;
    [SerializeField]
    private Animator p2Animator;

    private enum CombatOptions { attack, grapple, counter };
    CombatOptions p1CombatChoice;
    CombatOptions p2CombatChoice;

    private void PerformCombat()
    {
        GetInput();
        DecideOutcome();
    }

    private void GetInput()
    {
        if (Input.GetButton("P1Attack"))
        {
            p1CombatChoice = CombatOptions.attack;
        }
        else if (Input.GetButton("P1Grapple"))
        {
            p1CombatChoice = CombatOptions.grapple;
        }
        else if (Input.GetButton("P1Counter"))
        {
            p1CombatChoice = CombatOptions.counter;
        }

        if (Input.GetButton("P2Attack"))
        {
            p2CombatChoice = CombatOptions.attack;
        }
        else if (Input.GetButton("P2Grapple"))
        {
            p2CombatChoice = CombatOptions.grapple;
        }
        else if (Input.GetButton("P2Counter"))
        {
            p2CombatChoice = CombatOptions.counter;
        }
    }

    private void DecideOutcome()
    {
        if (p1CombatChoice == CombatOptions.attack && p2CombatChoice == CombatOptions.attack)
        {
            p1Animator.SetTrigger("Attack");
            p2Animator.SetTrigger("Attack");

            p1Animator.SetBool("attacked", true);
            p2Animator.SetBool("attacked", true);

            AudioComparisonManager.playerScore[0] = AudioComparisonManager.playerScore[0] - ((1 / 10) * AudioComparisonManager.playerScore[1]);
            AudioComparisonManager.playerScore[1] = AudioComparisonManager.playerScore[1] - ((1 / 10) * AudioComparisonManager.playerScore[0]);
        }
        else if (p1CombatChoice == CombatOptions.attack && p2CombatChoice == CombatOptions.grapple)
        {
            p1Animator.SetTrigger("Attack");
            p2Animator.SetTrigger("Grapple");

            p1Animator.SetBool("grappled", true);
            p2Animator.SetBool("attacked", true);

            AudioComparisonManager.playerScore[1] = AudioComparisonManager.playerScore[1] - ((1 / 5) * AudioComparisonManager.playerScore[0]);
        }
        else if (p1CombatChoice == CombatOptions.attack && p2CombatChoice == CombatOptions.counter)
        {
            p1Animator.SetTrigger("Attack");
            p2Animator.SetTrigger("Counter");

            p1Animator.SetBool("countered", true);
            p2Animator.SetBool("attacked", true);

            AudioComparisonManager.playerScore[0] = AudioComparisonManager.playerScore[0] - ((1 / 5) * AudioComparisonManager.playerScore[1]);
        }
        else if (p1CombatChoice == CombatOptions.grapple && p2CombatChoice == CombatOptions.attack)
        {
            p1Animator.SetTrigger("Grapple");
            p2Animator.SetTrigger("Attack");

            p1Animator.SetBool("attacked", true);
            p2Animator.SetBool("grappled", true);
        }
        else if (p1CombatChoice == CombatOptions.grapple && p2CombatChoice == CombatOptions.grapple)
        {
            p1Animator.SetTrigger("Grapple");
            p2Animator.SetTrigger("Grapple");

            p1Animator.SetBool("grappled", true);
            p2Animator.SetBool("grappled", true);
        }
        else if (p1CombatChoice == CombatOptions.grapple && p2CombatChoice == CombatOptions.counter)
        {
            p1Animator.SetTrigger("Grapple");
            p2Animator.SetTrigger("Counter");

            p1Animator.SetBool("countered", true);
            p2Animator.SetBool("grappled", true);
        }
        else if (p1CombatChoice == CombatOptions.counter && p2CombatChoice == CombatOptions.attack)
        {
            p1Animator.SetTrigger("Counter");
            p2Animator.SetTrigger("Attack");

            p1Animator.SetBool("countered", true);
            p2Animator.SetBool("attacked", true);
        }
        else if (p1CombatChoice == CombatOptions.counter && p2CombatChoice == CombatOptions.grapple)
        {
            p1Animator.SetTrigger("Counter");
            p2Animator.SetTrigger("Grapple");

            p1Animator.SetBool("countered", true);
            p2Animator.SetBool("grappled", true);
        }
        else if (p1CombatChoice == CombatOptions.counter && p2CombatChoice == CombatOptions.counter)
        {
            p1Animator.SetTrigger("Counter");
            p2Animator.SetTrigger("Counter");

            p1Animator.SetBool("countered", true);
            p2Animator.SetBool("countered", true);
        }
    }

}
