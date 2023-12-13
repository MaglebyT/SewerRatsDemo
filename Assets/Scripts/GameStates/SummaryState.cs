using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;

    int selectedPage = 0;

    public int SelectedFighterIndex { get; set; }

    public static SummaryState i { get; set; }

    private void Awake()
    {
        i = this; 
    }

    List<Fighter> playerParty;

    private void Start()
    {
      playerParty =  PlayerController.i.GetComponent<FighterParty>().Fighters;
    }

    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedFighterIndex]);
        summaryScreen.ShowPage(selectedPage);
    }

    public override void Execute()
    { 

        if (!summaryScreen.InMoveSelection)
        {
            // Page Selection
            int prevPage = selectedPage;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedPage = Mathf.Abs((selectedPage - 1) % 2);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedPage = (selectedPage + 1) % 2;
            }

            if (selectedPage != prevPage)
            {
                summaryScreen.ShowPage(selectedPage);
            }

            // Fighter Selection
            int prevSelection = SelectedFighterIndex;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectedFighterIndex += 1;

                if (SelectedFighterIndex >= playerParty.Count)
                    SelectedFighterIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectedFighterIndex -= 1;

                if (SelectedFighterIndex <= 0)
                    SelectedFighterIndex = playerParty.Count - 1;
            }

            if (SelectedFighterIndex != prevSelection)
            {
                summaryScreen.SetBasicDetails(playerParty[SelectedFighterIndex]);
                summaryScreen.ShowPage(selectedPage);
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectedPage == 1 && !summaryScreen.InMoveSelection)
            {
                summaryScreen.InMoveSelection = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (summaryScreen.InMoveSelection)
            {
                summaryScreen.InMoveSelection = false;
            }
            else
            {
                gc.StateMachine.Pop();
                return;
            }
        }

        summaryScreen.HandleUpdate();
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
    }
}
