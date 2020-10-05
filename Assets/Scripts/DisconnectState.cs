using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Mirror;
public delegate void SetLocalPlayerDel(TankManager lp);
class DisconnectState : State
{
  GameManager m_GameManager;
  public DisconnectState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    m_GameManager.LocalPlayer = null;
    m_GameManager.GameStart = false;
    m_GameManager.m_Tanks.Clear();
    m_GameManager.m_MessageText.text = "TANKS!";
    m_GameManager.m_RoundNumber = 0;
    m_GameManager.SetConnectHUD(true);
  }

  public override void Update()
  {
    base.Update();
    if (NetworkClient.isConnected && !ClientScene.ready)
    {
      ClientScene.Ready(NetworkClient.connection);
    }
    else if (NetworkManagerTanks.singleton.isNetworkActive)
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.WAITING_PLAYER));
    }
  }

  public override void Exit()
  {
    base.Exit();
    m_GameManager.SetConnectHUD(false);
    Debug.Log("Exit of DisconnectState");
  }
}