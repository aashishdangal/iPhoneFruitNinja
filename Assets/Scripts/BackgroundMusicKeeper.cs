using UnityEngine;

public class BackgroundMusicKeeper : MonoBehaviour
{
    private static BackgroundMusicKeeper instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
