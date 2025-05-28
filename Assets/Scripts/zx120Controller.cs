using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

public class zx120Controller : MonoBehaviour
{
    ROSConnection ros;
    private float boomIncreaseCoefficient = 0.271f;
    private float boomDecreaseCoefficient = 0.216f;
    private float swingCoefficient = 0.5f;
    private float armIncreaseCoefficient = 0.482f;
    private float armDecreaseCoefficient = 0.670f;
    private float bucketIncreaseCoefficient = 0.991f;
    private float bucketDecreaseCoefficient = 0.802f;

    private float boomUpperLimit = Mathf.Deg2Rad * 44.0f;
    private float boomLowerLimit = Mathf.Deg2Rad * -70.0f;
    private float armUpperLimit = Mathf.Deg2Rad * 152.0f;
    private float armLowerLimit = Mathf.Deg2Rad * 30.0f;
    private float bucketUpperLimit = Mathf.Deg2Rad * 143.0f;
    private float bucketLowerLimit = Mathf.Deg2Rad * -33f;

    public static float boomDirection = 1.0f;
    public static float swingDirection = 1.0f;
    public static float armDirection = 1.0f;
    public static float bucketDirection = 1.0f;
    public static float leftWheel = 0.0f;
    public static float rightWheel = 0.0f;
    private float translationVelocity = 6.0f;
    private float rotationVelocity = 2.0f;

    public string boomTopic = "zx120/boom/cmd";
    public string swingTopic = "zx120/swing/cmd";
    public string armTopic = "zx120/arm/cmd";
    public string bucketTopic = "zx120/bucket/cmd";
    public string tracksTopic = "zx120/tracks/cmd_vel";

    public bool isKeyboardControl = false;
    public bool isControllerControl = false;
    public bool isControllerControlKR = false; // KRのコントローラー用
    public bool isProControllerControl = false;

    private bool functionSwitch = false;
    private string functionSwitchButton = "joystick button 7";

    public AudioClip switchSound;

    public GameObject DriverCamera;

    public float initialBoomAngle;
    public float initialSwingAngle;
    public float initialArmAngle;
    public float initialBucketAngle;

    public float boomBottunAngle = 15.0f;
    private float boomBottunAngleNow = 0.0f;

    public float boomBottunAngleRate = 0.482f;

    private bool initialPublishDone = false;

    private float initialInterval = 0.5f;

    Float64Msg boomMsg;
    Float64Msg swingMsg;
    Float64Msg armMsg;
    Float64Msg bucketMsg;
    TwistMsg tracksMsg = new TwistMsg();

    void Start()
    {
        boomMsg = new Float64Msg() { data = initialBoomAngle };
        swingMsg = new Float64Msg() { data = initialSwingAngle };
        armMsg = new Float64Msg() { data = initialArmAngle };
        bucketMsg = new Float64Msg() { data = initialBucketAngle };
        ros = ROSConnection.GetOrCreateInstance();
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float64Msg>(boomTopic);
        ros.RegisterPublisher<Float64Msg>(swingTopic);
        ros.RegisterPublisher<Float64Msg>(armTopic);
        ros.RegisterPublisher<Float64Msg>(bucketTopic);
        ros.RegisterPublisher<TwistMsg>(tracksTopic);

        ros.Publish(boomTopic, boomMsg);
        ros.Publish(swingTopic, swingMsg);
        ros.Publish(armTopic, armMsg);
        ros.Publish(bucketTopic, bucketMsg);

        GameObject box = GameObject.Find("zx120/base_link/Collisions/unnamed/Box");
        box.SetActive(true);

        

    }

    void Update()
    {
        boomDirection = 0.0f;
        swingDirection = 0.0f;
        armDirection = 0.0f;
        bucketDirection = 0.0f;
        (leftWheel, rightWheel) = (0.0f, 0.0f);

        if (isKeyboardControl) InputKeys();
        if (isControllerControl) InputControllers();
        if (isControllerControlKR) InputControllersKR();
        if (isProControllerControl) InputProControllers();

        //initialIntervalの間はinitialPublishDoneをfalseにしておく
        if (!initialPublishDone)
        {
            initialInterval -= Time.deltaTime;
            if (initialInterval <= 0)
            {
                initialPublishDone = true;
            }
        }

        UpdateAndPublishMessages();


    }

    void UpdateAndPublishMessages()
    {
        if (boomDirection != 0 || !initialPublishDone)
        {
            boomMsg.data += boomDirection * boomIncreaseCoefficient * Time.deltaTime;
            boomMsg.data = (float)Mathf.Clamp((float)boomMsg.data, boomLowerLimit, boomUpperLimit);
            ros.Publish(boomTopic, boomMsg);
        }

        if (swingDirection != 0 || !initialPublishDone)
        {
            swingMsg.data += swingDirection * swingCoefficient * Time.deltaTime;
            ros.Publish(swingTopic, swingMsg);
        }

        if (armDirection != 0 || !initialPublishDone)
        {
            armMsg.data += armDirection * armIncreaseCoefficient * Time.deltaTime;
            armMsg.data = (float)Mathf.Clamp((float)armMsg.data, armLowerLimit, armUpperLimit);
            ros.Publish(armTopic, armMsg);
        }

        if (bucketDirection != 0 || !initialPublishDone)
        {
            bucketMsg.data += bucketDirection * bucketIncreaseCoefficient * Time.deltaTime;
            bucketMsg.data = (float)Mathf.Clamp((float)bucketMsg.data, bucketLowerLimit, bucketUpperLimit);
            ros.Publish(bucketTopic, bucketMsg);
        }


        tracksMsg.linear.x = translationVelocity * (leftWheel + rightWheel) / 2.0f;
        tracksMsg.angular.z = rotationVelocity * (rightWheel - leftWheel);
        ros.Publish(tracksTopic, tracksMsg);

        initialPublishDone = true;

    }

