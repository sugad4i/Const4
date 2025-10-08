using UnityEngine;
using Const;


public class ScoreManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static ScoreManager Instance;
    public int machine_number = 1;

    // メインスコアとサブスコアを管理する変数
    private static int[] mainScore = new int[100];
    private static int[] subScore = new int[100];
    private static int[] blueScore = new int[100];
    private static int[] boxScore = new int[100];

    // スコア表示用のTextMeshPro
    //public TextMeshProUGUI mainScoreText;
    //public TextMeshProUGUI subScoreText;
    //public TextMeshProUGUI blueScoreText;
    //public TextMeshProUGUI boxScoreText;

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
        
        stopper.boxmode[(int)machine_number] = 6; // 例えば移動モードを優先
    }

    // サブスコアを追加するメソッド
    public void AddSubScore(int amount,int machine_number)
    {
        subScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のサブスコア: {subScore[(int)machine_number]}");
        //UpdateSubScoreText();
    }

    public void AddBlueScore(int amount,int machine_number)
    {
        blueScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のBlueスコア: {blueScore[(int)machine_number]}");
        //UpdateBlueScoreText();
    }

    public void AddBoxScore(int amount,int machine_number)
    {
        boxScore[(int)machine_number] += amount;
        Debug.Log($"機体{machine_number} の現在のBoxスコア: {mainScore[(int)machine_number]}");
        //UpdateMainScoreText();
        
        // BoxManagerを探して、そのhideOnMultipleで判定
        //var relocationManagers = FindObjectsOfType<RelocationManager>();

    }

    // メインスコアを取得するメソッド
    public int GetMainScore(int machine_number)
    {
        return mainScore[(int)machine_number];
    }

    // サブスコアを取得するメソッド
    public int GetSubScore(int machine_number)
    {
        return subScore[(int)machine_number];
    }

    public int GetBoxScore(int machine_number)
    {
        return boxScore[(int)machine_number];
    }

    // メインスコアのテキストを更新
    //private void UpdateMainScoreText()
    //{
    //    if (mainScoreText != null)
    //    {
    //        mainScoreText.text = "Main Score: " + mainScore;
    //    }
    //}

    // サブスコアのテキストを更新
    //private void UpdateSubScoreText()
    //{
    //    if (subScoreText != null)
    //    {
    //        subScoreText.text = "Sub Score: " + subScore;
    //    }
    //}
    //}

    //private void UpdateBlueScoreText()
    //{
    //    if (blueScoreText != null)
    //    {
    //        blueScoreText.text = "Blue Score: " + blueScore;
    //    }
    //}
}
