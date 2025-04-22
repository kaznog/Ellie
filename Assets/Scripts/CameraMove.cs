using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class CameraMove : MonoBehaviour
{
    public GameObject playerObject;
    public Vector3 offset; // カメラのオフセットを設定
    public static GameObject cameraObject;
    public static Camera cameraCom;
    static public GameObject lightObject;
    Vector3 targetPos;
    Vector3 campos;
    Vector3 camLpos;
    Vector3 LookLpos = new Vector3(0, 1.2f, 0);//注視点の高さをすこしずらす
    Vector3 velocityCam = Vector3.zero;//スムーズ速度を次のフレームへ持ち越すための箱
    Vector3 velocityTarg = Vector3.zero;//スムーズ速度を次のフレームへ持ち越すための箱
    Vector3 backupOffset;
    public static float CamZDeg = 0;
    public static Vector3 mouseStartPosition = Vector3.zero;
    private static float SliderCameraDegCH = 0;
    private static float SliderCameraUpDownCH = 0;
    private Light dirLight;

    static public bool modeLightRotate = false;
    static public bool modeCameraMove = false;

    public static float SliderLight1 = 60.0f;
    public static float SliderLight2 = 230.0f;

    Vector3 camBasePos = new Vector3(0, 4.5f, -3.8f);
    public static float SliderCameraDeg = 202.0f;
    public static float SliderCameraZoom = 1000.0f;
    public static float SliderCameraUpDown = -40.0f;

    private void Awake()
    {
        cameraObject = transform.gameObject;
        campos = cameraObject.transform.localPosition;
        cameraCom = cameraObject.GetComponent<Camera>();

    }
    void Start()
    {
        Light[] Lights = FindObjectsByType<Light>(FindObjectsSortMode.InstanceID);
        int dirLightNum = -1;
        for (int i = 0; i < Lights.Length; i++)
        {
            if (Lights[i].type == UnityEngine.LightType.Directional)
            {
                dirLightNum = i;
            }
            else
            {
                //   Lights[i].enabled = false;
            }
        }
        if (dirLightNum >= 0)
        {
            lightObject = Lights[dirLightNum].gameObject;
        }
        else
        {
            lightObject = Lights[0].gameObject;
        }
        //     lightObject.transform.localRotation = Quaternion.Euler(60, 230, 0);

        dirLight = lightObject.GetComponent<Light>();
        //      dirLight.shadowCustomResolution = 0;


    }
    private bool CheckPointerOverObject(Touch t)
    {//タッチした場所にEventSystem系ボタンなどがあるかどうかチェックするやつ
        EventSystem current = EventSystem.current;
        if (current != null)
        {
            if (current.IsPointerOverGameObject(t.fingerId))
                return true;
            if (current.IsPointerOverGameObject())
                return true;
        }
        return false;
    }
    private bool CheckPointerOverObject()
    {//タッチした場所にEventSystem系ボタンなどがあるかどうかチェックするやつ
        EventSystem current = EventSystem.current;
        if (current != null)
        {
            if (current.IsPointerOverGameObject())
                return true;
            foreach (Touch t in Input.touches)
            {
                if (current.IsPointerOverGameObject(t.fingerId))
                    return true;
            }
        }
        return false;
    }

    float r_stick_old_x = -1f;
    float r_stick_old_y = -1f;
    //bool LookRotation = false;

    private void Update()
    {
        var gamePad = Gamepad.current;
        Touch[] itouches = Input.touches;
        foreach (Touch t in itouches)
        {
            if (t.fingerId == gamePad.deviceId)
                UnityEngine.Debug.Log("Joystick pointerID:" + t.fingerId);
        }

    }

    void LateUpdate()
    {
        var gamePad = Gamepad.current;
        //ここからカメラ位置操作予約
        //    float PadPadding = 0.15f;
        Touch t_CamTouch;
        Vector3 ipos = Vector3.zero;
        bool TouchOK = false;
        Touch[] itouches = Input.touches;
        foreach (Touch t in itouches)
        {
            if (t.fingerId == gamePad.deviceId)
                continue;
            if (t.phase == UnityEngine.TouchPhase.Began || t.phase == UnityEngine.TouchPhase.Moved || t.phase == UnityEngine.TouchPhase.Stationary)
            {
                if (CheckPointerOverObject(t))
                    continue;
                t_CamTouch = t;
                TouchOK = true;
                ipos = t_CamTouch.position;
                break;
            }
        }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WEBGL
        if (!TouchOK)
        {
#if MOBILE_INPUT
            if (Input.GetMouseButton(0))
            {
                if(!CheckPointerOverObject())
                {
                    ipos = Input.mousePosition;
                    UnityEngine.Debug.Log("mouse pos: " + ipos);
                    TouchOK = true;
                }
            }
#else
            if (r_stick_old_x == -1f)
            {
                r_stick_old_x = (MainSystem.LastScreenSize_x / 2);
                r_stick_old_y = (MainSystem.LastScreenSize_y / 2);
            }
            float r_stick_x = Input.GetAxis("Horizontal2");
            float r_stick_y = Input.GetAxis("Vertical2");
            float mouse_x = Input.GetAxis("Mouse X");
            float mouse_y = Input.GetAxis("Mouse Y");
            if (r_stick_x == 0 && mouse_x != 0f)
            {
                r_stick_x = mouse_x * 5;
            }
            if (r_stick_y == 0 && mouse_y != 0f)
            {
                r_stick_y = mouse_y;
            }

            if (r_stick_old_x + r_stick_x < 0)
            {
                r_stick_x += 0.01f;
            }
            else if (r_stick_old_x + r_stick_x > MainSystem.LastScreenSize_x)
            {
                r_stick_x -= 0.01f;
            }
            if (r_stick_old_y + r_stick_y < 0)
            {
                r_stick_y += 0.01f;
            }
            else if (r_stick_old_y + r_stick_y > MainSystem.LastScreenSize_y)
            {
                r_stick_y -= 0.01f;
            }
            r_stick_old_x += r_stick_x;
            r_stick_old_y += r_stick_y;
            UnityEngine.Debug.Log("(MashMainSystem.LastScreenSize_x / 2):" + (MainSystem.LastScreenSize_x / 2) + " r_stick_old_x: " + r_stick_old_x + " distance x:" + r_stick_x);
            UnityEngine.Debug.Log("(MashMainSystem.LastScreenSize_y / 2):" + (MainSystem.LastScreenSize_y / 2) + " r_stick_old_y: " + r_stick_old_y + " distance y:" + r_stick_y);
            ipos = new Vector2(r_stick_old_x, r_stick_old_y);
            TouchOK = true;
#endif
        }
#endif
        if (TouchOK)
        {
            //        UnityEngine.Debug.Log("Touch = " + ipos);
            if (mouseStartPosition == Vector3.zero)
            {
                mouseStartPosition = ipos;
                SliderCameraUpDownCH = SliderCameraUpDown;
                SliderCameraDegCH = SliderCameraDeg;
            }
            float stick2_x = (ipos.x - mouseStartPosition.x) / MainSystem.LastScreenSize_x;
            float stick2_z = (ipos.y - mouseStartPosition.y) / MainSystem.LastScreenSize_y;
            {
                stick2_x *= 450.0f;
                SliderCameraDeg = SliderCameraDegCH + stick2_x;
                if (SliderCameraDeg < 0)
                {
                    SliderCameraDeg += 360;
                }
                else if (SliderCameraDeg > 360)
                {
                    SliderCameraDeg -= 360;
                }
            }
            {
                stick2_z *= -250.0f;
                SliderCameraUpDown = SliderCameraUpDownCH + stick2_z;
                if (SliderCameraUpDown < -50)
                {
                    SliderCameraUpDown = -50;
                }
                else if (SliderCameraUpDown > 20)
                {
                    SliderCameraUpDown = 20;
                }
            }
        }
        else
        {
            mouseStartPosition = Vector3.zero;
            SliderCameraUpDownCH = 0;
            SliderCameraDegCH = 0;
        }
        if (modeLightRotate)
        {
            lightObject.transform.localRotation = Quaternion.Euler(SliderLight1, SliderLight2, 0);
        }
        else
        {
            CamZDeg = SliderCameraDeg;
        }

        Transform targetTransform;
        if (playerObject)
        {//プレイヤーオブジェクトがNullじゃないとき、カメラで追いかける
            Rigidbody rBody = playerObject.GetComponentInChildren<Rigidbody>();
            if (rBody != null)
            {
                targetTransform = rBody.transform;
            }
            else
            {
                targetTransform = playerObject.transform;
            }
            Vector3 unipos = targetTransform.position;//カメラが追いかける対象をセットする
            float udadder = 1f;
            Vector3 camBasePosSC = new Vector3(camBasePos.x, camBasePos.y + udadder, camBasePos.z);
            camBasePosSC *= 1.5f + 1.35f * ((SliderCameraUpDown - 20) / 80.0f);
            camLpos = Quaternion.Euler(SliderCameraUpDown, CamZDeg, 0) * camBasePosSC * (SliderCameraZoom * 0.001f);//カメラの回り込みを意識したうえで、注視点からカメラまでの距離ベクトルを生成する
            campos = unipos + camLpos;//時間をかけてカメラ座標をうごかす。
            cameraObject.transform.localPosition = campos;

            //注視点をセットする
            targetPos = unipos + LookLpos;//
            transform.LookAt(targetPos);
        }
    }
}



