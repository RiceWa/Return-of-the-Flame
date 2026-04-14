using UnityEngine;

public class ExitGate : MonoBehaviour
{
    [SerializeField] private EndSequence endSequence;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        // Stop timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }

        // Stop player movement script
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Stop player physics
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Play ending
        if (endSequence != null && GameTimer.Instance != null)
        {
            endSequence.PlayEndSequence(GameTimer.Instance.FinalTime);
        }
    }
}