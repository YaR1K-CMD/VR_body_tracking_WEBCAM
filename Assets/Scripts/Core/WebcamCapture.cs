using UnityEngine;
using System.Collections;

public class WebcamCapture : MonoBehaviour
{
    [Header("Webcam Settings")]
    public int webcamWidth = 1280;
    public int webcamHeight = 720;
    public int webcamFPS = 30;
    
    private WebCamTexture webcamTexture;
    private Renderer renderer;
    
    public bool IsInitialized { get; private set; }
    public Texture2D CurrentFrame { get; private set; }
    
    void Start()
    {
        InitializeWebcam();
    }
    
    void InitializeWebcam()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No webcam devices found!");
            return;
        }
        
        WebCamDevice device = WebCamTexture.devices[0];
        webcamTexture = new WebCamTexture(device.name, webcamWidth, webcamHeight, webcamFPS);
        
        renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = webcamTexture;
        }
        
        webcamTexture.Play();
        IsInitialized = true;
        
        Debug.Log($"Webcam initialized: {device.name}");
    }
    
    void Update()
    {
        if (webcamTexture != null && webcamTexture.isPlaying && webcamTexture.didUpdateThisFrame)
        {
            UpdateCurrentFrame();
        }
    }
    
    void UpdateCurrentFrame()
    {
        if (CurrentFrame == null || CurrentFrame.width != webcamTexture.width || CurrentFrame.height != webcamTexture.height)
        {
            CurrentFrame = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        }
        
        Color32[] pixels = webcamTexture.GetPixels32();
        CurrentFrame.SetPixels32(pixels);
        CurrentFrame.Apply();
    }
    
    public Color32[] GetFrameData()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            return webcamTexture.GetPixels32();
        }
        return null;
    }
    
    public void StopWebcam()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
    
    void OnDestroy()
    {
        StopWebcam();
    }
}
