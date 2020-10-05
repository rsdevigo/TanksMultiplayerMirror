using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TankHealth : NetworkBehaviour
{
  public float m_StartingHealth = 100f;
  public Slider m_Slider;
  public Image m_FillImage;
  public Color m_FullHealthColor = Color.green;
  public Color m_ZeroHealthColor = Color.red;
  public GameObject m_ExplosionPrefab;


  private AudioSource m_ExplosionAudio;
  private ParticleSystem m_ExplosionParticles;
  [SyncVar(hook = nameof(SyncCurrentHealth))]
  private float m_CurrentHealth;
  [SyncVar(hook = nameof(SyncDead))]
  private bool m_Dead;

  private void Awake()
  {
    m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
    m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();
    m_ExplosionParticles.gameObject.SetActive(false);
  }

  private void OnEnable()
  {
    if (isLocalPlayer)
    {
      CmdOnEnable();
    }
  }

  [Server]
  public void TakeDamage(float amount)
  {
    m_CurrentHealth -= amount;
    if (m_CurrentHealth <= 0f && !m_Dead)
    {
      SyncDead(m_Dead, true);
    }
  }

  //Server and Sync Methods
  public override void OnStartClient()
  {
    SyncCurrentHealth(m_CurrentHealth, m_StartingHealth);
    SyncDead(m_Dead, false);
    base.OnStartClient();
  }

  public override void OnStartServer()
  {
    SyncCurrentHealth(m_CurrentHealth, m_StartingHealth);
    SyncDead(m_Dead, false);
    base.OnStartServer();
  }

  private void SyncCurrentHealth(float OldValue, float NewValue)
  {
    m_CurrentHealth = NewValue;
    m_Slider.value = NewValue;
    m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, NewValue / m_StartingHealth);
  }

  private void SyncDead(bool OldValue, bool NewValue)
  {
    m_Dead = NewValue;
    if (m_Dead)
    {
      m_ExplosionParticles.transform.position = transform.position;
      m_ExplosionParticles.gameObject.SetActive(true);
      m_ExplosionParticles.Play();
      m_ExplosionAudio.Play();
      gameObject.SetActive(false);
    }
  }

  [Command]
  private void CmdOnEnable()
  {
    SyncDead(m_Dead, false);
    SyncCurrentHealth(m_CurrentHealth, m_StartingHealth);
  }

  // [Server]
  // private void EnableTank()
  // {
  //   SyncDead(m_Dead, false);
  //   SyncCurrentHealth(m_CurrentHealth, m_StartingHealth);
  //   gameObject.SetActive(true);
  // }

  // [ClientRpc]
  // private void RpcOnEnable()
  // {
  //   gameObject.SetActive(true);
  // }
}