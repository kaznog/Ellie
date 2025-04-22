using System.Diagnostics;
using UnityEngine;

public class MainSystem : MonoBehaviour
{
    //�Ƃ肠���������_���l���g�������Ƃ��A��������Next����
    static public System.Random rnd;
    //�A�i���O�X�e�B�b�N�̌X��
    static public float stick_x = 0;
    static public float stick_z = 0;
    static public float stick2_x = 0;
    static public float stick2_z = 0;

    static public bool Sprint = false;
    static public bool Action0 = false;
    static public bool Action1 = false;
    static public bool Action2 = false;

    //�}�V���̎���
    static public long tick;
    static public Stopwatch stopwatch = new Stopwatch();
    static public bool DoubleTapFlg = false;//�_�u���N���b�N���ꂽ��Ԃ��ǂ���
    private float LastClickTime = 0;//�Ō�ɃN���b�N���ꂽ���ԁi�_�u���N���b�N���o�p�j
    public GameObject selfGo;//����L�����̃Q�[���I�u�W�F�N�g

    static public MainSystem Core;//�O���烁�C���V�X�e���̎��̂��Ăт����ꍇ�̓R��

    public delegate void stdDelegate();//�Ƃ肠������{�^�̃f���Q�[�g
    public static stdDelegate OnGUIDelegate = null;//OnGUI�Ń{�^���Ȃ񂩂��o�������Ȃ�����A�����Ƀ��\�b�h�����蓖�Ă��
    public static stdDelegate ConnectionLostDelegate = null;//�ڑ����؂ꂽ�Ƃ��ɍĐڑ����邽�߂̃f���Q�[�g

    public static int LastScreenSize_x = 0;
    public static int LastScreenSize_y = 0;


    void OnApplicationQuit()
    {
        stopwatch.Stop();
    }

    [RuntimeInitializeOnLoadMethod()]
    static void Init()
    {
        Application.targetFrameRate = 60;
        //      Screen.SetResolution(1280, 720, true, Application.targetFrameRate);

        rnd = new System.Random();
        stopwatch.Start();

        LastScreenSize_x = Screen.width;
        LastScreenSize_y = Screen.height;

        GUIStyleState state = new GUIStyleState();
        state.textColor = Color.white;
    }


    public void Awake()
    {
        if (Core != null)
        {
            Destroy(transform.gameObject);
            return;
        }
        Core = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape")) { Application.Quit(); }//�Q�[���I��
        {//�𑜓x�̕ύX�����m�B�o�[�`�����X�e�B�b�N���Ȃ��ꍇ�́A�v���n�u��������Ă���B
            if (Screen.width != LastScreenSize_x || Screen.height != LastScreenSize_y)
            {
                UnityEngine.Debug.Log("Change Screen Size");
                LastScreenSize_x = Screen.width;
                LastScreenSize_y = Screen.height;
            }
        }
        //DeltaTime����؂藣���ꂽ�Q�[���p�̎��Ԃ���Ɍv�����Ă���
        tick = stopwatch.ElapsedMilliseconds;
        //�A�i���O�X�e�B�b�N��^�b�`�̏�Ԃ͂��ׂĂ����Ŏ擾���Ă���
        if (stick_x == 0f) {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A))
            {
                stick_x = -1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.D))
            {
                stick_x = 1;
            }
        }
        if (stick_z == 0f)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                stick_z = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                stick_z = -1;
            }
        }

        bool DTapFlgCH = false;
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.fixedTime - LastClickTime < 0.5f)
            {
                DTapFlgCH = true;
            }
            LastClickTime = Time.fixedTime;
        }
        else if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].tapCount > 1)
                {
                    DTapFlgCH = true;
                    break;
                }
            }
        }
        DoubleTapFlg = DTapFlgCH;
        //�C���v�b�g�֘A�����܂�

    }

    public static GameObject ChildAllFind(Transform trans, ref string name)
    {//�w�肵��Transform�̎q������A�w�肵�����O�̃Q�[���I�u�W�F�N�g�����o���֗����\�b�h
        int childcount = trans.childCount;
        Transform tr = trans.Find(name);
        if (tr != null)
        {
            return tr.gameObject;
        }
        for (int i = 0; i < childcount; i++)
        {
            tr = trans.GetChild(i);
            GameObject go = ChildAllFind(tr, ref name);
            if (go != null)
            {
                return go;
            }
        }
        return null;
    }

}
