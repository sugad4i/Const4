using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;

public class DashcamManager : MonoBehaviour
{
    public Camera recordingCamera; // 録画用カメラ
    public RawImage playbackScreen; // 再生用のUI (RawImage)
    public int frameRate = 30; // 録画と再生のフレームレート

    private Dictionary<int, List<Texture2D>> recordedFrames = new Dictionary<int, List<Texture2D>>(); // 各機体のフレームを保存
    private bool isRecording = false; // 録画中フラグ
    private bool isPlaying = false; // 再生中フラグ
    private float captureInterval; // フレームキャプチャ間隔
    private float timeSinceLastCapture = 0f; // 最後にキャプチャしたフレームからの経過時間
    private int currentPlaybackFrame = 0; // 現在の再生フレーム
    private float timeSinceLastFrame = 0f; // 前の再生フレームからの経過時間

    public int machine_number = 1;    
    public GameObject playbackScreenParent; // RawImageを持つ親オブジェクト
    public Color borderColor = Color.black; // 枠線の色
    public float borderWidth = 5f; // 枠線の太さ

    public void Delay(float delay)
    {
        StartCoroutine(DelayedStartRecording(delay, machine_number));
    }

    private IEnumerator DelayedStartRecording(float delay, int machine_number)
    {
        yield return new WaitForSeconds(delay); // 指定時間待機
        StopRecording(machine_number); // 待機後に録画停止
    }

    void Start()
    {
        captureInterval = 1f / frameRate; // 1フレームあたりの間隔
        playbackScreen.gameObject.SetActive(false); // 最初は再生画面を非表示
    }

    void Update()
    {
        // 録画処理
        if (isRecording)
        {
            timeSinceLastCapture += Time.deltaTime;
            if (timeSinceLastCapture >= captureInterval)
            {
                CaptureFrame(machine_number);
                timeSinceLastCapture = 0f; // 経過時間をリセット
            }
        }

        // 再生処理
        if (isPlaying)
        {
            timeSinceLastFrame += Time.deltaTime;
            if (timeSinceLastFrame >= captureInterval)
            {
                PlayFrame(machine_number);
                timeSinceLastFrame = 0f;
            }
        }

        if (stopper.capturemode[machine_number] == 1 && !isRecording) // 録画開始
        {
            StartRecording(machine_number);
        }

        if (stopper.manmode[machine_number] == 4) // 録画停止
        {
            Delay(5.0f); // 5秒待って録画を停止
        }

        if (Input.GetKeyDown(KeyCode.P)) // Pキーで再生開始
        {
            if (!isPlaying && recordedFrames.ContainsKey(machine_number) && recordedFrames[machine_number].Count > 0)
            {
                StartPlayback(machine_number);
            }
        }
    }

    // 録画開始
    private void StartRecording(int machine_number)
    {
        isRecording = true;
        if (!recordedFrames.ContainsKey(machine_number))
        {
            recordedFrames[machine_number] = new List<Texture2D>(); // 新しい機体のリストを作成
        }
        recordedFrames[machine_number].Clear(); // 古いフレームをクリア
        Debug.Log($"Recording started for machine {machine_number}.");
    }

    // 録画停止
    private void StopRecording(int machine_number)
    {
        isRecording = false;
        Debug.Log($"Recording stopped for machine {machine_number}. Frames recorded: {recordedFrames[machine_number].Count}");
    }

    // フレームをキャプチャ
    private void CaptureFrame(int machine_number)
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        recordingCamera.targetTexture = rt;
        Texture2D frame = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        recordingCamera.Render();
        RenderTexture.active = rt;
        frame.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        frame.Apply();

        // 機体ごとのフレームリストに追加
        if (recordedFrames.ContainsKey(machine_number))
        {
            recordedFrames[machine_number].Add(frame);
        }

        recordingCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
    }

    // 再生開始
    private void StartPlayback(int machine_number)
    {
        isPlaying = true;
        currentPlaybackFrame = 0;
        playbackScreen.gameObject.SetActive(true); // 再生画面を表示
        Debug.Log($"Playback started for machine {machine_number}.");
    }

    // フレーム再生
    private void PlayFrame(int machine_number)
    {
        if (currentPlaybackFrame < recordedFrames[machine_number].Count)
        {
            playbackScreen.texture = recordedFrames[machine_number][currentPlaybackFrame];
            currentPlaybackFrame++;
        }
        else
        {
            isPlaying = false; // 再生終了
            playbackScreen.gameObject.SetActive(false); // 再生画面を非表示にする
            Debug.Log("Playback finished.");
        }
    }

    // 親オブジェクトに枠線を追加
    private void AddBorderToPlaybackScreen()
    {
        // 親オブジェクトにImageコンポーネントを追加して枠線を設定
        Image image = playbackScreenParent.AddComponent<Image>();
        image.color = borderColor;

        // 親オブジェクトのサイズをRawImageに合わせて調整
        RectTransform rectTransform = playbackScreenParent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(playbackScreen.rectTransform.rect.width + borderWidth * 2, playbackScreen.rectTransform.rect.height + borderWidth * 2);
    }
}