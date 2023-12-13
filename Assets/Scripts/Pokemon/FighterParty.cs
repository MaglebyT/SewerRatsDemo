using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FighterParty : MonoBehaviour
{
    [SerializeField] List<Fighter> fighters;

    public event Action OnUpdated;

    public List<Fighter> Fighters
    {
        get
        {
            return fighters;
        }
        set
        {
            fighters = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var fighter in fighters)
        {
            fighter.Init();
        }
    }

    private void Start()
    {

    }

    public Fighter GetHealthyFighters()
    {
        return fighters.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddFighter(Fighter newFighter)
    {
        if (fighters.Count < 6)
        {
            fighters.Add(newFighter);
            OnUpdated?.Invoke();
        }
        else
        {
            // TODO: Add to the PC once that's implemented
        }
    }

    public bool CheckForEvolutions()
    {
        return fighters.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions()
    {
        foreach (var fighter in fighters)
        {
            var evolution = fighter.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionState.i.Evolve(fighter, evolution);
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static FighterParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<FighterParty>();
    }
}
