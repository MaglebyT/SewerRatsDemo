using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Fighter fighter)
    {
        // Revive
        if (revive || maxRevive)
        {
            if (fighter.HP > 0)
                return false;

            if (revive)
                fighter.IncreaseHP(fighter.MaxHp / 2);
            else if (maxRevive)
                fighter.IncreaseHP(fighter.MaxHp);

            fighter.CureStatus();

            return true;
        }

        // No other items can be used on fainted fighter
        if (fighter.HP == 0)
            return false;

        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (fighter.HP == fighter.MaxHp)
                return false;

            if (restoreMaxHP)
                fighter.IncreaseHP(fighter.MaxHp);
            else
                fighter.IncreaseHP(hpAmount);
        }

        // Recover Status
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (fighter.Status == null && fighter.VolatileStatus == null)
                return false;

            if (recoverAllStatus)
            {
                fighter.CureStatus();
                fighter.CureVolatileStatus();
            }
            else
            {
                if (fighter.Status.Id == status)
                    fighter.CureStatus();
                else if (fighter.VolatileStatus.Id == status)
                    fighter.CureVolatileStatus();
                else
                    return false;
            }
        }

        // Restore PP
        if (restoreMaxPP)
        {
            fighter.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
        {
            fighter.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
