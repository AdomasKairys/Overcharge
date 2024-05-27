using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GravityBombEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private void Awake()
    {
        ps.Stop();
        ps.Clear();
    }

    public void PlayParticles()
    {
        Debug.Log("Particles played");
        ps.Play();
        StartCoroutine(StopParticles());
    }

    private IEnumerator StopParticles()
    {
        yield return new WaitForSeconds(2f);
        ps.Stop();
        ps.Clear();
    }
}
