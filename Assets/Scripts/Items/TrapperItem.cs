using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new trapper item")]
public class TrapperItem : ItemBase
{
    [SerializeField] float catchRateModfier = 1;

    public override bool Use(Fighter fighter)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier => catchRateModfier;
}
