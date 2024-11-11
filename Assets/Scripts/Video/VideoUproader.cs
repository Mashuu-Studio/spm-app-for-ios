using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoUproader : MonoBehaviour
{
    public static VideoUproader Instance { get { return instance; } }
    private static VideoUproader instance;

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

    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    public Texture2D FrameTexture { get { return frameTexture; } }
    private Texture2D frameTexture;

    private IEnumerator prepareVideoCoroutine;
    public int FrameAmount { get { return (int)videoPlayer.frameCount; } }

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.skipOnDrop = true;

        videoPlayer.Pause();
    }

    public void UproadVideo()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            PermissionCallbacks PermissionCallbacks = new PermissionCallbacks();
            PermissionCallbacks.PermissionGranted += SelectVideo;
            // sd카드 권한 요청
            Permission.RequestUserPermission(Permission.ExternalStorageRead, PermissionCallbacks);
        }
        else SelectVideo();

    }

    public void SelectVideo(string obj = "")
    {
        // 기존 비디오 정보 삭제
        videoPlayer.url = "";
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile(path =>
        {
            // File Not Selected
            if (path == null)
            {
                return;
            }
            videoPlayer.enabled = true;
            videoPlayer.url = path;
            PrepareVideo();
        }, new string[] { "video/*" });
    }

    private void PrepareVideo()
    {
        VideoDataConverter.Instance.ClearVideo();
        videoPlayer.Prepare();

        if (prepareVideoCoroutine != null)
        {
            StopCoroutine(prepareVideoCoroutine);
            prepareVideoCoroutine = null;
        }
        prepareVideoCoroutine = ProgressPreparingVideo();
        StartCoroutine(prepareVideoCoroutine);
    }

    private IEnumerator ProgressPreparingVideo()
    {
        UIController.Instance.StartLoading((int)videoPlayer.frameCount);
        while (!videoPlayer.isPrepared)
        {
            UIController.Instance.Loading((int)videoPlayer.frame);
            yield return null;
        }

        renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
        frameTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        videoPlayer.targetTexture = renderTexture;

        videoPlayer.Play();

        UIController.Instance.EndLoading();
    }

    public void SetFrame(int frameIndex)
    {
        StartCoroutine(ExtractFrame(frameIndex));
    }

    public IEnumerator ExtractFrame(int frameIndex)
    {
        UIController.Instance.StartLoading(1);
        videoPlayer.frame = frameIndex;
        yield return new WaitUntil(() => videoPlayer.frame == frameIndex);

        RenderTexture.active = renderTexture;

        frameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        frameTexture.Apply();

        RenderTexture.active = null;
        UIController.Instance.EndLoading();
    }

    public bool Usable { get { return usable; } }
    private bool usable = false;
    public async void SetVideo(List<int> usingIndexes)
    {
        usable = false;
        UIController.Instance.StartLoading(usingIndexes.Count);
        for (int i = 0; i < usingIndexes.Count; i++)
        {
            int index = usingIndexes[i];
            videoPlayer.frame = index;
            while (videoPlayer.frame != index) await Task.Yield();

            RenderTexture.active = renderTexture;

            frameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            frameTexture.Apply();

            RenderTexture.active = null;

            VideoDataConverter.Instance.AddFrame(frameTexture);
            UIController.Instance.Loading(i + 1);
        }
        UIController.Instance.EndLoading();
        usable = true;
    }

}
