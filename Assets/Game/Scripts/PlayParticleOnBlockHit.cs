using UnityEngine;

public class PlayParticleOnBlockHit : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BlockShape>() != null)
        {
            if (particleEffect != null)
            {
                particleEffect.Play();
            }
        }
    }
    
}