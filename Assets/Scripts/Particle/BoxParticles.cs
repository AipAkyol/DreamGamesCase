using DG.Tweening;
using UnityEngine;

public class BoxParticles : MonoBehaviour
{
    public Transform[] particles;
    public float minScatterDistance = 0.3f;
    public float maxScatterDistance = 0.7f;
    public float duration = 0.5f;
    public float maxRotation = 180f; // Max degrees particles can rotate

    void Start()
    {
        foreach (Transform particle in particles)
        {
            // 1. Random direction (normalized)
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            ).normalized;

            // 2. Random distance
            float distance = Random.Range(minScatterDistance, maxScatterDistance);

            // 3. Animate movement
            particle.DOMove(
                transform.position + randomDirection * distance,
                duration
            ).SetEase(Ease.OutQuad);

            // 4. Add random rotation
            particle.DORotate(
                new Vector3(0, 0, Random.Range(-maxRotation, maxRotation)),
                duration
            );

            // 5. Fade out
            SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOFade(0, duration).OnComplete(() => Destroy(particle.gameObject));
            }
            else
            {
                Destroy(particle.gameObject, duration);
            }
        }

        // Destroy parent after all animations finish
        Destroy(gameObject, duration + 0.1f);
    }
}