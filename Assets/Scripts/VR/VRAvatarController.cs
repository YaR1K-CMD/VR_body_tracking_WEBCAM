using UnityEngine;
using System.Collections.Generic;

public class VRAvatarController : MonoBehaviour
{
    [Header("Avatar Setup")]
    public Animator avatarAnimator;
    public Transform avatarRoot;
    
    [Header("Body Mapping")]
    public Transform headBone;
    public Transform leftHandBone;
    public Transform rightHandBone;
    public Transform leftShoulderBone;
    public Transform rightShoulderBone;
    public Transform leftHipBone;
    public Transform rightHipBone;
    public Transform leftKneeBone;
    public Transform rightKneeBone;
    public Transform leftFootBone;
    public Transform rightFootBone;
    
    [Header("VR Tracking")]
    public Transform vrHead;
    public Transform vrLeftHand;
    public Transform vrRightHand;
    
    [Header("Body Tracking Integration")]
    public BodyTracker bodyTracker;
    public bool useBodyTracking = true;
    public bool useVRTracking = true;
    public float bodyTrackingWeight = 0.7f;
    public float vrTrackingWeight = 0.3f;
    
    [Header("Smoothing")]
    public float positionSmoothing = 0.1f;
    public float rotationSmoothing = 0.1f;
    
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
    private Vector3[] targetPositions;
    private Quaternion[] targetRotations;
    
    void Start()
    {
        InitializeBoneMapping();
        InitializeTargetArrays();
    }
    
    void InitializeBoneMapping()
    {
        boneMap.Clear();
        
        if (headBone != null) boneMap["Head"] = headBone;
        if (leftHandBone != null) boneMap["LeftHand"] = leftHandBone;
        if (rightHandBone != null) boneMap["RightHand"] = rightHandBone;
        if (leftShoulderBone != null) boneMap["LeftShoulder"] = leftShoulderBone;
        if (rightShoulderBone != null) boneMap["RightShoulder"] = rightShoulderBone;
        if (leftHipBone != null) boneMap["LeftHip"] = leftHipBone;
        if (rightHipBone != null) boneMap["RightHip"] = rightHipBone;
        if (leftKneeBone != null) boneMap["LeftKnee"] = leftKneeBone;
        if (rightKneeBone != null) boneMap["RightKnee"] = rightKneeBone;
        if (leftFootBone != null) boneMap["LeftFoot"] = leftFootBone;
        if (rightFootBone != null) boneMap["RightFoot"] = rightFootBone;
    }
    
    void InitializeTargetArrays()
    {
        int boneCount = boneMap.Count;
        targetPositions = new Vector3[boneCount];
        targetRotations = new Quaternion[boneCount];
        
        int index = 0;
        foreach (var bone in boneMap.Values)
        {
            targetPositions[index] = bone.position;
            targetRotations[index] = bone.rotation;
            index++;
        }
    }
    
    void Update()
    {
        if (!useBodyTracking && !useVRTracking) return;
        
        UpdateAvatarFromTracking();
        ApplySmoothing();
        UpdateAnimator();
    }
    
    void UpdateAvatarFromTracking()
    {
        if (useVRTracking)
        {
            UpdateFromVRTracking();
        }
        
        if (useBodyTracking && bodyTracker != null)
        {
            UpdateFromBodyTracking();
        }
    }
    
    void UpdateFromVRTracking()
    {
        if (vrHead != null && headBone != null)
        {
            targetPositions[GetBoneIndex("Head")] = vrHead.position;
            targetRotations[GetBoneIndex("Head")] = vrHead.rotation;
        }
        
        if (vrLeftHand != null && leftHandBone != null)
        {
            targetPositions[GetBoneIndex("LeftHand")] = vrLeftHand.position;
            targetRotations[GetBoneIndex("LeftHand")] = vrLeftHand.rotation;
        }
        
        if (vrRightHand != null && rightHandBone != null)
        {
            targetPositions[GetBoneIndex("RightHand")] = vrRightHand.position;
            targetRotations[GetBoneIndex("RightHand")] = vrRightHand.rotation;
        }
    }
    
