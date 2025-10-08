using System.Collections;
using UnityEngine;
using Const;
using Unity.VisualScripting;

public class BoxManager : MonoBehaviour
{
    public float showInterval_box = 2f;  // 最小の待機時間
    public float showInterval_relocation = 2f;  // 最小の待機時間
    public int machine_number = 1; // マシン番号（0～99）
    public GameObject item;  // 消える/現れるオブジェクト（インスペクタから設定）
    public int hidenumber = 1; // ← Inspectorで指定できる
    public int relocationnumber = 2; // ← Inspectorで指定できる
    int count = 0;


    private void Start()
    {
        item.SetActive(true);
        stopper.boxmode[(int)machine_number] = 0;  // 初期状態でアイテム表示
    }

    private void Update()
    {
        if (stopper.boxmode[(int)machine_number] == 3)
        {
            if (machine_number - 1 == FindObjectOfType<TotalExcavatorController>().GetControlExcavatorNumber())
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    stopper.boxmode[(int)machine_number] = 4;
                    FindObjectOfType<TotalExcavatorController>().EndIntervention(machine_number);
                }
            }
        }
        count = ScoreManager.Instance.GetMainScore(machine_number);
        Debug.Log("機体" + machine_number + "のcount: " + count);
    }


    public IEnumerator HideDump(int machine_number)
    {
        HideItems();
        yield return new WaitForSeconds(showInterval_box); // 一定時間待つ
        ShowItems(); // 再表示
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
        stopper.boxmode[(int)machine_number] = 3;
        yield break;
    }

    public IEnumerator MoveExcavator(int machine_number)
    {
        //Debug.Log("box");
        yield return new WaitForSeconds(showInterval_box); // 一定時間待つ
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
        stopper.boxmode[(int)machine_number] = 3;
        yield break;
    }


    // アイテムを全て表示するメソッド
    private void ShowItems()
    {
        item.SetActive(true);
    }

    // アイテムを全て非表示にするメソッド
    public void HideItems()
    {
        item.SetActive(false);
    }
}