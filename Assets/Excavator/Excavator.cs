
using System;
using System.Collections;
using UnityEngine;

public class ExcavatorState
{
    private string _state_name;

    private float _swing;
    private float _boom;
    private float _arm;
    private float _bucket;
    private float _trackRight;
    private float _trackLeft;

    private float _pos_x;
    private float _pos_y;
    private float _pos_z;

    public string state_name
    {
        set { _state_name = value; }
        get { return _state_name; }
    }

    public void clear()
    {
        ///--- Joint Control
        _swing = 0F;
        _boom = 0F;
        _arm = 0F;
        _bucket = 0F;
        _trackRight = 0F;
        _trackLeft = 0F;

        ///--- Position Control
        _pos_x = 0F;
        _pos_y = 0F;
        _pos_z = 0F;
    }

    public float swing
    {
        set { _swing = value; }
        get { return _swing; }
    }

    public float boom
    {
        set { _boom = value; }
        get => _boom;
    }

    public float arm
    {
        set { _arm = value; }
        get => _arm;
    }

    public float bucket
    {
        set { _bucket = value; }
        get => _bucket;
    }

    public float trackRight
    {
        set { _trackRight = value; }
        get => _trackRight;
    }

    public float trackLeft
    {
        set { _trackLeft = value; }
        get => _trackLeft;
    }

    public float pos_x
    {
        set { _pos_x = value; }
        get => _pos_x;
    }
    
    public float pos_y
    {
        set { _pos_y = value; }
        get => _pos_y;
    }

    public float pos_z
    {
        set { _pos_z = value; }
        get => _pos_z;
    }
}

public class Geometry {

    Transform swingAxis;
    Transform bodyBoomJoint;
    Transform boomArmJoint;
    Transform armBucketJoint;
    Transform hookJoint;

    public Geometry(Transform swingAxis, Transform bodyBoomJoint, Transform boomArmJoint, Transform armBucketJoint, Transform hookJoint)
    {
        this.swingAxis = swingAxis;
        this.bodyBoomJoint = bodyBoomJoint;
        this.boomArmJoint = boomArmJoint;
        this.armBucketJoint = armBucketJoint;
        this.hookJoint = hookJoint;
    }

    public float verticalAngle
    {
        get {
            Vector3 turnVector = swingAxis.up;
            Vector3 boomVector = boomArmJoint.position - bodyBoomJoint.position;
            return Vector3.Angle(turnVector, boomVector);
        }
    }

    public float boomArmAngle
    {
        get
        {
            Vector3 boomVector = boomArmJoint.position - bodyBoomJoint.position;
            Vector3 armVector = armBucketJoint.position - boomArmJoint.position;
            return 180F - Vector3.Angle(boomVector, armVector);
        }
    }

    public float boomArmHookAngle
    {
        get
        {
            Vector3 boomVector = boomArmJoint.position - bodyBoomJoint.position;
            Vector3 armVector = hookJoint.position - boomArmJoint.position;
            return 180F - Vector3.Angle(boomVector, armVector);
        }
    }

}

public class IkPosture
{
    //public float R;
    public float E;
    public float S;
    public float A;
    public float B;
    public float C;
    public float D;

    public float verticalAngleZero;
    public float boomArmAngleZero;
    public float boomArmHookAngleZero;
}

public class DriveParams
{
    private float _mass;

    private float _maxSpeed;
    private float _creepSpeed;

    private float _initialAccel;
    private float _deltaAccel;
    private float _maxAccel;

    private float _deltaSwing;

    public DriveParams(float mass, float maxSpeed, float creepSpeed, float initialAccel, float deltaAccel, float maxAccel, float deltaSwing)
    {
        _mass = mass;
        _maxSpeed = maxSpeed;
        _creepSpeed = creepSpeed;
        _initialAccel = initialAccel;
        _deltaAccel = deltaAccel;
        _maxAccel = maxAccel;
        _deltaSwing = deltaSwing;
    }

    public float mass
    {
        get { return _mass; }
    }

    public float maxSpeed
    {
        get { return _maxSpeed;  }
    }

    public float creepSpeed
    {
        get { return _creepSpeed; }
    }

    public float initialAccel
    {
        get { return _initialAccel;  }
    }

    public float deltaAccel
    {
        get { return _deltaAccel;  }
    }

    public float maxAccel
    {
        get { return _maxAccel;  }
    }

    public float deltaDirection
    {
        get { return _deltaSwing;  }
    }
}

