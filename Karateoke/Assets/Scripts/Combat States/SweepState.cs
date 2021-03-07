using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweepState : CombatState
{
    protected override int combatAnimationTrigger => throw new System.NotImplementedException();
    public override CombatMove AssociatedCombatMove => CombatMove.Sweep;

    public SweepState(Player player)
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
}
