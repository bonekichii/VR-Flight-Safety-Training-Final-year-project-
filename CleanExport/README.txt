# Airplane Safety VR - Clean Project Files

This folder contains all the scripts and assets you need for a fresh Unity project.

## What's Included:
- ✅ All 8 C# scripts (Core, UI, Interaction, Editor)
- ✅ Airplane model (boeing 737-800.glb)
- ✅ Automatic scene builder tool
- ✅ Material fixer tool

## How to Use:

### Step 1: Create New Unity Project
1. Open **Unity Hub**
2. Click **New Project**
3. Template: **3D (URP)** or **VR Core**
4. Name: `AirplaneSafetyVR`
5. Location: Wherever you want
6. Click **Create**

### Step 2: Install Required Packages
Once Unity opens:
1. **Window > Package Manager**
2. Install these packages:
   - **XR Interaction Toolkit** (Unity Registry)
   - **TextMeshPro** (will auto-prompt on first use)
   - **XR Plugin Management** (Unity Registry)
   
3. In **XR Plugin Management** settings:
   - Enable **Oculus** (for Quest 3)
   - Enable **OpenXR** (for PC testing)

### Step 3: Copy Scripts
1. Copy this entire `Scripts` folder
2. Paste into your new project's `Assets` folder

### Step 4: Copy Model
1. Copy the `Models` folder (with airplane .glb file)
2. Paste into your project's `Assets` folder

### Step 5: Auto-Build Scene
1. In Unity, wait for scripts to compile (check bottom-right corner)
2. Click **Airplane Safety > Fix Materials (URP)**
3. Click **Airplane Safety > ★ BUILD COMPLETE SCENE (Auto)**
4. Done! Scene is ready.

### Step 6: Final Setup
1. In Hierarchy, drag `boeing 737-800` from Project window into scene
2. Move `XR Origin` to a seat position inside the airplane
3. Press **Play** to test!

## Game Flow:
1. Welcome screen (5 sec)
2. Floating instructions appear
3. Interactive tablet with video (optional)
4. Quiz with 3 questions
5. Completion

## For Quest 3:
- Build Settings > Android
- Install via USB or SideQuest
- All scripts already support Quest 3!

## Need Help?
All scripts are well-commented. Check:
- `GameManager.cs` - Main state machine
- `AutoSceneBuilder.cs` - Scene creation tool
- `QuizManager.cs` - Quiz questions (edit here!)

Enjoy! 🚀
