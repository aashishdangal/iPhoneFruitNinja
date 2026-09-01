using UnityEngine;
using UnityEngine.SceneManagement;
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

    private bool gameOver = false;

    // Score
    private int score = 0;

    // Total number of fruits sliced
    private int fruitHits = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    // Called by SwordSlicer when one sword stroke is finished
    public void ProcessStroke(int fruitsInStroke)
    {
        if (gameOver || fruitsInStroke <= 0)
            return;

        // Every fruit is worth 1 point
        int baseScore = fruitsInStroke;

        // Multiple fruits in ONE stroke create a combo bonus
        int comboBonus = 0;

        if (fruitsInStroke >= 2)
        {
            comboBonus = fruitsInStroke;
        }

        int totalPoints = baseScore + comboBonus;

        score += totalPoints;
        fruitHits += fruitsInStroke;

        Debug.Log(
            "🗡️ STROKE: " + fruitsInStroke +
            " fruits | Base: +" + baseScore +
            " | Combo: +" + comboBonus +
            " | Total: +" + totalPoints
        );

        // Update combo display
        if (comboText != null)
        {
            if (fruitsInStroke >= 2)
            {
                comboText.text = "COMBO x" + fruitsInStroke;
            }
            else
            {
                comboText.text = "COMBO x0";
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }

        if (fruitHitsText != null)
        {
            fruitHitsText.text = "FRUIT HITS: " + fruitHits;
        }
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("💣 GAME OVER!");

        if (fruitSpawner != null)
        {
            fruitSpawner.StopSpawning();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}