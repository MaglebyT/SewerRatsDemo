using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Fighter fighter)
    {
        // Learning move is handled from Inventory UI, If it was learned then return true
        return fighter.HasMove(move);
    }

    public bool CanBeTaught(Fighter fighter)
    {
        return fighter.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
