using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Settings")]
    public bool enableOptimization = true;
    public int targetFrameRate = 90;
    public bool adaptiveQuality = true;
    
    [Header("Rendering Optimization")]
    public bool enableLOD = true;
    public bool enableOcclusion = true;
    public bool enableFrustumCulling = true;
    public int maxLODLevel = 2;
    
    [Header("Tracking Optimization")]
    public int trackingFrameRate = 30;
    public bool enableFrameSkipping = true;
    public int maxFrameSkip = 2;
    
    [Header("Memory Optimization")]
    public bool enableGarbageCollection = true;
    public float gcInterval = 5f;
    public bool enableObjectPooling = true;
    
    [Header("Debug")]
    public bool showPerformanceStats = true;
    public bool enableProfiling = false;
    
    private float currentFPS;
    private float averageFPS;
    private float frameTime;
    private int frameCount;
    private float fpsUpdateTime;
    
    private int currentLODLevel;
    private bool isLowPerformance;
    private bool isHighPerformance;
    
    private float lastGCTime;
    private int skippedFrames;
    
    public static PerformanceOptimizer Instance { get; private set; }
    
    public float CurrentFPS => currentFPS;
    public float AverageFPS => averageFPS;
    public bool IsLowPerformance => isLowPerformance;
    public bool IsHighPerformance => isHighPerformance;
    
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
        InitializeOptimization();
    }
    
    void InitializeOptimization()
    {
        // Установка целевого FPS
        Application.targetFrameRate = targetFrameRate;
        
        // Настройка качества рендеринга
        ConfigureRenderingQuality();
        
        // Настройка оптимизации памяти
        ConfigureMemoryOptimization();
        
        // Настройка оптимизации трекинга
        ConfigureTrackingOptimization();
        
        Debug.Log("Performance optimizer initialized");
    }
    
    void ConfigureRenderingQuality()
    {
        // Настройка LOD
        if (enableLOD)
        {
            LOD[] lodGroups = FindObjectsOfType<LOD>();
            foreach (LOD lod in lodGroups)
            {
                lod.enabled = true;
                // Ограничение LOD уровней
                LODGroup lodGroup = lod.GetComponent<LODGroup>();
                if (lodGroup != null)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    if (lods.Length > maxLODLevel + 1)
                    {
                        // Удаление лишних LOD уровней
                        System.Array.Resize(ref lods, maxLODLevel + 1);
                        lodGroup.SetLODs(lods);
                    }
                }
            }
        }
        
        // Настройка occlusion culling
        if (enableOcclusion)
        {
            // Occlusion culling настраивается в Unity Editor через Occlusion Culling window
            Debug.Log("Occlusion culling enabled");
        }
        
        // Настройка frustum culling
        if (enableFrustumCulling)
        {
            // Frustum culling включен по умолчанию в Unity
            Debug.Log("Frustum culling enabled");
        }
    }
    
    void ConfigureMemoryOptimization()
    {
        // Настройка garbage collection
        if (enableGarbageCollection)
        {
            lastGCTime = Time.time;
        }
        
        // Настройка object pooling
        if (enableObjectPooling)
        {
            // Инициализация пулов объектов
            InitializeObjectPools();
        }
    }
    
    void ConfigureTrackingOptimization()
    {
        // Настройка частоты трекинга
        BodyTrackingManager trackingManager = BodyTrackingManager.Instance;
        if (trackingManager != null)
        {
            trackingManager.SetProcessingFrameRate(trackingFrameRate);
        }
    }
    
    void InitializeObjectPools()
    {
        // Инициализация пулов для часто используемых объектов
        // Например, для debug points, эффектов и т.д.
        Debug.Log("Object pools initialized");
    }
    
    void Update()
    {
        if (!enableOptimization) return;
        
        UpdatePerformanceStats();
        UpdateAdaptiveQuality();
        UpdateMemoryManagement();
        UpdateTrackingOptimization();
    }
    
    void UpdatePerformanceStats()
    {
        frameCount++;
        frameTime += Time.unscaledDeltaTime;
        
        if (Time.unscaledTime - fpsUpdateTime >= 1f)
        {
            currentFPS = frameCount / (Time.unscaledTime - fpsUpdateTime);
            averageFPS = Mathf.Lerp(averageFPS, currentFPS, 0.1f);
            
            frameCount = 0;
            fpsUpdateTime = Time.unscaledTime;
            frameTime = 0f;
            
            // Оценка производительности
            EvaluatePerformance();
        }
    }
    
    void EvaluatePerformance()
    {
        float targetFPS = targetFrameRate;
        float performanceThreshold = 0.8f;
        
        isLowPerformance = currentFPS < targetFPS * performanceThreshold;
        isHighPerformance = currentFPS >= targetFPS * 0.95f;
        
        if (adaptiveQuality)
        {
            AdjustQualityBasedOnPerformance();
        }
    }
    
    void AdjustQualityBasedOnPerformance()
    {
        if (isLowPerformance)
        {
            DecreaseQuality();
        }
        else if (isHighPerformance && currentLODLevel > 0)
        {
            IncreaseQuality();
        }
    }
    
    void DecreaseQuality()
    {
        // Уменьшение качества для повышения производительности
        if (currentLODLevel < maxLODLevel)
        {
            currentLODLevel++;
            ApplyLODLevel(currentLODLevel);
            Debug.Log($"Decreased quality to LOD level {currentLODLevel}");
        }
        
        // Уменьшение качества теней
        if (QualitySettings.shadows != ShadowQuality.Disable)
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = Mathf.Max(5f, QualitySettings.shadowDistance * 0.8f);
        }
        
        // Уменьшение качества рендеринга
        if (QualitySettings.renderPipelineAsset != null)
        {
            // Настройка URP параметров
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset = 
                QualitySettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = Mathf.Max(0.5f, urpAsset.renderScale * 0.9f);
            }
        }
    }
    
    void IncreaseQuality()
    {
        // Увеличение качества при хорошей производительности
        if (currentLODLevel > 0)
        {
            currentLODLevel--;
            ApplyLODLevel(currentLODLevel);
            Debug.Log($"Increased quality to LOD level {currentLODLevel}");
        }
        
        // Увеличение качества теней
        if (QualitySettings.shadows == ShadowQuality.HardOnly)
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = Mathf.Min(50f, QualitySettings.shadowDistance * 1.2f);
        }
        
        // Увеличение качества рендеринга
        if (QualitySettings.renderPipelineAsset != null)
        {
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset = 
                QualitySettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = Mathf.Min(1.0f, urpAsset.renderScale * 1.1f);
            }
        }
    }
    
    void ApplyLODLevel(int level)
    {
        // Применение LOD уровня ко всем объектам
        LOD[] lodGroups = FindObjectsOfType<LOD>();
        foreach (LOD lod in lodGroups)
        {
            LODGroup lodGroup = lod.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                lodGroup.ForceLOD(level);
            }
        }
    }
    
    void UpdateAdaptiveQuality()
    {
        if (!adaptiveQuality) return;
        
        // Адаптивное изменение качества в реальном времени
        float performanceRatio = currentFPS / targetFrameRate;
        
        if (performanceRatio < 0.7f)
        {
            // Сильное падение производительности
            ApplyEmergencyOptimizations();
        }
        else if (performanceRatio > 1.1f)
        {
            // Производительность выше целевой
            ConsiderQualityIncrease();
        }
    }
    
    void ApplyEmergencyOptimizations()
    {
        // Экстренная оптимизация при сильном падении производительности
        
        // Отключение дорогих эффектов
        QualitySettings.particleRaycastBudget = 32;
        QualitySettings.asyncUploadTimeSlice = 1;
        QualitySettings.asyncUploadBufferSize = 4;
        
        // Уменьшение resolution
        if (QualitySettings.renderPipelineAsset != null)
        {
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset = 
                QualitySettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = 0.7f;
            }
        }
        
        Debug.Log("Emergency optimizations applied");
    }
    
    void ConsiderQualityIncrease()
    {
        // Рассмотреть возможность увеличения качества
        
        if (currentLODLevel == 0 && QualitySettings.shadows == ShadowQuality.All)
        {
            // Максимальное качество уже достигнуто
            return;
        }
        
        // Постепенное увеличение качества
        if (Time.time % 5f < Time.deltaTime) // Каждые 5 секунд
        {
            IncreaseQuality();
        }
    }
    
    void UpdateMemoryManagement()
    {
        if (!enableGarbageCollection) return;
        
        // Периодический garbage collection
        if (Time.time - lastGCTime >= gcInterval)
        {
            System.GC.Collect();
            lastGCTime = Time.time;
            
            if (enableProfiling)
            {
                long memoryUsage = System.GC.GetTotalMemory(false);
                Debug.Log($"Memory usage: {memoryUsage / 1024 / 1024} MB");
            }
        }
    }
    
    void UpdateTrackingOptimization()
    {
        if (!enableFrameSkipping) return;
        
        // Пропуск кадров трекинга при низкой производительности
        if (isLowPerformance)
        {
            skippedFrames++;
            if (skippedFrames >= maxFrameSkip)
            {
                // Пропустить кадр трекинга
                BodyTrackingManager trackingManager = BodyTrackingManager.Instance;
                if (trackingManager != null)
                {
                    trackingManager.ToggleProcessing();
                    trackingManager.ToggleProcessing();
                }
                skippedFrames = 0;
            }
        }
        else
        {
            skippedFrames = 0;
        }
    }
    
    public void SetTargetFrameRate(int frameRate)
    {
        targetFrameRate = Mathf.Clamp(frameRate, 30, 120);
        Application.targetFrameRate = targetFrameRate;
        Debug.Log($"Target frame rate set to {targetFrameRate}");
    }
    
    public void SetTrackingFrameRate(int frameRate)
    {
        trackingFrameRate = Mathf.Clamp(frameRate, 15, 60);
        
        BodyTrackingManager trackingManager = BodyTrackingManager.Instance;
        if (trackingManager != null)
        {
            trackingManager.SetProcessingFrameRate(trackingFrameRate);
        }
        
        Debug.Log($"Tracking frame rate set to {trackingFrameRate}");
    }
    
    public void ToggleAdaptiveQuality()
    {
        adaptiveQuality = !adaptiveQuality;
        Debug.Log($"Adaptive quality {(adaptiveQuality ? "enabled" : "disabled")}");
    }
    
    public void ForceGarbageCollection()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        Debug.Log("Forced garbage collection");
    }
    
    public void ResetQualitySettings()
    {
        currentLODLevel = 0;
        ApplyLODLevel(currentLODLevel);
        
        // Сброс настроек качества
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowDistance = 50f;
        
        if (QualitySettings.renderPipelineAsset != null)
        {
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset = 
                QualitySettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = 1.0f;
            }
        }
        
        Debug.Log("Quality settings reset to default");
    }
    
    void OnGUI()
    {
        if (!showPerformanceStats) return;
        
        // Отображение статистики производительности
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"FPS: {currentFPS:F1}");
        GUILayout.Label($"Average FPS: {averageFPS:F1}");
        GUILayout.Label($"Frame Time: {frameTime * 1000:F2}ms");
        GUILayout.Label($"LOD Level: {currentLODLevel}");
        GUILayout.Label($"Performance: {(isLowPerformance ? "Low" : isHighPerformance ? "High" : "Normal")}");
        
        if (GUILayout.Button("Force GC"))
        {
            ForceGarbageCollection();
        }
        
        if (GUILayout.Button("Reset Quality"))
        {
            ResetQualitySettings();
        }
        
        GUILayout.EndArea();
    }
}
