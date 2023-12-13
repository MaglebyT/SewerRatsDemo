using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartState : State<GameController> 
{
    [SerializeField] GameObject gameStartUI;
    

    public static GameStartState i { get; private set; }
    private void Awake()
    {
        i = this;
        
    }

    public void PlayGame()
    {
        GameController.Instance.PauseGame(false);
        gameStartUI.gameObject.SetActive(false);
      
    }

   
    
    public void LoadGame()
    {
        
        SavingSystem.i.Load("saveSlot1");
        GameController.Instance.PauseGame(false);
    }



}
