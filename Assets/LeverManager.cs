
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverCombinationManager : MonoBehaviour
{
    [System.Serializable]
    public class LeverRequirement
    {
        public VRLever lever;
        
        [Tooltip("How to check this lever's position")]
        public enum CheckMode { 
            ExactValue,    // Check for a specific value with tolerance
            NotchIndex,    // Check if lever is at a specific notch
            AboveValue,    // Check if value is above threshold
            BelowValue     // Check if value is below threshold
        }
        public CheckMode checkMode = CheckMode.ExactValue;
        
        [Tooltip("Value to check against (0-1)")]
        [Range(0, 1)] 
        public float requiredValue = 1f;
        
        [Tooltip("Notch index to check (if using NotchIndex mode)")]
        [Range(0, 9)]
        public int requiredNotch = 0;
        
        [Tooltip("Tolerance for value comparison")]
        [Range(0.01f, 0.5f)]
        public float tolerance = 0.05f;
        
        [HideInInspector] 
        public bool currentState = false;
        
        [Tooltip("Label to identify this lever (optional)")]
        public string leverLabel = "";
    }

    public List<LeverRequirement> requiredLevers = new List<LeverRequirement>();
    public UnityEvent onConditionMet;
    public UnityEvent onConditionFailed;
    
    [Tooltip("Print state changes to console")]
    public bool debugOutput = true;
    
    private bool conditionWasMet = false;
    private Dictionary<VRLever, LeverRequirement> leverLookup = new Dictionary<VRLever, LeverRequirement>();

    void Awake()
    {
        // Create lookup for faster access
        leverLookup.Clear();
        foreach (var req in requiredLevers)
        {
            if (req.lever != null)
            {
                leverLookup[req.lever] = req;
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to lever events
        foreach (var req in requiredLevers)
        {
            if (req.lever != null)
            {
                req.lever.LeverListeners.AddListener(OnLeverValueChanged);
                
                // Set initial state
                req.currentState = IsLeverInRequiredState(req);
            }
        }
        
        // Check initial conditions
        EvaluateConditions();
    }

    void OnDisable()
    {
        // Unsubscribe from lever events
        foreach (var req in requiredLevers)
        {
            if (req.lever != null)
            {
                req.lever.LeverListeners.RemoveListener(OnLeverValueChanged);
            }
        }
    }

    void OnLeverValueChanged(VRLever lever, float newValue, float oldValue)
    {
        if (leverLookup.TryGetValue(lever, out LeverRequirement req))
        {
            bool wasInPosition = req.currentState;
            req.currentState = IsLeverInRequiredState(req);
            
            // Optional debug output
            if (debugOutput && wasInPosition != req.currentState)
            {
                string label = string.IsNullOrEmpty(req.leverLabel) ? lever.name : req.leverLabel;
                Debug.Log($"Lever '{label}': Value={newValue:F2}, Requirement met: {req.currentState}");
            }
            
            EvaluateConditions();
        }
    }
    
    bool IsLeverInRequiredState(LeverRequirement req)
    {
        if (req.lever == null) return false;
        
        float value = req.lever.Value;
        
        switch (req.checkMode)
        {
            case LeverRequirement.CheckMode.ExactValue:
                return Mathf.Abs(value - req.requiredValue) <= req.tolerance;
                
            case LeverRequirement.CheckMode.NotchIndex:
                if (!req.lever.usePositionNotches) return false;
                return req.lever.GetCurrentNotch() == req.requiredNotch;
                
            case LeverRequirement.CheckMode.AboveValue:
                return value >= req.requiredValue;
                
            case LeverRequirement.CheckMode.BelowValue:
                return value <= req.requiredValue;
                
            default:
                return false;
        }
    }

    void EvaluateConditions()
    {
        bool allConditionsMet = true;
        
        foreach (var req in requiredLevers)
        {
            if (req.lever == null) continue;
            
            if (!req.currentState)
            {
                allConditionsMet = false;
                break;
            }
        }
        
        // Trigger events if state changed
        if (allConditionsMet && !conditionWasMet)
        {
            onConditionMet.Invoke();
            conditionWasMet = true;
            
            if (debugOutput)
                Debug.Log("All lever conditions met! Combination successful.");
        }
        else if (!allConditionsMet && conditionWasMet)
        {
            onConditionFailed.Invoke();
            conditionWasMet = false;
            
            if (debugOutput)
                Debug.Log("Lever conditions no longer met. Combination failed.");
        }
    }

    // Public method to manually check conditions
    public bool CheckAllConditions()
    {
        foreach (var req in requiredLevers)
        {
            if (req.lever == null) continue;
            
            if (!IsLeverInRequiredState(req))
            {
                return false;
            }
        }
        return true;
    }
    
    #if UNITY_EDITOR
    // Optional validation
    void OnValidate()
    {
        // Ensure tolerance values are reasonable
        foreach (var req in requiredLevers)
        {
            req.tolerance = Mathf.Clamp(req.tolerance, 0.01f, 0.5f);
        }
    }
    #endif
}