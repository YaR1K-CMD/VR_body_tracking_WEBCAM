using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyTrackingManager : MonoBehaviour
{
    [Header("Components")]
    public WebcamCapture webcamCapture;
    public BodyTracker bodyTracker;
    public VRAvatarController vrAvatarController;
    
    [Header("Processing Settings")]
    public bool enableProcessing = true;
    public int processingFrameRate = 30;
    public float processingInterval = 0.033f;
    
    [Header("MediaPipe Integration")]
    public bool useMediaPipe = true;
    public string mediaPipeModelPath = "pose_landmark_lite.tflite";
    
    private bool isProcessing = false;
    private Coroutine processingCoroutine;
    
    public static BodyTrackingManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeComponents();
        StartProcessing();
    }
    
    void InitializeComponents()
    {
        if (webcamCapture == null)
        {
            webcamCapture = FindObjectOfType<WebcamCapture>();
        }
        
        if (bodyTracker == null)
        {
            bodyTracker = FindObjectOfType<BodyTracker>();
        }
        
        if (vrAvatarController == null)
        {
            vrAvatarController = FindObjectOfType<VRAvatarController>();
        }
        
        Debug.Log("Components initialized");
    }
    
    void StartProcessing()
    {
        if (!enableProcessing) return;
        
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
        }
        
        processingCoroutine = StartCoroutine(ProcessWebcamFrame());
        isProcessing = true;
        
        Debug.Log("Body tracking processing started");
    }
    
    void StopProcessing()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
        }
        
        isProcessing = false;
        Debug.Log("Body tracking processing stopped");
    }
    
    IEnumerator ProcessWebcamFrame()
    {
        while (enableProcessing)
        {
            yield return new WaitForSeconds(processingInterval);
            
            if (webcamCapture != null && webcamCapture.IsInitialized)
            {
                ProcessCurrentFrame();
            }
        }
    }
    
    void ProcessCurrentFrame()
    {
        Color32[] frameData = webcamCapture.GetFrameData();
        if (frameData == null) return;
        
        if (useMediaPipe)
        {
            ProcessWithMediaPipe(frameData);
        }
        else
        {
            ProcessWithSimulatedTracking(frameData);
        }
    }
    
    void ProcessWithMediaPipe(Color32[] frameData)
    {
        // TODO: Интеграция с MediaPipe
        // Здесь будет вызов MediaPipe для обработки кадра
        // и получения landmarks
        
        // Временная симуляция
        ProcessWithSimulatedTracking(frameData);
    }
    
    void ProcessWithSimulatedTracking(Color32[] frameData)
    {
        // Временная симуляция трекинга для демонстрации
        Dictionary<BodyTracker.LandmarkType, Vector3> simulatedLandmarks = new Dictionary<BodyTracker.LandmarkType, Vector3>();
        Dictionary<BodyTracker.LandmarkType, float> simulatedConfidences = new Dictionary<BodyTracker.LandmarkType, float>();
        
        float time = Time.time;
        
        // Симуляция движения головы
        simulatedLandmarks[BodyTracker.LandmarkType.Nose] = new Vector3(
            Mathf.Sin(time * 0.5f) * 0.1f,
            1.6f + Mathf.Sin(time * 0.3f) * 0.05f,
            0f
        );
        simulatedConfidences[BodyTracker.LandmarkType.Nose] = 0.9f;
        
        // Симуляция движения рук
        simulatedLandmarks[BodyTracker.LandmarkType.LeftWrist] = new Vector3(
            -0.3f + Mathf.Sin(time * 0.8f) * 0.1f,
            1.2f + Mathf.Cos(time * 0.6f) * 0.1f,
            0.2f
        );
        simulatedConfidences[BodyTracker.LandmarkType.LeftWrist] = 0.8f;
        
        simulatedLandmarks[BodyTracker.LandmarkType.RightWrist] = new Vector3(
            0.3f + Mathf.Sin(time * 0.8f + Mathf.PI) * 0.1f,
            1.2f + Mathf.Cos(time * 0.6f + Mathf.PI) * 0.1f,
            0.2f
        );
        simulatedConfidences[BodyTracker.LandmarkType.RightWrist] = 0.8f;
        
        // Симуляция плеч
        simulatedLandmarks[BodyTracker.LandmarkType.LeftShoulder] = new Vector3(-0.2f, 1.4f, 0f);
        simulatedConfidences[BodyTracker.LandmarkType.LeftShoulder] = 0.9f;
        
        simulatedLandmarks[BodyTracker.LandmarkType.RightShoulder] = new Vector3(0.2f, 1.4f, 0f);
        simulatedConfidences[BodyTracker.LandmarkType.RightShoulder] = 0.9f;
        
        // Симуляция бедер
        simulatedLandmarks[BodyTracker.LandmarkType.LeftHip] = new Vector3(-0.1f, 0.9f, 0f);
        simulatedConfidences[BodyTracker.LandmarkType.LeftHip] = 0.9f;
        
        simulatedLandmarks[BodyTracker.LandmarkType.RightHip] = new Vector3(0.1f, 0.9f, 0f);
        simulatedConfidences[BodyTracker.LandmarkType.RightHip] = 0.9f;
        
        // Обновление body tracker
        if (bodyTracker != null)
        {
            bodyTracker.UpdateLandmarks(simulatedLandmarks, simulatedConfidences);
        }
    }
    
    public void ToggleProcessing()
    {
        enableProcessing = !enableProcessing;
        
        if (enableProcessing)
        {
            StartProcessing();
        }
        else
        {
            StopProcessing();
        }
    }
    
    public void SetProcessingFrameRate(int frameRate)
    {
        processingFrameRate = Mathf.Clamp(frameRate, 1, 60);
        processingInterval = 1f / processingFrameRate;
        
        if (isProcessing)
        {
            StopProcessing();
            StartProcessing();
        }
    }
    
    public bool IsSystemReady()
    {
        return webcamCapture != null && webcamCapture.IsInitialized &&
               bodyTracker != null && bodyTracker.IsTracking &&
               vrAvatarController != null;
    }
    
    void OnDestroy()
    {
        StopProcessing();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopProcessing();
        }
        else if (enableProcessing)
        {
            StartProcessing();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            StopProcessing();
        }
        else if (enableProcessing)
        {
            StartProcessing();
        }
    }
}
