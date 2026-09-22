using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game References")]
    public GameObject gameOverPanel;
    public FruitSpawner fruitSpawner;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text fruitHitsText;
    public TMP_Text livesText;

    [Header("Lives")]
    public int startingLives = 3;

    [Header("Miss Sound")]
    public AudioSource missAudioSource;
    public AudioClip fruitMissSound;

    [Range(0f, 1f)]
    public float missVolume = 0.5f;

    [Header("Bomb Sounds")]
    public AudioClip bombMetalHitSound;
    public AudioClip bombExplosionSound;

    [Range(0f, 1f)]
    public float metalHitVolume = 0.5f;

    [Range(0f, 1f)]
    public float explosionVolume = 0.45f;

    [Min(0f)]
    public float metalToExplosionDelay = 0.12f;

    [Header("Bomb White Flash")]
    public float explosionLeadTime = 0.15f;
    public float flashRiseTime = 0.08f;
    public float whiteHoldTime = 0.35f;
    public float flashFadeTime = 0.8f;

    [Header("Music During Explosion")]
    [Range(0f, 1f)]
    public float musicDuringBlast = 0.15f;

    private int lives;
    private bool gameOver;
    private int score;
    private int fruitHits;

    private Image flashImage;
    private AudioSource bombAudioSource;
    private AudioSource backgroundSource;
    private float originalMusicVolume;
    private bool musicDucked;

    void Awake()
    {
        Instance = this;
        CreateFlashOverlay();

        bombAudioSource = gameObject.AddComponent<AudioSource>();
        bombAudioSource.playOnAwake = false;
        bombAudioSource.loop = false;
        bombAudioSource.spatialBlend = 0f;
        bombAudioSource.volume = 1f;
    }

    void Start()
    {
        lives = startingLives;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void CreateFlashOverlay()
    {
        GameObject overlay = new GameObject(
            "BombFlashCanvas",
            typeof(RectTransform),
            typeof(Canvas)
        );

        overlay.transform.SetParent(transform, false);

        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        GameObject imageObject = new GameObject(
            "WhiteFlash",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        imageObject.transform.SetParent(overlay.transform, false);

        RectTransform rect =
            imageObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flashImage = imageObject.GetComponent<Image>();
        flashImage.raycastTarget = false;

        SetFlashAlpha(0f);
    }

    private void SetFlashAlpha(float alpha)
    {
        if (flashImage != null)
        {
            flashImage.color = new Color(
                1f, 1f, 1f, Mathf.Clamp01(alpha)
            );
        }
    }

    public void ProcessStroke(int fruitsInStroke)
    {
        if (gameOver || fruitsInStroke <= 0)
            return;

        int comboBonus =
            fruitsInStroke >= 2 ? fruitsInStroke : 0;

        score += fruitsInStroke + comboBonus;
        fruitHits += fruitsInStroke;

        if (comboText != null)
        {
            comboText.text = fruitsInStroke >= 2
                ? "COMBO x" + fruitsInStroke
                : "COMBO x0";
        }

        UpdateUI();
    }

    public void FruitMissed()
    {
        if (gameOver)
            return;

        lives--;

        if (missAudioSource != null && fruitMissSound != null)
        {
            missAudioSource.PlayOneShot(
                fruitMissSound,
                missVolume
            );
        }

        UpdateUI();

        if (lives <= 0)
            GameOver();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;

        if (fruitHitsText != null)
            fruitHitsText.text = "FRUIT HITS: " + fruitHits;

        if (livesText != null)
            livesText.text = "LIVES: " + lives;
    }

    private bool FreezeGame()
    {
        if (gameOver)
            return false;

        gameOver = true;

        if (fruitSpawner != null)
            fruitSpawner.StopSpawning();

        // Stop every falling whistle, including other bombs.
        Bomb[] bombs = FindObjectsByType<Bomb>(
            FindObjectsSortMode.None
        );

        foreach (Bomb bomb in bombs)
            bomb.StopFallingSound();

        Time.timeScale = 0f;
        return true;
    }

    public void GameOver()
    {
        if (!FreezeGame())
            return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Keeps compatibility with existing calls without arguments.
    public void GameOverWithBlast()
    {
        GameOverWithBlast(Vector3.zero, null);
    }

    public void GameOverWithBlast(
        Vector3 position,
        ParticleSystem blastPrefab
    )
    {
        if (!FreezeGame())
            return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        StartCoroutine(BombSequence(position, blastPrefab));
    }

    private IEnumerator BombSequence(
        Vector3 position,
        ParticleSystem blastPrefab
    )
    {
        BeginMusicDuck();

        if (bombMetalHitSound != null)
        {
            bombAudioSource.PlayOneShot(
                bombMetalHitSound,
                metalHitVolume
            );
        }

        // Fade music down during the brief clang-to-boom gap.
        float delay = Mathf.Max(0f, metalToExplosionDelay);
        float elapsed = 0f;

        while (elapsed < delay)
        {
            elapsed += Time.unscaledDeltaTime;

            if (musicDucked && backgroundSource != null)
            {
                backgroundSource.volume = Mathf.Lerp(
                    originalMusicVolume,
                    originalMusicVolume * musicDuringBlast,
                    Mathf.Clamp01(elapsed / delay)
                );
            }

            yield return null;
        }

        if (musicDucked && backgroundSource != null)
        {
            backgroundSource.volume =
                originalMusicVolume * musicDuringBlast;
        }

        if (bombExplosionSound != null)
        {
            bombAudioSource.PlayOneShot(
                bombExplosionSound,
                explosionVolume
            );
        }

        SpawnBlast(position, blastPrefab);

        // Let the expanding blast appear before the white flash.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, explosionLeadTime)
        );

        yield return FadeFlash(0f, 1f, flashRiseTime);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, whiteHoldTime)
        );

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        yield return FadeFlash(1f, 0f, flashFadeTime);

        // Keep music low until the explosion sound finishes.
        while (bombAudioSource != null && bombAudioSource.isPlaying)
            yield return null;

        yield return RestoreMusicGradually();
    }

    private void SpawnBlast(
        Vector3 position,
        ParticleSystem blastPrefab
    )
    {
        if (blastPrefab == null)
            return;

        ParticleSystem blast = Instantiate(
            blastPrefab,
            position,
            Quaternion.identity
        );

        var main = blast.main;
        main.useUnscaledTime = true;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.startLifetime = 0.6f;
        main.startSpeed = 0f;
        main.startSize = 6f;

        var size = blast.sizeOverLifetime;
        size.enabled = true;
        size.separateAxes = false;
        size.size = new ParticleSystem.MinMaxCurve(
            1f,
            AnimationCurve.Linear(0f, 0.1f, 1f, 1f)
        );

        foreach (ParticleSystem child in
                 blast.GetComponentsInChildren<ParticleSystem>())
        {
            var childMain = child.main;
            childMain.useUnscaledTime = true;
        }

        blast.Play(true);
    }

    private void BeginMusicDuck()
    {
        // Find the surviving music player after any scene restart.
        BackgroundMusicKeeper keeper =
            FindFirstObjectByType<BackgroundMusicKeeper>();

        if (keeper == null)
            return;

        backgroundSource = keeper.GetComponent<AudioSource>();

        if (backgroundSource == null)
            return;

        originalMusicVolume = backgroundSource.volume;
        musicDucked = true;
    }

    private IEnumerator RestoreMusicGradually()
    {
        if (!musicDucked || backgroundSource == null)
            yield break;

        float from = backgroundSource.volume;
        float elapsed = 0f;
        const float duration = 0.8f;

        while (elapsed < duration && backgroundSource != null)
        {
            elapsed += Time.unscaledDeltaTime;

            backgroundSource.volume = Mathf.Lerp(
                from,
                originalMusicVolume,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        RestoreMusicImmediately();
    }

    private void RestoreMusicImmediately()
    {
        if (musicDucked && backgroundSource != null)
            backgroundSource.volume = originalMusicVolume;

        musicDucked = false;
    }

    private IEnumerator FadeFlash(
        float from,
        float to,
        float duration
    )
    {
        if (duration <= 0f)
        {
            SetFlashAlpha(to);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            SetFlashAlpha(
                Mathf.Lerp(from, to, elapsed / duration)
            );

            yield return null;
        }

        SetFlashAlpha(to);
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        RestoreMusicImmediately();

        if (bombAudioSource != null)
            bombAudioSource.Stop();

        SetFlashAlpha(0f);
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    void OnDestroy()
    {
        RestoreMusicImmediately();

        if (Instance == this)
            Instance = null;
    }
}