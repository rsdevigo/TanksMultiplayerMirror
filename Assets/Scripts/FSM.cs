using UnityEngine;
using System.Collections.Generic;

namespace Patterns
{
  public class State
  {
    protected FSM m_fsm;

    public State(FSM fsm)
    {
      m_fsm = fsm;
    }
    public virtual void Enter() { }
    public virtual void Exit() { }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
  }

  public class FSM
  {
    protected Dictionary<int, State> m_States = new Dictionary<int, State>();
    public State m_CurrentState;
    public FSM()
    {

    }

    public void Add(int key, State state)
    {
      m_States[key] = state;
    }

    public State GetState(int key)
    {
      return m_States[key];
    }

    public void SetCurrentState(State state)
    {
      if (m_CurrentState != null)
      {
        m_CurrentState.Exit();
      }
      m_CurrentState = state;
      if (m_CurrentState != null)
      {
        m_CurrentState.Enter();
      }
    }

    public void Update()
    {
      if (m_CurrentState != null)
      {
        m_CurrentState.Update();
      }
    }

    public void FixedUpdate()
    {
      if (m_CurrentState != null)
      {
        m_CurrentState.FixedUpdate();
      }
    }
  }
}
