using Mediapipe;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class VideoDataConverter : MonoBehaviour
{
    private static Dictionary<int, int> jointInfoConverter = new Dictionary<int, int>()
    {
        { 7, 0 }, // Left Ear
        { 2, 1 }, // Left Eye
        { 8, 2 }, // Right Ear
        { 5, 3 }, // Right Eye
        { 0, 4 }, // Nose

        //{ , 5 }, // Neck(NONE)
        { 11, 6 }, // Left Shoulder
        { 13, 7 }, // Left Elbow
        { 15, 8 }, // Left Wrist
        //{ , 9 }, // Left Palm(NONE)

        { 12, 10 }, // Right Shoulder
        { 14, 11 }, // Right Elbow
        { 16, 12 }, // Right Wrist
        //{ , 13 }, // Right Palm(NONE)
        //{ , 14 }, // Back(NONE)

        //{ , 15 }, // Waist(NONE)
        { 23, 16 }, // Left Hip
        { 25, 17 }, // Left Knee
        { 27, 18 }, // Left Ankle
        { 31, 19 }, // Left Foot

        { 24, 20 }, // Right Hip
        { 26, 21 }, // Right Knee
        { 28, 22 }, // Right Ankle
        { 32, 23 }  // Right Foot
    };
    public static VideoDataConverter Instance { get { return instance; } }

    private static VideoDataConverter instance;

    public readonly PoseLandmarkDetectionConfig config = new PoseLandmarkDetectionConfig();
    private PoseLandmarkerOptions options;
    private ImageProcessingOptions imageProcessingOptions;
    private PoseLandmarkerResult result;
    private PoseLandmarker taskApi;


    private List<byte[]> outputImages = new List<byte[]>();
    private List<Vector2[]> outputJoints = new List<Vector2[]>();

    public List<byte[]> RecordingImages { get { return recordImages; } }
    private List<byte[]> recordImages = new List<byte[]>();
    private int width, height;

    public int SelectedExercise { get { return selectedExercise; } }
    private int selectedExercise;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(Init());
    }

    public IEnumerator Init()
    {
        AssetLoader.Provide(new StreamingAssetsResourceManager());
        yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

        config.RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE;
        options = config.GetPoseLandmarkerOptions(null);
        imageProcessingOptions = new ImageProcessingOptions(rotationDegrees: 0);
        taskApi = PoseLandmarker.CreateFromOptions(options);
        result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);
    }

    public void SelectExercise(int exerIndex)
    {
        selectedExercise = exerIndex;
    }

    public void ClearVideo()
    {
        outputImages.Clear();
        outputJoints.Clear();
        recordImages.Clear();
        Resources.UnloadUnusedAssets();
    }

    public void AddFrame(Texture2D frame)
    {
        width = frame.width;
        height = frame.height;
        // recordImages.Add(frame.EncodeToJPG(75));

        var imageForDetect = new Image(ImageFormat.Types.Format.Srgb, frame);
        if (taskApi.TryDetect(imageForDetect, imageProcessingOptions, ref result))
        {
            for (int i = 0; i < result.poseLandmarks[0].landmarks.Count; i++)
            {
                if (jointInfoConverter.ContainsKey(i))
                {
                    var mark = result.poseLandmarks[0].landmarks[i];
                    var point = new Vector2(mark.x, mark.y);

                    int jointIndex = jointInfoConverter[i];
                    //joints[jointIndex] = point;
                    UIController.Instance.AddJoint(point, jointIndex);
                }
            }
            // 처리가 되지 않은 부분들에 대해 추가 작업 진행.
            // 목, 왼손, 오른손, 등, 허리
        }
    }

    public void CreateVideo(List<int> usingFrames)
    {
        Texture2D frame = new Texture2D(2, 2);
        foreach (int index in usingFrames)
        {
            if (frame.LoadImage(recordImages[index]))
            {
                Vector2[] joints = new Vector2[24];
                var imageForDetect = new Image(ImageFormat.Types.Format.Srgb, frame);
                if (taskApi.TryDetect(imageForDetect, imageProcessingOptions, ref result))
                {
                    for (int i = 0; i < result.poseLandmarks[0].landmarks.Count; i++)
                    {
                        if (jointInfoConverter.ContainsKey(i))
                        {
                            var mark = result.poseLandmarks[0].landmarks[i];
                            var point = new Vector2(mark.x, mark.y);

                            int jointIndex = jointInfoConverter[i];
                            joints[jointIndex] = point;
                        }
                    }
                    // 처리가 되지 않은 부분들에 대해 추가 작업 진행.
                    // 목, 왼손, 오른손, 등, 허리
                }
                else
                {
                }
                outputImages.Add(recordImages[index]);
                outputJoints.Add(joints);
            }
        }
    }

    public ExerciseDataPacket CreatePacket()
    {
        if (recordImages.Count == 0) return null;

        // 우선 운동 타입은 임의로 세팅.
        ExerciseDataPacket packet = new ExerciseDataPacket(selectedExercise, outputJoints, outputImages, width, height);
        return packet;
    }
}
