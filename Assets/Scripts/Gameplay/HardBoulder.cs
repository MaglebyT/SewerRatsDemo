using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HardBoulder : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("This boulder looks very strong...");

        var fighterWithBlastBalls = initiator.GetComponent<FighterParty>().Fighters.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Acid Bomb"));

        if (fighterWithBlastBalls != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Should {fighterWithBlastBalls.Base.Name} use an Acid Bomb?",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                yield return DialogManager.Instance.ShowDialogText($"{fighterWithBlastBalls.Base.Name} used an Acid Bomb!");
                AudioManager.i.PlaySfx(AudioId.Blasted);
                gameObject.SetActive(false);
            }
        }
    }
}
