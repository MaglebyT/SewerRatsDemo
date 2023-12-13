using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<FighterEncounterRecord> wildFighter;
    [SerializeField] List<FighterEncounterRecord> wildFighterInSpace;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    [HideInInspector]
    [SerializeField] int totalChanceWater = 0;

    private void OnValidate()
    {
        CalculateChancePercentage();
    }

    private void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        totalChance = -1;
        totalChanceWater = -1;

        if (wildFighter.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildFighter)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.chancePercentage;

                totalChance = totalChance + record.chancePercentage;
            }
        }

        if (wildFighterInSpace.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildFighterInSpace)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.chancePercentage;

                totalChanceWater = totalChanceWater + record.chancePercentage;
            }
        }
    }

    public Fighter GetRandomWildFighter(BattleTrigger trigger)
    {
        var fighterList = (trigger == BattleTrigger.LongGrass) ? wildFighter : wildFighterInSpace;

        int randVal = Random.Range(1, 101);

        var fighterRecord = fighterList.FirstOrDefault(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        if (fighterRecord != null)
        {
            var levelRange = fighterRecord.levelRange;
            int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

            var wildFighter = new Fighter(fighterRecord.fighter, level);
            wildFighter.Init();
            return wildFighter;
        }
        else
        {
            // Handle the case where no matching element was found (e.g., log an error or return a default Fighter).
            Debug.LogError("No matching Fighter record found.");
            return null; // You can return a default Fighter or handle this case as needed.
        }
    }

}

[System.Serializable]
public class FighterEncounterRecord
{
    public FighterBase fighter;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}