public class Excavator
{

    static string excavatorForwardPath = "Armature";
    static string swingAxisPath = "Armature/TurnAxis";
    static string boomAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis";
    static string armAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis";
    static string bucketAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmAxis.003/BucketAxis";
    static string armLinkageAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmLinkageAxis";
    static string bucketLinkageAxisPath = "Armature/TurnAxis/Bone.001/BoomAxis/Bone.003/ArmAxis/ArmAxis.002/ArmAxis.003/BucketAxis/ArmAxis.005/BucketLinkageAxis";

    private Transform excavatorForwardAxis;
    private Transform swingAxis;
    private Transform boomAxis;
    private Transform armAxis;
    private Transform bucketAxis;

    private Transform boomCylinderRightAxis1;
    private Transform boomCylinderRight1Target;

    private Transform boomCylinderRightAxis2;
    private Transform boomCylinderRight2Target;

    private Transform boomCylinderLeftAxis1;
    private Transform boomCylinderLeft1Target;

    private Transform boomCylinderLeftAxis2;
    private Transform boomCylinderLeft2Target;

    private Transform armCylinderAxis1;
    private Transform armCylinder1Target;

    private Transform armCylinderAxis2;
    private Transform armCylinder2Target;

    private Transform bucketCylinderAxis1;
    private Transform bucketCylinder1Target;

    private Transform bucketCylinderAxis2;
    private Transform bucketCylinder2Target;

    private Transform armLinkageAxis;
    private Transform armLinkageTarget;

    private Transform bucketLinkageAxis;

    private Transform hookMainAxis;
    private Transform hookMainAxis_end;

    /* Cockpit */

    private Transform rightOperationLeverAxis;
    private Transform leftOperationLeverAxis;

    private Transform rightPedalAxis;
    private Transform leftPedalAxis;

    private Transform leftTravelLeverAxis;
    private Transform rightTravelLeverAxis;

    Vector3 joystickRightEulerAngles;
    Vector3 joystickLeftEulerAngles;

    Vector3 pedalRightEulerAngles;
    Vector3 pedalLeftEulerAngles;

    Vector3 leverRightEulerAngles;
    Vector3 leverLeftEulerAngles;

    /* Geometory */
    Transform bodyBoomJoint;
    Transform boomArmJoint;
    Transform armBucketJoint;

    float boomLength;
    float armLength;
    float armLengthHook;
    float boomLength2;
    float armLength2;
    float armLengthHook2;

    private GameObject excavator;

    Quaternion swingAxisInitialQ;
    Quaternion boomAxisInitialQ;
    Quaternion armAxisInitialQ;
    Quaternion bucketAxisInitialQ;

    private Geometry geometry;
    private float verticalAngleZero;
    private float boomArmAngleZero;
    private float boomArmHookAngleZero;

    GameObject bucket;
    Transform bucketCuttingEdges;
    Transform bucketPosition;
    GameObject arm;
    GameObject boom;
    GameObject body;
    GameObject cabin;
    GameObject rightTrack;
    GameObject leftTrack;
    Vector3    positionEffector;
    Vector3    positionHook;

    Rigidbody rb;

    Camera operatorViewCamera;
    Camera rearRightCamera;
    Camera rearCenterCamera;
    Camera rearLeftCamera;
    Camera mainCamera;
    Camera subCamera;

