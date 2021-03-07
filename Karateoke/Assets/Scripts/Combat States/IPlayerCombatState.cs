
using System.Collections;
using UnityEngine;

public interface IPlayerCombatState
{
    void ExecuteQueuedCombatMove();
    void GetAttacked(float possibleDamage, int bonus, int indexOfWinner);
    void GetBlocked();
    void GetSwept();
    void PlayVFX(ParticleSystem particles);
    IEnumerator ShowActionText();
}
