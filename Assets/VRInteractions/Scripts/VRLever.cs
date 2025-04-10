using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;

[System.Serializable]
public class LeverChangeEvent : UnityEvent<VRLever, float, float> {}

public class VRLever : VRInteractable 
{
    // Configuration
    public LeverChangeEvent LeverListeners;
    public float Min = 0;
    public float Max = 0;
    public float VibrationStrength = 0.2f;
    public bool triggerEnabled = false;
    
    // Wall mounting options
    [Tooltip("Enable if lever is mounted on wall and rotates up/down")]
    public bool isWallMounted = false;
    
    [Tooltip("Which axis to use for rotation when wall mounted (0=X, 1=Y, 2=Z)")]
    [Range(0, 2)]
    public int wallRotationAxis = 2; // Default to Z-axis for up/down
    
    // Variable position settings
    [Tooltip("Enable to add subtle haptic feedback at preset positions")]
    public bool usePositionNotches = false;
    
    [Tooltip("Number of notch positions (2=just ends, 3=ends+middle, etc)")]
    [Range(2, 10)]
    public int numberOfNotches = 5;
    
    [Tooltip("Strength of the notch haptic feedback")]
    [Range(0.05f, 0.5f)]
    public float notchFeedbackStrength = 0.3f;

    // State
    private HingeJoint CurrentHinge;
    private float mValue = 0;
    private float valueCache;
    private VRGripper controller;
    private bool isActing = false;
    private int currentNotchIndex = -1;
    
    // Constants
    private const float VALUE_CHANGE_THRESHOLD = 0.001f;
    private const float ANGLE_CHANGE_THRESHOLD = 0.1f;
    private const float NOTCH_SNAP_THRESHOLD = 0.03f;

    #if UNITY_EDITOR
    [Header("Debug Info")]
    [SerializeField, ReadOnly] private float _debugCurrentValue;
    [SerializeField, ReadOnly] private float _lastSentValue;
    [SerializeField, ReadOnly] private int _valueChangeCount;
    [SerializeField, ReadOnly] private int _currentNotch;
    #endif

    // Private cache for editor usage
    private float mMinCache;
    private float mMaxCache;

    public float Value
    {
        get => mValue;
        set {
            float newValue = Mathf.Clamp01(value);
            if (Mathf.Abs(mValue - newValue) > VALUE_CHANGE_THRESHOLD)
            {
                float oldValue = mValue;
                mValue = newValue;

                #if UNITY_EDITOR
                _lastSentValue = mValue;
                _valueChangeCount++;
                #endif

                CheckHingeValue();
                CheckNotchPosition();

                try {
                    LeverListeners?.Invoke(this, mValue, oldValue);
                } 
                catch {
                    Debug.LogError("Delegate failed in VRLever Value setter");
                }
                
                valueCache = mValue;
            }
        }
    }

    void Reset() => Validate();
    void OnValidate() => Validate();

    void Validate()
    {
        CurrentHinge = GetComponent<HingeJoint>();
        if (CurrentHinge != null)
            EditorUpdateHinges();
    }

    void EditorUpdateHinges()
    {
        if (CurrentHinge.limits.min != mMinCache)
        {
            mMinCache = CurrentHinge.limits.min;
            Min = mMinCache;
        }
        else if (mMinCache != Min)
        {
            mMinCache = Min;
            var limits = CurrentHinge.limits;
            limits.min = Min;
            CurrentHinge.limits = limits;
        }

        if (CurrentHinge.limits.max != mMaxCache)
        {
            mMaxCache = CurrentHinge.limits.max;
            Max = mMaxCache;
        }
        else if (mMaxCache != Max)
        {
            mMaxCache = Max;
            var limits = CurrentHinge.limits;
            limits.max = Max;
            CurrentHinge.limits = limits;
        }
    }
    
    void CheckNotchPosition()
    {
        if (!usePositionNotches || numberOfNotches < 2)
            return;
            
        // Calculate which notch we're closest to
        int newNotchIndex = CalculateClosestNotch(mValue);
        
        // Only provide feedback when crossing into a new notch position
        if (newNotchIndex != currentNotchIndex)
        {
            currentNotchIndex = newNotchIndex;
            
            #if UNITY_EDITOR
            _currentNotch = currentNotchIndex;
            #endif
            
            // Provide stronger haptic feedback when reaching a notch
            if (controller != null && isActing)
            {
                controller.HapticVibration(notchFeedbackStrength, 0.1f);
            }
        }
    }
    
    int CalculateClosestNotch(float value)
    {
        if (numberOfNotches <= 1)
            return 0;
            
        // Calculate notch positions
        float notchSpacing = 1.0f / (numberOfNotches - 1);
        
        // Find closest notch
        int closestNotch = Mathf.RoundToInt(value / notchSpacing);
        return Mathf.Clamp(closestNotch, 0, numberOfNotches - 1);
    }
    
    // Gets the normalized value (0-1) of a specific notch
    public float GetNotchValue(int notchIndex)
    {
        if (numberOfNotches <= 1)
            return 0;
            
        float notchSpacing = 1.0f / (numberOfNotches - 1);
        return Mathf.Clamp01(notchIndex * notchSpacing);
    }
    
    // Is the lever close to a specific notch?
    public bool IsAtNotch(int notchIndex)
    {
        if (!usePositionNotches || numberOfNotches < 2)
            return false;
            
        float notchValue = GetNotchValue(notchIndex);
        return Mathf.Abs(mValue - notchValue) <= NOTCH_SNAP_THRESHOLD;
    }
    
