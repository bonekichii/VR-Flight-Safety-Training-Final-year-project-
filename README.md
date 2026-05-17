# VR Flight Safety Training — StereoKit Edition

An immersive **Meta Quest** training experience built with **StereoKit** and **.NET 8**. Users board a 3D cabin, watch a safety briefing, complete a knowledge quiz, and practice an emergency evacuation drill with audio, lighting, and spatial UI.

> **Looking for the Unity version?** See the [`main`](https://github.com/bonekichii/VR-Flight-Safety-Training-Final-year-project-/tree/main) branch.

## Features

| Module | Description |
|--------|-------------|
| **Welcome & briefing** | Floating UI, text-to-speech announcements, ambient cabin audio |
| **Takeoff sequence** | Engine rumble, seatbelt chime, camera shake, cabin lighting |
| **Safety video** | In-world screen with synced playback (OpenCV + NAudio) |
| **Knowledge quiz** | 12 multiple-choice questions with instant feedback |
| **Emergency drill** | Dimmed cabin, red alert siren, floor strips, smoke particles, exit markers, timed evacuation |

## Tech stack

- [StereoKit](https://stereokit.net/) 0.3.9 — VR framework (OpenXR)
- .NET 8 (Windows)
- OpenCvSharp4 — video frames on a 3D texture
- NAudio — audio playback
- System.Speech — TTS announcements

## Requirements

- **Windows 10/11**
- **Visual Studio 2022** with the **.NET desktop development** workload
- **Meta Quest 3** (optional) — via [Meta Quest Link](https://www.meta.com/quest/quest-link/) for PC VR testing

## Quick start

1. Clone the repo and switch to this branch:
   ```bash
   git clone https://github.com/bonekichii/VR-Flight-Safety-Training-Final-year-project-.git
   cd VR-Flight-Safety-Training-Final-year-project-
   git checkout safetydemo
   ```

2. Open `SafetyDemo.sln` in Visual Studio 2022.

3. Set **SafetyDemo** as the startup project and press **F5**.

### Desktop simulator controls

| Input | Action |
|-------|--------|
| Mouse move | Hand cursor |
| Right-click + drag | Look around |
| Scroll wheel | Hand depth |
| Left click | Pinch / select UI |

### Quest testing (recommended)

1. Install **Meta Quest Link** and enable **Developer Mode** on the headset.
2. Connect the Quest via USB or Air Link.
3. Run from Visual Studio (**F5**) — the app launches in VR on the headset.
4. Use controllers or hand tracking to point at and select UI panels.

## Project structure

```
SafetyDemo/
├── Program.cs         # App logic: states, audio, quiz, emergency drill
├── SafetyDemo.csproj  # Dependencies and build config
└── Assets/
    ├── airplane.glb   # Cabin 3D model
    ├── *.mp4          # Safety briefing video
    └── *.png          # UI textures
```

## Training flow

```
Welcome → Takeoff → Safety Video → Quiz → Emergency Briefing → Emergency Drill → Finished
```

## Building for standalone Quest (future)

This project currently targets **Windows + Quest Link**. A standalone Android APK would require an additional StereoKit Android build pipeline.

## Large assets

Some files exceed GitHub’s 50 MB recommendation (video, 3D model). They are included for convenience; consider [Git LFS](https://git-lfs.github.com/) if the repo grows further.

## Author

**Vega** — Final-year VR safety training project  
GitHub: [@bonekichii](https://github.com/bonekichii)

## License

Academic / portfolio use. Contact the author before commercial redistribution.
