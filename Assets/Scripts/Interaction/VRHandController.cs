using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace AirplaneSafety.Interaction
{
    public class VRHandController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionProperty gripAction;
        [SerializeField] private InputActionProperty triggerAction;

        [Header("Hand Visuals")]
        [SerializeField] private Animator handAnimator;
        [SerializeField] private SkinnedMeshRenderer handMesh;

        [Header("Haptics")]
        [SerializeField] private float hapticIntensity = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;

        private XRDirectInteractor interactor;

        private void Start()
        {
            interactor = GetComponent<XRDirectInteractor>();
            
            if (interactor != null)
            {
                interactor.selectEntered.AddListener(OnGrabObject);
                interactor.selectExited.AddListener(OnReleaseObject);
            }
        }

        private void Update()
        {
            UpdateHandAnimation();
        }

        private void UpdateHandAnimation()
        {
            if (handAnimator == null) return;

            // Read input values
            float gripValue = gripAction.action?.ReadValue<float>() ?? 0f;
            float triggerValue = triggerAction.action?.ReadValue<float>() ?? 0f;

            // Update animator parameters
            handAnimator.SetFloat("Grip", gripValue);
            handAnimator.SetFloat("Trigger", triggerValue);
        }

        private void OnGrabObject(SelectEnterEventArgs args)
        {
            Debug.Log($"[VRHandController] Grabbed: {args.interactableObject.transform.name}");
            SendHapticFeedback();
        }

        private void OnReleaseObject(SelectExitEventArgs args)
        {
            Debug.Log($"[VRHandController] Released: {args.interactableObject.transform.name}");
        }

        private void SendHapticFeedback()
        {
            // Haptic feedback requires XR controller
            var controller = GetComponent<ActionBasedController>();
            if (controller != null)
            {
                controller.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
        }

        private void OnDestroy()
        {
            if (interactor != null)
            {
                interactor.selectEntered.RemoveListener(OnGrabObject);
                interactor.selectExited.RemoveListener(OnReleaseObject);
            }
        }
    }
}
