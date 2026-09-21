using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeFruitPiece : MonoBehaviour
{
    public float delayBeforeFade = 0.5f;
    public float fadeDuration = 2f;

    private readonly List<Material> materials = new();
    private readonly List<Color> originalColors = new();

    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");

    void Start()
    {
        foreach (Renderer pieceRenderer
                 in GetComponentsInChildren<Renderer>())
        {
            // Create separate material instances for this piece.
            foreach (Material material in pieceRenderer.materials)
            {
                materials.Add(material);

                originalColors.Add(
                    material.HasProperty(BaseColor)
                        ? material.GetColor(BaseColor)
                        : Color.white
                );
            }
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, delayBeforeFade)
        );

        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float opacity = 1f - Mathf.Clamp01(
                elapsed / duration
            );

            for (int i = 0; i < materials.Count; i++)
            {
                Material material = materials[i];

                if (material == null ||
                    !material.HasProperty(BaseColor))
                    continue;

                Color color = originalColors[i];
                color.a *= opacity;
                material.SetColor(BaseColor, color);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Clean up the material instances created for this piece.
        foreach (Material material in materials)
        {
            if (material != null)
                Destroy(material);
        }
    }
}