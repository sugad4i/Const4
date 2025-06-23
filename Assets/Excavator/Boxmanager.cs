using System.Collections;
using UnityEngine;
using Const;

public class BoxManager : MonoBehaviour
{
    public float minDisplayInterval = 2f;  // 最小の待機時間
    public float maxDisplayInterval = 10f; // 最大の待機時間
    public float visibleDuration = 5f;      // 表示される時間
    public float machine_number = 1f; // マシン番号（0～99）
    public GameObject[] items;  // 消える/現れるオブジェクト（インスペクタから設定）
    public GameObject targetObject;  // 監視対象のオブジェクト（インスペクタから設定）

    public float rangeThreshold = 5f;  // 指定範囲（範囲外の場合アイテムを非表示）

    private float minDistance;  // 現時点での最小距離
    private GameObject closestItem;  // 最も近いアイテム

    private void Start()
    {
        ShowItems();  // 初めにアイテムを表示
        StartCoroutine(HandleObjectAppearance());  // コルーチンを開始
    }

    private void Update()
    {
        // 最小距離と最も近いアイテムを計算
        CalculateMinDistance();
    }

    private void CalculateMinDistance()
    {
        if (targetObject == null || items == null || items.Length == 0)
        {
            Debug.LogWarning("TargetObject or items are not properly assigned!");
            return;
        }

        minDistance = float.MaxValue;
        closestItem = null;

        foreach (var item in items)
        {
            if (item != null)
            {
                float distance = Vector3.Distance(item.transform.position, targetObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestItem = item;
                }
            }
        }

        if (closestItem != null)
        {
            Debug.Log($"Closest item: {closestItem.name}, Distance: {minDistance}");
        }
    }

    IEnumerator HandleObjectAppearance()
    {
        while (true)
        {
            // ランダムな時間を待機
            float waitTime = Random.Range(minDisplayInterval, maxDisplayInterval);
            yield return new WaitForSeconds(waitTime);

            // アイテムを表示してから、指定時間だけ表示され続ける
            ShowItems();

            // 表示後、指定時間待つ
            yield return new WaitForSeconds(visibleDuration);

            // 最小距離が範囲外なら非表示
            if (minDistance > rangeThreshold)
            {
                HideItems();
            }
            else
            {
                // 最小距離が範囲内の場合は、範囲外に出るまで待機
                while (minDistance <= rangeThreshold)
                {
                    CalculateMinDistance();  // 距離を再計算
                    yield return null;
                }
                HideItems();
            }
        }
    }

    // アイテムを全て表示するメソッド
    private void ShowItems()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        stopper.movemode[(int)machine_number] = 2;  // アイテム表示時にモードを変更
    }

    // アイテムを全て非表示にするメソッド
    private void HideItems()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
        stopper.movemode[(int)machine_number] = 3;  // アイテム非表示時にモードを変更
    }
}