    public Excavator(GameObject excavator)
    {
        this.excavator = excavator;
        Transform transform = excavator.transform;
        excavator.AddComponent<Rigidbody>();
        rb = transform.GetComponent<Rigidbody>();
        rb.mass = 20000;  // 20,000 Kg
        rb.useGravity = true;

        excavatorForwardAxis = transform.Find(excavatorForwardPath);
        swingAxis = transform.Find(swingAxisPath);
        boomAxis = transform.Find(boomAxisPath);
        armAxis = transform.Find(armAxisPath);
        bucketAxis = transform.Find(bucketAxisPath);

        swingAxisInitialQ = swingAxis.localRotation;
        boomAxisInitialQ = boomAxis.localRotation;
        armAxisInitialQ = armAxis.localRotation;
        bucketAxisInitialQ = bucketAxis.localRotation;

        armLinkageAxis = transform.Find(armLinkageAxisPath);
        bucketLinkageAxis = transform.Find(bucketLinkageAxisPath);

        boomCylinderRightAxis1 = transform.Find(swingAxisPath + "/Bone.001/Bone.011/BoomCylinderRightAxis1");
        boomCylinderRight1Target = transform.Find(boomAxisPath + "/BoomCylinderRight1Target");

        boomCylinderRightAxis2 = transform.Find(boomAxisPath + "/Bone.015/BoomCylinderRightAxis2");
        boomCylinderRight2Target = transform.Find(swingAxisPath + "/BoomCylinderRight2Target");

        boomCylinderLeftAxis1 = transform.Find(swingAxisPath + "/Bone.001/Bone.012/BoomCylinderLeftAxis1");
        boomCylinderLeft1Target = transform.Find(boomAxisPath + "/BoomCylinderLeft1Target");

        boomCylinderLeftAxis2 = transform.Find(boomAxisPath + "/Bone.016/BoomCylinderLeftAxis2");
        boomCylinderLeft2Target = transform.Find(swingAxisPath + "/BoomCylinderLeft2Target");

        armCylinderAxis1 = transform.Find(boomAxisPath + "/ArmCylinderAxis1");
        armCylinder1Target = transform.Find(armAxisPath + "/ArmCylinder1Target");

        armCylinderAxis2 = transform.Find(armAxisPath + "/Bone.010/ArmCylinderAxis2");
        armCylinder2Target = transform.Find(boomAxisPath + "/ArmCylinder2Target");

        bucketCylinderAxis1 = transform.Find(armAxisPath + "/BucketCylinderAxis1");
        bucketCylinder1Target = transform.Find(armLinkageAxisPath + "/BucketCylinder1Target");

        bucketCylinderAxis2 = transform.Find(armLinkageAxisPath + "/BucketCylinderAxis2");
        bucketCylinder2Target = transform.Find(armAxisPath + "/BucketCylinder2Target");

        armLinkageAxis = transform.Find(armLinkageAxisPath);
        armLinkageTarget = transform.Find(bucketLinkageAxisPath + "/ArmLinkageTarget");

        bucketLinkageAxis = transform.Find(bucketLinkageAxisPath);

        hookMainAxis = transform.Find(bucketLinkageAxisPath + "/HookMainAxis");
        hookMainAxis_end = transform.Find(bucketLinkageAxisPath + "/Sphere.008");

        rightOperationLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.003/JoystickRightAxis");
        leftOperationLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.002/JoystickLeftAxis");

        rightPedalAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRightAxis");
        leftPedalAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeftAxis");

        rightTravelLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRight/LeverRightAxis");
        leftTravelLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeft/LeverLeftAxis");

        joystickLeftEulerAngles = leftOperationLeverAxis.eulerAngles;
        joystickRightEulerAngles = rightOperationLeverAxis.eulerAngles;

        pedalRightEulerAngles = rightPedalAxis.eulerAngles;
        pedalLeftEulerAngles = leftPedalAxis.eulerAngles;

        leverRightEulerAngles = leftTravelLeverAxis.eulerAngles;
        leverLeftEulerAngles = rightTravelLeverAxis.eulerAngles;

        rightOperationLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.003/JoystickRightAxis");
        leftOperationLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.002/JoystickLeftAxis");

        rightPedalAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRightAxis");
        leftPedalAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeftAxis");

        rightTravelLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.004/PedalRight/LeverRightAxis");
        leftTravelLeverAxis = transform.Find(swingAxisPath + "/TurnAxis.001/TurnAxis.005/PedalLeft/LeverLeftAxis");

        joystickLeftEulerAngles = leftOperationLeverAxis.eulerAngles;
        joystickRightEulerAngles = rightOperationLeverAxis.eulerAngles;

        pedalRightEulerAngles = rightPedalAxis.eulerAngles;
        pedalLeftEulerAngles = leftPedalAxis.eulerAngles;

        leverRightEulerAngles = leftTravelLeverAxis.eulerAngles;
        leverLeftEulerAngles = rightTravelLeverAxis.eulerAngles;

        // bodyBoomJoint = transform.Find(boomAxisPath);
        bodyBoomJoint = transform.Find(swingAxisPath + "/Cylinder.021");
        boomArmJoint = transform.Find(boomAxisPath + "/Cylinder.023");
        armBucketJoint = transform.Find(armAxisPath + "/Cylinder.041");

        boomLength = (boomArmJoint.position - bodyBoomJoint.position).magnitude;
        armLength = (armBucketJoint.position - boomArmJoint.position).magnitude;
        armLengthHook = (hookMainAxis.position - boomArmJoint.position).magnitude;
        boomLength2 = Mathf.Pow(boomLength, 2F);
        armLength2 = Mathf.Pow(armLength, 2F);
        armLengthHook2 = Mathf.Pow(armLengthHook, 2F);

        geometry = new Geometry(swingAxis, bodyBoomJoint, boomArmJoint, armBucketJoint, hookMainAxis);
        //Debug.Log($"verticalAngle={geometry.verticalAngle}, boomArmAngle={geometry.boomArmAngle}");
        verticalAngleZero = geometry.verticalAngle;
        boomArmAngleZero = geometry.boomArmAngle;
        boomArmHookAngleZero = geometry.boomArmHookAngle;

        bucket = transform.Find(bucketAxisPath + "/Vert.002").gameObject;
        bucketCuttingEdges = transform.Find(bucketAxisPath + "/Cube.019");
        bucketPosition = transform.Find(armLinkageAxisPath + "/BucketCylinderAxis2/Sphere.007");
        arm = transform.Find(armAxisPath + "/Vert.001").gameObject;
        boom = transform.Find(boomAxisPath + "/Cube.010").gameObject;
        body = transform.Find(swingAxisPath + "/Cube").gameObject;
        cabin = transform.Find(swingAxisPath + "/Vert.009").gameObject;
        leftTrack = transform.Find("TrackLeft").gameObject;
        rightTrack = transform.Find("TrackRight").gameObject;

        operatorViewCamera = transform.Find(swingAxisPath + "/Camera").GetComponent<Camera>();
        rearRightCamera = transform.Find(swingAxisPath + "/RearCameraRight").GetComponent<Camera>();
        rearCenterCamera = transform.Find(swingAxisPath + "/RearCameraCenter").GetComponent<Camera>();
        rearLeftCamera = transform.Find(swingAxisPath + "/RearCameraLeft").GetComponent<Camera>();
        mainCamera = transform.Find("CameraBirdView").GetComponent<Camera>();
        subCamera = transform.Find("CameraSubBirdView").GetComponent<Camera>();
        // mainCamera = transform.Find(swingAxisPath + "/Camera").GetComponent<Camera>();

        // Attach MirrorFlipCamera script to the rear cameras
        transform.Find(swingAxisPath + "/RearCameraRight").gameObject.AddComponent<MirrorFlipCamera>();
        transform.Find(swingAxisPath + "/RearCameraCenter").gameObject.AddComponent<MirrorFlipCamera>();
        transform.Find(swingAxisPath + "/RearCameraLeft").gameObject.AddComponent<MirrorFlipCamera>();
 
        bucket.AddComponent<MeshCollider>();
        bucket.GetComponent<MeshCollider>().convex = true;
        arm.AddComponent<MeshCollider>();
        arm.GetComponent<MeshCollider>().convex = true;
        boom.AddComponent<MeshCollider>();
        boom.GetComponent<MeshCollider>().convex = true;
        body.AddComponent<MeshCollider>();
        body.GetComponent<MeshCollider>().convex = true;
        cabin.AddComponent<MeshCollider>();
        cabin.GetComponent<MeshCollider>().convex = true;
        
        leftTrack.AddComponent<BoxCollider>();
        var lt = leftTrack.GetComponent<BoxCollider>().size;
        leftTrack.GetComponent<BoxCollider>().size = new Vector3(lt.x * 4 / 5F, lt.y * 16 / 17F, lt.z);
        rightTrack.AddComponent<BoxCollider>();
        var rt = rightTrack.GetComponent<BoxCollider>().size;
        rightTrack.GetComponent<BoxCollider>().size = new Vector3(rt.x * 4 / 5F, rt.y * 16 / 17F, rt.z);
        
    }

