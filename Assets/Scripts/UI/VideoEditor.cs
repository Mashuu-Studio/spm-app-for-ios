using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoEditor : MonoBehaviour
{
    public enum VideoType { RECORD = 0, FILE }

    [SerializeField] private RawImage viewImage;
    [SerializeField] private Slider frameSlider;
    [SerializeField] private TMP_InputField frameInputField;
    [SerializeField] private TextMeshProUGUI maxFrameText;
    [SerializeField] private TMP_InputField cycleInputField;
    private VideoType videoType;
    private int startFrameIndex, endFrameIndex;
    private int cycleAmount = 1;

    private Texture2D frameTexture;

    private const int TARGET_CYCLE_FRAME_AMOUNT = 8;

    private void Start()
    {
        frameTexture = new Texture2D(2, 2);
        frameSlider.onValueChanged.AddListener(value => SetFrame(value));
        frameInputField.onEndEdit.AddListener(value => SetFrame(value));
        cycleInputField.onValueChanged.AddListener(value => SetCycle(value));
        cycleInputField.text = "1";
    }

    public void SetVideoType(VideoType videoType)
    {
        this.videoType = videoType;
    }

    public void EditVideo()
    {
        frameSlider.minValue = startFrameIndex = 0;
        frameSlider.value = 0;
        switch (videoType)
        {
            case VideoType.RECORD:
                frameSlider.maxValue = endFrameIndex = VideoDataConverter.Instance.RecordingImages.Count;
                maxFrameText.text = endFrameIndex.ToString();
                viewImage.texture = frameTexture;
                break;

            case VideoType.FILE:
                viewImage.texture = VideoUproader.Instance.FrameTexture;
                frameSlider.maxValue = endFrameIndex = VideoUproader.Instance.FrameAmount;
                maxFrameText.text = VideoUproader.Instance.FrameAmount.ToString();
                break;
        }
        frameSlider.value = 1;
    }

    public void SetStart()
    {
        startFrameIndex = (int)frameSlider.value;
    }

    public void SetEnd()
    {
        endFrameIndex = (int)frameSlider.value;
    }

    private void SetCycle(string value)
    {
        if (int.TryParse(value, out cycleAmount))
        {
            if (cycleAmount < 1)
            {
                cycleAmount = 1;
                cycleInputField.text = "1";
            }
        }
    }

    private void SetFrame(string value)
    {
        int frameIndex;

        if (int.TryParse(value, out frameIndex))
        {
            if (frameIndex < 1) frameIndex = 1;
            if (frameIndex > frameSlider.maxValue) frameIndex = (int)frameSlider.maxValue;
            frameSlider.value = frameIndex;
        }
    }

    private void SetFrame(float value)
    {
        if (value == 0) return;
        SetFrame((int)value);
    }

    private void SetFrame(int value)
    {
        frameInputField.text = value.ToString();
        value -= 1;
        switch (videoType)
        {
            case VideoType.RECORD:
                frameTexture.LoadImage(VideoDataConverter.Instance.RecordingImages[value]);
                viewImage.texture = frameTexture;
                break;

            case VideoType.FILE:
                VideoUproader.Instance.SetFrame(value);
                break;
        }
    }

    public void CreateVideo()
    {
        StartCoroutine(CreatingVideo());
    }

    private IEnumerator CreatingVideo()
    {
        List<int> usingIndexes = new List<int>();
        int targetAmount = TARGET_CYCLE_FRAME_AMOUNT * cycleAmount;
        int interval = (endFrameIndex - startFrameIndex) / targetAmount;
        for (int index = startFrameIndex; index < endFrameIndex; index += interval)
        {
            usingIndexes.Add(index);
        }

        while (usingIndexes.Count % TARGET_CYCLE_FRAME_AMOUNT != 0) usingIndexes.RemoveAt(usingIndexes.Count - 1);
        // 만약에 갯수가 맞지 않을 경우 배수를 맞춰줌.

        if (videoType == VideoType.FILE)
        {
            // 비디오 프레임을 새롭게 세팅해줌.
            VideoUproader.Instance.SetVideo(usingIndexes);
            while (VideoUproader.Instance.Usable == false) yield return null;
            // 필요한 부분만 저장해두었기 때문에 index 자체는 전부 사용하는 index임.
            int count = usingIndexes.Count;
            usingIndexes.Clear();
            for (int i = 0; i < count; i++)
            {
                usingIndexes.Add(i);
            }
        }
        yield return null;
        VideoDataConverter.Instance.CreateVideo(usingIndexes);
    }
}
