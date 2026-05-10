using UnityEngine;
using System;

namespace AirplaneSafety.Core
{
    public enum GameState
    {
        Welcome,
        Briefing,
        Quiz,
        Completed
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State Management")]
        [SerializeField] private GameState currentState = GameState.Welcome;
        
        [Header("Timing")]
        [SerializeField] private float welcomeDuration = 5f;

        public event Action<GameState> OnStateChanged;
        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            ChangeState(GameState.Welcome);
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            OnStateChanged?.Invoke(newState);

            // Auto-transitions
            switch (newState)
            {
                case GameState.Welcome:
                    Invoke(nameof(StartBriefing), welcomeDuration);
                    break;
            }
        }

        private void StartBriefing()
        {
            ChangeState(GameState.Briefing);
        }

        public void StartQuiz()
        {
            ChangeState(GameState.Quiz);
        }

        public void CompleteQuiz()
        {
            ChangeState(GameState.Completed);
        }
    }
}
