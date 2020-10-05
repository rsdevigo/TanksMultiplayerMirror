using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

class WaitingPlayerState : State
{
  GameManager m_GameManager;
  public WaitingPlayerState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    FindLocalPlayer();
    m_GameManager.GameStart = false;
    m_GameManager.m_MessageText.text = "TANKS!\nWaiting Another Player To Connect";
  }


  public override void Update()
  {
    base.Update();
    if (NetworkManagerTanks.singleton.isNetworkActive)
    {
      if (m_GameManager.LocalPlayer == null)
      {
        FindLocalPlayer();
      }
      else
      {
        m_GameManager.m_Tanks.Clear();
        foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
        {
          GameObject comp = kvp.Value.gameObject;
          if (comp != null && !m_GameManager.m_Tanks.Contains(comp) && comp.GetComponent<TankManager>())
          {
            m_GameManager.m_Tanks.Add(comp);
          }
        }
        if (m_GameManager.m_Tanks.Count == m_GameManager.m_MinNumPlayers)
        {
          m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.NOT_READY));
        }
      }
    }
    else
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
    }
  }

  public override void Exit()
  {
    base.Exit();
    Debug.Log("Exit of WaitingPlayerState");
  }

  void FindLocalPlayer()
  {
    if (ClientScene.localPlayer == null)
      return;
    m_GameManager.LocalPlayer = ClientScene.localPlayer.gameObject.GetComponent<TankManager>();
    m_GameManager.LocalPlayer.DisableControl();
  }
}