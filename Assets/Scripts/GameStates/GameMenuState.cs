using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuState : State<GameController>
{
    [SerializeField] MenuController menuController;

    public static GameMenuState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        menuController.gameObject.SetActive(true);
        menuController.OnSelected += OnMenuItemSelected;
        menuController.OnBack += OnBack;
    }

    public override void Execute()
    {
        menuController.HandleUpdate();
    }

    public override void Exit()
    {
        menuController.gameObject.SetActive(false);
        menuController.OnSelected -= OnMenuItemSelected;
        menuController.OnBack -= OnBack;
    }
    void OnMenuItemSelected(int selection)
    {
        if (selection == 0) // Party
            gc.StateMachine.Push(PartyState.i);
        else if (selection == 1) // Inv
            gc.StateMachine.Push(InventoryState.i);
        else if (selection == 2) // Save
            SavingSystem.i.Save("saveSlot1");
        else if (selection == 3) // Load
            SavingSystem.i.Load("saveSlot1");
    }
 

    void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
