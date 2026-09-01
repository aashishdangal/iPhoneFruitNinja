using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject gameOverPanel;
    public FruitSpawner fruitSpawner;

    private bool gameOver = false;

    void Awake()
    {
        Instance = this;
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