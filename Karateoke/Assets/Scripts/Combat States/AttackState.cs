using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : CombatState
{
    protected override int combatAnimationTrigger => Animator.StringToHash("Attack");
    public override CombatMove AssociatedCombatMove => CombatMove.Attack;

    public override void SetStatePlayerObjects(Player player)
    {
        Player = player;
        actionTextObject = Player.AttackTextObject;
    }

    public override void GetAttacked(float possibleDamage, int bonus, int indexOfWinner)
    {
        if (indexOfWinner != Player.IndexAccordingToCombatManager)
        {
            // Apply damage to this player
            Player.PlayerAnimator.SetTrigger(getHitAnimationTrigger);
            PlayVFX(Player.GetHitParticleSystem);
            Player.Health -= (int)(possibleDamage + bonus);
        }
    }

    public override void GetBlocked()
    {
        Player.PlayerAnimator.SetTrigger(getBlockedAnimationTrigger);
    }

    public override void GetSwept(int indexOfWinner)
    {
        
    }
}
