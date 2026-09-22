using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float destroyBelowY = -8f;
    public ParticleSystem blastPrefab;

    [Header("Falling Sound")]
    public AudioClip bombFallingSound;

    [Range(0f, 1f)]
    public float fallingVolume = 0.3f;

    private AudioSource fallingSource;
    private bool exploded;

    void Awake()
    {
        // A dedicated source for this bomb's falling whistle.
        fallingSource = gameObject.AddComponent<AudioSource>();
        fallingSource.playOnAwake = false;
        fallingSource.loop = false;
        fallingSource.spatialBlend = 0f;
        fallingSource.volume = fallingVolume;
    }

    void Start()
    {
        if (bombFallingSound != null && Time.timeScale > 0f)
        {
            fallingSource.clip = bombFallingSound;
            fallingSource.Play();
        }
    }

    public void StopFallingSound()
    {
        if (fallingSource != null)
            fallingSource.Stop();
    }

    public void Explode()
    {
        if (exploded || Time.timeScale == 0f)
            return;

        exploded = true;
        StopFallingSound();

        if (GameManager.Instance != null)
        {
            // GameManager keeps the sequence running after
            // this bomb is destroyed.
            GameManager.Instance.GameOverWithBlast(
                transform.position,
                blastPrefab
            );
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (!exploded && transform.position.y < destroyBelowY)
        {
            StopFallingSound();
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        StopFallingSound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<SwordSlicer>() != null)
            Explode();
    }
}