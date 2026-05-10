#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

namespace AirplaneSafety.Editor
{
    public class AutoSceneBuilder : EditorWindow
    {
        [MenuItem("Airplane Safety/★ BUILD COMPLETE SCENE (Auto)")]
        public static void BuildCompleteScene()
        {
            if (!EditorUtility.DisplayDialog("Auto Build Scene",
                "This will create a FULLY CONFIGURED scene automatically!\n\n" +
                "After this, you only need to:\n" +
                "1. Drag airplane model into scene\n" +
                "2. Position XR Origin inside airplane\n" +
                "3. Add safety video (optional)\n\nContinue?",
                "Yes, Build It!", "Cancel"))
            {
                return;
            }

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Remove default camera
            var defaultCam = Camera.main;
            if (defaultCam != null) DestroyImmediate(defaultCam.gameObject);

            // Build everything
            CreateGameManager();
            CreateXROrigin();
            CreateWelcomeCanvas();
            CreateFloatingCanvas();
            CreateTablet();
            CreateQuizCanvas();
            CreateBoundaries();

            // Save scene
            if (!System.IO.Directory.Exists("Assets/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
            }
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/MainFlight.unity");
            
            EditorUtility.DisplayDialog("✅ Scene Built!", 
                "Complete scene created!\n\n" +
                "Next steps:\n" +
                "1. Drag 'boeing 737-800.glb' into scene\n" +
                "2. Move 'XR Origin' to a seat position\n" +
                "3. Press Play to test!\n\n" +
                "Optional: Add video to Assets/Videos/SafetyDemo.mp4", 
                "Got it!");
        }

        private static void CreateGameManager()
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<Core.GameManager>();
            Debug.Log("✓ GameManager created");
        }

        private static void CreateXROrigin()
        {
            GameObject xrOrigin = new GameObject("XR Origin");
            xrOrigin.transform.position = new Vector3(0, 0, 0);
            
            var cc = xrOrigin.AddComponent<CharacterController>();
            cc.radius = 0.3f;
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.9f, 0);
            
            xrOrigin.AddComponent<Core.PlayerBoundary>();
            
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOrigin.transform);
            
            GameObject mainCam = new GameObject("Main Camera");
            mainCam.tag = "MainCamera";
            mainCam.AddComponent<Camera>();
            mainCam.AddComponent<AudioListener>();
            mainCam.transform.SetParent(cameraOffset.transform);
            mainCam.transform.localPosition = new Vector3(0, 1.6f, 0);
            
            GameObject leftHand = new GameObject("LeftHand Controller");
            leftHand.transform.SetParent(cameraOffset.transform);
            leftHand.transform.localPosition = new Vector3(-0.15f, 1.4f, 0.2f);
            
            GameObject rightHand = new GameObject("RightHand Controller");
            rightHand.transform.SetParent(cameraOffset.transform);
            rightHand.transform.localPosition = new Vector3(0.15f, 1.4f, 0.2f);
            
