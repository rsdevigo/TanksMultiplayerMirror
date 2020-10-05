using Patterns;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;



public class GameManager : MonoBehaviour
{
  public int m_NumRoundsToWin = 2;
  public float m_StartDelay = 3f;
  public float m_EndDelay = 3f;
  public int m_MaxNumPlayers = 2;
  public int m_MinNumPlayers = 2;
  public CameraControl m_CameraControl;
  public Text m_MessageText;
  public GameObject m_ButtonIsReady;
  public GameObject m_ButtonDisconnect;
  public GameObject m_TankPrefab;
  public List<GameObject> m_Tanks;
  public TankManager LocalPlayer;
  public int m_RoundNumber;
  private WaitForSeconds m_StartWait;
  private WaitForSeconds m_EndWait;
  public TankManager m_RoundWinner;
  public TankManager m_GameWinner;
  public bool GameStart;
  private FSM m_Fsm = new FSM();
  public GameObject m_ConnectServerHUD;
  public InputField m_ConnectServerHUDAddress;




  public void Start()
  {
    m_StartWait = new WaitForSeconds(m_StartDelay);
    m_EndWait = new WaitForSeconds(m_EndDelay);
    m_Tanks = new List<GameObject>();
    GameStart = false;
    m_Fsm.Add((int)GameManagerStateEnum.DISCONNECT, new DisconnectState(m_Fsm, this));
    m_Fsm.Add((int)GameManagerStateEnum.WAITING_PLAYER, new WaitingPlayerState(m_Fsm, this));
    m_Fsm.Add((int)GameManagerStateEnum.NOT_READY, new NotReadyState(m_Fsm, this));
    m_Fsm.Add((int)GameManagerStateEnum.WAITING_ALL_READY, new WaitingAllReadyState(m_Fsm, this));
    m_Fsm.Add((int)GameManagerStateEnum.PLAYING, new PlayingState(m_Fsm, this));
    m_Fsm.Add((int)GameManagerStateEnum.FINISH_PLAY, new FinishPlayState(m_Fsm, this));
    m_Fsm.SetCurrentState(m_Fsm.GetState((int)GameManagerStateEnum.DISCONNECT));
  }


  public void Update()
  {
    m_Fsm.Update();
    SetCameraTargets();
  }

  public void ResetCamera()
  {
    SearchTanks();
    SetCameraTargets();
  }

  public void SearchTanks()
  {
    m_Tanks.Clear();
    foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
    {
      GameObject comp = kvp.Value.gameObject;
      if (comp != null && !m_Tanks.Contains(comp) && comp.GetComponent<TankManager>())
      {
        m_Tanks.Add(comp);
      }
    }
  }

  public void matchStart()
  {
    SetCameraTargets();
    StartCoroutine(GameLoop());
  }

  void FindLocalTank()
  {
    if (ClientScene.localPlayer == null)
      return;

    LocalPlayer = ClientScene.localPlayer.gameObject.GetComponent<TankManager>();
    LocalPlayer.DisableControl();
  }


  private void SetCameraTargets()
  {
    Transform[] targets = new Transform[m_Tanks.Count];

    for (int i = 0; i < targets.Length; i++)
    {
      targets[i] = m_Tanks[i].transform;
    }

    m_CameraControl.m_Targets = targets;
  }

  public void StopGameLoop()
  {
    StopAllCoroutines();
  }


  private IEnumerator GameLoop()
  {
    yield return StartCoroutine(RoundStarting());
    yield return StartCoroutine(RoundPlaying());
    yield return StartCoroutine(RoundEnding());

    if (m_GameWinner != null)
    {
      yield return null;
    }
    else
    {
      StartCoroutine(GameLoop());
    }

  }


  private IEnumerator RoundStarting()
  {
    ResetAllTanks();
    DisableTankControl();
    m_CameraControl.SetStartPositionAndSize();
    m_RoundNumber++;
    m_MessageText.text = "ROUND " + m_RoundNumber;
    yield return m_StartWait;
  }


  private IEnumerator RoundPlaying()
  {
    EnableTankControl();
    m_MessageText.text = "";
    while (!OneTankLeft())
    {
      yield return null;
    }
  }


  private IEnumerator RoundEnding()
  {
    DisableTankControl();
    m_RoundWinner = null;
    m_RoundWinner = GetRoundWinner();
    if (m_RoundWinner != null)
    {
      if (NetworkManagerMode.ServerOnly == NetworkManager.singleton.mode || LocalPlayer.isServer)
        m_RoundWinner.numberOfWins += 1;
    }
    m_GameWinner = GetGameWinner();
    string message = EndMessage();
    m_MessageText.text = message;
    yield return m_EndWait;
  }


  private bool OneTankLeft()
  {
    int numTanksLeft = 0;

    for (int i = 0; i < m_Tanks.Count; i++)
    {
      if (m_Tanks[i].activeSelf)
        numTanksLeft++;
    }

    return numTanksLeft <= 1;
  }

  private TankManager GetRoundWinner()
  {
    for (int i = 0; i < m_Tanks.Count; i++)
    {
      if (m_Tanks[i].activeSelf)
        return m_Tanks[i].GetComponent<TankManager>();
    }

    return null;
  }


  private TankManager GetGameWinner()
  {
    for (int i = 0; i < m_Tanks.Count; i++)
    {
      if (m_Tanks[i].GetComponent<TankManager>().numberOfWins == m_NumRoundsToWin)
        return m_Tanks[i].GetComponent<TankManager>();
    }

    return null;
  }


  private string EndMessage()
  {
    string message = "DRAW!";

    if (m_RoundWinner != null)
      message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

    message += "\n\n\n\n";

    for (int i = 0; i < m_Tanks.Count; i++)
    {
      message += m_Tanks[i].GetComponent<TankManager>().m_ColoredPlayerText + ": " + m_Tanks[i].GetComponent<TankManager>().numberOfWins + " WINS\n";
    }

    return message;
  }


  private void ResetAllTanks()
  {
    for (int i = 0; i < m_Tanks.Count; i++)
    {
      m_Tanks[i].GetComponent<TankManager>().Reset();
    }
  }


  private void EnableTankControl()
  {
    for (int i = 0; i < m_Tanks.Count; i++)
    {
      m_Tanks[i].GetComponent<TankManager>().EnableControl();
    }
  }


  private void DisableTankControl()
  {
    for (int i = 0; i < m_Tanks.Count; i++)
    {
      m_Tanks[i].GetComponent<TankManager>().DisableControl();
    }
  }

  public void IsReadyButtonAction()
  {
    LocalPlayer.isReady = true;
  }

  public void DisconnectButtonAction()
  {
    if (LocalPlayer.isServer)
    {
      Debug.Log("Aqui StopHost");
      NetworkManager.singleton.StopHost();
    }
    else
    {
      NetworkManager.singleton.StopClient();
    }
  }

  public void SetConnectHUD(bool state)
  {
    m_ConnectServerHUD.SetActive(state);
  }

  public void CreateServerButtonAction()
  {
    NetworkManager.singleton.StartHost();
  }

  public void ConnectServerButtonAction()
  {
    NetworkManager.singleton.networkAddress = m_ConnectServerHUDAddress.text;
    NetworkManager.singleton.StartClient();
  }
}