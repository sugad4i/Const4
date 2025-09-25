using System.Collections;
using UnityEngine;
using Const;

public class RelocationManager : MonoBehaviour
{
    public float showInterval = 2f;  // 最小の待機時間
    public int machine_number = 1; // マシン番号（0～99）
    public GameObject[] items;  // 消える/現れるオブジェクト（インスペクタから設定）
    public int hideOnMultiple = 5; // ← Inspectorで指定できる

    private void Start()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
        Debug.Log("setActive true");
        stopper.relocationmode[(int)machine_number] = 0;  // 初期状態でアイテム表示
        //StartCoroutine(HandleObjectAppearance());  // コルーチンを開始
    }

    private void Update()
    {
        //Debug.Log("機体" + machine_number + "のrelocationmode: " + stopper.relocationmode[(int)machine_number]);
        if (stopper.relocationmode[(int)machine_number] == 3)
        {
            if (machine_number - 1 == FindObjectOfType<TotalExcavatorController>().GetControlExcavatorNumber())
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    stopper.relocationmode[(int)machine_number] = 5;
                    FindObjectOfType<TotalExcavatorController>().EndIntervention(machine_number);
                }
            }
        }
    }


    public IEnumerator HandleObjectAppearance()
    {
        if (stopper.relocationmode[(int)machine_number] == 2)
        {
            Debug.Log("HandleObjectAppearance");
            ShowItems();
            yield return new WaitForSeconds(showInterval); // 一定時間待つ
            HideItems(); 
            
        }
    }

    // アイテムを全て表示するメソッド
    private void ShowItems()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        stopper.relocationmode[(int)machine_number] = 3;  // アイテム表示時にモードを変更
        //FindObjectOfType<TotalExcavatorController>().RequestIntervention((int)machine_number);
    }

    // アイテムを全て非表示にするメソッド
    public void HideItems()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
        stopper.relocationmode[(int)machine_number] = 4;  // アイテム非表示時にモードを変更
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
    }
}