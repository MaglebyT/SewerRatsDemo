using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Fighter fighter) =>
                {
                    fighter.DecreaseHP(fighter.MaxHp / 8);
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is sick with poisoning!");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Fighter fighter) =>
                {
                    fighter.DecreaseHP(fighter.MaxHp / 16);
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is in flames!");
                }
            }
        },
        {
            ConditionID.cut,
            new Condition()
            {
                Name = "Cut",
                StartMessage = "has been cut",
                OnAfterTurn = (Fighter fighter) =>
                {
                    fighter.DecreaseHP(fighter.MaxHp / 6);
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is bleeding!");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Fighter fighter) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is paralyzed and can't move!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Fighter fighter) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        fighter.CureStatus();
                        fighter.StatusChanges.Enqueue($"{fighter.Base.Name} thawed out!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Fighter fighter) =>
                {
                    // Sleep for 1-3 turns
                    fighter.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {fighter.StatusTime} moves");
                },
                OnBeforeMove = (Fighter fighter) =>
                {
                    if (fighter.StatusTime <= 0)
                    {
                        fighter.CureStatus();
                        fighter.StatusChanges.Enqueue($"{fighter.Base.Name} woke up!");
                        return true;
                    }

                    fighter.StatusTime--;
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} can't stay awake!");
                    return false;
                }
            }
        },

        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Fighter fighter) =>
                {
                    // Confused for 1 - 4 turns
                    fighter.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {fighter.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Fighter fighter) =>
                {
                    if (fighter.VolatileStatusTime <= 0)
                    {
                        fighter.CureVolatileStatus();
                        fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is no longer confused!");
                        return true;
                    }
                    fighter.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Hurt by confusion
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} is confused");
                    fighter.DecreaseHP(fighter.MaxHp / 8);
                    fighter.StatusChanges.Enqueue($"{fighter.Base.Name} punched themself in the face!");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn || condition.Id == ConditionID.cut)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, cut, slp, par, frz,
    confusion
}
