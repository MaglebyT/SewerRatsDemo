using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] ChoiceBox choiceBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;

    public static DialogManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true,
        List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        AudioManager.i.PlaySfx(AudioId.UISelect);
        yield return TypeDialog(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialog();
        }
        OnDialogFinished?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null, Action<int> onChoiceSelected = null, SpriteRenderer characterImage = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        if (characterImage != null)
        {
           
            // Display the character image using the provided SpriteRenderer (characterImage)
            // You need to implement the logic to show the character image in your dialog UI.
        }

        foreach (var line in dialog.Lines)
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogFinished?.Invoke();
    }

    //public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null,
    //    Action<int> onChoiceSelected = null)
    //{
    //    yield return new WaitForEndOfFrame();

    //    OnShowDialog?.Invoke();
    //    IsShowing = true;
    //    dialogBox.SetActive(true);

    //    foreach (var line in dialog.Lines)
    //    {
    //        AudioManager.i.PlaySfx(AudioId.UISelect);
    //        yield return TypeDialog(line);
    //        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
    //    }

    //    if (choices != null && choices.Count > 1)
    //    {
    //        yield return choiceBox.ShowChoices(choices, onChoiceSelected);
    //    }

    //    dialogBox.SetActive(false);
    //    IsShowing = false;
    //    OnDialogFinished?.Invoke();
    //}

    public void HandleUpdate()
    {

    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }
}
