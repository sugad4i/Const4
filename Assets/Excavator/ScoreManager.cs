using UnityEngine;
using TMPro;
using Const;

public class ScoreManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static ScoreManager Instance;
    public float machine_number = 1F;

    // メインスコアとサブスコアを管理する変数
    private static int[] mainScore = new int[100];
    private static int[] subScore = new int[100];
    private static int[] blueScore = new int[100];
    private static int[] boxScore = new int[100];

    // スコア表示用のTextMeshPro
    public TextMeshProUGUI mainScoreText;
    public TextMeshProUGUI subScoreText;
    public TextMeshProUGUI blueScoreText;
    public TextMeshProUGUI boxScoreText;

    private void Awake()
    {
        // シングルトンパターンでインスタンスを一つだけにする
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // シーンをまたいでもオブジェクトを破壊しない
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // メインスコアを追加するメソッド
    public void AddMainScore(int amount, int machine_number)
    {
        mainScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のメインスコア: {mainScore[(int)machine_number]}");
        UpdateMainScoreText();
        
        // BoxManagerを探して、そのhideOnMultipleで判定
        var boxManagers = FindObjectsOfType<BoxManager>();
        foreach (var box in boxManagers)
        {
            if ((int)box.machine_number == machine_number)
            {
                int n = box.hideOnMultiple;
                if (n > 0 && mainScore[(int)machine_number] % n == 0)
                {
                    StartCoroutine(box.HandleObjectAppearance());
                }
            }
        }
    }

    // サブスコアを追加するメソッド
    public void AddSubScore(int amount,int machine_number)
    {
        subScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のサブスコア: {subScore[(int)machine_number]}");
        UpdateSubScoreText();
    }

    public void AddBlueScore(int amount,int machine_number)
    {
        blueScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のBlueスコア: {blueScore[(int)machine_number]}");
        UpdateBlueScoreText();
    }

    public void AddBoxScore(int amount,int machine_number)
    {
        boxScore[(int)machine_number] += amount;
        //Debug.Log($"機体{machine_number} の現在のBoxスコア: {boxScore[(int)machine_number]}");
    }

    // メインスコアを取得するメソッド
    public int GetMainScore()
    {
        return mainScore[(int)machine_number];
    }

    // サブスコアを取得するメソッド
    public int GetSubScore()
    {
        return subScore[(int)machine_number];
    }

    // メインスコアのテキストを更新
    private void UpdateMainScoreText()
    {
        if (mainScoreText != null)
        {
            mainScoreText.text = "Main Score: " + mainScore;
        }
    }

    // サブスコアのテキストを更新
    private void UpdateSubScoreText()
    {
        if (subScoreText != null)
        {
            subScoreText.text = "Sub Score: " + subScore;
        }
    }

    private void UpdateBlueScoreText()
    {
        if (blueScoreText != null)
        {
            blueScoreText.text = "Blue Score: " + blueScore;
        }
    }
}
