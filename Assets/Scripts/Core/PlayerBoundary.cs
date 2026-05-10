using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AirplaneSafety.Core
{
    /// <summary>
    /// Keeps the player within the airplane boundaries using invisible colliders
    /// and restricts teleportation to valid areas
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerBoundary : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [SerializeField] private LayerMask boundaryLayer;
        [SerializeField] private float raycastDistance = 0.5f;

        private CharacterController characterController;
        private Vector3 lastValidPosition;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            lastValidPosition = transform.position;
        }

        private void Update()
        {
            CheckBoundaries();
        }

        private void CheckBoundaries()
        {
            // Check if player is trying to move outside boundaries
            Vector3 currentPos = transform.position;
            
            // If we detect collision with boundary, reset to last valid position
            if (Physics.CheckSphere(currentPos, characterController.radius, boundaryLayer))
            {
                Debug.LogWarning("[PlayerBoundary] Player attempted to leave airplane boundaries");
                transform.position = lastValidPosition;
            }
            else
            {
                lastValidPosition = currentPos;
            }
        }

        private void OnDrawGizmos()
        {
            if (characterController != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, characterController.radius);
            }
        }
    }
}
