using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cutscene : MonoBehaviour, IPlayerTriggerable
{
    [SerializeReference]
    [SerializeField] List<CutsceneAction> actions;
    [SerializeField] private bool destroyAfterExecution = false;
  


    public bool TriggerRepeatedly => false;

    public IEnumerator Play()
    {
        GameController.Instance.StateMachine.Push(CutsceneState.i);

        foreach (var action in actions)
        {
            if (action.WaitForCompletion)
            {
                yield return action.Play();
            }
            
            else
            {
                yield return StartCoroutine(action.Play());
            }
        }

        GameController.Instance.StateMachine.Pop();
        if (destroyAfterExecution)
        {
            // Destroy the GameObject to remove the cutscene
            Destroy(gameObject);
        }
        
        else
        {
            // Disable the collider if the cutscene doesn't destroy itself
            this.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public void AddAction(CutsceneAction action)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Add action to cutscene.");
#endif

        action.Name = action.GetType().ToString();
        actions.Add(action);
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(Play());
    }
}
