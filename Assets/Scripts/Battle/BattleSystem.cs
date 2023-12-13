using DG.Tweening;
using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleAction { Move, SwitchFighter, UseItem, Run }

public enum BattleTrigger { LongGrass, Water } 

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image attackerImage;
    [SerializeField] GameObject trapperSprite;
    [SerializeField] MoveToForgetSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip attackerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public event Action<bool> OnBattleOver;

    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Fighter SelectedFighter { get; set; }
    public ItemBase SelectedItem { get; set; }

    public bool IsBattleOver { get; private set; }

    public FighterParty PlayerParty { get; private set; }
    public FighterParty AttackerParty { get; private set; }
    public Fighter WildFighter { get; private set; }

    public bool IsAttackerBattle { get; private set; } = false;
    PlayerController player;
    public AttackerController Attacker { get; private set; }

    public int EscapeAttempts { get; set; }

    BattleTrigger battleTrigger;

    public void StartBattle(FighterParty playerParty, Fighter wildFighter,
        BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = playerParty;
        this.WildFighter = wildFighter;
        player = playerParty.GetComponent<PlayerController>();
        IsAttackerBattle = false;

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public void StartAttackerBattle(FighterParty playerParty, FighterParty attackerParty,
        BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = playerParty;
        this.AttackerParty = attackerParty;

        IsAttackerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        Attacker = attackerParty.GetComponent<AttackerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(attackerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        playerUnit.Clear();
        enemyUnit.Clear();

        backgroundImage.sprite = (battleTrigger == BattleTrigger.LongGrass) ? grassBackground : waterBackground;

        if (!IsAttackerBattle)
        {
            // Wild Fighter Battle
            playerUnit.Setup(PlayerParty.GetHealthyFighters());
            enemyUnit.Setup(WildFighter);

            dialogBox.SetMoveNames(playerUnit.Fighter.Moves);
            yield return dialogBox.TypeDialog($"{enemyUnit.Fighter.Base.Name} detected!");
        }
        else
        {
            // Attacker Battle

            // Show attacker and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            attackerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            attackerImage.sprite = Attacker.Sprite;

            //yield return dialogBox.TypeDialog($"{Attacker.Name} ");

            // Send out first fighter of the attacker
            attackerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyFighter = AttackerParty.GetHealthyFighters();
            enemyUnit.Setup(enemyFighter);
            yield return dialogBox.TypeDialog($"{enemyFighter.Base.Name} attacks!");

            // Send out first fighter of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerFighter = PlayerParty.GetHealthyFighters();
            playerUnit.Setup(playerFighter);
            yield return dialogBox.TypeDialog($"{playerFighter.Base.Name} steps in.");
            dialogBox.SetMoveNames(playerUnit.Fighter.Moves);
        }

        IsBattleOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.i);
    }

    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Fighters.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchFighter(Fighter newFighter)
    {
        if (playerUnit.Fighter.HP > 0)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} steps out.");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newFighter);
        dialogBox.SetMoveNames(newFighter.Moves);
        yield return dialogBox.TypeDialog($"{newFighter.Base.Name} steps in.");
    }

    public IEnumerator SendNextAttackerFighter()
    {
        var nextFighter = AttackerParty.GetHealthyFighters();
        enemyUnit.Setup(nextFighter);
        yield return dialogBox.TypeDialog($"{nextFighter.Base.Name} attacks!");
    }

    public IEnumerator ThrowTrap(TrapperItem trapperItem)
    {

        if (IsAttackerBattle)
        {
            yield return dialogBox.TypeDialog($"The enemy would rather die than join you!");
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used {trapperItem.Name.ToUpper()}!");

        var trapObj = Instantiate(trapperSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var trap = trapObj.GetComponent<SpriteRenderer>();
        trap.sprite = trapperItem.Icon;

        // Animations
        yield return trap.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return trap.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchFighter(enemyUnit.Fighter, trapperItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return trap.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Fighter is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Fighter.Base.Name} submitted!");
            yield return trap.DOFade(0, 1.5f).WaitForCompletion();

            PlayerParty.AddFighter(enemyUnit.Fighter);
            yield return dialogBox.TypeDialog($"{enemyUnit.Fighter.Base.Name} joined your party!");

            Destroy(trap);
            BattleOver(true);
        }
        else
        {
            // Fighter broke out
            yield return new WaitForSeconds(1f);
            trap.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Fighter.Base.Name} is not giving in yet");
            else
                yield return dialogBox.TypeDialog($"almost gave in!");

            Destroy(trap);
        }
    }

    int TryToCatchFighter(Fighter fighter, TrapperItem trapperItem)
    {
        float a = (3 * fighter.MaxHp - 2 * fighter.HP) * fighter.Base.CatchRate * trapperItem.CatchRateModifier * ConditionsDB.GetStatusBonus(fighter.Status) / (3 * fighter.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    public BattleDialogBox DialogBox => dialogBox;

    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;

    public PartyScreen PartyScreen => partyScreen;

    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}
