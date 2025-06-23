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

    private List<Texture2D> recordedFrames = new List<Texture2D>(); // 保存するフレームのリスト
    private bool isRecording = false; // 録画中フラグ
    private bool isPlaying = false; // 再生中フラグ
    private float captureInterval; // フレームキャプチャ間隔
    private float timeSinceLastCapture = 0f; // 最後にキャプチャしたフレームからの経過時間
    private int currentPlaybackFrame = 0; // 現在の再生フレーム
    private float timeSinceLastFrame = 0f; // 前の再生フレームからの経過時間

    public float machine_number = 1f;    
    public GameObject playbackScreenParent; // RawImageを持つ親オブジェクト
    public Color borderColor = Color.black; // 枠線の色
    public float borderWidth = 5f; // 枠線の太さ


    public void Delay(float delay)
    {
        StartCoroutine(DelayedStartRecording(delay));
    }

    private IEnumerator DelayedStartRecording(float delay)
    {
        yield return new WaitForSeconds(delay); // 指定時間待機
        StopRecording(); // 待機後に録画停止
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
                CaptureFrame();
                timeSinceLastCapture = 0f; // 経過時間をリセット
            }
        }

        // 再生処理
        if (isPlaying)
        {
            timeSinceLastFrame += Time.deltaTime;
            if (timeSinceLastFrame >= captureInterval)
            {
                PlayFrame();
                timeSinceLastFrame = 0f;
            }
        }

        if (stopper.capturemode[(int)machine_number] == 1 && !isRecording) // 録画開始
        {
            StartRecording();
        }

        if (stopper.movemode[(int)machine_number] == 4) // 録画停止
        {
            Delay(5.0f); // 15秒待って録画を停止
        }

        if (Input.GetKeyDown(KeyCode.P)) // Pキーで再生開始
        {
            if (!isPlaying && recordedFrames.Count > 0)
            {
                StartPlayback();
            }
        }
    }

    // 録画開始
    private void StartRecording()
    {
        isRecording = true;
        recordedFrames.Clear(); // 古いフレームをクリア
        Debug.Log("Recording started.");
    }

    // 録画停止
    private void StopRecording()
    {
        isRecording = false;
        Debug.Log($"Recording stopped. Frames recorded: {recordedFrames.Count}");
    }

    // フレームをキャプチャ
    private void CaptureFrame()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        recordingCamera.targetTexture = rt;
        Texture2D frame = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        recordingCamera.Render();
        RenderTexture.active = rt;
        frame.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        frame.Apply();

        recordedFrames.Add(frame);

        recordingCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
    }

    // 再生開始
    private void StartPlayback()
    {
        isPlaying = true;
        currentPlaybackFrame = 0;
        playbackScreen.gameObject.SetActive(true); // 再生画面を表示
        Debug.Log("Playback started.");
    }

    // フレーム再生
    private void PlayFrame()
    {
        if (currentPlaybackFrame < recordedFrames.Count)
        {
        playbackScreen.texture = recordedFrames[currentPlaybackFrame];
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