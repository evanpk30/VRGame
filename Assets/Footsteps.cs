using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class VRFootsteps : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float stepDelay = 0.4f;
    [SerializeField] private float minMoveSpeed = 0.2f;
    
    private CharacterController _controller;
    private AudioSource _audio;
    private float _stepTimer;
    private Vector3 _lastPos;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_controller.isGrounded && IsMoving())
        {
            _stepTimer += Time.deltaTime;
            
            if (_stepTimer >= stepDelay)
            {
                PlayRandomFootstep();
                _stepTimer = 0;
            }
        }
    }

    private bool IsMoving()
    {
        // Check actual movement (works with XR Simulator & real VR)
        float actualSpeed = Vector3.Distance(transform.position, _lastPos) / Time.deltaTime;
        _lastPos = transform.position;
        return actualSpeed > minMoveSpeed;
    }

    private void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        _audio.PlayOneShot(clip);
    }
}