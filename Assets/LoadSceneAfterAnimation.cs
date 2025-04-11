using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadSceneAfterAnimation : MonoBehaviour
{
    [Tooltip("The Animator component that plays the animation")]
    public Animator animator;

    [Tooltip("Name of the animation state to wait for")]
    public string animationStateName;

    [Tooltip("Name of the scene to load after animation ends")]
    public string sceneToLoad;

    [Tooltip("Optional delay after animation ends (in seconds)")]
    public float delayAfterAnimation = 0f;

    private bool animationFinished = false;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("No Animator component found!");
                return;
            }
        }

        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        
        // Wait until the animation starts playing
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName))
        {
            yield return null;
        }

        // Wait until the animation finishes
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Optional delay
        if (delayAfterAnimation > 0f)
        {
            yield return new WaitForSeconds(delayAfterAnimation);
        }

        // Load the next scene
        SceneManager.LoadScene(sceneToLoad);
    }
}