using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text messageText;

    Fighter _fighter;

    public void Init(Fighter fighter)
    {
        _fighter = fighter;
        UpdateData();
        SetMessage("");

        _fighter.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _fighter.Base.Name;
        levelText.text = "Lvl " + _fighter.Level;
        hpBar.SetHP((float)_fighter.HP / _fighter.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.cyan;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
