using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;


public class SphereManager5 : MonoBehaviour
{
    // サブスコアを加算するための設定
    public float minDisplayInterval = 2f;  // 最小の待機時間
    public float maxDisplayInterval = 10f; // 最大の待機時間
    public float visibleDuration = 5f;      // オブジェクトが表示される時間
    public int scoreAmount = 1;             // サブスコアを加算する量
    public int machine_number = 1; // マシン番号（0～99）
    public Vector3 relativeMove1 = new Vector3(1,0,0); // 相対座標の移動量
    public Vector3 relativeMove2 = new Vector3(1,0,0); // 相対座標の移動量
    public float moveSpeed = 1.0f; // 移動速度（単位: ユニット/秒）

    private Vector3 targetPosition; // 計算された目的地
    private bool isMoving1 = false; // 移動1フラグ
    private bool isMoving2 = false; // 移動2フラグ


    // 消える/現れるアイテム（インスペクタから設定）
    public GameObject item;

    // 移動する対象のオブジェクト（インスペクタから設定）
    public GameObject targetObject;

    // 距離のしきい値
    public float distanceThreshold = 3f; // 触れる距離のしきい値
    public float initialDistanceThreshold = 5f; // 初期位置の距離しきい値

    // 移動範囲のオフセット
    //public Vector3 minOffset = new Vector3(-1, -1, -1); // 最小オフセット
    //public Vector3 maxOffset = new Vector3(1, 1, 1);    // 最大オフセット

    private bool isTriggered = false;      // トリガーされたかどうかを追跡
    private Vector3 initialPosition;        // アイテムの初期位置を保存
    private float displayTimer;              // アイテムの表示までのタイマー
    private bool isHKeyPressed = false;     // Hキーが押されたかどうか

    private float forcedHideTimer = -1f; // Kキーでの強制消去タイマー
    public float forcedHideDelay = 3f; // Kキー押下後に消えるまでの遅延時間


    private void StartMove1()
    {
        targetPosition = initialPosition + relativeMove1; // 移動1の目的地
        isMoving1 = true;
    }

    private void StartMove2()
    {
        targetPosition = initialPosition + relativeMove1 + relativeMove2; // 移動2の目的地
        isMoving2 = true;
    }

    

    private void Start()
    {
        // 初めはアイテムを非表示にしておく
        item.SetActive(false);

        // アイテムの初期位置を記憶
        initialPosition = item.transform.position;

        // タイマーをリセット
        displayTimer = -5f;
    }

    private void Update()
    {
        // Hキーが押された場合
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!item.activeSelf)
            {
                stopper.manmode[(int)machine_number] = 2;
                isHKeyPressed = true; // Hキーが押されたことを記録
                displayTimer = 0f; // Hキーが押された時点でタイマーをリセット
                Debug.Log("Hキーが押され、球が出ていないためmanmode = 2に設定しました。");
                FindObjectOfType<TotalExcavatorController>().EndIntervention(machine_number);
            }
        }

        // Kキーでの消去タイマーを制御
        if (Input.GetKeyDown(KeyCode.K) && item.activeSelf)
        {
            forcedHideTimer = Mathf.Min(visibleDuration, forcedHideDelay); // 強制消去のタイマーを設定
            Debug.Log("Kキーが押され、球の消去がスケジュールされました。");
        }

        if (forcedHideTimer >= 0)
        {
            forcedHideTimer -= Time.deltaTime;
            if (forcedHideTimer <= 0)
            {
                HideItem();
                forcedHideTimer = -1f; // タイマーをリセット
            }
        }

        // アイテムが表示されるまでのタイマーをカウント
        if (!item.activeSelf) // Hキーが押された後にカウントを開始
        {
            displayTimer += Time.deltaTime;

            // `manmode == 0` を条件に追加してアイテムの表示を制御
            if (stopper.manmode[(int)machine_number] == 0 &&
                displayTimer >= Random.Range(minDisplayInterval, maxDisplayInterval) )
            {
                ShowItem();
            }
        }

        // 移動1を処理
        if (isMoving1)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(item.transform.position, targetPosition) < 0.01f)
            {
                isMoving1 = false;
                StartMove2(); // 移動2を開始
            }
        }

        // 移動2を処理
        if (isMoving2)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(item.transform.position, targetPosition) < 0.01f)
            {
                isMoving2 = false;
                OnMoveComplete(); // 全移動完了
            }
        }
    }


    private void OnMoveComplete()
    {
        Debug.Log("全ての移動完了");
    }

    private void ShowItem()
    {
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
        // アイテムを表示し、トリガーフラグをリセット
        isTriggered = false;
        stopper.capturemode[(int)machine_number] = 1;
        Debug.Log("manmode = " + stopper.manmode);   
        Invoke("HideItem", visibleDuration);
    }

    private void HideItem()
    {
        // アイテムが触れられていなければ、非アクティブにする
        if (!isTriggered)
        {
            item.SetActive(false);
        }

        // タイマーをリセット
        displayTimer = 0f;
        isHKeyPressed = false; // Hキーが押された後は、次のタイマー開始までリセット
        stopper.manmode[(int)machine_number] = 4;
        stopper.capturemode[(int)machine_number] = 0;
    }

}