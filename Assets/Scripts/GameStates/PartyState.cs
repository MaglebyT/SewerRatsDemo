using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public Fighter SelectedFighter { get; private set; }

    public static PartyState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    GameController gc;
    
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedFighter = null;
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnFighterSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        
        partyScreen.gameObject.SetActive(false);
        
        partyScreen.OnSelected -= OnFighterSelected;
       
        partyScreen.OnBack -= OnBack;
      
       

    }

    void OnFighterSelected(int selection)
    {
        SelectedFighter = partyScreen.SelectedMember;

        StartCoroutine(FighterSelectedAction(selection));
        

    }

    IEnumerator FighterSelectedAction(int selectedFighterIndex)
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.i.MenuItems = new List<string>() { "Shift", "Summary", "Cancel" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                if (SelectedFighter.HP <= 0)
                {
                    partyScreen.SetMessageText("You can't send out a KO fighter");
                    yield break;
                }
                if (SelectedFighter == battleState.BattleSystem.PlayerUnit.Fighter)
                {
                    partyScreen.SetMessageText("You can't switch with the same fighter!");
                    yield break;
                }


                gc.StateMachine.Pop();
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                SummaryState.i.SelectedFighterIndex = selectedFighterIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else
            {
                yield break;
            }

            
        }
        else
        {
            DynamicMenuState.i.MenuItems = new List<string>() { "Summary", "Switch Fighters", "Cancel" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                SummaryState.i.SelectedFighterIndex = selectedFighterIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i); 
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //Switch Fighter
            }
            else
            {
                yield break; 
            }


        }

    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedFighter = null;

        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnit.Fighter.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a fighter to continue");
                return;
            }
        }

        gc.StateMachine.Pop();
    }
}
