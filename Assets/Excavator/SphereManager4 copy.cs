using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;


public class SphereManager4copy : MonoBehaviour
{
    // サブスコアを加算するための設定
    public float minDisplayInterval = 2f;  // 最小の待機時間
    public float maxDisplayInterval = 10f; // 最大の待機時間
    public float visibleDuration = 5f;      // オブジェクトが表示される時間
    int scoreAmount = 1;             // サブスコアを加算する量
    public int machine_number = 1; // マシン番号（0～99）
    public Vector3 relativeMove = new Vector3(1,0,0); // 相対座標の移動量
    float moveSpeed = 2.0f; // 移動速度（単位: ユニット/秒）


    private Vector3 targetPosition; // 計算された目的地
    private bool isMoving1 = false; // 移動1フラグ
    // 消える/現れるアイテム（インスペクタから設定）
    public GameObject item;
    private bool isTriggered = false;      // トリガーされたかどうかを追跡
    private Vector3 initialPosition;        // アイテムの初期位置を保存
    float displayTimer = 0f;          // アイテムの表示までのタイマー
    private float FirstDisplayTimer = 10f;              // アイテムの表示までのタイマー
    private bool isHKeyPressed = false;     // Hキーが押されたかどうか

    private float forcedHideTimer = -1f; // Kキーでの強制消去タイマー
    private float forcedHideDelay = 3f; // Kキー押下後に消えるまでの遅延時間

    public void Delay(float delay, int machine_number)
    {
        StartCoroutine(DelayedStartRecording(delay, machine_number));
    }

    private IEnumerator DelayedStartRecording(float delay, int machine_number)
    {
        yield return new WaitForSeconds(delay); // 指定時間待機
        stopper.manmode[(int)machine_number] = 1; // manmodeを更新
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
        item.SetActive(true); // アイテムをアクティブ化
        StartMove();
    }

    private void StartMove()
    {
        targetPosition = initialPosition + relativeMove; // 移動1の目的地
        isMoving1 = true;
    }


    

    private void Start()
    {
        // 初めはアイテムを非表示にしておく
        item.SetActive(false);

        // アイテムの初期位置を記憶
        initialPosition = item.transform.position;

        // タイマーをリセット
        displayTimer = -1 * FirstDisplayTimer;

        stopper.manmode[(int)machine_number] = 0;
    }

    private void Update()
    {
        // スペースキーが押された場合
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!item.activeSelf)
            {
                stopper.manmode[(int)machine_number] = 0;
                isHKeyPressed = true; // Hキーが押されたことを記録
                displayTimer = 0f; // Hキーが押された時点でタイマーをリセット
                Debug.Log("Hキーが押され、球が出ていないためmanmode = 0に設定しました。");
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
                //HideItem(machine_number);
                forcedHideTimer = -1f; // タイマーをリセット
            }
        }

        if (!item.activeSelf &&
            stopper.movemode[(int)machine_number] == 0 &&
            stopper.boxmode[(int)machine_number] == 0 &&
            stopper.bluemode[(int)machine_number] == 0 &&
            stopper.manmode[(int)machine_number] == 0)
        {
            displayTimer += Time.deltaTime;

            if (stopper.movemode[(int)machine_number] == 0 &&
            stopper.boxmode[(int)machine_number] == 0 &&
            stopper.bluemode[(int)machine_number] == 0 &&
            displayTimer >= Random.Range(minDisplayInterval, maxDisplayInterval))
            {
                ShowItem(machine_number);
            }
        }

        // 移動1を処理
        if (isMoving1)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(item.transform.position, targetPosition) < 0.01f)
            {
                isMoving1 = false;
                OnMoveComplete(); // 全移動完了
            }
        }
    }


    private void OnMoveComplete()
    {
        Debug.Log("全ての移動完了");
        HideItem(machine_number);
    }

    private void ShowItem(int machine_number)
    {
        isTriggered = false;
        stopper.capturemode[(int)machine_number] = 1;
        // Debug.Log("manmode = " + stopper.manmode);
        Delay(2.0f, machine_number);
        // Debug.Log("delay");
        Invoke("HideItem", visibleDuration);
    }

    private void HideItem(int machine_number)
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