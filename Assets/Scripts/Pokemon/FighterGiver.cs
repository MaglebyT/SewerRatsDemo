using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterGiver : MonoBehaviour, ISavable
{
    [SerializeField] Fighter fighterToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveFighter(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        fighterToGive.Init();
        player.GetComponent<FighterParty>().AddFighter(fighterToGive);

        used = true;

        AudioManager.i.PlaySfx(AudioId.FighterObtained, pauseMusic: true);

        string dialogText = $"{fighterToGive.Base.Name} joined your party.";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return fighterToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
