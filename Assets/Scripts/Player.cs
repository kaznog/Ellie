using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using StarterAssets;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed = 5; // Inspector�r���[�ŕύX�\
    public float jumpForce = 5.0f;
    public float waitTime = 2.0f;
    Animator animator;
    public Rigidbody rBody;//�̂̃��O
    public LayerMask groundLayer; // �n�ʂ̃��C���[�}�X�N
    public float checkDistance = 0.1f; // ���C�L���X�g�̋���
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
    bool walking = false; // �t�B�[���h
    bool Walking
    { // �v���p�e�B
        get { return walking; }
        set
        { // �l���قȂ�Z�b�g���̂�animator.SetBool���ĂԂ悤�ɂ��܂�
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

        // Animator����ObservableStateMachineTrigger�̎Q�Ƃ��擾
        ObservableStateMachineTrigger trigger =
            animator.GetBehaviour<ObservableStateMachineTrigger>();

        // State�̊J�n�C�x���g
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

        // State�̏I���C�x���g
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
        // �I�u�W�F�N�g�̐^���Ƀ��C�L���X�g���s��
        if (Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer) && animator.GetBool("isLanding") == false)
        { // ���n?
            Debug.Log("Grounded");
            Grounded = true;
        }
        else
        { // ���n���Ă��Ȃ�
            Debug.Log("Not Grounded");
            Grounded = false;
        }
        float stick_x = MainSystem.stick_x;
        float stick_z = MainSystem.stick_z;
        Vector3 XZmove = new Vector3(stick_x, 0, stick_z);//�X�e�B�b�N�X�΃x�N�g��
        {
            UnityEngine.Debug.Log("XZmove.magnitude:" + XZmove.magnitude);
            //Editor���Ƀf�[�^���Z�b�g
            float XZmag = XZmove.magnitude;
            float AtanDeg = Mathf.Atan2(stick_x, stick_z) * Mathf.Rad2Deg;//�X�e�B�b�N�̌X�Ίp�x
            Quaternion XZQT = rBody.transform.rotation;
            if (XZmag > 0 /*&& animator.GetBool("isLanding") == false && animator.GetBool("Guard") == false*/)
            {
                //�ړ����̂Ƃ��A���U�����[�V�������ł͂Ȃ��Ƃ�
                float AngleTarget = CameraMove.SliderCameraDeg + AtanDeg;
                float AngleBodyY = rBody.transform.rotation.eulerAngles.y;
                float AngleTest = Mathf.DeltaAngle(AngleBodyY, AngleTarget);//�����Ă�������ƃX�e�B�b�N�̊p�x��
                if (AngleTest > -30 && AngleTest < 30)
                {//�������l�ȓ��̂Ƃ��A����
                    rBody.transform.rotation = Quaternion.Euler(0, AngleTarget, 0);
                    // �O�Ɉړ�����
                    transform.Translate(Vector3.forward * Time.deltaTime * speed);
                    if (MainSystem.Sprint == false || XZmove.magnitude < 0.5f)
                    {
                        Walking = true; // �v���p�e�B�ɂ��Z�b�g
                        Running = false;
                        MainSystem.Sprint = false;
                    }
                    else if (MainSystem.Sprint && XZmove.magnitude > 0.5f)
                    {
                        Walking = false; // �v���p�e�B�ɂ��Z�b�g
                        Running = true;
                    }

                }
                else
                {//���܂�ɂ��p�x�����傫���Ƃ��A�U������Ɏ��Ԃ��Ƃ�
                    AngleTest = (AngleTest < 0) ? -30 : 30;
                    rBody.transform.rotation = Quaternion.Euler(0, AngleBodyY + AngleTest, 0);
                }
                CameraMove.SliderCameraDeg = Mathf.LerpAngle(CameraMove.SliderCameraDeg, rBody.transform.rotation.eulerAngles.y, 0.015f);//�J�����̉�荞�݊p�x���C������
            }
            else
            {
                Walking = false; // �v���p�e�B�ɂ��Z�b�g
                transform.Translate(Vector3.zero);
            }

            if (starterAssetsInputs.attack)
            {
                UnityEngine.Debug.Log("Attack pressed!"); // �����ɉ������񂾎��̏������L�q }
                animator.SetTrigger("Attack");
                starterAssetsInputs.attack = false;
            }
            if (MainSystem.Action1)
            {
                UnityEngine.Debug.Log("joystick button 2 pressed!"); // �����ɉ������񂾎��̏������L�q }
                animator.SetTrigger("Attack2");
            }
            if (MainSystem.Action2)
            {
                UnityEngine.Debug.Log("joystick button 4 pressed!"); // �����ɉ������񂾎��̏������L�q }
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
        Debug.Log("�ڐG");
        if (collision.gameObject.CompareTag("Grounded"))
        {
            OnLanding();
        }
    }

    private void OnCollisionStay()
    {
        Debug.Log("�ڐG��");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("���E");
    }

}
