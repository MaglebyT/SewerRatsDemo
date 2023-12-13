using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Fighter> fighters;
    FighterParty party;

    public Fighter SelectedMember => fighters[selectedItem];

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);

        party = FighterParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        fighters = party.Fighters;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < fighters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(fighters[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
                 
        }

        var textSlots = memberSlots.Select(m => m.GetComponent<TextSlot>());
        SetItems(textSlots.Take(fighters.Count).ToList());

        messageText.text = "Choose a Fighter";
    }

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < fighters.Count; i++)
        {
            string message = tmItem.CanBeTaught(fighters[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < fighters.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
