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

    [Header("Bomb White Flash")]
    public float explosionLeadTime = 0.15f;
    public float flashRiseTime = 0.08f;
    public float whiteHoldTime = 0.35f;
    public float flashFadeTime = 0.8f;

    private int lives;
    private bool gameOver;
    private int score;
    private int fruitHits;

    private Image flashImage;

    void Awake()
    {
        Instance = this;
        CreateFlashOverlay();
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

        imageObject.transform.SetParent(
            overlay.transform,
            false
        );

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

    public void GameOverWithBlast()
    {
        if (!FreezeGame())
            return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        StartCoroutine(BombFlashSequence());
    }

    private IEnumerator BombFlashSequence()
    {
        // Let the expanding explosion appear first.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, explosionLeadTime)
        );

        yield return FadeFlash(0f, 1f, flashRiseTime);

        // Hold the entire screen white.
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, whiteHoldTime)
        );

        // Reveal Game Over as the white fades away.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        yield return FadeFlash(1f, 0f, flashFadeTime);
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
        SetFlashAlpha(0f);

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}