using System.Collections;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Prefabs")]
    public GameObject[] fruitPrefabs;

    [Header("Bomb")]
    public GameObject bombPrefab;

    [Header("Spawn Area")]
    public float minX = -5f;
    public float maxX = 5f;
    public float spawnY = -5f;
    public float spawnZ = 0f;

    [Header("Launch Settings")]
    public float minForce = 8f;
    public float maxForce = 12f;
    public float minHorizontalForce = -2f;
    public float maxHorizontalForce = 2f;

    [Header("Wave Settings")]
    public float minSpawnDelay = 1.5f;
    public float maxSpawnDelay = 2.2f;

    public int minFruitsPerWave = 1;
    public int maxFruitsPerWave = 3;

    public float fruitSpacing = 0.12f;

    [Header("Bomb Settings")]
    [Range(0f, 1f)]
    public float bombChance = 0.12f;

    [Header("Difficulty")]
    public float difficultyIncreaseTime = 20f;
    public float forceIncreasePerLevel = 0.5f;

    private bool spawning = true;
    private float gameTime = 0f;

    void Start()
    {
        Debug.Log("🍎 FruitSpawner started!");

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (spawning)
        {
            gameTime += Time.deltaTime;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            float delay =
                Random.Range(
                    minSpawnDelay,
                    maxSpawnDelay
                );

            yield return new WaitForSeconds(delay);

            if (!spawning)
                yield break;

            SpawnWave();
        }
    }

    void SpawnWave()
    {
        Debug.Log("🌊 SPAWN WAVE");

        int difficultyLevel =
            Mathf.FloorToInt(
                gameTime / difficultyIncreaseTime
            );

        int fruitCount =
            Random.Range(
                minFruitsPerWave,
                maxFruitsPerWave + 1
            );

        if (Random.value < bombChance)
        {
            SpawnBomb(difficultyLevel);
        }

        StartCoroutine(
            SpawnFruitWave(
                fruitCount,
                difficultyLevel
            )
        );
    }

    IEnumerator SpawnFruitWave(
        int fruitCount,
        int difficultyLevel
    )
    {
        for (int i = 0; i < fruitCount; i++)
        {
            if (!spawning)
                yield break;

            SpawnFruit(difficultyLevel);

            yield return new WaitForSeconds(
                fruitSpacing
            );
        }
    }

    void SpawnFruit(int difficultyLevel)
    {
        if (fruitPrefabs == null ||
            fruitPrefabs.Length == 0)
        {
            Debug.LogError(
                "❌ NO FRUIT PREFABS ASSIGNED!"
            );

            return;
        }

        GameObject fruitPrefab =
            fruitPrefabs[
                Random.Range(
                    0,
                    fruitPrefabs.Length
                )
            ];

        if (fruitPrefab == null)
        {
            Debug.LogError(
                "❌ Fruit prefab slot is EMPTY!"
            );

            return;
        }

        float x =
            Random.Range(
                minX,
                maxX
            );

        Vector3 spawnPosition =
            new Vector3(
                x,
                spawnY,
                spawnZ
            );

        GameObject fruit =
            Instantiate(
                fruitPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Debug.Log(
            "🍎 FRUIT SPAWNED at " +
            spawnPosition
        );

        Rigidbody rb =
            fruit.GetComponent<Rigidbody>();

        if (rb != null)
        {
            float verticalForce =
                Random.Range(
                    minForce,
                    maxForce
                ) +
                difficultyLevel *
                forceIncreasePerLevel;

            float horizontalForce =
                Random.Range(
                    minHorizontalForce,
                    maxHorizontalForce
                );

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(
                new Vector3(
                    horizontalForce,
                    verticalForce,
                    0f
                ),
                ForceMode.Impulse
            );

            // LOCK ONLY DEPTH
            rb.constraints =
                RigidbodyConstraints.FreezePositionZ;

            // Spin only around Z
            rb.AddTorque(
                Vector3.forward *
                Random.Range(-3f, 3f),
                ForceMode.Impulse
            );
        }
        else
        {
            Debug.LogWarning(
                "⚠️ Fruit has NO Rigidbody!"
            );
        }
    }

    void SpawnBomb(int difficultyLevel)
    {
        if (bombPrefab == null)
        {
            Debug.LogWarning(
                "⚠️ Bomb prefab not assigned!"
            );

            return;
        }

        float x =
            Random.Range(
                minX,
                maxX
            );

        Vector3 spawnPosition =
            new Vector3(
                x,
                spawnY,
                spawnZ
            );

        GameObject bomb =
            Instantiate(
                bombPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Rigidbody rb =
            bomb.GetComponent<Rigidbody>();

        if (rb != null)
        {
            float verticalForce =
                Random.Range(
                    minForce,
                    maxForce
                ) +
                difficultyLevel *
                forceIncreasePerLevel;

            float horizontalForce =
                Random.Range(
                    minHorizontalForce,
                    maxHorizontalForce
                );

            rb.linearVelocity = Vector3.zero;

            rb.AddForce(
                new Vector3(
                    horizontalForce,
                    verticalForce,
                    0f
                ),
                ForceMode.Impulse
            );

            rb.constraints =
                RigidbodyConstraints.FreezePositionZ;

            rb.AddTorque(
                Vector3.forward *
                Random.Range(-3f, 3f),
                ForceMode.Impulse
            );
        }
    }

    public void StopSpawning()
    {
        spawning = false;

        StopAllCoroutines();

        Debug.Log(
            "🛑 Fruit Spawner Stopped"
        );
    }
}