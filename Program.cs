using StereoKit;
using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using OpenCvSharp;
using NAudio.Wave;
using NAudio.CoreAudioApi;
namespace SafetyDemo;

enum AppState { Welcome, Video, Quiz, Finished }

class Program
{
    static AppState currentState = AppState.Welcome;
    static Model? airplane = null;
    static Pose quizScreenPose = new Pose(new Vec3(0, -0.8f, -4.0f), Quat.FromAngles(0, 180, 0));
    static Pose quizUiPose = new Pose(new Vec3(0, -0.8f, -2.05f), Quat.LookDir(0, 0, 1));
    static Vec3 quizScreenScale = new Vec3(0.9f, 0.55f, 0.04f);
    static Vec3 targetSeatPosition = new Vec3(0.5f, -1.0f, -4.5f);

    // Background Music
    static Sound? backgroundMusic = null;
    static SoundInst musicInstance;

    // TTS
    static SpeechSynthesizer? tts = null;
    static bool welcomePlayed = false;

    // Video Playback
    static VideoCapture? videoCapture = null;
    static Tex? videoTexture = null;
    static Mat? videoFrame = null;
    static bool videoPlaying = false;
    static float videoTimer = 0f;
    static float videoFPS = 30f;
    static Pose videoScreenPose = new Pose(new Vec3(0, -0.8f, -2.0f), Quat.LookDir(0, 0, 1));
    static Vec3 videoScreenScale = new Vec3(0.8f, 0.45f, 0.02f);
    static IWavePlayer? audioPlayer = null;
    static AudioFileReader? audioFile = null;
    static bool videoFinished = false;

    // Quiz Data
    static int quizIndex = 0;
    static int score = 0;
    static int? selectedAnswer = null;
    static bool showingFeedback = false;
    
    static List<QuizQuestion> questions = new List<QuizQuestion>() {
        new QuizQuestion(
            "When should you inflate your life vest?",
            new string[]{"Inside the plane", "Outside the plane"},
            1,
            "Always inflate your life vest AFTER exiting the aircraft."
        ),
        new QuizQuestion(
            "Where is the oxygen mask located?",
            new string[]{"Under the seat", "Overhead compartment", "Seat pocket"},
            1,
            "Oxygen masks are stored in overhead compartments above your seat."
        ),
        new QuizQuestion(
            "What is the correct brace position?",
            new string[]{"Head down, hands behind head", "Arms crossed on chest", "Hands on armrests"},
            0,
            "The brace position is head down with hands behind your head."
        ),
        new QuizQuestion(
            "How many emergency exits are typically on a Boeing 737?",
            new string[]{"4 exits", "6 exits", "8 exits"},
            2,
            "A Boeing 737 typically has 8 emergency exits."
        ),
        new QuizQuestion(
            "What should you do first in case of cabin smoke?",
            new string[]{"Stand up immediately", "Stay low and breathe through cloth", "Open a window"},
            1,
            "Smoke rises, so stay low where the air is clearer."
        ),
        new QuizQuestion(
            "When can you use electronic devices during flight?",
            new string[]{"Anytime", "Only in airplane mode", "Never"},
            1,
            "Electronic devices must be in airplane mode during flight."
        ),
        new QuizQuestion(
            "What does the seatbelt sign indicate?",
            new string[]{"Turbulence expected", "Remain seated with belt fastened", "Both A and B"},
            2,
            "The seatbelt sign means you must remain seated with your seatbelt fastened."
        ),
        new QuizQuestion(
            "How do you open an emergency exit door?",
            new string[]{"Pull the handle up", "Push it outward", "Follow the instructions on the door"},
            2,
            "Emergency exit doors have specific instructions printed on them."
        ),
        new QuizQuestion(
            "What should you leave behind during evacuation?",
            new string[]{"Nothing", "Carry-on luggage", "Shoes"},
            1,
            "NEVER take carry-on luggage during evacuation."
        ),
        new QuizQuestion(
            "How long does oxygen flow from overhead masks?",
            new string[]{"5 minutes", "12-15 minutes", "30 minutes"},
            1,
            "Oxygen masks provide 12-15 minutes of oxygen."
        ),
        new QuizQuestion(
            "What is the safest seat position during turbulence?",
            new string[]{"Reclined back", "Upright with seatbelt fastened", "Leaning forward"},
            1,
            "Keep your seat upright and seatbelt fastened during turbulence."
        ),
        new QuizQuestion(
            "Where should you place your carry-on during takeoff?",
            new string[]{"On your lap", "Under the seat in front", "Between your feet"},
            1,
            "Carry-on items must be stored under the seat in front of you or in overhead bins."
        )
    };


