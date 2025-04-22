using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using StarterAssets;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed = 5; // Inspectorビューで変更可能
    public float jumpForce = 5.0f;
    public float waitTime = 2.0f;
    Animator animator;
    public Rigidbody rBody;//体のリグ
    public LayerMask groundLayer; // 地面のレイヤーマスク
    public float checkDistance = 0.1f; // レイキャストの距離
    public StarterAssetsInputs starterAssetsInputs;
    public GameObject WeaponJointBack;
    public GameObject WeaponJointRHand;
    public GameObject Weapon_01;

    bool grounded = false;
    bool Grounded
    {
        get { return grounded; }
        set 
        { 
            grounded = value;
            animator.SetBool("Grounded", grounded);
        }
    }
    bool walking = false; // フィールド
    bool Walking
    { // プロパティ
        get { return walking; }
        set
        { // 値が異なるセット時のみanimator.SetBoolを呼ぶようにします
            if (value != walking)
            {
                walking = value;
                animator.SetBool("Walking", walking);
            }
        }
    }
    bool running = false;
    bool Running
    {
        get { return running; }
        set { 
            if (value != running) 
            { 
                running = value; 
                animator.SetBool("Running", running);
            }
        }
    }
    bool standingJump;
    bool StandingJump
    {
        get { return standingJump; }
        set
        {
            if (value != standingJump)
            {
                standingJump = value;
                animator.SetTrigger("StandingJump");
            }
        }
    }

    //private static readonly int hashAttack = Animator.StringToHash("Attack");
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isLanding", false);
    }
    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        string fname = "mixamorig:RightHandIndex1";
        WeaponJointRHand = MainSystem.ChildAllFind(transform, ref fname);
        fname = "WeaponJointBack";
        WeaponJointBack = MainSystem.ChildAllFind(transform, ref fname);

        string PrefabPath = "Prefabs/Weapon_01";
        var prefab = Resources.Load<GameObject>(PrefabPath);
        Weapon_01 = Instantiate(prefab);
        Weapon_01.transform.parent = WeaponJointBack.transform;
        Weapon_01.name = "Weapon_01";
        Weapon_01.transform.localPosition = Vector3.zero;
        Weapon_01.transform.localRotation = Quaternion.identity;

        // AnimatorからObservableStateMachineTriggerの参照を取得
        ObservableStateMachineTrigger trigger =
            animator.GetBehaviour<ObservableStateMachineTrigger>();

        // Stateの開始イベント
        IDisposable enterState = trigger
            .OnStateEnterAsObservable()
            .Subscribe(onStateInfo =>
            {
                AnimatorStateInfo info = onStateInfo.StateInfo;
                // Base Layer
                if (info.IsName("Base Layer.Attack"))
                {
                    if (Weapon_01.transform.parent == WeaponJointBack.transform)
                    {
                        Weapon_01.transform.parent = WeaponJointRHand.transform;
                        Weapon_01.transform.localPosition = Vector3.zero;
                        Weapon_01.transform.localRotation = Quaternion.Euler(180f,0f,110f);
                    }
                    UnityEngine.Debug.Log("startState Attack");
                }
            }).AddTo(this);

        // Stateの終了イベント
        IDisposable exitState = trigger
            .OnStateExitAsObservable()
            .Subscribe(onStateInfo =>
            {
                AnimatorStateInfo info = onStateInfo.StateInfo;
                // Base Layer
                // Base Layer
                if (info.IsName("Base Layer.Attack"))
                {
                    starterAssetsInputs.attack = false;
                    if (Weapon_01.transform.parent == WeaponJointRHand.transform)
                    {
                        Weapon_01.transform.parent = WeaponJointBack.transform;
                        Weapon_01.transform.localPosition = Vector3.zero;
                        Weapon_01.transform.localRotation = Quaternion.identity ;
                    }
                    UnityEngine.Debug.Log("exitState Attack");
                }
            }).AddTo(this);
    }

    void Update()
    {
        // オブジェクトの真下にレイキャストを行う
        if (Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer) && animator.GetBool("isLanding") == false)
        { // 着地?
            Debug.Log("Grounded");
            Grounded = true;
        }
        else
        { // 着地していない
            Debug.Log("Not Grounded");
            Grounded = false;
        }
        float stick_x = MainSystem.stick_x;
        float stick_z = MainSystem.stick_z;
        Vector3 XZmove = new Vector3(stick_x, 0, stick_z);//スティック傾斜ベクトル
        {
            UnityEngine.Debug.Log("XZmove.magnitude:" + XZmove.magnitude);
            //Editor側にデータをセット
            float XZmag = XZmove.magnitude;
            float AtanDeg = Mathf.Atan2(stick_x, stick_z) * Mathf.Rad2Deg;//スティックの傾斜角度
            Quaternion XZQT = rBody.transform.rotation;
            if (XZmag > 0 /*&& animator.GetBool("isLanding") == false && animator.GetBool("Guard") == false*/)
            {
                //移動中のとき、かつ攻撃モーション中ではないとき
                float AngleTarget = CameraMove.SliderCameraDeg + AtanDeg;
                float AngleBodyY = rBody.transform.rotation.eulerAngles.y;
                float AngleTest = Mathf.DeltaAngle(AngleBodyY, AngleTarget);//向いている方向とスティックの角度差
                if (AngleTest > -30 && AngleTest < 30)
                {//差が一定値以内のとき、歩く
                    rBody.transform.rotation = Quaternion.Euler(0, AngleTarget, 0);
                    // 前に移動する
                    transform.Translate(Vector3.forward * Time.deltaTime * speed);
                    if (MainSystem.Sprint == false || XZmove.magnitude < 0.5f)
                    {
                        Walking = true; // プロパティによるセット
                        Running = false;
                        MainSystem.Sprint = false;
                    }
                    else if (MainSystem.Sprint && XZmove.magnitude > 0.5f)
                    {
                        Walking = false; // プロパティによるセット
                        Running = true;
                    }

                }
                else
                {//あまりにも角度差が大きいとき、振り向きに時間をとる
                    AngleTest = (AngleTest < 0) ? -30 : 30;
                    rBody.transform.rotation = Quaternion.Euler(0, AngleBodyY + AngleTest, 0);
                }
                CameraMove.SliderCameraDeg = Mathf.LerpAngle(CameraMove.SliderCameraDeg, rBody.transform.rotation.eulerAngles.y, 0.015f);//カメラの回り込み角度を修正する
            }
            else
            {
                Walking = false; // プロパティによるセット
                transform.Translate(Vector3.zero);
            }

            if (starterAssetsInputs.attack)
            {
                UnityEngine.Debug.Log("Attack pressed!"); // ここに押し込んだ時の処理を記述 }
                animator.SetTrigger("Attack");
                starterAssetsInputs.attack = false;
            }
            if (MainSystem.Action1)
            {
                UnityEngine.Debug.Log("joystick button 2 pressed!"); // ここに押し込んだ時の処理を記述 }
                animator.SetTrigger("Attack2");
            }
            if (MainSystem.Action2)
            {
                UnityEngine.Debug.Log("joystick button 4 pressed!"); // ここに押し込んだ時の処理を記述 }
                animator.SetTrigger("Attack3");
            }
        }
    }

    void OnLanding()
    {
        StartCoroutine(WaitAfterLanding());
    }

    IEnumerator WaitAfterLanding()
    {
        animator.SetBool("isLanding", true);
        transform.Translate(Vector3.zero);
        Grounded = true;
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isLanding", false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("接触");
        if (collision.gameObject.CompareTag("Grounded"))
        {
            OnLanding();
        }
    }

    private void OnCollisionStay()
    {
        Debug.Log("接触中");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("離脱");
    }

}
