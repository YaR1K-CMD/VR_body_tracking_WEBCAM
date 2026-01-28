using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Oculus;
using UnityEngine.XR.Management;

public class OculusVRSetup : MonoBehaviour
{
    [Header("VR Settings")]
    public bool enableVR = true;
    public bool enablePassthrough = false;
    public bool enableHandTracking = true;
    
    [Header("Performance")]
    public int targetFrameRate = 90;
    public bool enableFixedFoveatedRendering = true;
    public OculusSettings.FixedFoveatedRenderingLevel foveationLevel = OculusSettings.FixedFoveatedRenderingLevel.High;
    
    [Header("Controllers")]
    public bool enableControllers = true;
    public float controllerUpdateFrequency = 90f;
    
    private bool isVRInitialized = false;
    private bool isHandTrackingEnabled = false;
    
    public static OculusVRSetup Instance { get; private set; }
    
    public bool IsVRInitialized => isVRInitialized;
    public bool IsHandTrackingEnabled => isHandTrackingEnabled;
    
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
        if (enableVR)
        {
            InitializeVR();
        }
    }
    
    void InitializeVR()
    {
        StartCoroutine(InitializeVRCoroutine());
    }
    
    IEnumerator InitializeVRCoroutine()
    {
        Debug.Log("Initializing VR...");
        
        // Проверка доступности VR
        if (!XRSettings.isDeviceActive)
        {
            yield return new WaitForSeconds(1f);
        }
        
        // Настройка XR Loader
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.LogError("XR General Settings not found!");
            yield break;
        }
        
        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.LogError("XR Manager not found!");
            yield break;
        }
        
        // Инициализация XR
        if (!xrManager.isInitializationComplete)
        {
            yield return StartCoroutine(xrManager.InitializeLoader());
        }
        
        if (xrManager.activeLoader == null)
        {
            Debug.LogError("Failed to initialize XR loader!");
            yield break;
        }
        
        // Запуск XR subsystems
        xrManager.StartSubsystems();
        
        // Настройка Oculus-specific параметров
        ConfigureOculusSettings();
        
        // Настройка производительности
        ConfigurePerformanceSettings();
        
        // Включение hand tracking если нужно
        if (enableHandTracking)
        {
            EnableHandTracking();
        }
        
        // Включение passthrough если нужно
        if (enablePassthrough)
        {
            EnablePassthrough();
        }
        
        isVRInitialized = true;
        Debug.Log("VR initialized successfully!");
        
        // Настройка контроллеров
        if (enableControllers)
        {
            SetupControllers();
        }
    }
    
    void ConfigureOculusSettings()
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            // Настройка fixed foveated rendering
            if (enableFixedFoveatedRendering)
            {
                oculusSettings.fixedFoveatedRenderingLevel = foveationLevel;
            }
            
            // Настройка hand tracking
            if (enableHandTracking)
            {
                oculusSettings.handTrackingSupport = true;
            }
            
            Debug.Log("Oculus settings configured");
        }
        else
        {
            Debug.LogWarning("Oculus Settings not found!");
        }
    }
    
    void ConfigurePerformanceSettings()
    {
        // Установка целевого FPS
        Application.targetFrameRate = targetFrameRate;
        
        // Настройка качества рендеринга для Quest 3
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadowDistance = 10f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.lodBias = 0.7f;
        
        Debug.Log($"Performance settings configured - Target FPS: {targetFrameRate}");
    }
    
    void EnableHandTracking()
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            oculusSettings.handTrackingSupport = true;
            isHandTrackingEnabled = true;
            
            Debug.Log("Hand tracking enabled");
            
            // Подписка на события hand tracking
            OVRManager.HandTracking += OnHandTrackingUpdated;
        }
    }
    
    void EnablePassthrough()
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            oculusSettings.insightPassthroughSupport = true;
            
            Debug.Log("Passthrough enabled");
        }
    }
    
    void SetupControllers()
    {
        // Настройка частоты обновления контроллеров
        OVRManager.controllerUpdateFrequency = controllerUpdateFrequency;
        
        Debug.Log($"Controllers setup complete - Update frequency: {controllerUpdateFrequency}Hz");
    }
    
    void OnHandTrackingUpdated(OVRPlugin.HandState handState)
    {
        // Обработка обновлений hand tracking
        // Здесь можно получить данные о положении рук
    }
    
    public Vector3 GetControllerPosition(OVRInput.Controller controller)
    {
        if (enableControllers)
        {
            return OVRInput.GetLocalControllerPosition(controller);
        }
        return Vector3.zero;
    }
    
    public Quaternion GetControllerRotation(OVRInput.Controller controller)
    {
        if (enableControllers)
        {
            return OVRInput.GetLocalControllerRotation(controller);
        }
        return Quaternion.identity;
    }
    
    public bool IsControllerConnected(OVRInput.Controller controller)
    {
        if (enableControllers)
        {
            return OVRInput.IsControllerConnected(controller);
        }
        return false;
    }
    
    public Vector3 GetHandPosition(OVRPlugin.Hand hand)
    {
        if (isHandTrackingEnabled)
        {
            OVRPlugin.GetHandState(hand, out OVRPlugin.HandState handState);
            return handState.RootPose.Position;
        }
        return Vector3.zero;
    }
    
    public Quaternion GetHandRotation(OVRPlugin.Hand hand)
    {
        if (isHandTrackingEnabled)
        {
            OVRPlugin.GetHandState(hand, out OVRPlugin.HandState handState);
            return handState.RootPose.Orientation;
        }
        return Quaternion.identity;
    }
    
    public bool IsHandTracked(OVRPlugin.Hand hand)
    {
        if (isHandTrackingEnabled)
        {
            OVRPlugin.GetHandState(hand, out OVRPlugin.HandState handState);
            return handState.Status == OVRPlugin.HandStatus.Tracked;
        }
        return false;
    }
    
    public void TogglePassthrough()
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            enablePassthrough = !enablePassthrough;
            oculusSettings.insightPassthroughSupport = enablePassthrough;
            
            Debug.Log($"Passthrough {(enablePassthrough ? "enabled" : "disabled")}");
        }
    }
    
    public void ToggleHandTracking()
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            enableHandTracking = !enableHandTracking;
            oculusSettings.handTrackingSupport = enableHandTracking;
            
            if (enableHandTracking)
            {
                OVRManager.HandTracking += OnHandTrackingUpdated;
            }
            else
            {
                OVRManager.HandTracking -= OnHandTrackingUpdated;
            }
            
            isHandTrackingEnabled = enableHandTracking;
            Debug.Log($"Hand tracking {(enableHandTracking ? "enabled" : "disabled")}");
        }
    }
    
    public void SetFoveationLevel(OculusSettings.FixedFoveatedRenderingLevel level)
    {
        var oculusSettings = OculusSettings.Instance;
        if (oculusSettings != null)
        {
            foveationLevel = level;
            oculusSettings.fixedFoveatedRenderingLevel = level;
            
            Debug.Log($"Foveation level set to: {level}");
        }
    }
    
    void OnDestroy()
    {
        // Очистка подписок на события
        if (isHandTrackingEnabled)
        {
            OVRManager.HandTracking -= OnHandTrackingUpdated;
        }
        
        // Остановка VR subsystems
        if (isVRInitialized)
        {
            var xrManager = XRGeneralSettings.Instance?.Manager;
            if (xrManager != null && xrManager.activeLoader != null)
            {
                xrManager.StopSubsystems();
                xrManager.DeinitializeLoader();
            }
        }
        
        Debug.Log("VR system shutdown complete");
    }
}