    public void EnableRearCameras(bool enable)
    {
        rearLeftCamera.enabled = false;
        rearCenterCamera.enabled = false;
        rearRightCamera.enabled = false;

        if (enable)
        {
            mainCamera.rect = new Rect(0F, 0.5F, 0.333F, 0.5F);
            operatorViewCamera.rect = new Rect(0.333F, 0.5F, 0.666F, 0.5F);
            rearLeftCamera.rect = new Rect(0, 0, 0.333F, 0.5F);
            rearCenterCamera.rect = new Rect(0.333F, 0F, 0.333F, 0.5F);
            rearRightCamera.rect = new Rect(0.666F, 0F, 0.333F, 0.5F);
        } else
        {
            mainCamera.rect = new Rect(0F, 0F, 1F, 1F);
            // operatorViewCamera.rect = new Rect(0.5F, 0F, 0.5F, 1F);
            operatorViewCamera.rect = new Rect(0.68F, 0.05F, 0.3F, 0.3F);
        }
    }

    public void InitializeCamera( int _number_excavator )
    {
        rearLeftCamera.enabled = false;
        rearCenterCamera.enabled = false;
        rearRightCamera.enabled = false;

        if( _number_excavator == 0 )
            subCamera.rect = new Rect(0F, 0F, 0.5F, 1F);
        else if( _number_excavator == 1 )
            subCamera.rect = new Rect(0.5F, 0F, 0.5F, 1F);
    }

