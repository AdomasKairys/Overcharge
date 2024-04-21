using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParticleController : NetworkBehaviour
{
    [SerializeField] private List<ParticleSystem> particles;
    [SerializeField] private ParticleSystem particleToTurnOff;
    private void Awake()
    {
        foreach (var particle in particles)
        {
            particle.Stop();
        }
    }
    public void PlayParticles()
    {
        PlayeParticlesServerRPC();
    }
    [ServerRpc]
    private void PlayeParticlesServerRPC()
    {
        PlayeParticlesClientRPC();
    }
    [ClientRpc]
    private void PlayeParticlesClientRPC()
    {
        particleToTurnOff.Clear();
        particleToTurnOff.Stop();
        foreach (var particle in particles)
        {
            particle.Play();
        }
    }
}
