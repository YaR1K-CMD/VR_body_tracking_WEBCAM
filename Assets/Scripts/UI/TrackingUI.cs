using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TrackingUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI trackingInfoText;
    public TextMeshProUGUI webcamInfoText;
    
    [Header("Control Buttons")]
    public Button toggleTrackingButton;
    public Button toggleWebcamButton;
    public Button toggleVRTrackingButton;
    public Button calibrateButton;
    
    [Header("Settings Sliders")]
    public Slider confidenceSlider;
    public Slider smoothingSlider;
    public Slider bodyWeightSlider;
    public Slider vrWeightSlider;
    
    [Header("Status Indicators")]
    public Image webcamStatusIndicator;
    public Image trackingStatusIndicator;
    public Image vrStatusIndicator;
    
    [Header("Colors")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;
    public Color warningColor = Color.yellow;
    
    private BodyTrackingManager trackingManager;
    private WebcamCapture webcamCapture;
    private BodyTracker bodyTracker;
    private VRAvatarController vrAvatarController;
    
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer;
    private int frameCount;
    private float currentFPS;
    
    void Start()
    {
        InitializeComponents();
        SetupUI();
        StartFPSCounter();
    }
    
    void InitializeComponents()
    {
        trackingManager = BodyTrackingManager.Instance;
        
        if (trackingManager != null)
        {
            webcamCapture = trackingManager.webcamCapture;
            bodyTracker = trackingManager.bodyTracker;
            vrAvatarController = trackingManager.vrAvatarController;
        }
    }
    
    void SetupUI()
    {
        // Настройка кнопок
        if (toggleTrackingButton != null)
        {
            toggleTrackingButton.onClick.AddListener(ToggleTracking);
        }
        
        if (toggleWebcamButton != null)
        {
            toggleWebcamButton.onClick.AddListener(ToggleWebcam);
        }
        
        if (toggleVRTrackingButton != null)
        {
            toggleVRTrackingButton.onClick.AddListener(ToggleVRTracking);
        }
        
        if (calibrateButton != null)
        {
            calibrateButton.onClick.AddListener(CalibrateSystem);
        }
        
        // Настройка слайдеров
        if (confidenceSlider != null)
        {
            confidenceSlider.minValue = 0f;
            confidenceSlider.maxValue = 1f;
            confidenceSlider.value = 0.5f;
            confidenceSlider.onValueChanged.AddListener(UpdateConfidenceThreshold);
        }
        
        if (smoothingSlider != null)
        {
            smoothingSlider.minValue = 0f;
            smoothingSlider.maxValue = 1f;
            smoothingSlider.value = 0.3f;
            smoothingSlider.onValueChanged.AddListener(UpdateSmoothingFactor);
        }
        
        if (bodyWeightSlider != null)
        {
            bodyWeightSlider.minValue = 0f;
            bodyWeightSlider.maxValue = 1f;
            bodyWeightSlider.value = 0.7f;
            bodyWeightSlider.onValueChanged.AddListener(UpdateBodyWeight);
        }
        
        if (vrWeightSlider != null)
        {
            vrWeightSlider.minValue = 0f;
            vrWeightSlider.maxValue = 1f;
            vrWeightSlider.value = 0.3f;
            vrWeightSlider.onValueChanged.AddListener(UpdateVRWeight);
        }
    }
    
    void Update()
    {
        UpdateStatusIndicators();
        UpdateTrackingInfo();
        UpdateFPS();
    }
    
    void UpdateStatusIndicators()
    {
        // Статус вебкамеры
        if (webcamStatusIndicator != null)
        {
            if (webcamCapture != null && webcamCapture.IsInitialized)
            {
                webcamStatusIndicator.color = activeColor;
            }
            else
            {
                webcamStatusIndicator.color = inactiveColor;
            }
        }
        
        // Статус трекинга
        if (trackingStatusIndicator != null)
        {
            if (bodyTracker != null && bodyTracker.IsTracking)
            {
                trackingStatusIndicator.color = activeColor;
            }
            else
            {
                trackingStatusIndicator.color = inactiveColor;
            }
        }
        
        // Статус VR
        if (vrStatusIndicator != null)
        {
            if (vrAvatarController != null)
            {
                vrStatusIndicator.color = activeColor;
            }
            else
            {
                vrStatusIndicator.color = warningColor;
            }
        }
    }
    
    void UpdateTrackingInfo()
    {
        if (trackingInfoText != null && bodyTracker != null)
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
            
            trackingInfoText.text = $"Tracked: {trackedLandmarks}/{totalLandmarks} landmarks";
        }
        
        if (webcamInfoText != null && webcamCapture != null)
        {
            if (webcamCapture.IsInitialized)
            {
                webcamInfoText.text = "Webcam: Active";
            }
            else
            {
                webcamInfoText.text = "Webcam: Inactive";
            }
        }
    }
    
    void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.deltaTime;
        
        if (fpsTimer >= fpsUpdateInterval)
        {
            currentFPS = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0f;
            
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {currentFPS:F1}";
            }
        }
    }
    
    void StartFPSCounter()
    {
        frameCount = 0;
        fpsTimer = 0f;
    }
    
    void ToggleTracking()
    {
        if (trackingManager != null)
        {
            trackingManager.ToggleProcessing();
        }
        
        UpdateStatusText();
    }
    
    void ToggleWebcam()
    {
        if (webcamCapture != null)
        {
            if (webcamCapture.IsInitialized)
            {
                webcamCapture.StopWebcam();
            }
            else
            {
                webcamCapture.InitializeWebcam();
            }
        }
        
        UpdateStatusText();
    }
    
    void ToggleVRTracking()
    {
        if (vrAvatarController != null)
        {
            vrAvatarController.ToggleVRTracking();
        }
        
        UpdateStatusText();
    }
    
    void CalibrateSystem()
    {
        StartCoroutine(CalibrationCoroutine());
    }
    
    IEnumerator CalibrationCoroutine()
    {
        if (statusText != null)
        {
            statusText.text = "Calibrating...";
        }
        
        yield return new WaitForSeconds(2f);
        
        if (statusText != null)
        {
            statusText.text = "Calibration Complete";
        }
        
        yield return new WaitForSeconds(1f);
        
        UpdateStatusText();
    }
    
    void UpdateConfidenceThreshold(float value)
    {
        if (bodyTracker != null)
        {
            bodyTracker.confidenceThreshold = value;
        }
    }
    
    void UpdateSmoothingFactor(float value)
    {
        if (bodyTracker != null)
        {
            bodyTracker.smoothingFactor = value;
        }
    }
    
    void UpdateBodyWeight(float value)
    {
        if (vrAvatarController != null)
        {
            vrAvatarController.SetTrackingWeights(value, vrWeightSlider.value);
        }
    }
    
    void UpdateVRWeight(float value)
    {
        if (vrAvatarController != null)
        {
            vrAvatarController.SetTrackingWeights(bodyWeightSlider.value, value);
        }
    }
    
    void UpdateStatusText()
    {
        if (statusText == null) return;
        
        string status = "System Status:\n";
        
        if (trackingManager != null && trackingManager.IsSystemReady())
        {
            status += "✓ All systems ready";
        }
        else
        {
            status += "⚠ System not ready";
        }
        
        statusText.text = status;
    }
}
