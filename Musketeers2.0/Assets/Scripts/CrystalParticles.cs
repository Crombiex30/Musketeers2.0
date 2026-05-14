using UnityEngine;

public class CrystalParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            particleEffect.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            particleEffect.Stop();
        }
    }
}