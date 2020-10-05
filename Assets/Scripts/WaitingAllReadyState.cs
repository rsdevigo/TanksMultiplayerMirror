using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

class WaitingAllReadyState : State
{
  public GameManager m_GameManager;
  public WaitingAllReadyState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    m_GameManager.GameStart = false;
    m_GameManager.m_MessageText.text = "TANKS!\nWaiting Another Player";
    m_GameManager.m_ButtonIsReady.SetActive(false);
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

    if (m_GameManager.m_Tanks.Count >= m_GameManager.m_MinNumPlayers)
    {
      bool AllReady = true;
      foreach (GameObject tank in m_GameManager.m_Tanks)
      {
        TankManager tankManager = tank.GetComponent<TankManager>();
        if (!tankManager.isReady)
        {
          AllReady = false;
        }
      }

      if (AllReady)
      {
        m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.PLAYING));
      }
    }
  }

  public override void Exit()
  {
    base.Exit();
    Debug.Log("Exit of WaitingAllReadyState");
  }
}