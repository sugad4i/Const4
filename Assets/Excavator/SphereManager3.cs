using UnityEngine;
using Const;

public class SphereManager3 : MonoBehaviour
{
    // サブスコアを加算するための設定
    public float minDisplayInterval = 2f;  // 最小の待機時間
    public float maxDisplayInterval = 10f; // 最大の待機時間
    public float visibleDuration = 5f;      // オブジェクトが表示される時間
    public int scoreAmount = 1;
    public float machine_number = 1f;         // サブスコアを加算する量

    // 消える/現れるアイテム（インスペクタから設定）
    public GameObject item;

    // 移動する対象のオブジェクト（インスペクタから設定）
    public GameObject targetObject;

    // 距離のしきい値
    public float distanceThreshold = 3f; // 触れる距離のしきい値
    public float initialDistanceThreshold = 5f; // 初期位置の距離しきい値

    // 移動範囲のオフセット
    public Vector3 minOffset = new Vector3(-1, -1, -1); // 最小オフセット
    public Vector3 maxOffset = new Vector3(1, 1, 1);    // 最大オフセット

    private bool isTriggered = false;      // トリガーされたかどうかを追跡
    private Vector3 initialPosition;        // アイテムの初期位置を保存
    private float displayTimer;              // アイテムの表示までのタイマー


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
        // アイテムが表示されるまでのタイマーをカウント
        if (!item.activeSelf)
        {
            displayTimer += Time.deltaTime;

            // タイマーが指定した時間を超え、初期位置との距離がしきい値を超えたらアイテムを表示
            if (displayTimer >= Random.Range(minDisplayInterval, maxDisplayInterval) &&
                Vector3.Distance(targetObject.transform.position, initialPosition) >= initialDistanceThreshold)
            {
                ShowItem();
            }
        }

        // ターゲットオブジェクトとの距離を計算
        if (targetObject != null && item.activeSelf)
        {
            float distance = Vector3.Distance(item.transform.position, targetObject.transform.position);

            // 距離がしきい値以内であればサブスコアを加算しアイテムを移動させる
            if (distance <= distanceThreshold && !isTriggered)
            {
                isTriggered = true; // トリガーされたことを記録

                // サブスコアを加算
                ScoreManager.Instance.AddSubScore(scoreAmount, (int)machine_number);

                // アイテムをtargetObjectからの相対位置に移動
                MoveItemToRandomOffset();

                // 触れられた時点でアイテムを消す
                CancelInvoke("HideItem"); // 5秒後に消える処理をキャンセル
                item.SetActive(false);

                // 次の表示をランダムな時間後に再スケジュール
                ScheduleNextAppearance();
            }
        }
    }

    private void ShowItem()
    {
        // アイテムを表示し、トリガーフラグをリセット
        item.SetActive(true);
        isTriggered = false;
        stopper.movemode[(int)machine_number] = 1;
        Debug.Log("movemode = " + stopper.movemode);


        // 5秒後にアイテムを自動で消す
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
    }

    private void MoveItemToRandomOffset()
    {
        // 相対的にランダムなオフセットを計算
        Vector3 randomOffset = new Vector3(
            Random.Range(minOffset.x, maxOffset.x),
            Random.Range(minOffset.y, maxOffset.y),
            Random.Range(minOffset.z, maxOffset.z)
        );

        // 初期位置にオフセットを追加して新しい位置を設定
        item.transform.position = initialPosition + randomOffset;
    }

    private void ScheduleNextAppearance()
    {
        // 次の表示をランダムな時間後に再スケジュール
        displayTimer = 0f; // タイマーをリセット
    }
}