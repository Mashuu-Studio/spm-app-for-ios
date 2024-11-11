using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;

// �̴�� ��� or async�� ���� �� static���� ���.
public class CamRecorder : MonoBehaviour
{
    public static CamRecorder Instance { get { return instance; } }
    private static CamRecorder instance;
    private const int TARGET_FPS = 24;
    private const int EXERCISE_TIME = 10;
    public const int WAITING_TIME = 5;

    public int RemainTime { get { return Mathf.CeilToInt(remainTime); } }
    private float remainTime;

    public int WaitingTime { get { return Mathf.CeilToInt(waitingTime); } }
    private float waitingTime;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private WebCamTexture camTexture;
    private float camRatio;
    private IEnumerator recordingCoroutine;
    private void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            PermissionCallbacks camPermissionCallbacks = new PermissionCallbacks();
            camPermissionCallbacks.PermissionGranted += CheckCam;
            // ī�޶� ���� ��û
            Permission.RequestUserPermission(Permission.Camera, camPermissionCallbacks);
        }
    }

    private void CheckCam(string str = "")
    {
        // �켱�� permission�� ���޾ƿ��� ����
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) Application.Quit();
    }

    private void SetCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        // ��밡���� ����̽��� ���� ���
        if (devices.Length > 0)
        {
#if UNITY_EDITOR
            camTexture = new WebCamTexture(devices[0].name);
#elif UNITY_ANDROID || UNITY_IOS
            var temp = WebCamTexture.devices.Last();
            Vector2Int size = Vector2Int.zero;
            int max = 0;
            foreach (Resolution res in temp.availableResolutions)
            {
                int val = res.width * res.height;
                if (val > max && res.width * 3 == res.height * 4)
                {
                    max = val;
                    size = new Vector2Int(res.width, res.height);
                    camRatio = (float)res.height / res.width;
                }
            }
            camTexture = new WebCamTexture(WebCamTexture.devices.Last().name, size.x, size.y);
#endif
        }
    }

    public void ReadyToCamera()
    {
        SetCamera();
        UIController.Instance.SetCam(camTexture, camRatio);
        camTexture.Play();
    }

    public void StartRecord()
    {
        VideoDataConverter.Instance.ClearVideo();

        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }
        recordingCoroutine = Recording();
        StartCoroutine(recordingCoroutine);
    }

    public void StopRecord()
    {
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }
        camTexture.Stop();
        //UIController.Instance.SetVideoType(VideoEditor.VideoType.RECORD);
    }

    private IEnumerator Recording()
    {
        waitingTime = WAITING_TIME;
        while (waitingTime > 0)
        {
            waitingTime -= Time.deltaTime;
            yield return null;
            UIController.Instance.WaitRecordingTimer(WaitingTime);
        }

        UIController.Instance.ShowRecordText(true);

        Texture2D frame = new Texture2D(camTexture.width, camTexture.height, TextureFormat.RGB24, false);
        RenderTexture renderTexture = new RenderTexture(frame.width, frame.height, 0);
        Graphics.Blit(camTexture, renderTexture);

        remainTime = EXERCISE_TIME;
        while (remainTime > 0)
        {
            if (camTexture.didUpdateThisFrame)
            {
                RenderTexture.active = renderTexture;
                // ReadPixels�� ����Ͽ� RenderTexture���� �ȼ� �����͸� ��������
                frame.ReadPixels(new Rect(0, 0, frame.width, frame.height), 0, 0);
                frame.Apply();
                RenderTexture.active = null;
                //VideoDataConverter.Instance.AddFrame(frame);
            }
            yield return null;
            remainTime -= Time.deltaTime;
            UIController.Instance.SetTimer(RemainTime);
        }

        UIController.Instance.ClearJoint();
        UIController.Instance.NextRecord();
    }
    private void OnDisable()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }

    private void OnDestroy()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }
}
