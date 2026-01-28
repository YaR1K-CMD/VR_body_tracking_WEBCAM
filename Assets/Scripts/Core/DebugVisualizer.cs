using UnityEngine;
using System.Collections.Generic;

public class DebugVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public bool showDebugInfo = true;
    public bool showBodyLandmarks = true;
    public bool showSkeleton = true;
    public bool showTrackingArea = true;
    public bool showPerformanceInfo = true;
    
    [Header("Body Landmarks")]
    public GameObject landmarkPrefab;
    public float landmarkSize = 0.05f;
    public Color trackedColor = Color.green;
    public Color untrackedColor = Color.red;
    public Color lowConfidenceColor = Color.yellow;
    
    [Header("Skeleton")]
    public Material skeletonMaterial;
    public float skeletonLineWidth = 0.01f;
    public Color skeletonColor = Color.white;
    
    [Header("Tracking Area")]
    public Material trackingAreaMaterial;
    public Vector3 trackingAreaSize = new Vector3(2f, 2.5f, 2f);
    public Color trackingAreaColor = new Color(0, 1, 1, 0.1f);
    
    private BodyTracker bodyTracker;
    private Dictionary<BodyTracker.LandmarkType, GameObject> landmarkObjects = new Dictionary<BodyTracker.LandmarkType, GameObject>();
    private List<GameObject> skeletonLines = new List<GameObject>();
    private GameObject trackingAreaCube;
    
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeVisualizer();
    }
    
    void InitializeVisualizer()
    {
        bodyTracker = FindObjectOfType<BodyTracker>();
        
        if (bodyTracker == null)
        {
            Debug.LogWarning("BodyTracker not found for debug visualization!");
            return;
        }
        
        CreateLandmarkObjects();
        CreateTrackingArea();
        
        isInitialized = true;
        Debug.Log("Debug visualizer initialized");
    }
    
    void CreateLandmarkObjects()
    {
        if (landmarkPrefab == null)
        {
            // Создание простого sphere prefab если не указан
            landmarkPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            landmarkPrefab.name = "DebugLandmark";
            landmarkPrefab.transform.localScale = Vector3.one * landmarkSize;
            Destroy(landmarkPrefab.GetComponent<Collider>());
        }
        
        // Создание объектов для всех landmarks
        foreach (BodyTracker.LandmarkType landmarkType in System.Enum.GetValues(typeof(BodyTracker.LandmarkType)))
        {
            GameObject landmarkObj = Instantiate(landmarkPrefab, transform);
            landmarkObj.name = landmarkType.ToString();
            landmarkObj.SetActive(false);
            
            landmarkObjects[landmarkType] = landmarkObj;
        }
    }
    
    void CreateTrackingArea()
    {
        if (trackingAreaMaterial == null)
        {
            trackingAreaMaterial = new Material(Shader.Find("Standard"));
            trackingAreaMaterial.color = trackingAreaColor;
            trackingAreaMaterial.SetFloat("_Mode", 3); // Transparent mode
            trackingAreaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trackingAreaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            trackingAreaMaterial.SetInt("_ZWrite", 0);
            trackingAreaMaterial.DisableKeyword("_ALPHATEST_ON");
            trackingAreaMaterial.EnableKeyword("_ALPHABLEND_ON");
            trackingAreaMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            trackingAreaMaterial.renderQueue = 3000;
        }
        
        trackingAreaCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trackingAreaCube.name = "TrackingArea";
        trackingAreaCube.transform.SetParent(transform);
        trackingAreaCube.transform.localScale = trackingAreaSize;
        trackingAreaCube.transform.position = new Vector3(0, trackingAreaSize.y / 2, 0);
        trackingAreaCube.GetComponent<Renderer>().material = trackingAreaMaterial;
        Destroy(trackingAreaCube.GetComponent<Collider>());
        
        trackingAreaCube.SetActive(showTrackingArea);
    }
    
    void Update()
    {
        if (!isInitialized || !showDebugInfo) return;
        
        UpdateLandmarkVisualization();
        UpdateSkeletonVisualization();
        UpdateDebugInfo();
    }
    
    void UpdateLandmarkVisualization()
    {
        if (!showBodyLandmarks) return;
        
        foreach (var kvp in landmarkObjects)
        {
            BodyTracker.LandmarkType landmarkType = kvp.Key;
            GameObject landmarkObj = kvp.Value;
            
            if (bodyTracker.IsLandmarkTracked(landmarkType))
            {
                Vector3 position = bodyTracker.GetLandmarkPosition(landmarkType);
                float confidence = bodyTracker.GetLandmarkConfidence(landmarkType);
                
                landmarkObj.SetActive(true);
                landmarkObj.transform.position = position;
                
                // Изменение цвета в зависимости от уверенности
                Renderer renderer = landmarkObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = Color.Lerp(lowConfidenceColor, trackedColor, confidence);
                    renderer.material.color = color;
                }
            }
            else
            {
                landmarkObj.SetActive(false);
            }
        }
    }
    
    void UpdateSkeletonVisualization()
    {
        if (!showSkeleton) return;
        
        // Очистка старых линий скелета
        ClearSkeletonLines();
        
        // Создание линий между связанными landmarks
        CreateSkeletonLines();
    }
    
    void ClearSkeletonLines()
    {
        foreach (GameObject line in skeletonLines)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }
        skeletonLines.Clear();
    }
    
    void CreateSkeletonLines()
    {
        // Определение соединений между landmarks
        var connections = new List<(BodyTracker.LandmarkType, BodyTracker.LandmarkType)>
        {
            (BodyTracker.LandmarkType.Nose, BodyTracker.LandmarkType.LeftEye),
            (BodyTracker.LandmarkType.Nose, BodyTracker.LandmarkType.RightEye),
            (BodyTracker.LandmarkType.LeftEye, BodyTracker.LandmarkType.LeftEar),
            (BodyTracker.LandmarkType.RightEye, BodyTracker.LandmarkType.RightEar),
            (BodyTracker.LandmarkType.LeftShoulder, BodyTracker.LandmarkType.RightShoulder),
            (BodyTracker.LandmarkType.LeftShoulder, BodyTracker.LandmarkType.LeftElbow),
            (BodyTracker.LandmarkType.LeftElbow, BodyTracker.LandmarkType.LeftWrist),
            (BodyTracker.LandmarkType.RightShoulder, BodyTracker.LandmarkType.RightElbow),
            (BodyTracker.LandmarkType.RightElbow, BodyTracker.LandmarkType.RightWrist),
            (BodyTracker.LandmarkType.LeftShoulder, BodyTracker.LandmarkType.LeftHip),
            (BodyTracker.LandmarkType.RightShoulder, BodyTracker.LandmarkType.RightHip),
            (BodyTracker.LandmarkType.LeftHip, BodyTracker.LandmarkType.RightHip),
            (BodyTracker.LandmarkType.LeftHip, BodyTracker.LandmarkType.LeftKnee),
            (BodyTracker.LandmarkType.LeftKnee, BodyTracker.LandmarkType.LeftAnkle),
            (BodyTracker.LandmarkType.RightHip, BodyTracker.LandmarkType.RightKnee),
            (BodyTracker.LandmarkType.RightKnee, BodyTracker.LandmarkType.RightAnkle)
        };
        
        foreach (var connection in connections)
        {
            if (bodyTracker.IsLandmarkTracked(connection.Item1) && 
                bodyTracker.IsLandmarkTracked(connection.Item2))
            {
                Vector3 start = bodyTracker.GetLandmarkPosition(connection.Item1);
                Vector3 end = bodyTracker.GetLandmarkPosition(connection.Item2);
                
                GameObject line = CreateLine(start, end);
                skeletonLines.Add(line);
            }
        }
    }
    
    GameObject CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("SkeletonLine");
        line.transform.SetParent(transform);
        
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        
        if (skeletonMaterial != null)
        {
            lineRenderer.material = skeletonMaterial;
        }
        
        lineRenderer.startWidth = skeletonLineWidth;
        lineRenderer.endWidth = skeletonLineWidth;
        lineRenderer.startColor = skeletonColor;
        lineRenderer.endColor = skeletonColor;
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        return line;
    }
    
    void UpdateDebugInfo()
    {
        // Эта функция обновляет отладочную информацию
        // Визуализация через OnGUI
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 400));
        GUILayout.Label("Debug Info", GUI.skin.box);
        
        // Информация о трекинге
        if (bodyTracker != null)
        {
            int trackedLandmarks = 0;
            int totalLandmarks = System.Enum.GetValues(typeof(BodyTracker.LandmarkType)).Length;
            
            foreach (BodyTracker.LandmarkType landmarkType in System.Enum.GetValues(typeof(BodyTracker.LandmarkType)))
            {
                if (bodyTracker.IsLandmarkTracked(landmarkType))
                {
                    trackedLandmarks++;
                }
            }
            
            GUILayout.Label($"Tracked Landmarks: {trackedLandmarks}/{totalLandmarks}");
            GUILayout.Label($"Tracking Active: {bodyTracker.IsTracking}");
        }
        
        // Информация о производительности
        if (showPerformanceInfo && PerformanceOptimizer.Instance != null)
        {
            GUILayout.Label($"FPS: {PerformanceOptimizer.Instance.CurrentFPS:F1}");
            GUILayout.Label($"Average FPS: {PerformanceOptimizer.Instance.AverageFPS:F1}");
            GUILayout.Label($"Performance: {(PerformanceOptimizer.Instance.IsLowPerformance ? "Low" : "Normal")}");
        }
        
        // Информация о вебкамере
        WebcamCapture webcamCapture = FindObjectOfType<WebcamCapture>();
        if (webcamCapture != null)
        {
            GUILayout.Label($"Webcam: {(webcamCapture.IsInitialized ? "Active" : "Inactive")}");
        }
        
        // Информация о VR
        OculusVRSetup vrSetup = FindObjectOfType<OculusVRSetup>();
        if (vrSetup != null)
        {
            GUILayout.Label($"VR: {(vrSetup.IsVRInitialized ? "Active" : "Inactive")}");
            GUILayout.Label($"Hand Tracking: {(vrSetup.IsHandTrackingEnabled ? "Enabled" : "Disabled")}");
        }
        
        GUILayout.Space(10);
        
        // Кнопки управления
        if (GUILayout.Button("Toggle Landmarks"))
        {
            showBodyLandmarks = !showBodyLandmarks;
        }
        
        if (GUILayout.Button("Toggle Skeleton"))
        {
            showSkeleton = !showSkeleton;
        }
        
        if (GUILayout.Button("Toggle Tracking Area"))
        {
            showTrackingArea = !showTrackingArea;
            if (trackingAreaCube != null)
            {
                trackingAreaCube.SetActive(showTrackingArea);
            }
        }
        
        if (GUILayout.Button("Toggle Performance Info"))
        {
            showPerformanceInfo = !showPerformanceInfo;
        }
        
        GUILayout.EndArea();
    }
    
    public void ToggleDebugInfo()
    {
        showDebugInfo = !showDebugInfo;
        
        if (!showDebugInfo)
        {
            // Скрыть все элементы визуализации
            foreach (var landmarkObj in landmarkObjects.Values)
            {
                if (landmarkObj != null)
                {
                    landmarkObj.SetActive(false);
                }
            }
            
            ClearSkeletonLines();
            
            if (trackingAreaCube != null)
            {
                trackingAreaCube.SetActive(false);
            }
        }
        else
        {
            // Показать элементы визуализации
            if (showTrackingArea && trackingAreaCube != null)
            {
                trackingAreaCube.SetActive(true);
            }
        }
    }
    
    public void SetLandmarkSize(float size)
    {
        landmarkSize = size;
        
        foreach (var landmarkObj in landmarkObjects.Values)
        {
            if (landmarkObj != null)
            {
                landmarkObj.transform.localScale = Vector3.one * landmarkSize;
            }
        }
    }
    
    public void SetSkeletonLineWidth(float width)
    {
        skeletonLineWidth = width;
        
        // Обновление существующих линий
        foreach (GameObject line in skeletonLines)
        {
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = skeletonLineWidth;
                lineRenderer.endWidth = skeletonLineWidth;
            }
        }
    }
    
    public void SetTrackingAreaSize(Vector3 size)
    {
        trackingAreaSize = size;
        
        if (trackingAreaCube != null)
        {
            trackingAreaCube.transform.localScale = trackingAreaSize;
            trackingAreaCube.transform.position = new Vector3(0, trackingAreaSize.y / 2, 0);
        }
    }
    
    void OnDestroy()
    {
        // Очистка всех созданных объектов
        foreach (var landmarkObj in landmarkObjects.Values)
        {
            if (landmarkObj != null)
            {
                Destroy(landmarkObj);
            }
        }
        
        ClearSkeletonLines();
        
        if (trackingAreaCube != null)
        {
            Destroy(trackingAreaCube);
        }
    }
}
