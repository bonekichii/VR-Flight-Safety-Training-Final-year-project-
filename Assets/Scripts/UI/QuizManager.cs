using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AirplaneSafety.UI
{
    [System.Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public string[] options = new string[4];
        public int correctAnswerIndex;
        public string explanation;
    }

    public class QuizManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button finishButton;

        [Header("Quiz Data")]
        [SerializeField] private List<QuizQuestion> questions = new List<QuizQuestion>();

        private int currentQuestionIndex = 0;
        private int correctAnswers = 0;
        private bool questionAnswered = false;

        private void Start()
        {
            Core.GameManager.Instance.OnStateChanged += OnStateChanged;
            
            // Setup buttons
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i; // Capture for lambda
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextQuestion);
                nextButton.gameObject.SetActive(false);
            }

            if (finishButton != null)
            {
                finishButton.onClick.AddListener(FinishQuiz);
                finishButton.gameObject.SetActive(false);
            }

            // Add sample questions if none exist
            if (questions.Count == 0)
            {
                AddSampleQuestions();
            }

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
            if (state == Core.GameState.Quiz)
            {
                Show();
                StartQuiz();
            }
            else
            {
                Hide();
            }
        }

        private void Show()
        {
            quizPanel.SetActive(true);
        }

        private void Hide()
        {
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }
        }

        private void StartQuiz()
        {
            currentQuestionIndex = 0;
            correctAnswers = 0;
            DisplayQuestion();
        }

        private void DisplayQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                ShowResults();
                return;
            }

            questionAnswered = false;
            QuizQuestion q = questions[currentQuestionIndex];

            questionText.text = q.questionText;
            feedbackText.text = "";

            // Setup answer buttons
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.options.Length && !string.IsNullOrEmpty(q.options[i]))
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.options[i];
                    answerButtons[i].interactable = true;
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            if (nextButton != null) nextButton.gameObject.SetActive(false);
            if (finishButton != null) finishButton.gameObject.SetActive(false);

            UpdateScore();
        }

        private void OnAnswerSelected(int selectedIndex)
        {
            if (questionAnswered) return;

            questionAnswered = true;
            QuizQuestion q = questions[currentQuestionIndex];

            // Disable all buttons
            foreach (var btn in answerButtons)
            {
                btn.interactable = false;
            }

            if (selectedIndex == q.correctAnswerIndex)
            {
                correctAnswers++;
                feedbackText.text = $"<color=green>Correct!</color>\n{q.explanation}";
            }
            else
            {
                feedbackText.text = $"<color=red>Incorrect.</color>\nCorrect answer: {q.options[q.correctAnswerIndex]}\n{q.explanation}";
            }

            UpdateScore();

            // Show next/finish button
            if (currentQuestionIndex < questions.Count - 1)
            {
                if (nextButton != null) nextButton.gameObject.SetActive(true);
            }
            else
            {
                if (finishButton != null) finishButton.gameObject.SetActive(true);
            }
        }

        private void NextQuestion()
        {
            currentQuestionIndex++;
            DisplayQuestion();
        }

        private void ShowResults()
        {
            float percentage = (float)correctAnswers / questions.Count * 100f;
            questionText.text = "Quiz Complete!";
            feedbackText.text = $"You got {correctAnswers} out of {questions.Count} questions correct ({percentage:F0}%)";

            foreach (var btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }

            if (finishButton != null)
            {
                finishButton.gameObject.SetActive(true);
            }
        }

        private void FinishQuiz()
        {
            Core.GameManager.Instance.CompleteQuiz();
        }

        private void UpdateScore()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {correctAnswers}/{questions.Count}";
            }
        }

        private void AddSampleQuestions()
        {
            questions.Add(new QuizQuestion
            {
                questionText = "What should you do first during an emergency landing?",
                options = new string[] { "Take photos", "Fasten seatbelt", "Stand up", "Open window" },
                correctAnswerIndex = 1,
                explanation = "Always fasten your seatbelt first during an emergency."
            });

            questions.Add(new QuizQuestion
            {
                questionText = "Where is the nearest emergency exit located?",
                options = new string[] { "Front only", "Back only", "Multiple locations", "No exits" },
                correctAnswerIndex = 2,
                explanation = "Emergency exits are located at multiple points throughout the aircraft."
            });

            questions.Add(new QuizQuestion
            {
                questionText = "How do you use an oxygen mask?",
                options = new string[] { "Pull down, place over nose and mouth", "Wait for crew", "Share with others first", "Don't use it" },
                correctAnswerIndex = 0,
                explanation = "Pull the mask down, place it over your nose and mouth, and breathe normally."
            });
        }
    }
}