    public void ChangeCameraMode( bool camera_mode_operator )
    {
        if ( camera_mode_operator )
        {
            mainCamera.depth = 1;
            operatorViewCamera.depth = 0;
            mainCamera.rect = new Rect(0.68F, 0.05F, 0.3F, 0.3F);
            operatorViewCamera.rect = new Rect(0F, 0F, 1.0F, 1F);
        }
        else
        {
            mainCamera.depth = 0;
            operatorViewCamera.depth = 1;
            mainCamera.rect = new Rect(0F, 0F, 1.0F, 1F);
            operatorViewCamera.rect = new Rect(0.68F, 0.05F, 0.3F, 0.3F);
        }
    }

    public void SetCameraOn()
    {
        mainCamera.enabled = operatorViewCamera.enabled = true;
    }

    public void SetCameraOff()
    {
        mainCamera.enabled = operatorViewCamera.enabled = false;
    }

    private void OrientHydraulicCylinder(Transform cylinder1, Transform cylinder1Target,
    Transform cylinder2, Transform cylinder2Target, Vector3 up)
    {
        cylinder1.LookAt(cylinder1Target, up);
        cylinder2.LookAt(cylinder2Target, up);

        cylinder1.Rotate(new Vector3(90F, 0F, 0F));
        cylinder2.Rotate(new Vector3(90F, 0F, 0F));
    }

    private void OrientLinkage(Transform linkage, Transform linkageTarget, Vector3 up)
    {
        linkage.LookAt(linkageTarget, up);
        linkage.Rotate(new Vector3(90F, 0F, 0F));
    }

    private void OrientArmCylinder()
    {
        OrientHydraulicCylinder(armCylinderAxis1, armCylinder1Target, armCylinderAxis2, armCylinder2Target, arm.transform.right);
    }

    private void OrientBoomCylinder()
    {
        OrientHydraulicCylinder(boomCylinderRightAxis1, boomCylinderRight1Target, boomCylinderRightAxis2, boomCylinderRight2Target, arm.transform.right);
        OrientHydraulicCylinder(boomCylinderLeftAxis1, boomCylinderLeft1Target, boomCylinderLeftAxis2, boomCylinderLeft2Target, arm.transform.right);
    }

    private void OrientBucketCylinder()
    {
        OrientLinkage(armLinkageAxis, armLinkageTarget, arm.transform.right);
        OrientLinkage(bucketLinkageAxis, bucketCylinder1Target,  arm.transform.forward);
        OrientHydraulicCylinder(bucketCylinderAxis1, bucketCylinder1Target, bucketCylinderAxis2, bucketCylinder2Target, boom.transform.right);
    }

    public Transform transform
    {
        get { return excavator.transform; }
    }

    public Vector3 position
    {
        set { excavator.transform.position = value; }
        get { return excavator.transform.position; }
    }

    float boomAngleLimitLow = 0F;
    float boomAngleLimitHigh = 56F;
    float armAngleLimitLow = 0F;
    float armAngleLimitHigh = 120F;
    float bucketAngleLimitLow = 0F;
    float bucketAngleLimitHigh = 160F;
    float swingRotateLimit  = 0.2F;
    float bucketRotateLimit = 0.4F;

    public void swingRotate(float value)
    {
        if( value > swingRotateLimit )
            value = swingRotateLimit;
        else if( value < -swingRotateLimit )
            value = -swingRotateLimit;

        swingAngle += value;
    }

    public void boomRotate(float value)
    {
        boomAngle += value;
    }

    public void armRotate(float value)
    {
        armAngle -= value;
    }

    public void bucketRotate(float value)
    {
        if( value > bucketRotateLimit )
            value = bucketRotateLimit;
        else if( value < -bucketRotateLimit )
            value = -bucketRotateLimit;

        bucketAngle += value;
    }

