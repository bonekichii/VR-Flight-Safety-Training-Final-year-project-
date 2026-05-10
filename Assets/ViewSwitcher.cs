using UnityEngine;

public class ViewSwitcher : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCam = 0;

    void Start()
    {
        // Enable only the first camera
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(i == 0);
    }

    void Update()
    {
        // Legacy Input detected - commenting out to prevent errors with New Input System
        /*
        if (Input.GetKeyDown(KeyCode.V))
        {
            cameras[currentCam].gameObject.SetActive(false);
            currentCam = (currentCam + 1) % cameras.Length;
            cameras[currentCam].gameObject.SetActive(true);
        }
        */
    }
}
