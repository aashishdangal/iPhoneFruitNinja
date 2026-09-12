using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float destroyBelowY = -8f;
    public ParticleSystem blastPrefab;

    private bool exploded;

    public void Explode()
    {
        if (exploded || Time.timeScale == 0f)
            return;

        exploded = true;

        if (blastPrefab != null)
        {
            ParticleSystem blast = Instantiate(
                blastPrefab,
                transform.position,
                Quaternion.identity
            );

            var main = blast.main;
            main.useUnscaledTime = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.startLifetime = 0.6f;
            main.startSpeed = 0f;
            main.startSize = 6f;
            var size = blast.sizeOverLifetime;
            size.enabled = true;
            size.separateAxes = false;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.1f, 1f, 1f));

            blast.Play(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOverWithBlast();
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (!exploded && transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<SwordSlicer>() != null)
        {
            Explode();
        }
    }
}