    void UpdateFromBodyTracking()
    {
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.Nose) && headBone != null)
        {
            Vector3 headPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.Nose);
            targetPositions[GetBoneIndex("Head")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("Head")], 
                headPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftWrist) && leftHandBone != null)
        {
            Vector3 leftHandPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftWrist);
            targetPositions[GetBoneIndex("LeftHand")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("LeftHand")], 
                leftHandPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.RightWrist) && rightHandBone != null)
        {
            Vector3 rightHandPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.RightWrist);
            targetPositions[GetBoneIndex("RightHand")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("RightHand")], 
                rightHandPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftShoulder) && leftShoulderBone != null)
        {
            Vector3 leftShoulderPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftShoulder);
            targetPositions[GetBoneIndex("LeftShoulder")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("LeftShoulder")], 
                leftShoulderPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.RightShoulder) && rightShoulderBone != null)
        {
            Vector3 rightShoulderPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.RightShoulder);
            targetPositions[GetBoneIndex("RightShoulder")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("RightShoulder")], 
                rightShoulderPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.LeftHip) && leftHipBone != null)
        {
            Vector3 leftHipPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.LeftHip);
            targetPositions[GetBoneIndex("LeftHip")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("LeftHip")], 
                leftHipPos, 
                bodyTrackingWeight);
        }
        
        if (bodyTracker.IsLandmarkTracked(BodyTracker.LandmarkType.RightHip) && rightHipBone != null)
        {
            Vector3 rightHipPos = bodyTracker.GetLandmarkPosition(BodyTracker.LandmarkType.RightHip);
            targetPositions[GetBoneIndex("RightHip")] = Vector3.Lerp(
                targetPositions[GetBoneIndex("RightHip")], 
                rightHipPos, 
                bodyTrackingWeight);
        }
    }
    
    void ApplySmoothing()
    {
        int index = 0;
        foreach (var bone in boneMap.Values)
        {
            bone.position = Vector3.Lerp(bone.position, targetPositions[index], positionSmoothing);
            bone.rotation = Quaternion.Lerp(bone.rotation, targetRotations[index], rotationSmoothing);
            index++;
        }
    }
    
    void UpdateAnimator()
    {
        if (avatarAnimator == null) return;
        
        avatarAnimator.SetFloat("LeftHandX", GetHandPosition(leftHandBone).x);
        avatarAnimator.SetFloat("LeftHandY", GetHandPosition(leftHandBone).y);
        avatarAnimator.SetFloat("LeftHandZ", GetHandPosition(leftHandBone).z);
        
        avatarAnimator.SetFloat("RightHandX", GetHandPosition(rightHandBone).x);
        avatarAnimator.SetFloat("RightHandY", GetHandPosition(rightHandBone).y);
        avatarAnimator.SetFloat("RightHandZ", GetHandPosition(rightHandBone).z);
    }
    
    Vector3 GetHandPosition(Transform handBone)
    {
        if (handBone == null) return Vector3.zero;
        return handBone.localPosition;
    }
    
    int GetBoneIndex(string boneName)
    {
        int index = 0;
        foreach (var kvp in boneMap)
        {
            if (kvp.Key == boneName) return index;
            index++;
        }
        return 0;
    }
    
    public void ToggleBodyTracking()
    {
        useBodyTracking = !useBodyTracking;
    }
    
    public void ToggleVRTracking()
    {
        useVRTracking = !useVRTracking;
    }
    
    public void SetTrackingWeights(float bodyWeight, float vrWeight)
    {
        bodyTrackingWeight = Mathf.Clamp01(bodyWeight);
        vrTrackingWeight = Mathf.Clamp01(vrWeight);
    }
}
