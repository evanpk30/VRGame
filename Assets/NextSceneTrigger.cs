using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Tooltip("The name of the scene to load when player enters trigger")]
    public string sceneToLoad;
    
    [Tooltip("Should we use a loading screen (requires setup)")]
    public bool useLoadingScreen = false;
    
    [Tooltip("Optional transition delay in seconds")]
    public float transitionDelay = 0f;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            if (transitionDelay > 0)
            {
                Invoke("LoadScene", transitionDelay);
            }
            else
            {
                LoadScene();
            }
        }
    }
    
    private void LoadScene()
    {
        if (useLoadingScreen)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}