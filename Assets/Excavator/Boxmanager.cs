using System.Collections;
using UnityEngine;
using Const;

public class BoxManager : MonoBehaviour
{
    public float showInterval_box = 2f;  // 最小の待機時間
    public float showInterval_relocation = 2f;  // 最小の待機時間
    public int machine_number = 1; // マシン番号（0～99）
    public GameObject[] items;  // 消える/現れるオブジェクト（インスペクタから設定）
    public int hidenumber = 5; // ← Inspectorで指定できる
    public int relocationnumber = 5; // ← Inspectorで指定できる
    public int scoreAmount = 1;
    int count = 0;

    private void Start()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        //Debug.Log("setActive true");
        stopper.boxmode[(int)machine_number] = 0;  // 初期状態でアイテム表示
        //StartCoroutine(HandleObjectAppearance());  // コルーチンを開始
    }

    private void Update()
    {
        //Debug.Log("機体" + machine_number + "のboxmode: " + stopper.boxmode[(int)machine_number]);
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
        Debug.Log("機体" + machine_number + "のcount: " + count);
    }


    public IEnumerator HandleObjectAppearance()
    {
        
        //Debug.Log("HandleObjectAppearance");
        count = ScoreManager.Instance.GetBoxScore(machine_number);
        if (count % relocationnumber != 0)
        {
            Debug.Log("box");
            HideItems();
            yield return new WaitForSeconds(showInterval_box); // 一定時間待つ
            ShowItems(); // 再表示
        }
        else
        {
            Debug.Log("移動中");
            yield return new WaitForSeconds(showInterval_relocation); // 一定時間待つ
            FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
            stopper.boxmode[(int)machine_number] = 3;  // アイテム表示時にモードを変更
        }
        
    }

    // アイテムを全て表示するメソッド
    private void ShowItems()
    {
        foreach (var item in items)
        {
            item.SetActive(true);
        }
        stopper.boxmode[(int)machine_number] = 3;  // アイテム表示時にモードを変更
        FindObjectOfType<TotalExcavatorController>().RequestIntervention(machine_number);
        //FindObjectOfType<TotalExcavatorController>().RequestIntervention((int)machine_number);
    }

    // アイテムを全て非表示にするメソッド
    public void HideItems()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
        }
    }
}