            Debug.Log("✓ XR Origin created (add XR Interaction components manually for full VR support)");
        }

        private static void CreateWelcomeCanvas()
        {
            GameObject canvasGO = new GameObject("WelcomeCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.position = new Vector3(0, 2, 3);
            canvasRect.sizeDelta = new Vector2(400, 200);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            // Panel
            GameObject panelGO = new GameObject("WelcomePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            CanvasGroup cg = panelGO.AddComponent<CanvasGroup>();
            
            // Text
            GameObject textGO = new GameObject("WelcomeText");
            textGO.transform.SetParent(panelGO.transform, false);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Welcome to the Board";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Add script
            var script = canvasGO.AddComponent<UI.WelcomeScreen>();
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("welcomePanel").objectReferenceValue = panelGO;
            so.FindProperty("welcomeText").objectReferenceValue = text;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedProperties();
            
            Debug.Log("✓ WelcomeCanvas created");
        }

        private static void CreateFloatingCanvas()
        {
            GameObject canvasGO = new GameObject("FloatingCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.position = new Vector3(0, 2.5f, 2);
            canvasRect.sizeDelta = new Vector2(500, 150);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            // Panel
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            // Text
            GameObject textGO = new GameObject("InstructionText");
            textGO.transform.SetParent(panelGO.transform, false);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Instructions will appear here";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.05f);
            textRect.anchorMax = new Vector2(0.95f, 0.95f);
            textRect.sizeDelta = Vector2.zero;
            
            // Add script
            var script = canvasGO.AddComponent<UI.FloatingScreen>();
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("screenPanel").objectReferenceValue = panelGO;
            so.FindProperty("instructionText").objectReferenceValue = text;
            so.ApplyModifiedProperties();
            
            Debug.Log("✓ FloatingCanvas created");
        }

        private static void CreateTablet()
        {
            // Tablet object
            GameObject tabletGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tabletGO.name = "Tablet";
            tabletGO.transform.position = new Vector3(0, 1.2f, 1);
            tabletGO.transform.localScale = new Vector3(0.3f, 0.02f, 0.2f);
            
            var tabletRenderer = tabletGO.GetComponent<MeshRenderer>();
            Material tabletMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tabletMat.color = Color.gray;
            tabletRenderer.material = tabletMat;
            
            // Canvas on tablet
            GameObject canvasGO = new GameObject("TabletCanvas");
            canvasGO.transform.SetParent(tabletGO.transform, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.localPosition = new Vector3(0, 0.015f, 0);
            canvasRect.localRotation = Quaternion.Euler(90, 0, 0);
            canvasRect.sizeDelta = new Vector2(300, 200);
            canvasRect.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            
            // Video display
            GameObject videoGO = new GameObject("VideoDisplay");
            videoGO.transform.SetParent(canvasGO.transform, false);
            RawImage rawImg = videoGO.AddComponent<RawImage>();
            rawImg.color = Color.black;
            RectTransform videoRect = videoGO.GetComponent<RectTransform>();
            videoRect.anchorMin = new Vector2(0.1f, 0.3f);
            videoRect.anchorMax = new Vector2(0.9f, 0.9f);
            videoRect.sizeDelta = Vector2.zero;
            
            // Buttons
            GameObject playBtnGO = CreateButton("PlayButton", "▶ Play", canvasGO.transform);
            RectTransform playRect = playBtnGO.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.1f, 0.05f);
            playRect.anchorMax = new Vector2(0.45f, 0.2f);
            
            GameObject proceedBtnGO = CreateButton("ProceedButton", "Continue →", canvasGO.transform);
            RectTransform proceedRect = proceedBtnGO.GetComponent<RectTransform>();
            proceedRect.anchorMin = new Vector2(0.55f, 0.05f);
            proceedRect.anchorMax = new Vector2(0.9f, 0.2f);
            
            // Video Player
            VideoPlayer vp = tabletGO.AddComponent<VideoPlayer>();
            vp.playOnAwake = false;
            vp.renderMode = VideoRenderMode.RenderTexture;
            RenderTexture rt = new RenderTexture(512, 512, 0);
            vp.targetTexture = rt;
            rawImg.texture = rt;
            
            // Add script
            var script = tabletGO.AddComponent<UI.InteractiveTablet>();
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("videoPlayer").objectReferenceValue = vp;
            so.FindProperty("playButton").objectReferenceValue = playBtnGO.GetComponent<Button>();
            so.FindProperty("proceedButton").objectReferenceValue = proceedBtnGO.GetComponent<Button>();
            so.FindProperty("videoContainer").objectReferenceValue = canvasGO;
            so.ApplyModifiedProperties();
            
            Debug.Log("✓ Interactive Tablet created");
        }

        private static void CreateQuizCanvas()
        {
            GameObject canvasGO = new GameObject("QuizCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.position = new Vector3(0, 2, 2.5f);
            canvasRect.sizeDelta = new Vector2(600, 400);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            // Panel
            GameObject panelGO = new GameObject("QuizPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            // Question Text
            GameObject questionGO = new GameObject("QuestionText");
            questionGO.transform.SetParent(panelGO.transform, false);
            TextMeshProUGUI questionText = questionGO.AddComponent<TextMeshProUGUI>();
            questionText.fontSize = 28;
            questionText.alignment = TextAlignmentOptions.Center;
            questionText.color = Color.white;
            RectTransform qRect = questionGO.GetComponent<RectTransform>();
            qRect.anchorMin = new Vector2(0.05f, 0.7f);
            qRect.anchorMax = new Vector2(0.95f, 0.95f);
            qRect.sizeDelta = Vector2.zero;
            
            // Answer Buttons (4)
            Button[] answerBtns = new Button[4];
            string[] btnNames = { "AnswerA", "AnswerB", "AnswerC", "AnswerD" };
            for (int i = 0; i < 4; i++)
            {
                float yMin = 0.5f - (i * 0.12f);
                float yMax = yMin + 0.1f;
                GameObject btnGO = CreateButton(btnNames[i], $"Option {(char)('A' + i)}", panelGO.transform);
                RectTransform btnRect = btnGO.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.1f, yMin);
                btnRect.anchorMax = new Vector2(0.9f, yMax);
                answerBtns[i] = btnGO.GetComponent<Button>();
            }
            
            // Feedback Text
            GameObject feedbackGO = new GameObject("FeedbackText");
            feedbackGO.transform.SetParent(panelGO.transform, false);
            TextMeshProUGUI feedbackText = feedbackGO.AddComponent<TextMeshProUGUI>();
            feedbackText.fontSize = 20;
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.color = Color.yellow;
            RectTransform fRect = feedbackGO.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.05f, 0.15f);
            fRect.anchorMax = new Vector2(0.95f, 0.3f);
            fRect.sizeDelta = Vector2.zero;
            
            // Score Text
            GameObject scoreGO = new GameObject("ScoreText");
            scoreGO.transform.SetParent(panelGO.transform, false);
            TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0/0";
            scoreText.fontSize = 18;
            scoreText.alignment = TextAlignmentOptions.TopRight;
            scoreText.color = Color.white;
            RectTransform sRect = scoreGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.7f, 0.9f);
            sRect.anchorMax = new Vector2(0.95f, 0.98f);
            sRect.sizeDelta = Vector2.zero;
            
            // Next/Finish Buttons
            GameObject nextBtnGO = CreateButton("NextButton", "Next →", panelGO.transform);
            RectTransform nextRect = nextBtnGO.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(0.55f, 0.02f);
            nextRect.anchorMax = new Vector2(0.75f, 0.12f);
            
            GameObject finishBtnGO = CreateButton("FinishButton", "Finish", panelGO.transform);
            RectTransform finishRect = finishBtnGO.GetComponent<RectTransform>();
            finishRect.anchorMin = new Vector2(0.78f, 0.02f);
            finishRect.anchorMax = new Vector2(0.95f, 0.12f);
            
            // Add script
            var script = canvasGO.AddComponent<UI.QuizManager>();
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("quizPanel").objectReferenceValue = panelGO;
            so.FindProperty("questionText").objectReferenceValue = questionText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            so.FindProperty("scoreText").objectReferenceValue = scoreText;
            so.FindProperty("nextButton").objectReferenceValue = nextBtnGO.GetComponent<Button>();
            so.FindProperty("finishButton").objectReferenceValue = finishBtnGO.GetComponent<Button>();
            
            SerializedProperty answerBtnsArray = so.FindProperty("answerButtons");
            answerBtnsArray.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                answerBtnsArray.GetArrayElementAtIndex(i).objectReferenceValue = answerBtns[i];
            }
            so.ApplyModifiedProperties();
            
            Debug.Log("✓ QuizCanvas created");
        }

        private static GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            
            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.7f, 1f);
            
            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.2f, 0.4f, 0.7f, 1f);
            cb.highlightedColor = new Color(0.3f, 0.5f, 0.8f, 1f);
            cb.pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
            btn.colors = cb;
            
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI tmpText = textGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 18;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            return btnGO;
        }

        private static void CreateBoundaries()
        {
            GameObject boundaries = new GameObject("Boundaries");
            
            Vector3[] positions = {
                new Vector3(0, 1, 5),   // Front
                new Vector3(0, 1, -5),  // Back
                new Vector3(-2, 1, 0),  // Left
                new Vector3(2, 1, 0)    // Right
            };
            
            Vector3[] scales = {
                new Vector3(4, 3, 0.1f),  // Front
                new Vector3(4, 3, 0.1f),  // Back
                new Vector3(0.1f, 3, 10), // Left
                new Vector3(0.1f, 3, 10)  // Right
            };
            
            string[] names = { "FrontWall", "BackWall", "LeftWall", "RightWall" };
            
            for (int i = 0; i < 4; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = names[i];
                wall.transform.SetParent(boundaries.transform);
                wall.transform.position = positions[i];
                wall.transform.localScale = scales[i];
                
                // Make invisible but keep collider
                var renderer = wall.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false; // Invisible but still has collider
                }
            }
            
            Debug.Log("✓ Boundary walls created");
        }

        [MenuItem("Airplane Safety/Fix Materials (URP)")]
        public static void FixMaterials()
        {
            string[] guids = AssetDatabase.FindAssets("t:Material");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat != null && mat.shader != null)
                {
                    string shaderName = mat.shader.name;
                    if (shaderName.Contains("Standard") || shaderName.Contains("Legacy") || shaderName == "HDRP/Lit")
                    {
                        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                        if (urpLit != null)
                        {
                            mat.shader = urpLit;
                            count++;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("✅ Materials Fixed", 
                $"Upgraded {count} materials to URP Lit shader.\n\nYour airplane should no longer be blue!", 
                "OK");
        }
    }
}
#endif
