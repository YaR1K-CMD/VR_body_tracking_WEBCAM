using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BodyTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    public bool enableTracking = true;
    public float confidenceThreshold = 0.5f;
    public float smoothingFactor = 0.3f;
    
    [Header("Debug")]
    public bool showDebugPoints = true;
    public GameObject debugPointPrefab;
    
    public struct BodyLandmark
    {
        public Vector3 position;
        public float confidence;
        public LandmarkType type;
    }
    
    public enum LandmarkType
    {
        Nose, LeftEye, RightEye, LeftEar, RightEar,
        LeftShoulder, RightShoulder, LeftElbow, RightElbow,
        LeftWrist, RightWrist, LeftHip, RightHip,
        LeftKnee, RightKnee, LeftAnkle, RightAnkle
    }
    
    private Dictionary<LandmarkType, BodyLandmark> currentLandmarks = new Dictionary<LandmarkType, BodyLandmark>();
    private Dictionary<LandmarkType, BodyLandmark> previousLandmarks = new Dictionary<LandmarkType, BodyLandmark>();
    private List<GameObject> debugPoints = new List<GameObject>();
    
    public bool IsTracking { get; private set; }
    public Dictionary<LandmarkType, BodyLandmark> CurrentLandmarks => currentLandmarks;
    
    void Start()
    {
        InitializeDebugPoints();
        IsTracking = enableTracking;
    }
    
    void InitializeDebugPoints()
    {
        if (!showDebugPoints || debugPointPrefab == null) return;
        
        foreach (LandmarkType landmarkType in System.Enum.GetValues(typeof(LandmarkType)))
        {
            GameObject point = Instantiate(debugPointPrefab, transform);
            point.name = landmarkType.ToString();
            debugPoints.Add(point);
        }
    }
    
    public void UpdateLandmarks(Dictionary<LandmarkType, Vector3> landmarks, Dictionary<LandmarkType, float> confidences)
    {
        if (!enableTracking) return;
        
        previousLandmarks = new Dictionary<LandmarkType, BodyLandmark>(currentLandmarks);
        currentLandmarks.Clear();
        
        foreach (var kvp in landmarks)
        {
            LandmarkType type = kvp.Key;
            Vector3 position = kvp.Value;
            float confidence = confidences.ContainsKey(type) ? confidences[type] : 0f;
            
            if (confidence >= confidenceThreshold)
            {
                BodyLandmark landmark = new BodyLandmark
                {
                    position = ApplySmoothing(type, position),
                    confidence = confidence,
                    type = type
                };
                
                currentLandmarks[type] = landmark;
            }
        }
        
        UpdateDebugVisualization();
    }
    
    Vector3 ApplySmoothing(LandmarkType type, Vector3 newPosition)
    {
        if (previousLandmarks.ContainsKey(type))
        {
            Vector3 previousPosition = previousLandmarks[type].position;
            return Vector3.Lerp(previousPosition, newPosition, smoothingFactor);
        }
        return newPosition;
    }
    
    void UpdateDebugVisualization()
    {
        if (!showDebugPoints) return;
        
        for (int i = 0; i < debugPoints.Count; i++)
        {
            LandmarkType type = (LandmarkType)i;
            GameObject point = debugPoints[i];
            
            if (currentLandmarks.ContainsKey(type))
            {
                point.SetActive(true);
                point.transform.position = currentLandmarks[type].position;
                
                Renderer renderer = point.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = Color.Lerp(Color.red, Color.green, currentLandmarks[type].confidence);
                    renderer.material.color = color;
                }
            }
            else
            {
                point.SetActive(false);
            }
        }
    }
    
    public Vector3 GetLandmarkPosition(LandmarkType type)
    {
        if (currentLandmarks.ContainsKey(type))
        {
            return currentLandmarks[type].position;
        }
        return Vector3.zero;
    }
    
    public float GetLandmarkConfidence(LandmarkType type)
    {
        if (currentLandmarks.ContainsKey(type))
        {
            return currentLandmarks[type].confidence;
        }
        return 0f;
    }
    
    public bool IsLandmarkTracked(LandmarkType type)
    {
        return currentLandmarks.ContainsKey(type) && currentLandmarks[type].confidence >= confidenceThreshold;
    }
    
    public void ToggleTracking()
    {
        enableTracking = !enableTracking;
        IsTracking = enableTracking;
        
        if (!enableTracking)
        {
            currentLandmarks.Clear();
            UpdateDebugVisualization();
        }
    }
    
    void OnDestroy()
    {
        foreach (GameObject point in debugPoints)
        {
            if (point != null)
            {
                Destroy(point);
            }
        }
    }
}
