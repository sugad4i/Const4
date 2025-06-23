using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static ScoreManager Instance;

    // メインスコアとサブスコアを管理する変数
    private int mainScore = 0;
    private int subScore = 0;
    private int blueScore = 0;

    // スコア表示用のTextMeshPro
    public TextMeshProUGUI mainScoreText;
    public TextMeshProUGUI subScoreText;
    public TextMeshProUGUI blueScoreText;

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
    public void AddMainScore(int amount)
    {
        mainScore += amount;
        Debug.Log("現在のメインスコア: " + mainScore);
        UpdateMainScoreText();
    }

    // サブスコアを追加するメソッド
    public void AddSubScore(int amount)
    {
        subScore += amount;
        Debug.Log("現在のサブスコア: " + subScore);
        UpdateSubScoreText();
    }

    public void AddBlueScore(int amount)
    {
        subScore += amount;
        Debug.Log("現在のBlueスコア: " + blueScore);
        UpdateBlueScoreText();
    }

    // メインスコアを取得するメソッド
    public int GetMainScore()
    {
        return mainScore;
    }

    // サブスコアを取得するメソッド
    public int GetSubScore()
    {
        return subScore;
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
