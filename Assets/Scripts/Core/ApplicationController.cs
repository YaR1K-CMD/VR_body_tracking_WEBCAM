using UnityEngine;
using System.Collections;

public class ApplicationController : MonoBehaviour
{
    [Header("Application Settings")]
    public bool autoStart = true;
    public bool enableDebugMode = false;
    public string applicationVersion = "1.0.0";
    
    [Header("Core Systems")]
    public BodyTrackingManager bodyTrackingManager;
    public OculusVRSetup oculusVRSetup;
    public PerformanceOptimizer performanceOptimizer;
    public CalibrationSystem calibrationSystem;
    public DebugVisualizer debugVisualizer;
    
    [Header("UI")]
    public TrackingUI trackingUI;
    
    [Header("Startup Sequence")]
    public float startupDelay = 1f;
    public bool showStartupScreen = true;
    
    private bool isInitialized = false;
    private bool isStartupComplete = false;
    
    public static ApplicationController Instance { get; private set; }
    
    public bool IsInitialized => isInitialized;
    public bool IsStartupComplete => isStartupComplete;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (autoStart)
        {
            StartCoroutine(StartupSequence());
        }
    }
    
    IEnumerator StartupSequence()
    {
        Debug.Log($"Starting VR Body Tracking Application v{applicationVersion}");
        
        // Показать экран загрузки
        if (showStartupScreen)
        {
            ShowStartupScreen();
        }
        
        yield return new WaitForSeconds(startupDelay);
        
        // Инициализация систем в правильном порядке
        yield return StartCoroutine(InitializeCoreSystems());
        yield return StartCoroutine(InitializeVRSystem());
        yield return StartCoroutine(InitializeTrackingSystem());
        yield return StartCoroutine(InitializeUISystem());
        yield return StartCoroutine(InitializeDebugSystem());
        
        // Завершение запуска
        yield return StartCoroutine(CompleteStartup());
    }
    
    IEnumerator InitializeCoreSystems()
    {
        UpdateStartupStatus("Initializing core systems...");
        
        // Инициализация оптимизатора производительности
        if (performanceOptimizer == null)
        {
            performanceOptimizer = FindObjectOfType<PerformanceOptimizer>();
        }
        
        if (performanceOptimizer != null)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Performance optimizer initialized");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator InitializeVRSystem()
    {
        UpdateStartupStatus("Initializing VR system...");
        
        // Инициализация Oculus VR
        if (oculusVRSetup == null)
        {
            oculusVRSetup = FindObjectOfType<OculusVRSetup>();
        }
        
        if (oculusVRSetup != null)
        {
            // Ожидание инициализации VR
            float timeout = 10f;
            float elapsed = 0f;
            
            while (!oculusVRSetup.IsVRInitialized && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (oculusVRSetup.IsVRInitialized)
            {
                Debug.Log("VR system initialized successfully");
            }
            else
            {
                Debug.LogError("VR system initialization failed!");
            }
        }
        else
        {
            Debug.LogWarning("OculusVRSetup not found!");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator InitializeTrackingSystem()
    {
        UpdateStartupStatus("Initializing body tracking system...");
        
        // Инициализация менеджера трекинга
        if (bodyTrackingManager == null)
        {
            bodyTrackingManager = BodyTrackingManager.Instance;
        }
        
        if (bodyTrackingManager != null)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Body tracking system initialized");
        }
        else
        {
            Debug.LogWarning("BodyTrackingManager not found!");
        }
        
        // Инициализация системы калибровки
        if (calibrationSystem == null)
        {
            calibrationSystem = FindObjectOfType<CalibrationSystem>();
        }
        
        if (calibrationSystem != null)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Calibration system initialized");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator InitializeUISystem()
    {
        UpdateStartupStatus("Initializing user interface...");
        
        // Инициализация UI
        if (trackingUI == null)
        {
            trackingUI = FindObjectOfType<TrackingUI>();
        }
        
        if (trackingUI != null)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("UI system initialized");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator InitializeDebugSystem()
    {
        UpdateStartupStatus("Initializing debug systems...");
        
        // Инициализация визуализатора отладки
        if (debugVisualizer == null)
        {
            debugVisualizer = FindObjectOfType<DebugVisualizer>();
        }
        
        if (debugVisualizer != null)
        {
            debugVisualizer.enabled = enableDebugMode;
            Debug.Log($"Debug visualizer {(enableDebugMode ? "enabled" : "disabled")}");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator CompleteStartup()
    {
        UpdateStartupStatus("Completing startup...");
        
        // Проверка готовности всех систем
        bool allSystemsReady = CheckSystemsReady();
        
        if (!allSystemsReady)
        {
            Debug.LogWarning("Some systems may not be fully initialized!");
        }
        
        isInitialized = true;
        isStartupComplete = true;
        
        // Скрыть экран загрузки
        if (showStartupScreen)
        {
            HideStartupScreen();
        }
        
        // Автоматическая калибровка если нужно
        if (ShouldAutoCalibrate())
        {
            yield return StartCoroutine(AutoCalibrate());
        }
        
        Debug.Log("Application startup complete!");
        
        // Запуск основного функционала
        StartMainFunctionality();
    }
    
    bool CheckSystemsReady()
    {
        bool vrReady = oculusVRSetup == null || oculusVRSetup.IsVRInitialized;
        bool trackingReady = bodyTrackingManager == null || bodyTrackingManager.IsSystemReady();
        bool performanceReady = performanceOptimizer == null || performanceOptimizer.enabled;
        
        return vrReady && trackingReady && performanceReady;
    }
    
    bool ShouldAutoCalibrate()
    {
        // Проверка необходимости автоматической калибровки
        return calibrationSystem != null && !PlayerPrefs.HasKey("CalibrationCompleted");
    }
    
    IEnumerator AutoCalibrate()
    {
        UpdateStartupStatus("Auto-calibrating...");
        
        if (calibrationSystem != null)
        {
            calibrationSystem.StartCalibration();
            
            // Ожидание завершения калибровки
            while (calibrationSystem.IsCalibrating)
            {
                yield return null;
            }
            
            PlayerPrefs.SetInt("CalibrationCompleted", 1);
            PlayerPrefs.Save();
            
            Debug.Log("Auto-calibration completed");
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    void StartMainFunctionality()
    {
        // Запуск основного функционала приложения
        
        // Включение трекинга если все системы готовы
        if (bodyTrackingManager != null && bodyTrackingManager.IsSystemReady())
        {
            Debug.Log("Starting body tracking...");
            // Трекинг уже должен быть включен через BodyTrackingManager
        }
        
        // Настройка VR параметров
        if (oculusVRSetup != null && oculusVRSetup.IsVRInitialized)
        {
            Debug.Log("VR system ready");
        }
        
        // Отображение главного UI
        if (trackingUI != null)
        {
            trackingUI.gameObject.SetActive(true);
        }
    }
    
    void ShowStartupScreen()
    {
        // Показать экран загрузки
        Debug.Log("Showing startup screen");
        // Здесь можно показать UI элемент с прогрессом загрузки
    }
    
    void HideStartupScreen()
    {
        // Скрыть экран загрузки
        Debug.Log("Hiding startup screen");
        // Здесь можно скрыть UI элемент загрузки
    }
    
    void UpdateStartupStatus(string status)
    {
        Debug.Log($"Startup: {status}");
        // Здесь можно обновить UI элемент с прогрессом загрузки
    }
    
    public void RestartApplication()
    {
        Debug.Log("Restarting application...");
        
        // Остановка всех систем
        StopAllSystems();
        
        // Перезапуск
        StartCoroutine(StartupSequence());
    }
    
    public void StopAllSystems()
    {
        Debug.Log("Stopping all systems...");
        
        // Остановка трекинга
        if (bodyTrackingManager != null)
        {
            bodyTrackingManager.StopProcessing();
        }
        
        // Остановка VR
        if (oculusVRSetup != null)
        {
            // VR системы останавливаются автоматически при OnDestroy
        }
        
        // Очистка UI
        if (trackingUI != null)
        {
            trackingUI.gameObject.SetActive(false);
        }
        
        isInitialized = false;
        isStartupComplete = false;
    }
    
    public void ToggleDebugMode()
    {
        enableDebugMode = !enableDebugMode;
        
        if (debugVisualizer != null)
        {
            debugVisualizer.enabled = enableDebugMode;
        }
        
        Debug.Log($"Debug mode {(enableDebugMode ? "enabled" : "disabled")}");
    }
    
    public void ForceCalibration()
    {
        if (calibrationSystem != null)
        {
            calibrationSystem.StartCalibration();
        }
        else
        {
            Debug.LogWarning("Calibration system not available!");
        }
    }
    
    public void ResetCalibration()
    {
        PlayerPrefs.DeleteKey("CalibrationCompleted");
        PlayerPrefs.Save();
        
        if (calibrationSystem != null)
        {
            calibrationSystem.ResetCalibration();
        }
        
        Debug.Log("Calibration reset");
    }
    
    public void SetPerformanceProfile(string profile)
    {
        if (performanceOptimizer != null)
        {
            switch (profile.ToLower())
            {
                case "low":
                    performanceOptimizer.SetTargetFrameRate(60);
                    performanceOptimizer.SetTrackingFrameRate(15);
                    break;
                case "medium":
                    performanceOptimizer.SetTargetFrameRate(72);
                    performanceOptimizer.SetTrackingFrameRate(30);
                    break;
                case "high":
                    performanceOptimizer.SetTargetFrameRate(90);
                    performanceOptimizer.SetTrackingFrameRate(30);
                    break;
                case "ultra":
                    performanceOptimizer.SetTargetFrameRate(120);
                    performanceOptimizer.SetTrackingFrameRate(60);
                    break;
                default:
                    Debug.LogWarning($"Unknown performance profile: {profile}");
                    break;
            }
            
            Debug.Log($"Performance profile set to: {profile}");
        }
    }
    
    void OnApplicationQuit()
    {
        Debug.Log("Application quitting...");
        
        // Сохранение настроек
        PlayerPrefs.Save();
        
        // Очистка ресурсов
        StopAllSystems();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("Application paused");
            
            // Приостановка трекинга для экономии ресурсов
            if (bodyTrackingManager != null)
            {
                bodyTrackingManager.StopProcessing();
            }
        }
        else
        {
            Debug.Log("Application resumed");
            
            // Возобновление трекинга
            if (bodyTrackingManager != null && isInitialized)
            {
                bodyTrackingManager.StartProcessing();
            }
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Debug.Log("Application lost focus");
            
            // Приостановка трекинга при потере фокуса
            if (bodyTrackingManager != null)
            {
                bodyTrackingManager.StopProcessing();
            }
        }
        else
        {
            Debug.Log("Application gained focus");
            
            // Возобновление трекинга при получении фокуса
            if (bodyTrackingManager != null && isInitialized)
            {
                bodyTrackingManager.StartProcessing();
            }
        }
    }
}
