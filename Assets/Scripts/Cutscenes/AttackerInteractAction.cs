using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInteractAction : CutsceneAction
{
    [SerializeField] AttackerController attacker;

    public override IEnumerator Play()
    {

        GameController.Instance.StateMachine.Pop();
        yield return attacker.Interact(PlayerController.i.transform);
        GameController.Instance.StateMachine.Push(CutsceneState.i);


    }


}