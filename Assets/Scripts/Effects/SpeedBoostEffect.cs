using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private void Awake()
    {
        ps.Stop();
        ps.Clear();
    }

    public void PlayParticles(float duration)
    {
        ps.Play();
        StartCoroutine(StopParticles(duration));
    }

    private IEnumerator StopParticles(float duration)
    {
        yield return new WaitForSeconds(duration);
        ps.Stop();
        ps.Clear();
    }
}
