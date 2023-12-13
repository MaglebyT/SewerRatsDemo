using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    // Output
    public bool ItemUsed { get; private set; }

    public static UseItemState i { get; private set; }
    Inventory inventory;
    private void Awake()
    {
        i = this;
        inventory = Inventory.GetInventory();
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        ItemUsed = false;

        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var fighter = partyScreen.SelectedMember;

        if (item is TmItem)
        {
            yield return HandleTmItems();
        }
        else
        {
            // Handle Evolution Items
            if (item is EvolutionItem)
            {
                var evolution = fighter.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(fighter, evolution);
                }
                else
                {
                   
                   // yield return DialogManager.Instance.ShowDialogText($"");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item, partyScreen.SelectedMember);
            if (usedItem != null)
            {
                ItemUsed = true;

                if (usedItem is RecoveryItem)
                    yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} used {usedItem.Name}.");
            }
            else
            {
                // add a na-naaa buzzer sound
               if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                    yield return DialogManager.Instance.ShowDialogText($"It won't have any affect!");
            }
        }

        gc.StateMachine.Pop();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var fighter = partyScreen.SelectedMember;

        if (fighter.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} already knows {tmItem.Move.Name}");
            yield break;
        }

        if (!tmItem.CanBeTaught(fighter))
        {
            yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} can't learn {tmItem.Move.Name}");
            yield break;
        }

        if (fighter.Moves.Count < FighterBase.MaxNumOfMoves)
        {
            fighter.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} learned {tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} is trying to learn {tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"But cannot learn more than {FighterBase.MaxNumOfMoves} moves");

            yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget", true, false);

            MoveToForgetState.i.CurrentMoves = fighter.Moves.Select(x => x.Base).ToList();
            MoveToForgetState.i.NewMove = tmItem.Move;
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == FighterBase.MaxNumOfMoves || moveIndex == -1)
            {
                // Don't learn the new move
                yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} did not learn {tmItem.Move.Name}");
            }
            else
            {
                // Forget the selected move and learn new move
                var selectedMove = fighter.Moves[moveIndex].Base;
                yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} forgot {selectedMove.Name} and learned {tmItem.Move.Name}");

                fighter.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }
    }
}
