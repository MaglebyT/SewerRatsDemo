using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlastingBoulder : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("This boulder looks weak...");

        var fighterWithBlastBalls = initiator.GetComponent<FighterParty>().Fighters.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Blast Ball"));

        if (fighterWithBlastBalls != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Should {fighterWithBlastBalls.Base.Name} use a Blast Ball?",
                choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                yield return DialogManager.Instance.ShowDialogText($"{fighterWithBlastBalls.Base.Name} used a Blast Ball!");
                AudioManager.i.PlaySfx(AudioId.Blasted);
                gameObject.SetActive(false);
            }
        }
    }
}
