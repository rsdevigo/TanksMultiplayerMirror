using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

class NotReadyState : State
{
  public GameManager m_GameManager;
  public NotReadyState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    m_GameManager.GameStart = false;
    m_GameManager.m_MessageText.text = "TANKS!\nYou are Ready ?";
    m_GameManager.m_ButtonIsReady.SetActive(true);
  }

  public override void Update()
  {
    base.Update();
    if (!NetworkManagerTanks.singleton.isNetworkActive)
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
    }

    m_GameManager.SearchTanks();

    if (m_GameManager.m_Tanks.Count < m_GameManager.m_MinNumPlayers)
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.WAITING_PLAYER));
    }

    if (m_GameManager.LocalPlayer.isReady)
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.WAITING_ALL_READY));
    }
  }

  public override void Exit()
  {
    base.Exit();
    m_GameManager.m_ButtonIsReady.SetActive(false);
    Debug.Log("Exit of NotReadyState");
  }
}