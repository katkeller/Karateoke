using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweepState : CombatState
{
    protected override int combatAnimationTrigger => Animator.StringToHash("Sweep");
    public override CombatMove AssociatedCombatMove => CombatMove.Sweep;

    public override void SetStatePlayerObjects(Player player)
    {
        Player = player;
        actionTextObject = Player.SweepTextObject;
    }

    public override void GetAttacked(float possibleDamage, int bonus, int indexOfWinner)
    {
        Player.PlayerAnimator.SetTrigger(getHitAnimationTrigger);
        PlayVFX(Player.GetHitParticleSystem);
        int damage = (int)possibleDamage;
        if (indexOfWinner != Player.IndexAccordingToCombatManager)
        {
            damage += bonus;
        }
        Player.Health -= damage;
    }

    public override void GetBlocked()
    {

    }

    public override void GetSwept(int indexOfWinner)
    {
        if (indexOfWinner != Player.IndexAccordingToCombatManager)
        {
            Player.PlayerAnimator.SetTrigger(fallAnimationTrigger);
        }
    }
}
