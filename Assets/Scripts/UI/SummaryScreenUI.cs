using GDE.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image image;

    [Header("Pages")]
    [SerializeField] Text pageNameTxt;
    [SerializeField] GameObject skillsPage;
    [SerializeField] GameObject movesPage;

    [Header("Fighter Skills")]
    
        [SerializeField] Text hpText;
        [SerializeField] Text attackText;
        [SerializeField] Text defenseText;
        [SerializeField] Text spAttackText;
        [SerializeField] Text spDefenseText;
        [SerializeField] Text speedText;
        [SerializeField] Text expPointsText;
        [SerializeField] Text nextLevelExpText;
        [SerializeField] Transform expBar;

     [Header("Fighter Moves")]
    [SerializeField] List<Text> moveTypes;
    [SerializeField] List<Text> moveNames;
    [SerializeField] List<Text> moveCharges;
    [SerializeField] Text moveDescriptionText;
    [SerializeField] Text movePowerText;
    [SerializeField] Text moveAccuracyText;
    [SerializeField] GameObject moveEffectsUI;

    List<TextSlot> moveSlots;

    private void Start()
    {
       moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
        moveEffectsUI.SetActive(false);
        moveDescriptionText.text = "";
       
    }

    bool inMoveSelection;

    public bool InMoveSelection
    {
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;

            if (inMoveSelection)
            {
                moveEffectsUI.SetActive(true); 
                SetItems(moveSlots.Take(fighter.Moves.Count).ToList());
            }
            else
            {
                moveEffectsUI.SetActive(false);
                moveDescriptionText.text = "";
                ClearItems();
            }
        }
    }

    Fighter fighter;

    public void SetBasicDetails(Fighter fighter)
    {
        this.fighter = fighter;
        nameText.text = fighter.Base.Name;
        levelText.text = "Lv. " + fighter.Level;
        image.sprite = fighter.Base.FrontSprite;
    }

    public void ShowPage(int pageNum)
    {

        if (pageNum == 0)
        {
            //Show the skills page

            pageNameTxt.text = "Fighter Skills";

            skillsPage.SetActive(true);
            movesPage.SetActive(false);

            SetSkills();

        }
        else if (pageNum == 1)
        {
            //Show the moves page
            pageNameTxt.text = "Fighter Moves";

            skillsPage.SetActive(false);
            movesPage.SetActive(true);

            SetMoves();
        }
    }


    public void SetSkills()
    {
        hpText.text = $"{fighter.HP}/{fighter.MaxHp}";
        attackText.text = "" + fighter.Attack;
        defenseText.text = "" + fighter.Defense;
        spAttackText.text = "" + fighter.SpAttack;
        spDefenseText.text = "" + fighter.SpDefense;
        speedText.text = "" + fighter.Speed;

        expPointsText.text = "" + fighter.Exp;
        nextLevelExpText.text = "" + (fighter.Base.GetExpForLevel(fighter.Level + 1) - fighter.Exp); // the "" + converts it to a string
        expBar.localScale = new Vector2(fighter.GetNormalizedExp(), 1); 
    }

    public void SetMoves()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < fighter.Moves.Count)
            {
                var move = fighter.Moves[i];
                moveTypes[i].text = move.Base.Type.ToString().ToUpper();
                moveNames[i].text = move.Base.Name.ToUpper();
                moveCharges[i].text = $"Charges {move.PP}/{move.Base.PP}";
            }
            else
            {
                moveTypes[i].text = "-";
                moveNames[i].text = "-";
                moveCharges[i].text = "-";
            }
        }

    }

    public override void HandleUpdate()
    {
        if (InMoveSelection)
        base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        var move = fighter.Moves[selectedItem];

        moveDescriptionText.text = move.Base.Description;
        movePowerText.text = "" + move.Base.Power;
        moveAccuracyText.text = "" + move.Base.Accuracy;
    }



}
