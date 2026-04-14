using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour
{
    [SerializeField] private Light2D torchLight;
    [SerializeField] private SpriteRenderer torchSprite;
    [SerializeField] private Color unlitColor = Color.gray;
    [SerializeField] private Color litColor = Color.white;
    [SerializeField] private bool startLit = false;
    [SerializeField] private AudioClip torchClip;

    private AudioSource audioSource;
    private bool isLit = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        isLit = startLit;

        if (torchLight != null)
        {
            torchLight.enabled = startLit;
        }

        if (torchSprite != null)
        {
            torchSprite.color = startLit ? litColor : unlitColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLit && other.CompareTag("Player"))
        {
            LightTorch();
        }
    }

    private void LightTorch()
    {
        isLit = true;

        if (torchLight != null)
        {
            torchLight.enabled = true;
        }

        if (torchSprite != null)
        {
            torchSprite.color = litColor;
        }

        if (audioSource != null && torchClip != null)
        {
            audioSource.PlayOneShot(torchClip);
        }
    }
}