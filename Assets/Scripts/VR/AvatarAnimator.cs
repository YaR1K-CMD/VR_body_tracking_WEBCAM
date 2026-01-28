using UnityEngine;

public class AvatarAnimator : MonoBehaviour
{
    [Header("Animator Setup")]
    public Animator animator;
    public HumanBodyBones[] trackedBones;
    
    [Header("Animation Parameters")]
    public float movementThreshold = 0.01f;
    public float rotationThreshold = 1f;
    public float animationSpeed = 1f;
    
    [Header("IK Settings")]
    public bool enableIK = true;
    public float ikWeight = 1f;
    public Transform ikTarget;
    
    private Vector3[] previousPositions;
    private Quaternion[] previousRotations;
    private bool[] isMoving;
    private bool[] isRotating;
    
    void Start()
    {
        InitializeAnimator();
        InitializeTrackingArrays();
    }
    
    void InitializeAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
            return;
        }
        
        // Настройка параметров аниматора
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        
        Debug.Log("Animator initialized");
    }
    
    void InitializeTrackingArrays()
    {
        if (trackedBones == null || trackedBones.Length == 0)
        {
            trackedBones = new HumanBodyBones[]
            {
                HumanBodyBones.Head,
                HumanBodyBones.LeftHand,
                HumanBodyBones.RightHand,
                HumanBodyBones.LeftShoulder,
                HumanBodyBones.RightShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.RightFoot
            };
        }
        
        int boneCount = trackedBones.Length;
        previousPositions = new Vector3[boneCount];
        previousRotations = new Quaternion[boneCount];
        isMoving = new bool[boneCount];
        isRotating = new bool[boneCount];
        
        // Инициализация начальных позиций
        for (int i = 0; i < boneCount; i++)
        {
            Transform boneTransform = animator.GetBoneTransform(trackedBones[i]);
            if (boneTransform != null)
            {
                previousPositions[i] = boneTransform.position;
                previousRotations[i] = boneTransform.rotation;
            }
        }
        
        Debug.Log($"Tracking arrays initialized for {boneCount} bones");
    }
    
    void Update()
    {
        if (animator == null) return;
        
        UpdateBoneTracking();
        UpdateAnimationParameters();
        UpdateIK();
    }
    
    void UpdateBoneTracking()
    {
        for (int i = 0; i < trackedBones.Length; i++)
        {
            Transform boneTransform = animator.GetBoneTransform(trackedBones[i]);
            if (boneTransform == null) continue;
            
            Vector3 currentPosition = boneTransform.position;
            Quaternion currentRotation = boneTransform.rotation;
            
            // Проверка движения
            Vector3 positionDelta = currentPosition - previousPositions[i];
            isMoving[i] = positionDelta.magnitude > movementThreshold;
            
            // Проверка вращения
            float rotationDelta = Quaternion.Angle(currentRotation, previousRotations[i]);
            isRotating[i] = rotationDelta > rotationThreshold;
            
            // Обновление предыдущих значений
            previousPositions[i] = currentPosition;
            previousRotations[i] = currentRotation;
        }
    }
    
    void UpdateAnimationParameters()
    {
        // Обновление параметров аниматора на основе движения костей
        
        // Скорость движения (на основе рук и ног)
        float movementSpeed = CalculateMovementSpeed();
        animator.SetFloat("MovementSpeed", movementSpeed * animationSpeed);
        
        // Положение рук
        UpdateHandParameters();
        
        // Положение ног
        UpdateLegParameters();
        
        // Положение головы
        UpdateHeadParameters();
        
        // Общие параметры
        animator.SetFloat("AnimationSpeed", animationSpeed);
    }
    
    float CalculateMovementSpeed()
    {
        float totalSpeed = 0f;
        int movingBones = 0;
        
        // Учитываем движение рук и ног для определения скорости
        for (int i = 0; i < trackedBones.Length; i++)
        {
            if (isMoving[i])
            {
                HumanBodyBones bone = trackedBones[i];
                if (IsMovementBone(bone))
                {
                    Vector3 positionDelta = animator.GetBoneTransform(bone).position - previousPositions[i];
                    totalSpeed += positionDelta.magnitude;
                    movingBones++;
                }
            }
        }
        
        return movingBones > 0 ? totalSpeed / movingBones : 0f;
    }
    
    bool IsMovementBone(HumanBodyBones bone)
    {
        return bone == HumanBodyBones.LeftHand || bone == HumanBodyBones.RightHand ||
               bone == HumanBodyBones.LeftFoot || bone == HumanBodyBones.RightFoot ||
               bone == HumanBodyBones.LeftLowerLeg || bone == HumanBodyBones.RightLowerLeg;
    }
    
    void UpdateHandParameters()
    {
        // Левая рука
        Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        if (leftHand != null)
        {
            Vector3 leftHandPos = leftHand.position;
            animator.SetFloat("LeftHandX", leftHandPos.x);
            animator.SetFloat("LeftHandY", leftHandPos.y);
            animator.SetFloat("LeftHandZ", leftHandPos.z);
            
            // Проверка, поднята ли рука
            bool leftHandRaised = leftHandPos.y > transform.position.y + 0.5f;
            animator.SetBool("LeftHandRaised", leftHandRaised);
        }
        
        // Правая рука
        Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (rightHand != null)
        {
            Vector3 rightHandPos = rightHand.position;
            animator.SetFloat("RightHandX", rightHandPos.x);
            animator.SetFloat("RightHandY", rightHandPos.y);
            animator.SetFloat("RightHandZ", rightHandPos.z);
            
            // Проверка, поднята ли рука
            bool rightHandRaised = rightHandPos.y > transform.position.y + 0.5f;
            animator.SetBool("RightHandRaised", rightHandRaised);
        }
    }
    
    void UpdateLegParameters()
    {
        // Левая нога
        Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        if (leftFoot != null)
        {
            Vector3 leftFootPos = leftFoot.position;
            animator.SetFloat("LeftFootX", leftFootPos.x);
            animator.SetFloat("LeftFootY", leftFootPos.y);
            animator.SetFloat("LeftFootZ", leftFootPos.z);
            
            // Проверка, находится ли нога на земле
            bool leftFootGrounded = leftFootPos.y <= transform.position.y - 1.5f;
            animator.SetBool("LeftFootGrounded", leftFootGrounded);
        }
        
        // Правая нога
        Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        if (rightFoot != null)
        {
            Vector3 rightFootPos = rightFoot.position;
            animator.SetFloat("RightFootX", rightFootPos.x);
            animator.SetFloat("RightFootY", rightFootPos.y);
            animator.SetFloat("RightFootZ", rightFootPos.z);
            
            // Проверка, находится ли нога на земле
            bool rightFootGrounded = rightFootPos.y <= transform.position.y - 1.5f;
            animator.SetBool("RightFootGrounded", rightFootGrounded);
        }
    }
    
    void UpdateHeadParameters()
    {
        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
        if (head != null)
        {
            Vector3 headPos = head.position;
            Quaternion headRot = head.rotation;
            
            animator.SetFloat("HeadX", headPos.x);
            animator.SetFloat("HeadY", headPos.y);
            animator.SetFloat("HeadZ", headPos.z);
            
            // Направление взгляда
            Vector3 lookDirection = headRot * Vector3.forward;
            animator.SetFloat("LookDirectionX", lookDirection.x);
            animator.SetFloat("LookDirectionY", lookDirection.y);
            animator.SetFloat("LookDirectionZ", lookDirection.z);
        }
    }
    
    void UpdateIK()
    {
        if (!enableIK || ikTarget == null) return;
        
        // Здесь можно добавить IK-логику для более точного позиционирования конечностей
        // Например, для следования рук за объектами или для правильной постановки ног на поверхность
    }
    
    public void SetBonePosition(HumanBodyBones bone, Vector3 position)
    {
        if (animator == null) return;
        
        Transform boneTransform = animator.GetBoneTransform(bone);
        if (boneTransform != null)
        {
            boneTransform.position = position;
        }
    }
    
    public void SetBoneRotation(HumanBodyBones bone, Quaternion rotation)
    {
        if (animator == null) return;
        
        Transform boneTransform = animator.GetBoneTransform(bone);
        if (boneTransform != null)
        {
            boneTransform.rotation = rotation;
        }
    }
    
    public Vector3 GetBonePosition(HumanBodyBones bone)
    {
        if (animator == null) return Vector3.zero;
        
        Transform boneTransform = animator.GetBoneTransform(bone);
        return boneTransform != null ? boneTransform.position : Vector3.zero;
    }
    
    public Quaternion GetBoneRotation(HumanBodyBones bone)
    {
        if (animator == null) return Quaternion.identity;
        
        Transform boneTransform = animator.GetBoneTransform(bone);
        return boneTransform != null ? boneTransform.rotation : Quaternion.identity;
    }
    
    public bool IsBoneMoving(HumanBodyBones bone)
    {
        for (int i = 0; i < trackedBones.Length; i++)
        {
            if (trackedBones[i] == bone)
            {
                return isMoving[i];
            }
        }
        return false;
    }
    
    public bool IsBoneRotating(HumanBodyBones bone)
    {
        for (int i = 0; i < trackedBones.Length; i++)
        {
            if (trackedBones[i] == bone)
            {
                return isRotating[i];
            }
        }
        return false;
    }
    
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void ToggleIK()
    {
        enableIK = !enableIK;
    }
    
    public void SetIKWeight(float weight)
    {
        ikWeight = Mathf.Clamp01(weight);
    }
    
    public void SetIKTarget(Transform target)
    {
        ikTarget = target;
    }
}
