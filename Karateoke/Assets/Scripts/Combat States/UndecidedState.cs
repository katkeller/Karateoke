using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndecidedState : CombatState
{
    protected override int combatAnimationTrigger => Animator.StringToHash("SittingDuck");
    public override CombatMove AssociatedCombatMove => CombatMove.Undecided;

    public override void SetStatePlayerObjects(Player player)
    {
        Player = player;
        actionTextObject = null;
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
        Player.PlayerAnimator.SetTrigger(fallAnimationTrigger);
    }
}
