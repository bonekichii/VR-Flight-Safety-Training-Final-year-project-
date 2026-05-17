# VR Airplane Passenger Safety Training - Project Handover Document

## 1. Project Overview
This project is a Virtual Reality (VR) airplane passenger safety training simulation designed to educate users on standard airplane safety protocols. It is built to run on Meta Quest devices, providing an interactive, state-driven experience where users go through a safety briefing and complete a knowledge check (quiz) to reinforce learning.

## 2. Technology Stack & Architecture
- **Engine:** Unity (2022/2023+ based on package structures)
- **Programming Language:** C#
- **VR Frameworks:** 
  - Unity XR Interaction Toolkit (XRIT) for cross-platform VR interactions (grabbing, UI interaction).
  - OpenXR / Meta XR SDKs (including Meta XR Movement and Networking packages) for tracking and deployment to Meta Quest devices.
- **UI Framework:** Unity Canvas with TextMeshPro (TMP) for world-space UI (Floating Screens, Interactive Tablets).

## 3. Core Architecture & Scripts
The project uses a modular, state-driven architecture located in `Assets/Scripts/`:

### Core Systems (`Assets/Scripts/Core/`)
- **`GameManager.cs`**: Operates as a Singleton orchestrating the main flow of the simulation (`Welcome` -> `Briefing` -> `Quiz` -> `Completed`). Handles automatic state transitions and emits events (`OnStateChanged`) that UI components listen to.
- **`PlayerBoundary.cs`**: Keeps the player within the airplane boundaries. It requires a `CharacterController` and uses invisible colliders (`Physics.CheckSphere`) to prevent the player from teleporting or walking through the walls of the airplane cabin, resetting them to their last valid position if a boundary is breached.

### Interaction (`Assets/Scripts/Interaction/`)
- **`VRHandController.cs`**: Manages player hand inputs (Grip, Trigger) via Unity's Input System. It maps inputs to `Hand Animator` parameters for visual hand articulation, handles `XRDirectInteractor` events (`selectEntered`, `selectExited`) for grabbing objects, and sends haptic feedback impulses to the controllers upon interaction.

### User Interface (`Assets/Scripts/UI/`)
- **`WelcomeScreen.cs`**: Listens to the `GameManager`. When in the `Welcome` state, it smoothly fades in a "Welcome to the Board" UI canvas using a Coroutine, and fades it out when the state changes.
- **`FloatingScreen.cs`**: A world-space UI that floats with a sine-wave animation (`floatAmplitude`, `floatSpeed`) and uses `Quaternion.LookRotation` to always face the player. It provides contextual instructions based on the current game state (e.g., "Please watch the safety demonstration...").
- **`InteractiveTablet.cs`**: A virtual tablet that holds a `VideoPlayer` component. During the `Briefing` state, it allows the user to press a play button to watch a safety demo video (`Assets/Videos/SafetyDemo.mp4`). Upon completion (`loopPointReached`), it enables a "Proceed" button that triggers the `Quiz` state.
- **`QuizManager.cs`**: Activated during the `Quiz` state. It displays a UI panel with text and 4 multiple-choice options. It evaluates answers, updates the score, provides instant colored feedback (Correct/Incorrect + Explanation), and features hardcoded fallback questions about emergency landings, exits, and oxygen masks. Triggering the finish button completes the simulation.

## 4. Current State & Known Behaviors
- **Deployment:** The project is configured to build as an Android `.apk` (`android.apk` is present) for standalone Meta Quest headsets.
- **Audio Routing:** A known historical issue involved audio routing during Quest Link (audio playing on PC instead of the headset). The setup requires strict adherence to OpenXR / Meta SDK audio routing settings.
- **Mechanics:** The current iteration heavily relies on UI-based interaction (e.g., answering quiz questions on a virtual tablet/screen) rather than physical interactions with cabin elements.

---

## 5. Suggested Advancements for a More Immersive Experience
To elevate this project from a standard UI-driven application into a highly immersive and effective VR training tool, consider the following implementations:

### A. Physical, Task-Based Interactions (Show, Don't Tell)
Instead of relying on a multiple-choice UI quiz, integrate the XR Interaction Toolkit to test knowledge through physical actions:
- **Oxygen Mask Deployment:** Have oxygen masks physically drop from overhead compartments. The user must reach up, grab the mask, and pull it towards their face using collision triggers to pass the objective.
- **Life Vest Operation:** Require the user to look under their seat, pull out a life vest, simulate putting it over their head, and pull a physical toggle to inflate it.
- **Seatbelt Fastening:** Implement a physics-based seatbelt that the user must physically connect using both hands.

### B. Environmental Immersion & Sensory Feedback
- **Dynamic Lighting & Particles:** During emergency scenarios, fade out the main cabin lights, turn on the emergency floor strip lighting, and deploy particle effects like mild smoke to teach users the importance of staying low.
- **Spatial Audio (3D Sound):** Utilize Unity's Audio Spatializer. Ensure the safety announcements sound like they are coming from cabin speakers. Add directional ambient noises (engine hum, wind) and make emergency alarms localized so the user can locate exits by following sound.
- **Advanced Haptics:** Increase the use of haptic feedback beyond just grabbing objects. Provide a rumble when the plane "takes off" or hits "turbulence," and subtle clicks when the seatbelt latches or the oxygen mask is pulled.

### C. Hand Tracking & Physics Hands
- Upgrade from controller-based inputs to **Meta Quest Hand Tracking**. Allowing users to interact using their bare hands greatly reduces friction and increases the feeling of presence.
- Implement physics-based hands that collide with the environment (e.g., the user's hand stops when pressing against the seat back) rather than passing through solid objects.

### D. Intelligent NPCs and Passengers
- Introduce animated AI passengers or flight attendants. A flight attendant NPC could physically demonstrate the safety procedures during the `Briefing` state.
- In advanced scenarios, have NPCs react to emergencies (e.g., some panicking, some acting correctly) to test the user's ability to focus and follow protocols under simulated psychological stress.

### E. Gamification and Scoring
- Add a time-attack mode where users must complete physical safety tasks within a strict time limit representing a real-world emergency.
- Provide a detailed end-of-simulation scorecard that assesses reaction time, correct order of operations (e.g., putting your own mask on before helping others), and spatial awareness.
