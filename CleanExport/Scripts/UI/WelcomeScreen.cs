using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AirplaneSafety.UI
{
    public class WelcomeScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private TextMeshProUGUI welcomeText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 1f;

        private void Start()
        {
            Core.GameManager.Instance.OnStateChanged += OnStateChanged;
            Hide();
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnStateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(Core.GameState state)
        {
            if (state == Core.GameState.Welcome)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Show()
        {
            welcomePanel.SetActive(true);
            welcomeText.text = "Welcome to the Board";
            StartCoroutine(FadeIn());
        }

        private void Hide()
        {
            if (welcomePanel != null)
            {
                StartCoroutine(FadeOut());
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            welcomePanel.SetActive(false);
        }
    }
}