    public float swingAngle
    {
        set
        {
            swingAxis.localRotation = swingAxisInitialQ;
            swingAxis.Rotate(0, value, 0);
        }
        get
        {
            float angle = Vector3.SignedAngle(excavatorForwardAxis.right, swingAxis.right, swingAxis.up);
            // Debug.Log($"swing angle = {angle}");
            return angle;
        }
    }

    public float boomAngle
    {
        set
        {
            if (value >= boomAngleLimitLow && value <= boomAngleLimitHigh)
            {
                boomAxis.localRotation = boomAxisInitialQ;
                boomAxis.Rotate(value, 0, 0);
                OrientBoomCylinder();
            }
            // Debug.Log($"boomAngle: {boomAngle}");
            // Debug.Log($"boomAngle: {boomAngle}"+"boomAxisInitialQ"+boomAxisInitialQ);
        }
        get
        {
            return Quaternion.Angle(boomAxisInitialQ, boomAxis.localRotation);
        }
    }

    public float armAngle
    {
        set
        {
            if (value >= armAngleLimitLow && value <= armAngleLimitHigh)
            {
                armAxis.localRotation = armAxisInitialQ;
                armAxis.Rotate(-value, 0, 0);
                OrientArmCylinder();
            }
            // Debug.Log($"armAngle: {armAngle}");
        }
        get
        {
            return Quaternion.Angle(armAxisInitialQ, armAxis.localRotation);
        }
    }

    public float bucketAngle
    {
        set
        {
            if (value >= bucketAngleLimitLow && value <= bucketAngleLimitHigh)
            {
                bucketAxis.localRotation = bucketAxisInitialQ;
                bucketAxis.Rotate(value, 0, 0);
                OrientBucketCylinder();
            }
            // Debug.Log($"bucketAngle: {bucketAngle}");
        }
        get
        {
            return Quaternion.Angle(bucketAxisInitialQ, bucketAxis.localRotation);
        }
    }

    public void leftOperationLeverRotate(float leftRight, float upDown)
    {
        leftOperationLeverAxis.Rotate(new Vector3(leftRight, 0, upDown));
    }

    public void rightOperationLeverRotate(float leftRight, float upDown)
    {
        rightOperationLeverAxis.Rotate(new Vector3(leftRight, 0, upDown));
    }

    public void leftTravelLeverRotate(float upDown)
    {
        leftTravelLeverAxis.Rotate(new Vector3(upDown, 0, 0));
    }

    public void rightTravelLeverRotate(float upDown)
    {
        rightTravelLeverAxis.Rotate(new Vector3(upDown, 0, 0));
    }

    public void rightPedalRotate(float upDown)
    {
        rightPedalAxis.Rotate(new Vector3(upDown, 0, 0));

    }

    public void leftPedalRotate(float upDown)
    {
        leftPedalAxis.Rotate(new Vector3(upDown, 0, 0));

    }

    float accel = 0;
    // float accelInput = 0;

    ///--- Call this method in Update()
    public void Move(float deltaRotation, float accel, DriveParams driveParams)
    {
        transform.Rotate(0, deltaRotation, 0);

        Vector3 forwardDirection = (excavatorForwardAxis.rotation * Vector3.right).normalized;
        float force = driveParams.mass * accel;  // F = m * a
        rb.AddForceAtPosition(forwardDirection * force, transform.position, ForceMode.Force);
        Debug.Log($"Force: {driveParams.mass * accel}");

    }

