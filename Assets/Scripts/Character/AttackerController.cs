using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] AudioClip attackerAppearsClip;

    // State
    bool battleLost = false;

    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            AudioManager.i.PlayMusic(attackerAppearsClip);

            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartAttackerBattle(this);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }

    }

    public IEnumerator TriggerAttackerBattle(PlayerController player)
    {
        GameController.Instance.StateMachine.Push(CutsceneState.i);

        AudioManager.i.PlayMusic(attackerAppearsClip);

        // Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        exclamation.SetActive(false);

        // Walk towards the player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // Show dialog
        yield return DialogManager.Instance.ShowDialog(dialog);

        GameController.Instance.StateMachine.Pop();

        GameController.Instance.StartAttackerBattle(this);
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
