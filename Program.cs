using StereoKit;
using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using OpenCvSharp;
using NAudio.Wave;
using NAudio.CoreAudioApi;
namespace SafetyDemo;

enum AppState { Welcome, Takeoff, Video, Quiz, EmergencyBriefing, EmergencyDrill, Finished }

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

    // ===== AMBIENT CABIN ATMOSPHERE =====
    static Sound? engineHumSound = null;
    static SoundInst engineHumLeft;
    static SoundInst engineHumRight;
    static Sound? seatbeltChimeSound = null;
    static Sound? alarmSound = null;
    static SoundInst alarmInstance;
    static float ambientTime = 0f;
    static bool chimePlayed = false;

    // Takeoff sequence
    static float takeoffTimer = 0f;
    static float takeoffDuration = 8f; // seconds of takeoff rumble
    static Sound? takeoffRumbleSound = null;
    static SoundInst takeoffRumbleInstance;
    static bool takeoffAnnouncementPlayed = false;
    static float cameraShakeIntensity = 0f;

    // Cabin Lighting
    static Color normalCabinLight = new Color(0.9f, 0.88f, 0.8f); // warm white
    static Color emergencyCabinLight = new Color(0.15f, 0.05f, 0.02f); // very dim red
    static Color currentCabinLight;

    // ===== EMERGENCY DRILL SYSTEM =====
    static float emergencyTimer = 0f;
    static float emergencyTimeLimit = 45f; // seconds to reach exit
    static bool drillCompleted = false;
    static float drillCompletionTime = 0f;

    // Emergency floor strip lights
    static List<Vec3> floorStripPositions = new List<Vec3>();
    static float stripPulseTime = 0f;

    // Smoke particles
    static List<SmokeParticle> smokeParticles = new List<SmokeParticle>();
    static float smokeSpawnTimer = 0f;

    // Exit doors
    static Vec3 frontExitPosition = new Vec3(0, 0.5f, 8f);
    static Vec3 rearExitPosition = new Vec3(0, 0.5f, -9f);
    static float exitReachDistance = 1.5f;
    static Material? exitGlowMaterial = null;
    static Material? floorStripMaterial = null;
    static Material? smokeMaterial = null;

    // Emergency briefing
    static bool emergencyBriefingShown = false;
    static float emergencyBriefingTimer = 0f;

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
        GenerateAmbientSounds();
        InitializeVideo();
        InitializeTTS();
        InitializeEmergencySystem();

        currentCabinLight = normalCabinLight;

        while (SK.Step(() =>
        {
            if (airplane != null) airplane.Draw(Matrix.TRS(new Vec3(0, -2, 0), Quat.Identity, 1));
            HandleMovement();
            HandleCabinLighting();
            HandleAmbientAtmosphere();

            switch (currentState)
            {
                case AppState.Welcome:
                    HandleBackgroundMusic();
                    ShowWelcomeScreen();
                    break;

                case AppState.Takeoff:
                    HandleTakeoffSequence();
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
                    HandleBackgroundMusic();
                    ShowQuizUI();
                    break;

                case AppState.EmergencyBriefing:
                    ShowEmergencyBriefing();
                    break;

                case AppState.EmergencyDrill:
                    HandleEmergencyDrill();
                    break;

                case AppState.Finished:
                    ShowFinishedUI();
                    break;
            }
        }));

        SK.Shutdown();
    }

    // ===== AMBIENT CABIN ATMOSPHERE =====

    static void GenerateAmbientSounds()
    {
        // Engine hum - low frequency drone with harmonics
        {
            int sampleRate = 48000;
            int duration = 15;
            float[] samples = new float[sampleRate * duration];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                // Deep engine drone with subtle variation
                float fundamental = (float)Math.Sin(t * 55f * 2 * Math.PI) * 0.25f;
                float harmonic1 = (float)Math.Sin(t * 110f * 2 * Math.PI) * 0.15f;
                float harmonic2 = (float)Math.Sin(t * 165f * 2 * Math.PI) * 0.08f;
                // Subtle modulation for realism
                float modulation = 1f + (float)Math.Sin(t * 0.3f * 2 * Math.PI) * 0.1f;
                // Wind noise (filtered noise approximation)
                float windNoise = (float)(Math.Sin(t * 800 * 2 * Math.PI) * Math.Sin(t * 1.7 * 2 * Math.PI)) * 0.04f;

                samples[i] = (fundamental + harmonic1 + harmonic2 + windNoise) * modulation * 0.5f;
            }
            engineHumSound = Sound.FromSamples(samples);
        }

        // Seatbelt chime - classic two-tone ding
        {
            int sampleRate = 48000;
            float duration = 1.5f;
            float[] samples = new float[(int)(sampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = (float)Math.Exp(-t * 3.0);
                float tone1 = (float)Math.Sin(t * 880f * 2 * Math.PI) * envelope;
                // Second ding slightly delayed
                float t2 = t - 0.4f;
                float tone2 = t2 > 0 ? (float)Math.Sin(t2 * 1047f * 2 * Math.PI) * (float)Math.Exp(-t2 * 3.0) : 0;
                samples[i] = (tone1 + tone2) * 0.6f;
            }
            seatbeltChimeSound = Sound.FromSamples(samples);
        }

        // Takeoff rumble - deep bass with increasing intensity
        {
            int sampleRate = 48000;
            int duration = 10;
            float[] samples = new float[sampleRate * duration];
            Random rng = new Random(42);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                float intensity = Math.Min(1f, t / 5f); // ramp up over 5 seconds
                // Deep rumble
                float bass = (float)Math.Sin(t * 40f * 2 * Math.PI) * 0.4f * intensity;
                float midRumble = (float)Math.Sin(t * 80f * 2 * Math.PI) * 0.25f * intensity;
                // Turbine whine increasing in pitch
                float turbinePitch = 200f + intensity * 600f;
                float turbine = (float)Math.Sin(t * turbinePitch * 2 * Math.PI) * 0.15f * intensity;
                // Noise component for vibration feel
                float noise = ((float)rng.NextDouble() * 2 - 1) * 0.1f * intensity;

                samples[i] = Math.Clamp((bass + midRumble + turbine + noise) * 0.7f, -0.95f, 0.95f);
            }
            takeoffRumbleSound = Sound.FromSamples(samples);
        }

        // Alarm klaxon for emergency
        {
            int sampleRate = 48000;
            int duration = 8;
            float[] samples = new float[sampleRate * duration];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                // Alternating two-tone alarm
                float alarmCycle = (float)Math.Sin(t * 2f * 2 * Math.PI); // 2 Hz switching
                float freq = alarmCycle > 0 ? 800f : 600f;
                float tone = (float)Math.Sin(t * freq * 2 * Math.PI) * 0.5f;
                // Pulsing envelope
                float pulse = (float)Math.Abs(Math.Sin(t * 3f * 2 * Math.PI));
                samples[i] = tone * pulse * 0.6f;
            }
            alarmSound = Sound.FromSamples(samples);
        }

        Log.Info("Ambient sounds generated: engine hum, seatbelt chime, takeoff rumble, alarm");
    }

    static void HandleAmbientAtmosphere()
    {
        ambientTime += Time.Stepf;

        // Engine hum - always playing (positioned at wing engines)
        if (engineHumSound != null && currentState != AppState.EmergencyDrill)
        {
            if (!engineHumLeft.IsPlaying)
                engineHumLeft = engineHumSound.Play(new Vec3(-3f, -1f, -2f), 0.3f);
            if (!engineHumRight.IsPlaying)
                engineHumRight = engineHumSound.Play(new Vec3(3f, -1f, -2f), 0.3f);
        }

        // Seatbelt chime at specific moments
        if (!chimePlayed && currentState == AppState.Welcome && ambientTime > 2f)
        {
            if (seatbeltChimeSound != null)
                seatbeltChimeSound.Play(new Vec3(0, 1.5f, 0), 0.8f); // from overhead
            chimePlayed = true;
        }
    }

    static void HandleCabinLighting()
    {
        // Smoothly transition cabin lighting
        Color targetLight = currentState == AppState.EmergencyDrill ? emergencyCabinLight : normalCabinLight;

        float lerpSpeed = currentState == AppState.EmergencyDrill ? 1.5f : 0.5f;
        currentCabinLight = new Color(
            Lerp(currentCabinLight.r, targetLight.r, Time.Stepf * lerpSpeed),
            Lerp(currentCabinLight.g, targetLight.g, Time.Stepf * lerpSpeed),
            Lerp(currentCabinLight.b, targetLight.b, Time.Stepf * lerpSpeed)
        );

        // Adjust scene lighting to match cabin state
        // Using Renderer.SkyLight with a uniform color spherical harmonic
        Renderer.EnableSky = true;
    }

    static float Lerp(float a, float b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return a + (b - a) * t;
    }

    // ===== TAKEOFF SEQUENCE =====

    static void HandleTakeoffSequence()
    {
        takeoffTimer += Time.Stepf;

        // Captain announcement at start
        if (!takeoffAnnouncementPlayed)
        {
            takeoffAnnouncementPlayed = true;
            if (tts != null)
            {
                try
                {
                    tts.SpeakAsync("Cabin crew, prepare for takeoff. Ladies and gentlemen, please ensure your seatbelts are fastened and your tray tables are stowed.");
                }
                catch (Exception ex) { Log.Warn($"TTS takeoff announcement failed: {ex.Message}"); }
            }

            // Start rumble sound
            if (takeoffRumbleSound != null)
                takeoffRumbleInstance = takeoffRumbleSound.Play(Vec3.Zero, 0.7f);

            // Play seatbelt chime
            if (seatbeltChimeSound != null)
                seatbeltChimeSound.Play(new Vec3(0, 1.5f, 0), 0.9f);
        }

        // Camera shake during takeoff (intensity ramps up then settles)
        float progress = takeoffTimer / takeoffDuration;
        if (progress < 0.7f)
            cameraShakeIntensity = progress * 0.03f; // increasing shake
        else
            cameraShakeIntensity = (1f - progress) * 0.03f; // settling down

        // Show takeoff HUD text
        Vec3 textPos = playerPosition + new Vec3(0, 0.2f, -1.2f);
        string takeoffMsg = progress < 0.3f ? "Preparing for takeoff..." :
                           progress < 0.7f ? "Taking off!" :
                           "Reaching cruising altitude...";
        Text.Add(takeoffMsg, Matrix.T(textPos), TextAlign.Center);

        // Transition to video after takeoff completes
        if (takeoffTimer >= takeoffDuration)
        {
            cameraShakeIntensity = 0f;
            currentState = AppState.Video;

            // Start video playback
            if (audioPlayer != null && videoCapture != null)
            {
                try
                {
                    if (audioFile != null) audioFile.Position = 0;
                    videoPlaying = true;
                    audioPlayer.Play();
                    Log.Info("Video started after takeoff sequence");
                }
                catch (Exception ex) { Log.Err($"Failed to start video: {ex.Message}"); }
            }

            // Captain announcement for video
            if (tts != null)
            {
                try { tts.SpeakAsync("We have reached cruising altitude. Please direct your attention to the safety demonstration."); }
                catch { }
            }
        }
    }

    // ===== EMERGENCY DRILL SYSTEM =====

    static void InitializeEmergencySystem()
    {
        // Build floor strip light positions along the aisle
        floorStripPositions.Clear();
        for (float z = -8f; z <= 8f; z += 0.8f)
        {
            floorStripPositions.Add(new Vec3(-0.3f, -1.95f, z)); // left strip
            floorStripPositions.Add(new Vec3(0.3f, -1.95f, z));  // right strip
        }

        // Create glowing materials
        exitGlowMaterial = Material.Default.Copy();
        exitGlowMaterial.SetColor("color", new Color(0, 1f, 0, 0.8f)); // bright green

        floorStripMaterial = Material.Default.Copy();
        floorStripMaterial.SetColor("color", new Color(0, 0.9f, 0.2f, 0.9f));

        smokeMaterial = Material.Default.Copy();
        smokeMaterial.Transparency = Transparency.Add;
        smokeMaterial.SetColor("color", new Color(0.5f, 0.5f, 0.5f, 0.15f));

        Log.Info($"Emergency system initialized: {floorStripPositions.Count} floor strip lights, 2 exits");
    }

    static void ShowEmergencyBriefing()
    {
        emergencyBriefingTimer += Time.Stepf;

        // Play alarm and announcement once
        if (!emergencyBriefingShown)
        {
            emergencyBriefingShown = true;
            if (seatbeltChimeSound != null)
                seatbeltChimeSound.Play(new Vec3(0, 1.5f, 0), 1.0f);

            if (tts != null)
            {
                try
                {
                    tts.SpeakAsync("Attention passengers. This is an emergency drill. " +
                        "Cabin pressure has been lost. Please locate your nearest emergency exit. " +
                        "Stay low, follow the illuminated floor strip, and proceed to the nearest exit. " +
                        "You have 45 seconds. Go!");
                }
                catch { }
            }
        }

        // Show briefing UI
        Pose briefPose = new Pose(new Vec3(0, -0.5f, -2.0f), Quat.LookDir(0, 0, 1));
        UI.WindowBegin("Emergency Drill", ref briefPose, new Vec2(55, 35) * U.cm);

        UI.PushTint(new Color(1f, 0.2f, 0.2f));
        UI.Text("⚠ EMERGENCY DRILL ⚠", TextAlign.Center);
        UI.PopTint();
        UI.HSeparator();
        UI.Text("Scenario: Cabin depressurization!", TextAlign.Center);
        UI.Text("", TextAlign.Center);
        UI.Text("Your objectives:", TextAlign.TopLeft);
        UI.Text("• Stay LOW (smoke rises!)", TextAlign.TopLeft);
        UI.Text("• Follow the GREEN floor strip lights", TextAlign.TopLeft);
        UI.Text("• Reach the nearest EXIT within 45 seconds", TextAlign.TopLeft);
        UI.Text("", TextAlign.Center);
        UI.Text("Exits are at the FRONT and REAR of the cabin.", TextAlign.Center);
        UI.HSeparator();

        if (emergencyBriefingTimer > 3f) // Wait a moment before allowing start
        {
            UI.PushTint(new Color(0, 0.8f, 0));
            if (UI.Button("BEGIN DRILL", new Vec2(40, 0) * U.cm))
            {
                currentState = AppState.EmergencyDrill;
                emergencyTimer = 0f;
                drillCompleted = false;

                // Start alarm
                if (alarmSound != null)
                    alarmInstance = alarmSound.Play(new Vec3(0, 1.0f, 0), 0.5f);
            }
            UI.PopTint();
        }
        else
        {
            UI.Text("(Preparing drill...)", TextAlign.Center);
        }

        UI.WindowEnd();
    }

    static void HandleEmergencyDrill()
    {
        if (drillCompleted) return;

        emergencyTimer += Time.Stepf;
        stripPulseTime += Time.Stepf;

        // Draw emergency floor strip lights (pulsing green)
        DrawFloorStrips();

        // Draw smoke particles
        UpdateAndDrawSmoke();

        // Draw exit markers
        DrawExitMarkers();

        // Show emergency HUD
        DrawEmergencyHUD();

        // Check if player reached an exit
        float distToFront = Vec3.Distance(playerPosition, frontExitPosition);
        float distToRear = Vec3.Distance(playerPosition, rearExitPosition);
        float nearestExitDist = Math.Min(distToFront, distToRear);

        if (nearestExitDist <= exitReachDistance)
        {
            // Drill completed successfully!
            drillCompleted = true;
            drillCompletionTime = emergencyTimer;
            if (alarmInstance.IsPlaying) alarmInstance.Stop();
            currentState = AppState.Finished;

            if (tts != null)
            {
                try { tts.SpeakAsync("Well done! You have successfully reached the emergency exit. Drill complete."); }
                catch { }
            }
        }
        else if (emergencyTimer >= emergencyTimeLimit)
        {
            // Time's up!
            drillCompleted = true;
            drillCompletionTime = emergencyTimeLimit;
            if (alarmInstance.IsPlaying) alarmInstance.Stop();
            currentState = AppState.Finished;

            if (tts != null)
            {
                try { tts.SpeakAsync("Time is up. In a real emergency, every second counts. Please review emergency exit locations."); }
                catch { }
            }
        }

        // Keep alarm looping
        if (alarmSound != null && !alarmInstance.IsPlaying && !drillCompleted)
            alarmInstance = alarmSound.Play(new Vec3(0, 1.0f, 0), 0.4f);
    }

    static void DrawFloorStrips()
    {
        if (floorStripMaterial == null) return;

        float pulse = ((float)Math.Sin(stripPulseTime * 4f) + 1f) * 0.5f; // 0 to 1, pulsing
        float brightness = 0.5f + pulse * 0.5f;

        // Directional flow effect - lights chase toward nearest exit
        for (int i = 0; i < floorStripPositions.Count; i++)
        {
            Vec3 pos = floorStripPositions[i];

            // Wave pattern flowing toward exits
            float flowPhase = (pos.z * 0.5f + stripPulseTime * 3f) % (2f * (float)Math.PI);
            float flowBrightness = ((float)Math.Sin(flowPhase) + 1f) * 0.5f;

            float finalBrightness = brightness * (0.5f + flowBrightness * 0.5f);
            Color stripColor = new Color(0, finalBrightness * 0.9f, finalBrightness * 0.2f, finalBrightness);
            floorStripMaterial.SetColor("color", stripColor);

            Mesh.Cube.Draw(floorStripMaterial,
                Matrix.TRS(pos, Quat.Identity, new Vec3(0.08f, 0.02f, 0.15f)));
        }
    }

    static void UpdateAndDrawSmoke()
    {
        if (smokeMaterial == null) return;

        // Spawn new smoke particles
        smokeSpawnTimer += Time.Stepf;
        if (smokeSpawnTimer > 0.15f) // spawn every 150ms
        {
            smokeSpawnTimer = 0f;
            Random rng = new Random();
            float x = (float)(rng.NextDouble() * 2.4 - 1.2); // cabin width
            float z = (float)(rng.NextDouble() * 16 - 8);    // cabin length
            smokeParticles.Add(new SmokeParticle
            {
                Position = new Vec3(x, -0.5f, z), // starts at mid-height (smoke accumulating)
                Velocity = new Vec3(
                    (float)(rng.NextDouble() * 0.2 - 0.1),
                    (float)(rng.NextDouble() * 0.3 + 0.1), // rises
                    (float)(rng.NextDouble() * 0.2 - 0.1)),
                Life = 0f,
                MaxLife = (float)(rng.NextDouble() * 4 + 3),
                Size = (float)(rng.NextDouble() * 0.2 + 0.1)
            });
        }

        // Update and draw existing particles
        for (int i = smokeParticles.Count - 1; i >= 0; i--)
        {
            var p = smokeParticles[i];
            p.Life += Time.Stepf;
            p.Position += p.Velocity * Time.Stepf;
            p.Size += Time.Stepf * 0.05f; // slowly expand
            smokeParticles[i] = p;

            if (p.Life >= p.MaxLife)
            {
                smokeParticles.RemoveAt(i);
                continue;
            }

            float alpha = 1f - (p.Life / p.MaxLife);
            alpha *= 0.12f; // keep it subtle
            smokeMaterial.SetColor("color", new Color(0.6f, 0.6f, 0.6f, alpha));
            Mesh.Cube.Draw(smokeMaterial,
                Matrix.TRS(p.Position, Quat.Identity, Vec3.One * p.Size));
        }

        // Cap particle count
        while (smokeParticles.Count > 200)
            smokeParticles.RemoveAt(0);
    }

    static void DrawExitMarkers()
    {
        if (exitGlowMaterial == null) return;

        float exitPulse = ((float)Math.Sin(stripPulseTime * 5f) + 1f) * 0.5f;
        Color exitColor = new Color(0, 0.5f + exitPulse * 0.5f, 0, 0.9f);
        exitGlowMaterial.SetColor("color", exitColor);

        // Front exit door marker
        Mesh.Cube.Draw(exitGlowMaterial,
            Matrix.TRS(frontExitPosition, Quat.Identity, new Vec3(1.2f, 2f, 0.1f)));
        Text.Add("EXIT →", Matrix.T(frontExitPosition + new Vec3(0, 1.2f, -0.1f)),
            Text.MakeStyle(Font.Default, 4f * U.cm, new Color(0, 1, 0)));

        // Rear exit door marker
        Mesh.Cube.Draw(exitGlowMaterial,
            Matrix.TRS(rearExitPosition, Quat.Identity, new Vec3(1.2f, 2f, 0.1f)));
        Text.Add("← EXIT", Matrix.T(rearExitPosition + new Vec3(0, 1.2f, 0.1f)),
            Text.MakeStyle(Font.Default, 4f * U.cm, new Color(0, 1, 0)));
    }

    static void DrawEmergencyHUD()
    {
        // Timer display (floating near player)
        float timeRemaining = emergencyTimeLimit - emergencyTimer;
        Color timerColor = timeRemaining > 15f ? new Color(1, 1, 1) :
                          timeRemaining > 5f ? new Color(1, 0.7f, 0) :
                          new Color(1, 0, 0);

        Vec3 hudPos = playerPosition + new Vec3(0, 0.35f, -0.8f);
        string timerText = $"TIME: {timeRemaining:F1}s";
        Text.Add(timerText, Matrix.T(hudPos), Text.MakeStyle(Font.Default, 3f * U.cm, timerColor));

        // Distance to nearest exit
        float distToFront = Vec3.Distance(playerPosition, frontExitPosition);
        float distToRear = Vec3.Distance(playerPosition, rearExitPosition);
        string nearestDir = distToFront < distToRear ? "FRONT" : "REAR";
        float nearestDist = Math.Min(distToFront, distToRear);

        Vec3 distPos = hudPos + new Vec3(0, -0.06f, 0);
        Text.Add($"Nearest exit: {nearestDir} ({nearestDist:F1}m)",
            Matrix.T(distPos), Text.MakeStyle(Font.Default, 2f * U.cm, new Color(0, 1, 0)));

        // Crouch reminder if player is standing
        if (playerHeight > 1.0f)
        {
            Vec3 warnPos = distPos + new Vec3(0, -0.06f, 0);
            Text.Add("⚠ GET LOW! Smoke rises!",
                Matrix.T(warnPos), Text.MakeStyle(Font.Default, 2.5f * U.cm, new Color(1, 0.3f, 0)));
        }
    }

    // ===== ORIGINAL SYSTEMS (ENHANCED) =====

    static void InitializeTTS()
    {
        try
        {
            tts = new SpeechSynthesizer();
            try { tts.SetOutputToDefaultAudioDevice(); }
            catch (Exception ex) { Log.Warn($"Could not set TTS output device: {ex.Message}"); }
            try { tts.SelectVoiceByHints(VoiceGender.Female); }
            catch { Log.Warn("Female voice not available, using default"); }
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
     