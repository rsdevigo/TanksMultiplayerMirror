using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

class FinishPlayState : State
{
  public GameManager m_GameManager;
  public FinishPlayState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    m_GameManager.StopGameLoop();
    m_GameManager.GameStart = false;
    m_GameManager.LocalPlayer.DisableControl();
    m_GameManager.m_MessageText.text = m_GameManager.m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
    m_GameManager.m_ButtonDisconnect.SetActive(true);
    m_GameManager.m_GameWinner = null;
  }

  public override void Update()
  {
    base.Update();
    if (!NetworkManager.singleton.isNetworkActive)
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
    //m_GameManager.SearchTanks();
    if (NetworkManager.singleton.numPlayers == 1 && m_GameManager.LocalPlayer.isServer)
    {
      NetworkManager.singleton.StopHost();
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
    }
  }

  public override void Exit()
  {
    base.Exit();
    m_GameManager.m_ButtonDisconnect.SetActive(false);
    Debug.Log("Exit of FinishPlayState");
  }
}