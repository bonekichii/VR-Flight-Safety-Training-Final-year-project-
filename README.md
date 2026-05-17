# VR Flight Safety Training — Unity Edition

A **Meta Quest 3** virtual-reality application that teaches airplane passenger safety procedures inside an interactive 3D cabin. Built with **Unity 2022.3 LTS** and the **XR Interaction Toolkit**.

> **Looking for the StereoKit / C# version?** See the [`safetydemo`](https://github.com/bonekichii/VR-Flight-Safety-Training-Final-year-project-/tree/safetydemo) branch — a standalone rewrite with video playback, a 12-question quiz, and an emergency evacuation drill.

## Features

- **State-driven training flow** — Welcome → Briefing → Quiz → Completion
- **World-space UI** — Floating screens and an interactive safety tablet
- **Safety video** — Playable briefing on an in-cabin tablet (`InteractiveTablet`)
- **Knowledge quiz** — Multiple-choice questions with instant feedback (`QuizManager`)
- **Player boundary** — Keeps the user inside the cabin during exploration
- **VR hand interaction** — Grip, trigger, and haptics via XR Interaction Toolkit
- **Editor tooling** — One-click scene builder and URP material fixer

## Tech stack

| Component | Version / package |
|-----------|-------------------|
| Unity | 2022.3.62f3 LTS |
| XR Interaction Toolkit | Unity Package Manager |
| XR Plugin Management | Oculus + OpenXR |
| TextMeshPro | UI text |
| Target platform | Android (Meta Quest 3) |

## Requirements

- **Unity Hub** with **Unity 2022.3.62f3** (or compatible 2022.3 LTS)
- **Android Build Support** module (for Quest builds)
- **Meta Quest 3** with Developer Mode enabled (for device testing)

## Getting started

### 1. Clone and open

```bash
git clone https://github.com/bonekichii/VR-Flight-Safety-Training-Final-year-project-.git
cd VR-Flight-Safety-Training-Final-year-project-
```

Open the project folder in **Unity Hub** (the folder containing `Assets/`, `Packages/`, and `ProjectSettings/`).

### 2. Install XR packages (if prompted)

In **Window → Package Manager**, ensure these are installed:

- XR Interaction Toolkit
- XR Plugin Management
- TextMeshPro

In **Edit → Project Settings → XR Plug-in Management**:

- Enable **Oculus** (Quest builds)
- Enable **OpenXR** (PC / Link testing)

### 3. Build the scene (first time)

After scripts compile:

1. **Airplane Safety → Fix Materials (URP)**
2. **Airplane Safety → ★ BUILD COMPLETE SCENE (Auto)**

Place the `boeing 737-800` model in the scene and position **XR Origin** at a passenger seat.

### 4. Run

Press **Play** in the Editor for desktop XR simulation, or build to Android for standalone Quest.

## Project structure

```
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, PlayerBoundary
│   │   ├── UI/             # WelcomeScreen, FloatingScreen, QuizManager, InteractiveTablet
│   │   ├── Interaction/    # VRHandController
│   │   └── Editor/         # AutoSceneBuilder, material tools
│   ├── Models/             # Boeing 737 cabin (.glb)
│   └── Videos/             # Safety briefing video
├── CleanExport/            # Portable script + model bundle for fresh Unity projects
├── Packages/
└── ProjectSettings/
```

See `CleanExport/README.txt` for step-by-step instructions to bootstrap a new Unity project from the exported scripts and assets.

## Training flow

```
Welcome (5s) → Floating instructions → Safety video on tablet → Quiz → Completion
```

Core logic lives in `Assets/Scripts/Core/GameManager.cs`, which drives state transitions and notifies UI components.

## Building for Meta Quest 3

1. **File → Build Settings** → Platform: **Android**
2. Switch platform and resolve any Gradle / SDK prompts
3. Connect the Quest via USB (or use **Meta Quest Developer Hub**)
4. Build and Run, or install the generated `.apk` via SideQuest / `adb`

Pre-built releases (if published) are available under **GitHub Releases**.

## Branches in this repository

| Branch | Description |
|--------|-------------|
| **`main`** (this branch) | Full Unity project — original FYP implementation |
| **`safetydemo`** | StereoKit / .NET rewrite with enhanced emergency drill |

## Large assets

The cabin model (`boeing 737-800.glb`, ~74 MB) exceeds GitHub’s 50 MB recommendation. It uploads successfully under the 100 MB hard limit; use **Git LFS** if you add larger assets.

## Author

**Vega** — Final-year VR safety training project  
GitHub: [@bonekichii](https://github.com/bonekichii)

## License

Academic / portfolio use. Contact the author before commercial redistribution.
