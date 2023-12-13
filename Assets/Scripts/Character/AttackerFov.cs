using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerFov : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEnterAttackerView(GetComponentInParent<AttackerController>());
    }
    public bool TriggerRepeatedly => false;
}
