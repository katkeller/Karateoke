using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockState : CombatState
{
    protected override int combatAnimationTrigger => Animator.StringToHash("Block");
    public override CombatMove AssociatedCombatMove => CombatMove.Block;

    public override void SetStatePlayerObjects(Player player)
    {
        Player = player;
        actionTextObject = Player.BlockTextObject;
    }

    public override void GetAttacked(float possibleDamage, int bonus, int indexOfWinner)
    {
        Player.PlayerAnimator.SetTrigger(getHitFromBlockAnimationTrigger);
    }

    public override void GetBlocked()
    {
        
    }
}
