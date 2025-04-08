using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class LeverCombinationManager : MonoBehaviour
{
    [System.Serializable]
    public class LeverRequirement
    {
        public VRLever lever;
        [Range(0, 1)] public float requiredValue = 1f;
        public float tolerance = 0.05f;
    }

    public List<LeverRequirement> requiredLevers = new List<LeverRequirement>();
    public UnityEvent onConditionMet;
    public UnityEvent onConditionFailed;
    
    private bool conditionWasMet = false;

    void Update()
    {
        bool allConditionsMet = CheckAllConditions();
        
        if (allConditionsMet && !conditionWasMet)
        {
            onConditionMet.Invoke();
            conditionWasMet = true;
        }
        else if (!allConditionsMet && conditionWasMet)
        {
            onConditionFailed.Invoke();
            conditionWasMet = false;
        }
    }

    public bool CheckAllConditions()
    {
        foreach (var req in requiredLevers)
        {
            if (req.lever == null) continue;
            
            float currentValue = req.lever.Value;
            if (Mathf.Abs(currentValue - req.requiredValue) > req.tolerance)
            {
                return false;
            }
        }
        return true;
    }
}