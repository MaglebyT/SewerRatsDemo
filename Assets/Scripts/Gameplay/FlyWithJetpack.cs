using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlyWithJetpack : MonoBehaviour, Interactable, IPlayerTriggerable
{
    bool isJumpingToFly = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsFlying || isJumpingToFly)
            yield break;

       // yield return DialogManager.Instance.ShowDialogText("");

        var playerHasJetpack = initiator.GetComponent<FighterParty>().Fighters.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Jetpack"));

        if (playerHasJetpack != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Should {playerHasJetpack.Base.Name} turn on the jetpack?",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                // yield return DialogManager.Instance.ShowDialogText($"{playerHasJetpack.Base.Name} used !");
                AudioManager.i.PlaySfx(AudioId.Jetpack);

                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                isJumpingToFly = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpingToFly = false;

                animator.IsFlying = true;
            }
        }
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
           // GameController.Instance.StartBattle(BattleTrigger.Water); //Space?
        }
    }
}