    static void Main(string[] args)
    {
        SKSettings settings = new SKSettings
        {
            appName = "Airplane Safety Training",
            assetsFolder = "Assets",
            mode = AppMode.XR
        };

        if (!SK.Initialize(settings))
            Environment.Exit(1);

        try { airplane = Model.FromFile("airplane.glb"); } catch { Log.Warn("Airplane model not found!"); }
        
        GenerateBackgroundMusic();
        InitializeVideo();
        InitializeTTS();

        
        while (SK.Step(() =>
        {
            if (airplane != null) airplane.Draw(Matrix.TRS(new Vec3(0, -2, 0), Quat.Identity, 1));
            HandleMovement();
            HandleBackgroundMusic();

            switch (currentState)
            {
                case AppState.Welcome:
                    ShowWelcomeScreen();
                    break;
                    
                case AppState.Video:
                    if (!videoFinished)
                    {
                        UpdateVideoFrame();
                        DrawVideoScreen();
                    }
                    else
                    {
                        currentState = AppState.Quiz;
                    }
                    break;
                    
                case AppState.Quiz:
                    ShowQuizUI();
                    break;
                    
                case AppState.Finished:
                    ShowFinishedUI();
                    break;
            }
        }));

        SK.Shutdown();
    }

    static void InitializeTTS()
    {
        try
        {
            tts = new SpeechSynthesizer();
            
            // Try to set output to VR headset audio device
            try
            {
                tts.SetOutputToDefaultAudioDevice();
                Log.Info("TTS output set to default audio device");
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not set TTS output device: {ex.Message}");
            }
            
            try
            {
                tts.SelectVoiceByHints(VoiceGender.Female);
            }
            catch
            {
                Log.Warn("Female voice not available, using default");
            }
            
            tts.Volume = 100;
            tts.Rate = 0;
            Log.Info("TTS initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Err($"TTS initialization failed: {ex.Message}");
        }
    }

    static int FindVRWaveOutDeviceNumber()
    {
        try
        {
            int vrDeviceNumber = -1;
            Log.Info($"Enumerating {WaveOut.DeviceCount} WaveOut audio devices:");
            
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                Log.Info($"  WaveOut device {i}: {caps.ProductName}");
                string name = caps.ProductName.ToLower();
                if (name.Contains("oculus") || name.Contains("meta") || 
                    name.Contains("quest") || name.Contains("rift"))
                {
                    vrDeviceNumber = i;
                    Log.Info($"  -> Matched as VR audio device at index {i}!");
                }
            }
            
            // Also log WASAPI devices for diagnostics
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                Log.Info($"WASAPI devices ({devices.Count}):");
                foreach (var device in devices)
                    Log.Info($"  WASAPI: {device.FriendlyName}");
            }
            catch { }
            
            if (vrDeviceNumber < 0)
                Log.Warn("No VR audio device found in WaveOut device list");
            
            return vrDeviceNumber;
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to enumerate audio devices: {ex.Message}");
            return -1;
        }
    }

    static void ShowWelcomeScreen()
    {
        Pose welcomePose = new Pose(new Vec3(0, -0.8f, -2.0f), Quat.LookDir(0, 0, 1));
        
        UI.WindowBegin("Welcome", ref welcomePose, new Vec2(50, 40) * U.cm);
        
        UI.Text("Airplane Safety Training", TextAlign.Center);
        UI.HSeparator();
        UI.Text("Welcome aboard!", TextAlign.Center);
        UI.Text("This VR experience will teach you", TextAlign.Center);
        UI.Text("important airplane safety procedures.", TextAlign.Center);
        UI.HSeparator();
        
        if (UI.Button("Start Training", new Vec2(40, 0) * U.cm))
        {
            // Play TTS announcement immediately
            if (!welcomePlayed && tts != null)
            {
                welcomePlayed = true;
                try
                {
                    Log.Info("Playing TTS announcement...");
                    tts.SpeakAsync("Welcome to the Airlines. Please watch the safety video carefully.");
                }
                catch (Exception ex)
                {
                    Log.Err($"TTS failed: {ex.Message}");
                }
            }
            
            currentState = AppState.Video;
            
            // Start video playback
            if (audioPlayer != null && videoCapture != null)
            {
                try
                {
                    // Reset audio position to start
                    if (audioFile != null)
                    {
                        audioFile.Position = 0;
                        Log.Info($"Audio position reset to 0");
                    }
                    
                    videoPlaying = true;
                    audioPlayer.Play();
                    Log.Info($"Video audio started - State: {audioPlayer.PlaybackState}, Volume: {audioPlayer.Volume}");
                }
                catch (Exception ex)
                {
                    Log.Err($"Failed to start video playback: {ex.Message}");
                }
            }
            else
            {
                Log.Warn($"Cannot start video - audioPlayer: {audioPlayer != null}, videoCapture: {videoCapture != null}");
                if (audioPlayer == null) Log.Err("Audio player is NULL!");
                if (videoCapture == null) Log.Err("Video capture is NULL!");
            }
        }
        
        UI.WindowEnd();
    }

    static void ShowQuizUI()
    {
        if (quizIndex >= questions.Count)
        {
            currentState = AppState.Finished;
            return;
        }

        var q = questions[quizIndex];

        UI.WindowBegin("Quiz Window", ref quizUiPose, new Vec2(60, 40) * U.cm);

        UI.Text(q.Text, TextAlign.TopLeft);
        UI.HSeparator();

        if (showingFeedback)
        {
            for (int i = 0; i < q.Options.Length; i++)
            {
                if (i == q.CorrectIdx)
                    UI.PushTint(new Color(0, 0.8f, 0));
                else if (i == selectedAnswer)
                    UI.PushTint(new Color(1, 0, 0));
                else
                    UI.PushTint(new Color(0.5f, 0.5f, 0.5f));

                UI.Button(q.Options[i]);
                UI.PopTint();
            }

            UI.HSeparator();
            UI.Text(q.Explanation, TextAlign.TopLeft);
            UI.HSeparator();

            if (UI.Button("Next Question ->"))
            {
                showingFeedback = false;
                selectedAnswer = null;
                quizIndex++;
                if (quizIndex >= questions.Count)
                    currentState = AppState.Finished;
            }
        }
        else
        {
            for (int i = 0; i < q.Options.Length; i++)
            {
                if (UI.Button(q.Options[i]))
                {
                    selectedAnswer = i;
                    showingFeedback = true;
                    if (selectedAnswer == q.CorrectIdx)
                        score++;
                }
            }
        }

        UI.WindowEnd();
    }

    static void ShowFinishedUI()
    {
        UI.WindowBegin("Finished Window", ref quizUiPose, new Vec2(60, 40) * U.cm);
        
        UI.Text($"Score: {score} / {questions.Count}", TextAlign.Center);
        UI.HSeparator();

        if (score == questions.Count)
            UI.Text("PERFECT SCORE!\nYou are ready to fly.", TextAlign.Center);
        else
            UI.Text("Please review the safety card.\nSafety is our top priority.", TextAlign.Center);

        UI.HSeparator();
        
        if (UI.Button("Restart Training"))
        {
            quizIndex = 0;
            score = 0;
            videoFinished = false;
            videoPlaying = false;
            welcomePlayed = false;
            currentState = AppState.Welcome;
            
            if (videoCapture != null)
                videoCapture.Set(VideoCaptureProperties.PosFrames, 0);
            if (audioFile != null)
                audioFile.Position = 0;
        }
        UI.WindowEnd();
    }

    static List<SeatCollider> seatColliders = BuildSeatColliders();

    static float playerYaw = 0f;
    static float playerPitch = 0f;
    static Vec3 playerPosition = new Vec3(0, 1.2f, -5);
    static float playerHeight = 1.2f;
    static bool isSeated = true;

    static Vec3 GetControllerMovement(Vec3 forward, Vec3 right)
    {
        Vec2 input = Vec2.Zero;
        Controller leftController = Input.Controller(Handed.Left);
        Controller rightController = Input.Controller(Handed.Right);

        if (leftController.IsTracked)
            input += leftController.stick;

        if (rightController.IsTracked)
            input += rightController.stick;

        if (input.Length > 1)
            input = input.Normalized;

        return (forward * input.y) + (right * input.x);
    }

    static void HandleMovement()
    {
        float rotSpeed = 120f * Time.Stepf;
        
        if (Input.Key(Key.Left).IsActive())  playerYaw -= rotSpeed;
        if (Input.Key(Key.Right).IsActive()) playerYaw += rotSpeed;
        if (Input.Key(Key.Up).IsActive())    playerPitch += rotSpeed;
        if (Input.Key(Key.Down).IsActive())  playerPitch -= rotSpeed;
        playerPitch = Math.Clamp(playerPitch, -80, 80);

        if (Input.Key(Key.Shift).IsJustActive())
        {
            isSeated = !isSeated;
            playerHeight = isSeated ? 1.2f : 0.6f;
        }

        float speed = 2.0f * Time.Stepf;
        float yawRad = playerYaw * (float)Math.PI / 180f;
        Vec3 forward = new Vec3((float)Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
        Vec3 right = new Vec3((float)Math.Cos(yawRad), 0, -(float)Math.Sin(yawRad));
        
        Vec3 keyboardMove = Vec3.Zero;
        if (Input.Key(Key.W).IsActive()) keyboardMove += forward;
        if (Input.Key(Key.S).IsActive()) keyboardMove -= forward;
        if (Input.Key(Key.D).IsActive()) keyboardMove += right;
        if (Input.Key(Key.A).IsActive()) keyboardMove -= right;

        if (keyboardMove.Length > 0.01f)
            keyboardMove = keyboardMove.Normalized;
        else
            keyboardMove = Vec3.Zero;

        Vec3 controllerMove = GetControllerMovement(forward, right);
        Vec3 moveInput = keyboardMove + controllerMove;

        Vec3 newPos = playerPosition;
        if (moveInput.Length > 0.01f)
            newPos += moveInput * speed;
        newPos.y = playerHeight;

        float cabinHalfWidth = 1.2f;
        float cabinFront = 10f;
        float cabinBack = -10f;
        
        newPos.x = Math.Clamp(newPos.x, -cabinHalfWidth, cabinHalfWidth);
        newPos.z = Math.Clamp(newPos.z, cabinBack, cabinFront);

        if (!IsInsideSeatCollision(newPos))
            playerPosition = newPos;

        Quat rotation = Quat.FromAngles(playerPitch, playerYaw, 0);
        Matrix cameraTransform = Matrix.TR(playerPosition, rotation);
        Renderer.CameraRoot = cameraTransform.Inverse;

        if (Input.Key(Key.Space).IsJustActive())
        {
            playerPosition = new Vec3(0, 1.2f, -5);
            playerYaw = 0;
            playerPitch = 0;
            isSeated = true;
            playerHeight = 1.2f;
        }

        Text.Add("Arrow Keys: Look | WASD: Move | Shift: Stand/Sit | Space: Reset", 
                 Matrix.T(playerPosition + new Vec3(0, 0.4f, -0.8f)), TextAlign.Center);
    }

    static void GenerateBackgroundMusic()
    {
        int sampleRate = 48000;
        int duration = 30;
        float[] samples = new float[sampleRate * duration];
        
        float[] melodyNotes = { 261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 493.88f, 523.25f };
        int[] melodyPattern = { 0, 2, 4, 2, 0, 2, 4, 7, 4, 2, 0, 4, 2, 0 };
        float noteLength = duration / (float)melodyPattern.Length;
        
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / (float)sampleRate;
            
            float bass1 = (float)Math.Sin(t * 65.41f * 2 * Math.PI) * 0.15f;
            float bass2 = (float)Math.Sin(t * 98.00f * 2 * Math.PI) * 0.12f;
            float pad1 = (float)Math.Sin(t * 130.81f * 2 * Math.PI) * 0.12f;
            float pad2 = (float)Math.Sin(t * 164.81f * 2 * Math.PI) * 0.10f;
            float pad3 = (float)Math.Sin(t * 196.00f * 2 * Math.PI) * 0.10f;
            
            int noteIndex = (int)(t / noteLength) % melodyPattern.Length;
            float melodyFreq = melodyNotes[melodyPattern[noteIndex]];
            float noteTime = (t % noteLength) / noteLength;
            float melodyEnvelope = (float)Math.Sin(noteTime * Math.PI);
            float melody = (float)Math.Sin(t * melodyFreq * 2 * Math.PI) * 0.18f * melodyEnvelope;
            
            float shimmerMod = (float)Math.Sin(t * 0.4f * 2 * Math.PI) * 0.3f + 0.7f;
            float sparkle1 = (float)Math.Sin(t * 523.25f * 2 * Math.PI) * 0.08f * shimmerMod;
            float sparkle2 = (float)Math.Sin(t * 659.25f * 2 * Math.PI) * 0.06f * shimmerMod;
            float chorus1 = (float)Math.Sin(t * 262.00f * 2 * Math.PI) * 0.05f;
            float chorus2 = (float)Math.Sin(t * 330.00f * 2 * Math.PI) * 0.04f;
            
            float fadeWindow = 3.0f;
            float fadeIn = Math.Min(1.0f, t / fadeWindow);
            float fadeOut = Math.Min(1.0f, (duration - t) / fadeWindow);
            float masterEnvelope = Math.Min(fadeIn, fadeOut);
            
            float mixedSample = (bass1 + bass2 + pad1 + pad2 + pad3 + melody + 
                                sparkle1 + sparkle2 + chorus1 + chorus2) * masterEnvelope;
            
            samples[i] = Math.Clamp(mixedSample * 0.7f, -0.95f, 0.95f);
        }
        
        backgroundMusic = Sound.FromSamples(samples);
        Log.Info("Background music generated");
    }

    static void HandleBackgroundMusic()
    {
        if (backgroundMusic == null) return;
        if (videoPlaying) return;
        
        if (!musicInstance.IsPlaying)
            musicInstance = backgroundMusic.Play(Vec3.Zero, 0.6f);
    }

    static void InitializeVideo()
    {
        try
        {
            string videoPath = "Assets/Our New No-Nonsense Safety Video Emirates.mp4";
            videoCapture = new VideoCapture(videoPath);
            
            if (!videoCapture.IsOpened())
            {
                Log.Err("Failed to open video file");
                return;
            }
            
            int width = (int)videoCapture.Get(VideoCaptureProperties.FrameWidth);
            int height = (int)videoCapture.Get(VideoCaptureProperties.FrameHeight);
            videoFPS = (float)videoCapture.Get(VideoCaptureProperties.Fps);
            if (videoFPS <= 0) videoFPS = 30f;
            
            videoTexture = new Tex(TexType.Image, TexFormat.Rgba32);
            videoFrame = new Mat();
            
            // Initialize audio
            try
            {
                Log.Info($"Initializing audio from: {videoPath}");
                audioFile = new AudioFileReader(videoPath);
                
                Log.Info($"Audio file opened - Length: {audioFile.Length}, WaveFormat: {audioFile.WaveFormat}");
                Log.Info($"Audio Duration: {audioFile.TotalTime}, Sample Rate: {audioFile.WaveFormat.SampleRate}, Channels: {audioFile.WaveFormat.Channels}");
                
                // Find VR headset audio device and route audio there
                int vrDeviceNum = FindVRWaveOutDeviceNumber();
                var waveOut = new WaveOutEvent();
                if (vrDeviceNum >= 0)
                {
                    waveOut.DeviceNumber = vrDeviceNum;
                    Log.Info($"Audio routed to VR headset via WaveOut device #{vrDeviceNum}");
                }
                else
                {
                    Log.Warn("No VR audio device found, falling back to default PC speakers");
                }
                audioPlayer = waveOut;
                
                audioPlayer.Init(audioFile);
                audioPlayer.Volume = 1.0f;
                
                Log.Info($"Video audio initialized successfully - Duration: {audioFile.TotalTime}");
                Log.Info($"Audio player state after init: {audioPlayer.PlaybackState}");
            }
            catch (Exception ex)
            {
                Log.Err($"Video audio init failed: {ex.Message}");
                Log.Err($"Stack trace: {ex.StackTrace}");
                audioPlayer = null;
                audioFile = null;
            }
            
            videoPlaying = false;
            Log.Info($"Video initialized: {width}x{height} @ {videoFPS} FPS");
        }
        catch (Exception ex)
        {
            Log.Err($"Video initialization failed: {ex.Message}");
        }
    }

    static void UpdateVideoFrame()
    {
        if (!videoPlaying || videoCapture == null || videoTexture == null || videoFrame == null) return;
        
        videoTimer += Time.Stepf;
        float frameDuration = 1.0f / videoFPS;
        
        if (videoTimer >= frameDuration)
        {
            videoTimer -= frameDuration;
            
            if (videoCapture.Read(videoFrame) && !videoFrame.Empty())
            {
                Mat rgbaFrame = new Mat();
                Cv2.CvtColor(videoFrame, rgbaFrame, ColorConversionCodes.BGR2RGBA);
                
                int width = rgbaFrame.Width;
                int height = rgbaFrame.Height;
                byte[] frameData = new byte[width * height * 4];
                System.Runtime.InteropServices.Marshal.Copy(rgbaFrame.Data, frameData, 0, frameData.Length);
                
                videoTexture.SetColors(width, height, frameData);
                rgbaFrame.Dispose();
            }
            else
            {
                videoPlaying = false;
                videoFinished = true;
                if (audioPlayer != null) audioPlayer.Stop();
                Log.Info("Video finished");
            }
        }
    }

    static void ToggleVideoPlayback()
    {
        if (videoCapture == null) return;

        videoPlaying = !videoPlaying;

        if (videoPlaying)
        {
            if (audioPlayer != null)
            {
                try
                {
                    audioPlayer.Play();
                    Log.Info("Video audio playing");
                }
                catch (Exception ex)
                {
                    Log.Err($"Audio play failed: {ex.Message}");
                }
            }
        }
        else
        {
            if (audioPlayer != null)
            {
                audioPlayer.Pause();
                Log.Info("Video audio paused");
            }
        }
    }

    static void SkipVideo()
    {
        if (videoCapture == null) return;
        
        if (audioPlayer != null) audioPlayer.Stop();
        videoPlaying = false;
        videoFinished = true;
        Log.Info("Video skipped");
    }

    static void DrawVideoScreen()
    {
        if (videoTexture == null || videoFinished) return;
        
        Mesh.Cube.Draw(Material.Default,
            Matrix.TRS(videoScreenPose.position, videoScreenPose.orientation, videoScreenScale),
            new Color(0.05f, 0.05f, 0.05f));
        
        Material videoMat = Material.Default.Copy();
        videoMat[MatParamName.DiffuseTex] = videoTexture;
        
        Vec3 videoPos = videoScreenPose.position + videoScreenPose.orientation * Vec3.Forward * 0.015f;
        Vec3 videoScale = new Vec3(videoScreenScale.x * 0.98f, videoScreenScale.y * 0.98f, 0.001f);
        
        Mesh.Quad.Draw(videoMat,
            Matrix.TRS(videoPos, videoScreenPose.orientation, videoScale));

        Pose controlPose = new Pose(
            videoScreenPose.position + new Vec3(0, -0.35f, 0),
            videoScreenPose.orientation
        );
        
        UI.WindowBegin("Video Controls", ref controlPose, new Vec2(30, 10) * U.cm, UIWin.Empty);
        
        if (!videoPlaying)
        {
            if (UI.Button("▶ Play Video"))
                ToggleVideoPlayback();
        }
        else
        {
            if (UI.Button("⏸ Pause"))
                ToggleVideoPlayback();
        }
        
        UI.SameLine();
        
        if (UI.Button("⏭ Skip"))
            SkipVideo();
        
        UI.WindowEnd();
    }

    static List<SeatCollider> BuildSeatColliders()
    {
        List<SeatCollider> seats = new List<SeatCollider>();
        float rowSpacing = 1.3f;
        int rowCount = 12;
        float firstRowZ = -7f;
        float seatOffsetX = 0.8f;
        Vec3 halfExtents = new Vec3(0.45f, 0.8f, 0.55f);

        for (int i = 0; i < rowCount; i++)
        {
            float z = firstRowZ + i * rowSpacing;
            seats.Add(new SeatCollider(new Vec3(-seatOffsetX, 0.8f, z), halfExtents));
            seats.Add(new SeatCollider(new Vec3(seatOffsetX, 0.8f, z), halfExtents));
        }

        return seats;
    }

    static bool IsInsideSeatCollision(Vec3 position)
    {
        foreach (SeatCollider seat in seatColliders)
        {
            if (seat.Contains(position))
                return true;
        }
        return false;
    }
}

struct SeatCollider
{
    public Vec3 Center;
    public Vec3 HalfExtents;

    public SeatCollider(Vec3 center, Vec3 halfExtents)
    {
        Center = center;
        HalfExtents = halfExtents;
    }

    public bool Contains(Vec3 point)
    {
        return Math.Abs(point.x - Center.x) <= HalfExtents.x &&
               Math.Abs(point.y - Center.y) <= HalfExtents.y &&
               Math.Abs(point.z - Center.z) <= HalfExtents.z;
    }
}

class QuizQuestion
{
    public string Text;
    public string[] Options;
    public int CorrectIdx;
    public string Explanation;

    public QuizQuestion(string text, string[] options, int correct, string explanation)
    {
        Text = text;
        Options = options;
        CorrectIdx = correct;
        Explanation = explanation;
    }
}
