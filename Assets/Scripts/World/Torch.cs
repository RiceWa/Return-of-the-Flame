using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour
{
    [SerializeField] private Light2D torchLight; // The light that turns on when the torch is lit
    [SerializeField] private SpriteRenderer torchSprite; // The visible torch sprite
    [SerializeField] private Color unlitColor = Color.gray; // Color used before the torch is lit
    [SerializeField] private Color litColor = Color.white; // Color used after the torch is lit

    private bool isLit = false; // Prevents the torch from being lit multiple times

    private void Start()
    {
        // Make sure the torch starts turned off
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }

        // Make the torch look unlit at the start
        if (torchSprite != null)
        {
            torchSprite.color = unlitColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react if the player touches the torch and it is not already lit
        if (!isLit && other.CompareTag("Player"))
        {
            LightTorch();
        }
    }

    private void LightTorch()
    {
        isLit = true;

        // Turn on the light
        if (torchLight != null)
        {
            torchLight.enabled = true;
        }

        // Change the torch sprite so it looks lit
        if (torchSprite != null)
        {
            torchSprite.color = litColor;
        }
    }
}