    void UpBoomMessages(){
        if(boomBottunAngleNow < boomBottunAngle){
            boomBottunAngleNow += boomBottunAngleRate * Time.deltaTime / Mathf.Deg2Rad;
            boomMsg.data += boomBottunAngleRate * Time.deltaTime;
            boomMsg.data = (float)Mathf.Clamp((float)boomMsg.data, boomLowerLimit, boomUpperLimit);
            ros.Publish(boomTopic, boomMsg);
            Debug.Log("Up");
        }
    }

    void DownBoomMessages(){
        if(0<boomBottunAngleNow){
            boomBottunAngleNow -= boomBottunAngleRate * Time.deltaTime / Mathf.Deg2Rad;
            boomMsg.data -= boomBottunAngleRate * Time.deltaTime;
            boomMsg.data = (float)Mathf.Clamp((float)boomMsg.data, boomLowerLimit, boomUpperLimit);
            ros.Publish(boomTopic, boomMsg);
            Debug.Log("Down");
        }
    }

    void InputKeys()
    {
        if (Input.GetKey("i") ^ Input.GetKey("k"))
        {
            if (Input.GetKey("i")) boomDirection = 1.0f;
            else if (Input.GetKey("k")) boomDirection = -1.0f;
        }

        if (Input.GetKey("d") ^ Input.GetKey("e"))
        {
            if (Input.GetKey("d")) swingDirection = 1.0f;
            else if (Input.GetKey("e")) swingDirection = -1.0f;
        }

        if (Input.GetKey("f") ^ Input.GetKey("s"))
        {
            if (Input.GetKey("f")) armDirection = 1.0f;
            else if (Input.GetKey("s")) armDirection = -1.0f;
        }


        if (Input.GetKey("j") ^ Input.GetKey("l"))
        {
            if (Input.GetKey("j")) bucketDirection = 1.0f;
            else if (Input.GetKey("l")) bucketDirection = -1.0f;
        }

        if (Input.GetKey("t") ^ Input.GetKey("g"))
        {
            if (Input.GetKey("t")) {
                leftWheel = 1.0f;
                rightWheel = 1.0f;
            }else if (Input.GetKey("g")){
                leftWheel = -1.0f;
                rightWheel = -1.0f;
            } 
        }

        // if (Input.GetKey("y") ^ Input.GetKey("h"))
        // {
        //     if (Input.GetKey("y")) rightWheel = 1.0f;
        //     else if (Input.GetKey("h")) rightWheel = -1.0f;
        // }
    }

    void InputControllers()
    {
        boomDirection = Input.GetAxis("Joystick3Vertical");
        swingDirection = Input.GetAxis("Joystick2Vertical");
        armDirection = Input.GetAxis("Joystick2Horizontal");
        bucketDirection = Input.GetAxis("Joystick3Horizontal");
        leftWheel = Input.GetAxis("Joystick1Vertical1");
        rightWheel = Input.GetAxis("Joystick1Vertical2");
    }

    void InputControllersKR()
    {
        boomDirection = Input.GetAxis("Joystick2Vertical");
        swingDirection = -Input.GetAxis("Joystick2Horizontal");
        //armDirection = Input.GetAxis("Joystick2Vertical");
        //bucketDirection = Input.GetAxis("Joystick3Horizontal");
        //Input.GetAxis("Joystick3Vertical")が0.3以上のときは1を代入，-0.3以下のときは-1を代入それ以外は0を代入
        float joystickValue = Input.GetAxis("Joystick3Vertical");
        if (joystickValue >= 0.3f)
        {
            leftWheel = 1.0f;
            rightWheel = 1.0f;
        }
        else if (joystickValue <= -0.3f)
        {
            leftWheel = -1.0f;
            rightWheel = -1.0f;
        }
        else
        {
            leftWheel = 0.0f;
            rightWheel = 0.0f;
        }
        //Joystick3Buttonが押しっぱのときの処理
        if (Input.GetButton("Joystick3Button")) UpBoomMessages();
        else DownBoomMessages();
    }

    void InputProControllers()
    {
        if (Input.GetKeyDown(functionSwitchButton)){ functionSwitch = !functionSwitch;
            AudioSource.PlayClipAtPoint(switchSound, DriverCamera.transform.position);
        }
        if (functionSwitch)
        {
            boomDirection = Input.GetAxis("ProJoystick1Vertical2");
            swingDirection = -Input.GetAxis("ProJoystick1Vertical1");
            armDirection = -Input.GetAxis("Joystick1Vertical1");
            bucketDirection = -Input.GetAxis("ProJoystick1Horizontal2");
        }
        else
        {
            leftWheel = Input.GetAxis("ProJoystick1Vertical1");
            rightWheel = Input.GetAxis("ProJoystick1Vertical2");
        }
    }
}