    private bool Move(Transform target, DriveParams driveParams)
    {

        float up = Mathf.Abs(Vector3.Angle(excavatorForwardAxis.rotation * Vector3.up, Vector3.up));
        if (up < 40F || up >= 140F) throw new Exception();

        //Debug.Log($"up = {Vector3.Angle(excavatorForwardAxis.rotation*Vector3.up, Vector3.up)}");

        Vector3 directionToTarget = target.position - transform.position;
        Vector3 forwardDirection = excavatorForwardAxis.rotation * Vector3.right;
        float distance = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
        directionToTarget.y = 0F;
        forwardDirection.y = 0;

        if (distance > 7F || distance <= 5F)
        {
            float angle = Vector3.SignedAngle(forwardDirection, directionToTarget, Vector3.up);
            //Debug.Log(angle);
            if (Mathf.Abs(angle) > driveParams.deltaDirection)
            {
                if (angle > 0)
                {
                    transform.Rotate(0, driveParams.deltaDirection, 0);
                } else
                {
                    transform.Rotate(0, -driveParams.deltaDirection, 0);
                }
            }
            forwardDirection = (excavatorForwardAxis.rotation * Vector3.right).normalized;
            if (distance <= 5F)  // reverse traveling range
            {
                forwardDirection = -forwardDirection;
            }
            float maxSpeed = driveParams.maxSpeed;
            if (distance < 15F) maxSpeed = driveParams.creepSpeed;  // slow down range
            if (rb.velocity.magnitude < maxSpeed && this.accel + driveParams.deltaAccel < driveParams.maxAccel)
            {
                this.accel += driveParams.deltaAccel;
            }
            else
            {
                this.accel -= driveParams.deltaAccel;
            }

            float force = driveParams.mass * this.accel;  // F = m * a
            rb.AddForceAtPosition(forwardDirection * force, transform.position, ForceMode.Force);
            Debug.Log($"Force: {driveParams.mass * this.accel}");

            return true;
        }

        if (rb.velocity.magnitude < 0.01F)
        {
            accel = 0F;
            return false;
        }
        else return true;
    }

    private RaycastHit raycastHit;
    private RaycastHit hit;
    private bool _useHook = false;

    public void OrientHook()
    {
        if (_useHook) {
            Physics.Raycast(hookMainAxis.position, Vector3.down, out hit, 200F);
            hookMainAxis.LookAt(hit.point, hookMainAxis.up);
            hookMainAxis.Rotate(new Vector3(0F, 210F, 0F));
        }
    }

    public void EnableCuttingEdges(bool enable)
    {
        bucket.GetComponent<MeshCollider>().enabled = !enable;
    }

    public bool useHook
    {
        set
        {
            _useHook = value;
        }
        get
        {
            return _useHook;
        }
    }

    ///---Kinematic for boom and arm
    public Vector3 Kinematics()
    {
        float alfa = -0.12722482F + boomAngle * Mathf.Deg2Rad;
        float armJointAxisXZ = boomLength * Mathf.Cos( alfa );
        float armJointAxisY = boomLength * Mathf.Sin( alfa );

        float beta = -3.14156F + ( 0.365457107F + armAngle*Mathf.Deg2Rad + alfa );
        float BucketJointAxisXZ = armLength * Mathf.Cos( beta ) + armJointAxisXZ;
        float BucketJointAxisY = armLength * Mathf.Sin( beta ) + armJointAxisY;

        // float BucketJointAxisX = BucketJointAxisXZ * Mathf.Cos( -swingAngle * Mathf.Deg2Rad );// + bodyBoomJoint.position.x;
        // float BucketJointAxisZ = BucketJointAxisXZ * Mathf.Sin( -swingAngle * Mathf.Deg2Rad );// + bodyBoomJoint.position.z;
        // BucketJointAxisY += bodyBoomJoint.position.y;

        positionEffector.x = BucketJointAxisXZ;
        positionEffector.y = BucketJointAxisY;
        positionEffector.z = 0.0F;//BucketJointAxisZ;

        // float hookJointlength = Mathf.Pow( (armBucketJoint.position.x - hookMainAxis_end.position.x), 2F) + Mathf.Pow( (armBucketJoint.position.y - hookMainAxis_end.position.y), 2F ) + Mathf.Pow( (armBucketJoint.position.z - hookMainAxis_end.position.z), 2F );
        // hookJointlength = Mathf.Sqrt(hookJointlength);
        // float hookJointXZ = BucketJointAxisXZ;
        // float hookJointY  = BucketJointAxisY;

        return positionEffector;
    }

