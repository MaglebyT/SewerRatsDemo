using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    // Input
    public BattleTrigger trigger { get; set; }
    public AttackerController attacker { get; set; }

    public static BattleState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        battleSystem.gameObject.SetActive(true);
        gc.WorldCamera.gameObject.SetActive(false);

        var playerParty = gc.PlayerController.GetComponent<FighterParty>();

        if (attacker == null)
        {
            var wildFighter = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildFighter(trigger);
            var wildFighterCopy = new Fighter(wildFighter.Base, wildFighter.Level);
            battleSystem.StartBattle(playerParty, wildFighterCopy, trigger);
        }
        else
        {
            var attackerParty = attacker.GetComponent<FighterParty>();
            battleSystem.StartAttackerBattle(playerParty, attackerParty);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gc.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    void EndBattle(bool won)
    {
        if (attacker != null && won == true)
        {
            attacker.BattleLost();
            attacker = null;
        }

        gc.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
