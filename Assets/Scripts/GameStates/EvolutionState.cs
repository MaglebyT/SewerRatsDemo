using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image fighterImage;

    [SerializeField] AudioClip evolutionMusic;

    public static EvolutionState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Fighter fighter, Evolution evolution)
    {
        var gc = GameController.Instance;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);

        fighterImage.sprite = fighter.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{fighter.Base.Name} is transcending!");

        var oldFighter = fighter.Base;
        fighter.Evolve(evolution);

        fighterImage.sprite = fighter.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldFighter.Name} transcended into {fighter.Base.Name}");

        evolutionUI.SetActive(false);

        gc.PartyScreen.SetPartyData();
        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        gc.StateMachine.Pop();
    }
}