    ///--- Inverse Kinematic for boom and arm
    public Vector2 InverseKinematics( float input_delta_position_x, float input_delta_position_y )
    {
        float delta_x = input_delta_position_x;
        float delta_y = input_delta_position_y;
        if( delta_x > 0.03F )
            delta_x = 0.03F;
        else if( delta_x < -0.03F )
            delta_x = -0.03F;
        else if( Mathf.Abs(delta_x) < 0.01F )
            delta_x = 0F;

        if( delta_y > 0.03F )
            delta_y = 0.03F;
        else if( delta_y < -0.03F )
            delta_y = -0.03F;
        else if( Mathf.Abs(delta_y) < 0.01F )
            delta_y = 0F;

        float pos_x = positionEffector.x + delta_x;
        float pos_y = positionEffector.y + delta_y;
        float pos_x2 = pos_x*pos_x;
        float pos_y2 = pos_y*pos_y;
        float theta_1 = Mathf.Atan( pos_y/pos_x ) + Mathf.Acos( ( pos_x2 + pos_y2 + boomLength*boomLength - armLength*armLength ) / ( 2F * boomLength * Mathf.Sqrt( pos_x2 + pos_y2) ) );
        float theta_2 = 3.14156F - Mathf.Acos( (boomLength*boomLength + armLength*armLength - pos_x2 - pos_y2 ) / ( 2F * boomLength * armLength ) );
        
        float theta_1_conv = theta_1 + 0.12722482F;
        float theta_2_conv = - (theta_2 - 2.776102893F);
    
        Vector2 angleBoomAndArm;
        angleBoomAndArm.x = theta_1_conv;
        angleBoomAndArm.y = theta_2_conv;

        // Debug.Log ("input_delta_position_x:="+input_delta_position_x+" input_delta_position_y:="+input_delta_position_y );
        // Debug.Log ("pos_x:="+pos_x+" pos_y:="+pos_y );
        // Debug.Log ("delta_x:="+delta_x+" delta_y:="+delta_y );
        // Debug.Log ("theta_1:="+theta_1 * Mathf.Rad2Deg+" theta_2:="+theta_2 * Mathf.Rad2Deg );
        // Debug.Log ("theta_1_conv:="+theta_1_conv * Mathf.Rad2Deg+" theta_2_conv:="+theta_2_conv * Mathf.Rad2Deg );
        // Debug.Log ("boomAngle:="+boomAngle+" armAngle:="+armAngle );

        return angleBoomAndArm;
    }

    private bool coroutineIsRunning = false;

    public IEnumerator Reset(float targetSwingAngle=0F, float targetBoomAngle=55F, float targetArmAngle=45F, float targetBucketAngle=60F)
    {
        float t = 0F;
        float currentSwingAngle = swingAngle;
        float currentBoomAngle = boomAngle;
        float currentArmAngle = armAngle;
        float currentBucketAngle = bucketAngle;

        while(coroutineIsRunning)
        {
            yield return null;
        }

        coroutineIsRunning = true;

        while (t < 1F)
        {
            t += Time.deltaTime * 0.8F;
            swingAngle = Mathf.Lerp(currentSwingAngle, targetSwingAngle, t);
            boomAngle = Mathf.Lerp(currentBoomAngle, targetBoomAngle, t);
            armAngle = Mathf.Lerp(currentArmAngle, targetArmAngle, t);
            bucketAngle = Mathf.Lerp(currentBucketAngle, targetBucketAngle, t);
            yield return null;
        }

        coroutineIsRunning = false;
    }

    public IEnumerator PositionControlArm( float input_position_x, float input_position_y )
    {
        // Vector3 angle = Kinematics();
        // Debug.Log ("kinematics X:="+angle.x+" Y:="+angle.y+" Z:="+angle.z );
        // Debug.Log ("kinematicsBucketJointAxis:= "+positionEffector );
        // float bucketJointlength = Mathf.Pow( (armBucketJoint.position.x - hookMainAxis_end.position.x), 2F) + Mathf.Pow( (armBucketJoint.position.y - hookMainAxis_end.position.y), 2F ) + Mathf.Pow( (armBucketJoint.position.z - hookMainAxis_end.position.z), 2F );
        // bucketJointlength = Mathf.Sqrt(bucketJointlength);
        // Debug.Log (" bucketJointlength:= "+bucketJointlength );


        while (coroutineIsRunning)
        {
            yield return null;
        }
        Vector2 angleBoomAndArm = InverseKinematics( input_position_x, input_position_y );

        if (angleBoomAndArm.x >= boomAngleLimitLow && angleBoomAndArm.x <= boomAngleLimitHigh)
        {
            if (angleBoomAndArm.y >= armAngleLimitLow && angleBoomAndArm.y <= armAngleLimitHigh)
            {
                // boomAngle = angleBoomAndArm.x*Mathf.Rad2Deg;
                // armAngle =  angleBoomAndArm.y*Mathf.Rad2Deg;
                float currentBoomAngle = boomAngle;
                float currentArmAngle = armAngle;
                boomAngle = Mathf.Lerp(currentBoomAngle, angleBoomAndArm.x*Mathf.Rad2Deg, 0.5F);
                armAngle = Mathf.Lerp(currentArmAngle, angleBoomAndArm.y*Mathf.Rad2Deg, 0.5F);
            }
        }

        coroutineIsRunning = false;
    }
}
