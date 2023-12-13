using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Fighter _fighter;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Fighter fighter)
    {
        if (_fighter != null)
        {
            _fighter.OnHPChanged -= UpdateHP;
            _fighter.OnStatusChanged -= SetStatusText;
        }

        _fighter = fighter;

        nameText.text = fighter.Base.Name;
        SetLevel();
        hpBar.SetHP((float)fighter.HP / fighter.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
        };

        SetStatusText();
        _fighter.OnStatusChanged += SetStatusText;
        _fighter.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if (_fighter.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _fighter.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_fighter.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _fighter.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = _fighter.GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = _fighter.GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_fighter.HP / _fighter.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData()
    {
        if (_fighter != null)
        {
            _fighter.OnHPChanged -= UpdateHP;
            _fighter.OnStatusChanged -= SetStatusText;
        }
    }
}
