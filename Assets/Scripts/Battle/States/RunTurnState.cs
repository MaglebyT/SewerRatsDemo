using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogBox dialogBox;
    PartyScreen partyScreen;
    bool isAttackerBattle;
    FighterParty playerParty;
    FighterParty attackerParty;

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        playerUnit = bs.PlayerUnit;
        enemyUnit = bs.EnemyUnit;
        dialogBox = bs.DialogBox;
        partyScreen = bs.PartyScreen;
        isAttackerBattle = bs.IsAttackerBattle;
        playerParty = bs.PlayerParty;
        attackerParty = bs.AttackerParty;

        StartCoroutine(RunTurns(bs.SelectedAction));
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Fighter.CurrentMove = playerUnit.Fighter.Moves[bs.SelectedMove];
            enemyUnit.Fighter.CurrentMove = enemyUnit.Fighter.GetRandomMove();

            int playerMovePriority = playerUnit.Fighter.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Fighter.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Fighter.Speed >= enemyUnit.Fighter.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondFighter = secondUnit.Fighter;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Fighter.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (bs.IsBattleOver) yield break;

            if (secondFighter.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Fighter.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (bs.IsBattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchFighter)
            {
                yield return bs.SwitchFighter(bs.SelectedFighter);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                if (bs.SelectedItem is TrapperItem)
                {
                    yield return bs.ThrowTrap(bs.SelectedItem as TrapperItem);
                    if (bs.IsBattleOver) yield break;
                }
                else
                {
                    // This is handled from item screen, so do nothing and skip to enemy move
                }
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Fighter.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (bs.IsBattleOver) yield break;
        }

        if (!bs.IsBattleOver)
            bs.StateMachine.ChangeState(ActionSelectionState.i);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Fighter.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Fighter);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Fighter);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Fighter.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Fighter, targetUnit.Fighter))
        {
            int hitTimes = move.Base.GetHitTimes();

            float typeEffectiveness = 1f;
            int hit = 1;
            for (int i = 1; i <= hitTimes; ++i)
            {
                sourceUnit.PlayAttackAnimation();
                AudioManager.i.PlaySfx(move.Base.Sound);

                yield return new WaitForSeconds(1f);

                targetUnit.PlayHitAnimation();
                AudioManager.i.PlaySfx(AudioId.Hit);

                if (move.Base.Category == MoveCategory.Status)
                {
                    yield return RunMoveEffects(move.Base.Effects, sourceUnit.Fighter, targetUnit.Fighter, move.Base.Target);
                }
                else
                {
                    var damageDetails = targetUnit.Fighter.TakeDamage(move, sourceUnit.Fighter);
                    yield return targetUnit.Hud.WaitForHPUpdate();
                    yield return ShowDamageDetails(damageDetails);
                    typeEffectiveness = damageDetails.TypeEffectiveness;
                }

                if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Fighter.HP > 0)
                {
                    foreach (var secondary in move.Base.Secondaries)
                    {
                        var rnd = UnityEngine.Random.Range(1, 101);
                        if (rnd <= secondary.Chance)
                            yield return RunMoveEffects(secondary, sourceUnit.Fighter, targetUnit.Fighter, secondary.Target);
                    }
                }

                hit = i;
                if (targetUnit.Fighter.HP <= 0)
                    break;

            }

            yield return ShowEffectiveness(typeEffectiveness);

            if (hitTimes > 1)
                yield return dialogBox.TypeDialog($"Hit {hitTimes} times(s)!");
            

            if (targetUnit.Fighter.HP <= 0)
            {
                yield return HandleFighterFainted(targetUnit);
            }

        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Fighter.Base.Name}'s attack missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Fighter source, Fighter target, MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (bs.IsBattleOver) yield break;

        // Statuses like burn or psn will hurt the fighter after the turn
        sourceUnit.Fighter.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Fighter);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Fighter.HP <= 0)
        {
            yield return HandleFighterFainted(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Fighter source, Fighter target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Fighter fighter)
    {
        while (fighter.StatusChanges.Count > 0)
        {
            var message = fighter.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleFighterFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Fighter.Base.Name} fell.");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battlWon = true;
            if (isAttackerBattle)
                battlWon = attackerParty.GetHealthyFighters() == null;

            if (battlWon)
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);

            // Exp Gain
            int expYield = faintedUnit.Fighter.Base.ExpYield;
            int enemyLevel = faintedUnit.Fighter.Level;
            float attackerBonus = (isAttackerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * attackerBonus) / 7);
            playerUnit.Fighter.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check Level Up
            while (playerUnit.Fighter.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} is now level {playerUnit.Fighter.Level}");

                // Try to learn a new Move
                var newMove = playerUnit.Fighter.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Fighter.Moves.Count < FighterBase.MaxNumOfMoves)
                    {
                        playerUnit.Fighter.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Fighter.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {FighterBase.MaxNumOfMoves} moves");
                        yield return dialogBox.TypeDialog($"Choose a move a move to forget");

                        MoveToForgetState.i.CurrentMoves = playerUnit.Fighter.Moves.Select(x => x.Base).ToList();
                        MoveToForgetState.i.NewMove = newMove.Base;
                        yield return GameController.Instance.StateMachine.PushAndWait(MoveToForgetState.i);

                        var moveIndex = MoveToForgetState.i.Selection;
                        if (moveIndex == FighterBase.MaxNumOfMoves || moveIndex == -1)
                        {
                            // Don't learn the new move
                            yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} did not learn {newMove.Base.Name}");
                        }
                        else
                        {
                            // Forget the selected move and learn new move
                            var selectedMove = playerUnit.Fighter.Moves[moveIndex].Base;
                            yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.Name} forgot {selectedMove.Name} and learned {newMove.Base.Name}");

                            playerUnit.Fighter.Moves[moveIndex] = new Move(newMove.Base);
                        }
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }


            yield return new WaitForSeconds(1f);
        }

        yield return CheckForBattleOver(faintedUnit);
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextFighter = playerParty.GetHealthyFighters();
            if (nextFighter != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchFighter(PartyState.i.SelectedFighter);
            }

            else if (nextFighter == null) //added
            {
                yield return dialogBox.TypeDialog($"Your party has been wiped out!"); //added
                bs.BattleOver(true); // added
                yield return GameStartState.i;
            }
            else
                bs.BattleOver(false);
        }
        else
        {
            if (!isAttackerBattle)
            {
                bs.BattleOver(true);
            }
            else
            {
                var nextFighter = attackerParty.GetHealthyFighters();
                if (nextFighter != null)
                {
                    AboutToUseState.i.NewFighter = nextFighter;
                    yield return bs.StateMachine.PushAndWait(AboutToUseState.i);
                }
                else
                    bs.BattleOver(true);
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

       
    }

    IEnumerator ShowEffectiveness(float typeEffectiveness)
    {

        if (typeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (typeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
    }

    IEnumerator TryToEscape()
    {

        if (isAttackerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from this battle!");
            yield break;
        }

        ++bs.EscapeAttempts;

        int playerSpeed = playerUnit.Fighter.Speed;
        int enemySpeed = enemyUnit.Fighter.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Escaped!");
            bs.BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * bs.EscapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Escaped!");
                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
            }
        }
    }
}
