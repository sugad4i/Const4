using System.Collections;
using UnityEngine;
using Const;

public class BoxManager : MonoBehaviour
{
    public float hideInterval = 2f;  // 最小の待機時間
    public float showInterval = 2f;  // 最小の待機時間
    public float machine_number = 1f; // マシン番号（0～99）
    public GameObject[] items;  // 消える/現れるオブジェクト（インスペクタから設定）
    public int hideOnMultiple = 5; // ← Inspectorで指定できる

    private void Start()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        Debug.Log("setActive true");
        stopper.boxmode[(int)machine_number] = 0;  // 初期状態でアイテム表示
        //StartCoroutine(HandleObjectAppearance());  // コルーチンを開始
    }

    private void Update()
    {
        Debug.Log("機体" + machine_number + "のboxmode: " + stopper.boxmode[(int)machine_number]);
        if (stopper.boxmode[(int)machine_number] == 3)
        {
            if (machine_number - 1 == FindObjectOfType<TotalExcavatorController>().GetControlExcavatorNumber())
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    stopper.boxmode[(int)machine_number] = 4;
                }
            }
        }
    }


    public IEnumerator HandleObjectAppearance()
    {
        Debug.Log("HandleObjectAppearance");
        yield return new WaitForSeconds(hideInterval); // 一定時間待つ
        HideItems();
        yield return new WaitForSeconds(showInterval); // 一定時間待つ
        ShowItems(); // 再表示
    }

    // アイテムを全て表示するメソッド
    private void ShowItems()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        stopper.boxmode[(int)machine_number] = 3;  // アイテム表示時にモードを変更
        //FindObjectOfType<TotalExcavatorController>().RequestIntervention((int)machine_number);
    }

    // アイテムを全て非表示にするメソッド
    public void HideItems()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
        stopper.boxmode[(int)machine_number] = 1;  // アイテム非表示時にモードを変更
    }
}