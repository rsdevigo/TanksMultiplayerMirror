using UnityEngine;
using System.Collections;
using Mirror;

public class ShellExplosion : NetworkBehaviour
{
  public LayerMask m_TankMask;
  public ParticleSystem m_ExplosionParticles;
  public AudioSource m_ExplosionAudio;
  public float m_MaxDamage = 100f;
  public float m_ExplosionForce = 1000f;
  public float m_MaxLifeTime = 1f;
  public float m_ExplosionRadius = 5f;

  public override void OnStartServer()
  {
    Invoke(nameof(DestroySelf), m_MaxLifeTime);
  }

  [Server]
  public void DestroySelf()
  {
    NetworkServer.Destroy(gameObject);
  }
  [ServerCallback]
  private void OnTriggerEnter(Collider other)
  {
    // Find all the tanks in an area around the shell and damage them.
    Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);
    for (int i = 0; i < colliders.Length; i++)
    {
      Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
      if (!targetRigidbody)
      {
        continue;
      }

      targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

      TankHealth tankHealth = targetRigidbody.GetComponent<TankHealth>();
      if (!tankHealth)
      {
        continue;
      }

      float damage = CalculateDamage(targetRigidbody.position);

      tankHealth.TakeDamage(damage);
    }
    RpcExplode();
    gameObject.SetActive(false);
  }

  [ClientRpc]
  void RpcExplode()
  {
    gameObject.SetActive(false);
    m_ExplosionParticles.transform.parent = null;
    m_ExplosionParticles.Play();
    m_ExplosionAudio.Play();
    Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
  }

  private float CalculateDamage(Vector3 targetPosition)
  {
    // Calculate the amount of damage a target should take based on it's position.
    Vector3 explosionToTarget = targetPosition - transform.position;
    float explosionDistance = explosionToTarget.magnitude;
    float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
    float damage = m_MaxDamage * relativeDistance;
    damage = Mathf.Max(0, damage);
    return damage;
  }
}