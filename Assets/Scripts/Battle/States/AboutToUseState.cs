using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutToUseState : State<BattleSystem>
{
    // Input
    public Fighter NewFighter { get; set; }

    bool aboutToUseChoice;

    public static AboutToUseState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        StartCoroutine(StartState());
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{NewFighter.Base.Name} attacks! Switch fighters?");
        bs.DialogBox.EnableChoiceBox(true);
    }

    public override void Execute()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        bs.DialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            bs.DialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Yes Option
                StartCoroutine(SwitchAndContinueBattle());
            }
            else
            {
                // No Option
                StartCoroutine(ContinueBattle());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            bs.DialogBox.EnableChoiceBox(false);
            StartCoroutine(ContinueBattle());
        }
    }

    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedFighter = PartyState.i.SelectedFighter;
        if (selectedFighter != null)
        {
            yield return bs.SwitchFighter(selectedFighter);
        }

        yield return ContinueBattle();
    }

    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextAttackerFighter();
        bs.StateMachine.Pop();
    }
}