    // Get the current notch index (or -1 if not near a notch)
    public int GetCurrentNotch()
    {
        if (!usePositionNotches || numberOfNotches < 2)
            return -1;
            
        for (int i = 0; i < numberOfNotches; i++)
        {
            if (IsAtNotch(i))
                return i;
        }
        
        return -1; // Not at any notch
    }

    void OnEnable() => SetAngleToValue();

    void CheckHingeValue()
    {
        float currentAngle = GetCurrentRotationAngle();
        if (!isActing && Mathf.Abs(ValueToAngle() - currentAngle) > ANGLE_CHANGE_THRESHOLD)
        {
            #if UNITY_EDITOR
            Debug.Log($"Hinge Check: Current={currentAngle:0.00}°, Target={ValueToAngle():0.00}°");
            #endif
            
            SetAngleToValue();
        }
    }

    void SetAngleToValue()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        
        if (isWallMounted)
        {
            // Set angle based on the selected wall rotation axis
            if (wallRotationAxis == 0)
                rotation.x = ValueToAngle();
            else if (wallRotationAxis == 1)
                rotation.y = ValueToAngle();
            else
                rotation.z = ValueToAngle();
        }
        else
        {
            // Original behavior - use X axis
            rotation.x = ValueToAngle();
        }
        
        transform.rotation = Quaternion.Euler(rotation);
    }

    float AngleToValue()
    {
        float currentAngle = GetCurrentRotationAngle();
        currentAngle = Mathf.Clamp(currentAngle, Min, Max);
        float normalizedValue = Mathf.InverseLerp(Min, Max, currentAngle);
        
        #if UNITY_EDITOR
        _debugCurrentValue = normalizedValue;
        #endif
        
        return normalizedValue;
    }
    
    float GetCurrentRotationAngle()
    {
        float angle;
        
        if (isWallMounted)
        {
            // Get the appropriate axis based on wallRotationAxis
            if (wallRotationAxis == 0)
                angle = transform.rotation.eulerAngles.x;
            else if (wallRotationAxis == 1)
                angle = transform.rotation.eulerAngles.y;
            else
                angle = transform.rotation.eulerAngles.z;
        }
        else
        {
            // Original behavior - use X axis
            angle = transform.rotation.eulerAngles.x;
        }
        
        return NormalizeAngle(angle);
    }

    float ValueToAngle() => Mathf.Lerp(Min, Max, mValue);

    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    new void Update()
    {
        if (!Interactable) return;

        float newValue = AngleToValue();
        if (ShouldSendValue(newValue))
            Value = newValue;

        if (isActing)
            controller?.HapticVibration(VibrationStrength, Time.deltaTime);
    }

    private bool ShouldSendValue(float newValue)
    {
        // Always send min/max values
        if (Mathf.Abs(newValue) <= VALUE_CHANGE_THRESHOLD || 
            Mathf.Abs(newValue - 1f) <= VALUE_CHANGE_THRESHOLD)
            return true;
            
        // Only send if significant change
        return Mathf.Abs(newValue - valueCache) > VALUE_CHANGE_THRESHOLD;
    }

    // Interaction methods
    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null) return;
        
        var gripper = collision.rigidbody.GetComponent<VRGripper>();
        if (gripper != null)
        {
            controller = gripper;
            BeginAction();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == null) return;
        
        var gripper = collision.rigidbody.GetComponent<VRGripper>();
        if (controller == gripper)
        {
            EndAction();
            controller = null;
        }
    }

    void BeginAction() => isActing = true;
    void EndAction() => isActing = false;
    public void OnGripBegin() => BeginAction();
    public void OnGripEnd() => EndAction();

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 axis = Vector3.right; // Default for X
        
        if (isWallMounted)
        {
            // Set axis based on wallRotationAxis
            if (wallRotationAxis == 1)
                axis = Vector3.up; // Y axis
            else if (wallRotationAxis == 2)
                axis = Vector3.forward; // Z axis
        }
        
        Vector3 perpendicular = (axis == Vector3.up) ? Vector3.forward : Vector3.up;
        
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position, axis, perpendicular, Min, 0.5f);
        Handles.Label(transform.position + Quaternion.AngleAxis(Min, axis) * perpendicular * 0.6f, $"Min: {Min:0}°");

        Handles.color = Color.green;
        Handles.DrawWireArc(transform.position, axis, perpendicular, Max, 0.5f);
        Handles.Label(transform.position + Quaternion.AngleAxis(Max, axis) * perpendicular * 0.6f, $"Max: {Max:0}°");

        // Draw notch positions if enabled
        if (usePositionNotches && numberOfNotches > 1)
        {
            Handles.color = Color.cyan;
            float notchSpacing = 1.0f / (numberOfNotches - 1);
            
            for (int i = 0; i < numberOfNotches; i++)
            {
                float notchValue = i * notchSpacing;
                float notchAngle = Mathf.Lerp(Min, Max, notchValue);
                
                Handles.DrawWireDisc(
                    transform.position + Quaternion.AngleAxis(notchAngle, axis) * perpendicular * 0.5f, 
                    axis, 
                    0.05f
                );
            }
        }

        Handles.color = Color.yellow;
        float currentAngle = GetCurrentRotationAngle();
        Handles.DrawWireArc(transform.position, axis, perpendicular, currentAngle, 0.3f);
    }
    #endif
}