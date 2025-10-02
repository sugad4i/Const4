using UnityEngine;
using Const;

public class SphereManager2 : MonoBehaviour
{
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    public GameObject targetObject;

    public float triggerDistanceItem1 = 1f;
    public float triggerDistanceItem2 = 1.2f;
    public float item3Probability = 1f;
    public int machine_number = 1;


    public int scoreAmount = 1;

    private float previousDistanceToItem1; // 前回の距離

    private void Start()
    {
        if (item1 != null) item1.SetActive(true);
        if (item2 != null) item2.SetActive(false);
        if (item3 != null) item3.SetActive(false);

        if (targetObject != null)
        {
            previousDistanceToItem1 = Vector3.Distance(item1.transform.position, targetObject.transform.position);
        }
    }

    private void Update()
    {
        if (targetObject == null) return;


        float distanceToItem1 = Vector3.Distance(item1.transform.position, targetObject.transform.position);
        float distanceToItem2 = Vector3.Distance(item2.transform.position, targetObject.transform.position);
        float distanceToItem3 = Vector3.Distance(item3.transform.position, targetObject.transform.position);

        // item2 に触れたら item1 を表示
        if (distanceToItem2 <= triggerDistanceItem2 && item2.activeSelf)
        {
            ToggleItems(item2, item1);
            ScoreManager.Instance.AddMainScore(scoreAmount, machine_number);
            ScoreManager.Instance.AddBoxScore(scoreAmount, machine_number);
        }
        // item1 に触れたら item2 に切り替え
        else if (item1.activeSelf && distanceToItem1 <= triggerDistanceItem1)
        {
            ToggleItems(item1, item2);
        }
        // `distanceToItem1` が `triggerDistanceItem2` を跨いだときに抽選
        else if (item1.activeSelf && HasCrossedThreshold(previousDistanceToItem1, distanceToItem1, triggerDistanceItem2) && stopper.movemode[(int)machine_number] == 0)
        {
            if (Random.value <= item3Probability)
            {
                ToggleItems(item1, item3);
                stopper.movemode[(int)machine_number] = 10;
                stopper.bluemode[(int)machine_number] = 2;
                FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
            }
            else
            {
                Debug.Log("item1 は維持される");
                stopper.movemode[(int)machine_number] = 0;
            }
        }
        // item3 に触れたら処理
        else if (item3.activeSelf && distanceToItem3 <= triggerDistanceItem1)
        {
            Debug.Log("item3にふれた");
            ToggleItems(item3, item2);
            stopper.movemode[(int)machine_number] = 10;
            stopper.bluemode[(int)machine_number] = 3;
            ScoreManager.Instance.AddBlueScore(scoreAmount, machine_number);
            FindObjectOfType<TotalExcavatorController>().EndIntervention(machine_number);
        }

        // 前回の距離を更新
        previousDistanceToItem1 = distanceToItem1;
    }

    // `distance` が `threshold` を跨いだかチェック
    private bool HasCrossedThreshold(float previous, float current, float threshold)
    {
        return (previous > threshold && current <= threshold) || (previous < threshold && current >= threshold);
    }

    private void ToggleItems(GameObject currentItem, GameObject nextItem)
    {
        if (currentItem != null) currentItem.SetActive(false);
        if (nextItem != null) nextItem.SetActive(true);
    }
}