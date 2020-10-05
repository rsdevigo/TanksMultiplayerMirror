using Patterns;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

class PlayingState : State
{
  public GameManager m_GameManager;
  public PlayingState(FSM fsm, GameManager gameManager) : base(fsm)
  {
    m_GameManager = gameManager;
  }

  public override void Enter()
  {
    base.Enter();
    m_GameManager.GameStart = true;
    m_GameManager.m_MessageText.text = "";
    m_GameManager.matchStart();
  }

  public override void Update()
  {
    base.Update();
    if (!NetworkManagerTanks.singleton.isNetworkActive)
    {
      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
    }

    if (m_GameManager.m_GameWinner != null || NetworkManagerTanks.singleton.numPlayers == 1)
    {
      if (NetworkManagerTanks.singleton.numPlayers == 1)
      {
        m_GameManager.m_GameWinner = m_GameManager.LocalPlayer;
      }

      m_fsm.SetCurrentState(m_fsm.GetState((int)GameManagerStateEnum.FINISH_PLAY));
    }
  }

  public override void Exit()
  {
    base.Exit();
    m_GameManager.StopGameLoop();
    Debug.Log("Exit of PlayingState");
  }
}