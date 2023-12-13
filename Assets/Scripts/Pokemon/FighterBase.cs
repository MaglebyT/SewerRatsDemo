using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fighter", menuName = "Fighter/Create new character")]
public class FighterBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] FighterType type1;
    [SerializeField] FighterType type2;

    [SerializeField] List<SpriteAnimator2> spriteAnimator2;

    // Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;

    public static int MaxNumOfMoves { get; set; } = 100;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public FighterType Type1
    {
        get { return type1; }
    }

    public FighterType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;

    public List<Evolution> Evolutions => evolutions;

    public int CatchRate => catchRate;

    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

[System.Serializable]
public class Evolution
{
    [SerializeField] FighterBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;

    public FighterBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
}

public enum FighterType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Earth,
    Ice,
    Poison,
    Psychic,
    Blast,
    Void,
    Nova,
    Undead
}

public enum GrowthRate
{
    Fast, MediumFast
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    // These 2 are not actual stats, they're used to boost the moveAccuracy
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart =
     {
                             /*NOR/FIR/WAT/ELEC/EAR/ICE/PSN/PSY/BLST/VOI/NOV/UND*/
      /* NORM */  new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 1f },
      /* FIRE */  new float[] { 1f, 0.5f, 0.5f, 1f, 2f, 2f, 2f, 1f, 2f, 0f, 0.5f, 2f },
      /* WATR */  new float[] { 1f, 2f, 0.5f, 0.5f, 0.5f, 1f, 1f, 1f, 2f, 0f, 0f, 1f,},
      /* ELEC */  new float[] { 1f, 1f, 2f, 0.5f, 0.5f, 2f, 1f, 0.5f, 2f, 0f, 0.5f, 1f },
      /* EART */  new float[] { 1f, 0.5f, 2f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 0f, 1f },
      /* ICE  */  new float[] { 1f, 0.5f, 2f, 2f, 2f, 0.5f, 2f, 1f, 0.5f, 0f, 0f, 1f },
      /* PSN  */  new float[] { 1f, 0.5f, 2f, 1f, 2f, 0.5f, 0.5f, 2f, 0.5f, 0f, 0f, 0f },
      /* PSY  */  new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 2f, 0f, 0.5f, 0f },
      /* BLST */  new float[] { 2f, 0.5f, 0.5f, 0.5f, 2f, 2f, 1f, 0.5f, 1f, 0f, 0f, 1f },
      /* VOID */  new float[] { 2f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 2f, 2f, 1f, 2f, 1f },
      /* NOVA */  new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 0.5f, 0.5f, 2f, 0.5f, 1f },
      /* UNDX */  new float[] { 1f, 0.5f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f }
    };

    public static float GetEffectiveness(FighterType attackType, FighterType defendType)
    {
        if (attackType == FighterType.None || defendType == FighterType.None)
            return 1f;

        int row = (int)attackType - 1;
        int col = (int)defendType - 1;

        return chart[row][col];
    }
}
