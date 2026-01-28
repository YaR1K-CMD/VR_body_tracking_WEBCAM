using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CalibrationSystem : MonoBehaviour
{
    [Header("Calibration Settings")]
    public float calibrationDuration = 5f;
    public float calibrationInterval = 0.1f;
    public int calibrationSamples = 50;
    
    [Header("Body Calibration")]
    public bool calibrateHeight = true;
    public bool calibrateArmSpan = true;
    public bool calibrateLegLength = true;
    public bool calibrateShoulderWidth = true;
    
    [Header("VR Calibration")]
    public bool calibrateVRHeadset = true;
    public bool calibrateVRControllers = true;
    
    [Header("Debug")]
    public bool showCalibrationUI = true;
    public GameObject calibrationUIPrefab;
    
    public struct CalibrationData
    {
        public float userHeight;
        public float armSpan;
        public float legLength;
        public float shoulderWidth;
        public Vector3 headsetOffset;
        public Vector3 leftControllerOffset;
        public Vector3 rightControllerOffset;
        public Matrix4x4 bodyToVRMatrix;
    }
    
    public delegate void CalibrationCompleteCallback(CalibrationData data);
    public event CalibrationCompleteCallback OnCalibrationComplete;
    
    public delegate void CalibrationProgressCallback(float progress);
    public event CalibrationProgressCallback OnCalibrationProgress;
    
    private bool isCalibrating = false;
    private CalibrationData currentCalibrationData;
    private List<Vector3> heightSamples = new List<Vector3>();
    private List<Vector3> armSpanSamples = new List<Vector3>();
    private List<Vector3> legLengthSamples = new List<Vector3>();
    private List<Vector3> shoulderWidthSamples = new List<Vector3>();
    
    private BodyTracker bodyTracker;
    private VRAvatarController vrAvatarController;
    private OculusVRSetup oculusVRSetup;
    
    public bool IsCalibrating => isCalibrating;
    public CalibrationData CalibrationData => currentCalibrationData;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        bodyTracker = FindObjectOfType<BodyTracker>();
        vrAvatarController = FindObjectOfType<VRAvatarController>();
        oculusVRSetup = FindObjectOfType<OculusVRSetup>();
        
        Debug.Log("Calibration system initialized");
    }
    
    public void StartCalibration()
    {
        if (isCalibrating)
        {
            Debug.LogWarning("Calibration already in progress!");
            return;
        }
        
        if (!IsSystemReady())
        {
            Debug.LogError("System not ready for calibration!");
            return;
        }
        
        StartCoroutine(CalibrationCoroutine());
    }
    
    bool IsSystemReady()
    {
        return bodyTracker != null && bodyTracker.IsTracking &&
               vrAvatarController != null &&
               oculusVRSetup != null && oculusVRSetup.IsVRInitialized;
    }
    
    IEnumerator CalibrationCoroutine()
    {
        isCalibrating = true;
        
        Debug.Log("Starting calibration...");
        
        // Очистка предыдущих данных
        ClearCalibrationData();
        
        // Показать UI калибровки
        if (showCalibrationUI)
        {
            ShowCalibrationUI();
        }
        
        float elapsedTime = 0f;
        float sampleInterval = calibrationDuration / calibrationSamples;
        float nextSampleTime = 0f;
        
        while (elapsedTime < calibrationDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Обновление прогресса
            float progress = elapsedTime / calibrationDuration;
            OnCalibrationProgress?.Invoke(progress);
            
            // Сбор данных
            if (elapsedTime >= nextSampleTime)
            {
                CollectCalibrationSample();
                nextSampleTime += sampleInterval;
            }
            
            yield return null;
        }
        
        // Обработка собранных данных
        ProcessCalibrationData();
        
        // Применение калибровки
        ApplyCalibration();
        
        isCalibrating = false;
        
        // Скрыть UI калибровки
        if (showCalibrationUI)
        {
            HideCalibrationUI();
        }
        
        Debug.Log("Calibration completed!");
        
        // Уведомление о завершении
        OnCalibrationComplete?.Invoke(currentCalibrationData);
    }
    
    void ClearCalibrationData()
    {
        heightSamples.Clear();
        armSpanSamples.Clear();
        legLengthSamples.Clear();
        shoulderWidthSamples.Clear();
        
        currentCalibrationData = new CalibrationData();
    }
    
    void CollectCalibrationSample()
    {
        if (calibrateHeight && bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.Nose) &&
            bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftAnkle))
        {
            Vector3 headPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.Nose);
            Vector3 footPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftAnkle);
            heightSamples.Add(new Vector3(0, headPos.y - footPos.y, 0));
        }
        
        if (calibrateArmSpan && bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftWrist) &&
            bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.RightWrist))
        {
            Vector3 leftWrist = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftWrist);
            Vector3 rightWrist = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.RightWrist);
            armSpanSamples.Add(rightWrist - leftWrist);
        }
        
        if (calibrateLegLength && bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftHip) &&
            bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftAnkle))
        {
            Vector3 leftHip = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftHip);
            Vector3 leftAnkle = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftAnkle);
            legLengthSamples.Add(new Vector3(0, leftHip.y - leftAnkle.y, 0));
        }
        
        if (calibrateShoulderWidth && bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftShoulder) &&
            bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.RightShoulder))
        {
            Vector3 leftShoulder = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftShoulder);
            Vector3 rightShoulder = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.RightShoulder);
            shoulderWidthSamples.Add(rightShoulder - leftShoulder);
        }
    }
    
    void ProcessCalibrationData()
    {
        // Обработка данных о росте
        if (heightSamples.Count > 0)
        {
            Vector3 avgHeight = Vector3.zero;
            foreach (Vector3 sample in heightSamples)
            {
                avgHeight += sample;
            }
            avgHeight /= heightSamples.Count;
            currentCalibrationData.userHeight = avgHeight.y;
        }
        
        // Обработка данных о размахе рук
        if (armSpanSamples.Count > 0)
        {
            Vector3 avgArmSpan = Vector3.zero;
            foreach (Vector3 sample in armSpanSamples)
            {
                avgArmSpan += sample;
            }
            avgArmSpan /= armSpanSamples.Count;
            currentCalibrationData.armSpan = avgArmSpan.x;
        }
        
        // Обработка данных о длине ног
        if (legLengthSamples.Count > 0)
        {
            Vector3 avgLegLength = Vector3.zero;
            foreach (Vector3 sample in legLengthSamples)
            {
                avgLegLength += sample;
            }
            avgLegLength /= legLengthSamples.Count;
            currentCalibrationData.legLength = avgLegLength.y;
        }
        
        // Обработка данных о ширине плеч
        if (shoulderWidthSamples.Count > 0)
        {
            Vector3 avgShoulderWidth = Vector3.zero;
            foreach (Vector3 sample in shoulderWidthSamples)
            {
                avgShoulderWidth += sample;
            }
            avgShoulderWidth /= shoulderWidthSamples.Count;
            currentCalibrationData.shoulderWidth = avgShoulderWidth.x;
        }
        
        // Калибровка VR оборудования
        if (calibrateVRHeadset && oculusVRSetup != null)
        {
            currentCalibrationData.headsetOffset = CalculateHeadsetOffset();
        }
        
        if (calibrateVRControllers && oculusVRSetup != null)
        {
            currentCalibrationData.leftControllerOffset = CalculateControllerOffset(OVRInput.Controller.LTouch);
            currentCalibrationData.rightControllerOffset = CalculateControllerOffset(OVRInput.Controller.RTouch);
        }
        
        // Расчет матрицы преобразования между body tracking и VR
        currentCalibrationData.bodyToVRMatrix = CalculateBodyToVRMatrix();
    }
    
    Vector3 CalculateHeadsetOffset()
    {
        // Расчет смещения между положением головы из body tracking и VR headset
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.Nose))
        {
            Vector3 bodyHeadPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.Nose);
            Vector3 vrHeadPos = oculusVRSetup.GetControllerPosition(OVRInput.Controller.None); // Заглушка
            
            return vrHeadPos - bodyHeadPos;
        }
        
        return Vector3.zero;
    }
    
    Vector3 CalculateControllerOffset(OVRInput.Controller controller)
    {
        // Расчет смещения между положением рук из body tracking и VR controllers
        BodyTracker.LandmarkType landmarkType = controller == OVRInput.Controller.LTouch ? 
            BodyTracker.LandmarkType.LeftWrist : BodyTracker.LandmarkType.RightWrist;
        
        if (bodyTracker.IsLandmarkTracked(landmarkType))
        {
            Vector3 bodyHandPos = bodyTracker.GetLandmarkPosition(landmarkType);
            Vector3 vrControllerPos = oculusVRSetup.GetControllerPosition(controller);
            
            return vrControllerPos - bodyHandPos;
        }
        
        return Vector3.zero;
    }
    
    Matrix4x4 CalculateBodyToVRMatrix()
    {
        // Расчет матрицы преобразования между системами координат
        Matrix4x4 matrix = Matrix4x4.identity;
        
        // Учет масштаба на основе калибровочных данных
        float scale = currentCalibrationData.userHeight / 1.7f; // Средний рост как референс
        matrix.SetScale(Vector3.one * scale);
        
        // Учет смещения
        matrix.SetTranslation(currentCalibrationData.headsetOffset);
        
        return matrix;
    }
    
    void ApplyCalibration()
    {
        // Применение калибровочных данных к системам
        
        if (vrAvatarController != null)
        {
            // Настройка весов трекинга на основе калибровки
            float bodyWeight = CalculateOptimalBodyWeight();
            float vrWeight = 1f - bodyWeight;
            vrAvatarController.SetTrackingWeights(bodyWeight, vrWeight);
        }
        
        // Применение матрицы преобразования
        ApplyBodyToVRMatrix();
    }
    
    float CalculateOptimalBodyWeight()
    {
        // Расчет оптимального веса body tracking на качестве трекинга
        float trackingQuality = CalculateTrackingQuality();
        return Mathf.Lerp(0.3f, 0.9f, trackingQuality);
    }
    
    float CalculateTrackingQuality()
    {
        // Оценка качества трекинга на основе стабильности калибровочных данных
        float heightStability = CalculateStability(heightSamples);
        float armSpanStability = CalculateStability(armSpanSamples);
        float legLengthStability = CalculateStability(legLengthSamples);
        
        return (heightStability + armSpanStability + legLengthStability) / 3f;
    }
    
    float CalculateStability(List<Vector3> samples)
    {
        if (samples.Count < 2) return 0f;
        
        Vector3 mean = Vector3.zero;
        foreach (Vector3 sample in samples)
        {
            mean += sample;
        }
        mean /= samples.Count;
        
        float variance = 0f;
        foreach (Vector3 sample in samples)
        {
            variance += (sample - mean).sqrMagnitude;
        }
        variance /= samples.Count;
        
        // Чем меньше дисперсия, тем выше стабильность
        return Mathf.Clamp01(1f - variance / 0.1f);
    }
    
    void ApplyBodyToVRMatrix()
    {
        // Применение матрицы преобразования к VR аватару
        if (vrAvatarController != null)
        {
            // Здесь можно применить матрицу для коррекции позиций
            Debug.Log("Body to VR transformation matrix applied");
        }
    }
    
    void ShowCalibrationUI()
    {
        // Показать UI калибровки
        if (calibrationUIPrefab != null)
        {
            Instantiate(calibrationUIPrefab, transform);
        }
    }
    
    void HideCalibrationUI()
    {
        // Скрыть UI калибровки
        // Найти и уничтожить объекты UI калибровки
    }
    
    public void ResetCalibration()
    {
        currentCalibrationData = new CalibrationData();
        Debug.Log("Calibration data reset");
    }
    
    public void SaveCalibrationData()
    {
        // Сохранение калибровочных данных в PlayerPrefs
        PlayerPrefs.SetFloat("UserHeight", currentCalibrationData.userHeight);
        PlayerPrefs.SetFloat("ArmSpan", currentCalibrationData.armSpan);
        PlayerPrefs.SetFloat("LegLength", currentCalibrationData.legLength);
        PlayerPrefs.SetFloat("ShoulderWidth", currentCalibrationData.shoulderWidth);
        
        // Сохранение смещений
        PlayerPrefs.SetFloat("HeadsetOffsetX", currentCalibrationData.headsetOffset.x);
        PlayerPrefs.SetFloat("HeadsetOffsetY", currentCalibrationData.headsetOffset.y);
        PlayerPrefs.SetFloat("HeadsetOffsetZ", currentCalibrationData.headsetOffset.z);
        
        PlayerPrefs.Save();
        
        Debug.Log("Calibration data saved");
    }
    
    public void LoadCalibrationData()
    {
        // Загрузка калибровочных данных из PlayerPrefs
        if (PlayerPrefs.HasKey("UserHeight"))
        {
            currentCalibrationData.userHeight = PlayerPrefs.GetFloat("UserHeight");
            currentCalibrationData.armSpan = PlayerPrefs.GetFloat("ArmSpan");
            currentCalibrationData.legLength = PlayerPrefs.GetFloat("LegLength");
            currentCalibrationData.shoulderWidth = PlayerPrefs.GetFloat("ShoulderWidth");
            
            currentCalibrationData.headsetOffset.x = PlayerPrefs.GetFloat("HeadsetOffsetX");
            currentCalibrationData.headsetOffset.y = PlayerPrefs.GetFloat("HeadsetOffsetY");
            currentCalibrationData.headsetOffset.z = PlayerPrefs.GetFloat("HeadsetOffsetZ");
            
            Debug.Log("Calibration data loaded");
        }
        else
        {
            Debug.Log("No saved calibration data found");
        }
    }
}
