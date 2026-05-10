using UnityEngine;
using TMPro;

namespace AirplaneSafety.UI
{
    public class FloatingScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject screenPanel;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Animation Settings")]
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private bool rotateTowardsPlayer = true;

        private Vector3 startPosition;
        private Transform playerCamera;

        private void Start()
        {
            Core.GameManager.Instance.OnStateChanged += OnStateChanged;
            startPosition = transform.position;
            
            // Find main camera (player's view)
            playerCamera = Camera.main?.transform;
            
            Hide();
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnStateChanged -= OnStateChanged;
            }
        }

        private void Update()
        {
            if (!screenPanel.activeSelf) return;

            // Floating animation
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Face player
            if (rotateTowardsPlayer && playerCamera != null)
            {
                Vector3 directionToPlayer = playerCamera.position - transform.position;
                directionToPlayer.y = 0; // Keep screen upright
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
                }
            }
        }

        private void OnStateChanged(Core.GameState state)
        {
            switch (state)
            {
                case Core.GameState.Briefing:
                    Show("Please watch the safety demonstration on the tablet in front of you.");
                    break;
                case Core.GameState.Quiz:
                    Show("Complete the safety quiz to finish your training.");
                    break;
                default:
                    Hide();
                    break;
            }
        }

        private void Show(string message)
        {
            screenPanel.SetActive(true);
            instructionText.text = message;
        }

        private void Hide()
        {
            if (screenPanel != null)
            {
                screenPanel.SetActive(false);
            }
        }
    }
}
