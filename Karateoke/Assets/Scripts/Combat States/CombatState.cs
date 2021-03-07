using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IPlayerCombatState;

public abstract class CombatState : MonoBehaviour, IPlayerCombatState
{
    public enum CombatMove
    {
        Attack,
        Block,
        Sweep,
        Undecided
    }

    public Player Player { get; set; }
    public abstract CombatMove AssociatedCombatMove { get; }

    protected GameObject actionTextObject { get; set; }
    protected abstract int combatAnimationTrigger { get; }

    protected int getHitAnimationTrigger = Animator.StringToHash("GetHitNormal");
    protected int getHitFromBlockAnimationTrigger = Animator.StringToHash("GetHitFromBlock");
    protected int getBlockedAnimationTrigger = Animator.StringToHash("GetBlocked");
    protected int fallAnimationTrigger = Animator.StringToHash("Fall");

    public abstract void SetStatePlayerObjects(Player player);

    public void ExecuteQueuedCombatMove()
    {
        Player.PlayerAnimator.SetTrigger(combatAnimationTrigger);
        StartCoroutine(ShowActionText());
    }

    public abstract void GetAttacked(float possibleDamage, int bonus, int indexOfWinner);

    public abstract void GetBlocked();

    public void GetSwept()
    {
        Player.PlayerAnimator.SetTrigger(fallAnimationTrigger);
    }

    public void PlayVFX(ParticleSystem particles)
    {
        particles.Play();
        // Was this like this for a reason? If they don't loop we don't need to stop them,
        // and they shouldn't be looping.
        //yield return new WaitForSeconds(particles.duration + 0.25f);
        //particles.Stop();
    }

    public IEnumerator ShowActionText()
    {
        if (actionTextObject != null)
        {
            actionTextObject.SetActive(true);
            yield return new WaitForSeconds(2.15f);
            actionTextObject.SetActive(false);
        }
    }
}
