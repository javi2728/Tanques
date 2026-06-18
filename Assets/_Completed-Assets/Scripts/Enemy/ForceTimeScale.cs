using UnityEngine;

public class ForceTimeScale : MonoBehaviour
{
    void Awake()
    {
        // Si algo dejó el juego en pausa, esto lo corrige
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        Time.timeScale = 1f;
    }
}
