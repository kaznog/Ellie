using System.Diagnostics;
using UnityEngine;

public class MainSystem : MonoBehaviour
{
    //とりあえずランダム値を使いたいとき、ここからNextする
    static public System.Random rnd;
    //アナログスティックの傾斜
    static public float stick_x = 0;
    static public float stick_z = 0;
    static public float stick2_x = 0;
    static public float stick2_z = 0;

    static public bool Sprint = false;
    static public bool Action0 = false;
    static public bool Action1 = false;
    static public bool Action2 = false;

    //マシンの時間
    static public long tick;
    static public Stopwatch stopwatch = new Stopwatch();
    static public bool DoubleTapFlg = false;//ダブルクリックされた状態かどうか
    private float LastClickTime = 0;//最後にクリックされた時間（ダブルクリック検出用）
    public GameObject selfGo;//操作キャラのゲームオブジェクト

    static public MainSystem Core;//外からメインシステムの実体を呼びたい場合はコレ

    public delegate void stdDelegate();//とりあえず基本型のデリゲート
    public static stdDelegate OnGUIDelegate = null;//OnGUIでボタンなんかを出したくなったら、ここにメソッドを割り当てるよ
    public static stdDelegate ConnectionLostDelegate = null;//接続が切れたときに再接続するためのデリゲート

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
        if (Input.GetKey("escape")) { Application.Quit(); }//ゲーム終了
        {//解像度の変更を検知。バーチャルスティックがない場合は、プレハブからもってくる。
            if (Screen.width != LastScreenSize_x || Screen.height != LastScreenSize_y)
            {
                UnityEngine.Debug.Log("Change Screen Size");
                LastScreenSize_x = Screen.width;
                LastScreenSize_y = Screen.height;
            }
        }
        //DeltaTimeから切り離されたゲーム用の時間を常に計測しておく
        tick = stopwatch.ElapsedMilliseconds;
        //アナログスティックやタッチの状態はすべてここで取得しておく
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
        //インプット関連ここまで

    }

    public static GameObject ChildAllFind(Transform trans, ref string name)
    {//指定したTransformの子供から、指定した名前のゲームオブジェクトを取り出す便利メソッド
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
