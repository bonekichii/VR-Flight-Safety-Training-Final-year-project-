using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace AirplaneSafety.UI
{
    public class InteractiveTablet : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private string videoPath = "Assets/Videos/SafetyDemo.mp4";
        
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button proceedButton;
        [SerializeField] private GameObject videoContainer;

        private bool videoCompleted = false;

        private void Start()
        {
            Core.GameManager.Instance.OnStateChanged += OnStateChanged;
            
            // Setup video player
            if (videoPlayer != null && !string.IsNullOrEmpty(videoPath))
            {
                videoPlayer.url = System.IO.Path.Combine(Application.dataPath, videoPath.Replace("Assets/", ""));
                videoPlayer.loopPointReached += OnVideoFinished;
            }

            // Setup buttons
            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayVideo);
            }

            if (proceedButton != null)
            {
                proceedButton.onClick.AddListener(ProceedToQuiz);
                proceedButton.gameObject.SetActive(false);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnStateChanged -= OnStateChanged;
            }

            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }

        private void OnStateChanged(Core.GameState state)
        {
            if (state == Core.GameState.Briefing)
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
            videoContainer.SetActive(true);
            videoCompleted = false;
            
            if (proceedButton != null)
            {
                proceedButton.gameObject.SetActive(false);
            }
        }

        private void Hide()
        {
            if (videoContainer != null)
            {
                videoContainer.SetActive(false);
            }
        }

        private void PlayVideo()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Play();
                Debug.Log("[InteractiveTablet] Playing safety video");
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            videoCompleted = true;
            Debug.Log("[InteractiveTablet] Video finished");
            
            if (proceedButton != null)
            {
                proceedButton.gameObject.SetActive(true);
            }
        }

        private void ProceedToQuiz()
        {
            if (videoCompleted)
            {
                Core.GameManager.Instance.StartQuiz();
            }
        }
    }
}
