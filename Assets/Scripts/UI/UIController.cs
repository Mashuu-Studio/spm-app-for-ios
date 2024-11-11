using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get { return instance; } }
    private static UIController instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        // EndLoading();
        CloseAlert();
        ChangeScene(SceneName.START);

#if UNITY_EDITOR
#elif UNITY_ANDROID
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            telephonyManager = activity.Call<AndroidJavaObject>("getSystemService", "phone");
            wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
        }
#elif UNITY_IOS
#endif
    }

    [Header("Scene")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private List<GameObject> scenes;
    private int currentScene = 0;
    public enum SceneName { START = 0, LOGIN, SIGN_UP, HOME, LOG, MEASURE, RESULT }

    public void ChangeScene(SceneName scene)
    {
        ChangeScene((int)scene);
    }

    private void ChangeScene(int index)
    {
        for (int i = 0; i < scenes.Count; i++)
        {
            scenes[i].SetActive(i == index);
        }
        CloseAlert();
        currentScene = index;
    }

    [Space]
    [SerializeField] private AlertUI alertUI;

    public void CloseAlert()
    {
        alertUI.gameObject.SetActive(false);
        findOrganization.SetActive(false);
        successSignup.SetActive(false);
        continueMeasure.SetActive(false);
        recordingPopup.SetActive(false);
    }

    public void Alert(string title, string message)
    {
        alertUI.Alert(title, message);
    }

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image lteImage;
    [SerializeField] private Sprite[] lteStrengthSprites;
    [SerializeField] private Image wifiImage;
    [SerializeField] private Sprite[] wifiStrengthSprites;
    [SerializeField] private Slider batterySlider;
#if UNITY_ANDROID
    private AndroidJavaObject telephonyManager;
    private AndroidJavaObject wifiManager;
    private int currentLteState;
    private int currentWifiState;
#endif

    private void Update()
    {
        timeText.text = $"<font-weight=\"700\">{DateTime.Now.ToString("HH:mm")}";
#if UNITY_EDITOR
#elif UNITY_ANDROID || UNITY_IOS
        batterySlider.value = SystemInfo.batteryLevel;
#if UNITY_ANDROID
        AndroidJavaObject cellInfoList = telephonyManager.Call<AndroidJavaObject>("getAllCellInfo");

        if (cellInfoList != null && cellInfoList.Call<int>("size") > 0)
        {
            // 첫 번째 기지국 정보 가져오기
            AndroidJavaObject cellInfo = cellInfoList.Call<AndroidJavaObject>("get", 0);
            var cellSignalStrength = cellInfo.Call<AndroidJavaObject>("getCellSignalStrength");
            int lteRsrp = cellSignalStrength.Call<int>("getRsrp");
            if (lteRsrp >= -85 && currentLteState != 0)
            {
                currentLteState = 0;
                lteImage.sprite = lteStrengthSprites[currentLteState];
            }
            else if (lteRsrp < -85 && lteRsrp >= -100 && currentLteState != 1)
            {
                currentLteState = 1;
                lteImage.sprite = lteStrengthSprites[currentLteState];
            }
            else if (lteRsrp < -100 && lteRsrp >= -110 && currentLteState != 2)
            {
                currentLteState = 2;
                lteImage.sprite = lteStrengthSprites[currentLteState];
            }
            else if (lteRsrp < -110 && currentLteState != 3)
            {
                currentLteState = 3;
                lteImage.sprite = lteStrengthSprites[currentLteState];
            }
        }
        
        // Wi-Fi 연결 정보 가져오기
        var wifiInfo = wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
        int wifisignalLevel = wifiInfo.Call<int>("getRssi");
        if (wifisignalLevel >= -50 && currentWifiState != 0)
        {
            currentWifiState = 0;
            wifiImage.sprite = wifiStrengthSprites[currentWifiState];
        }
        else if (wifisignalLevel < -50 && wifisignalLevel >= -70 && currentWifiState != 1)
        {
            currentWifiState = 1;
            wifiImage.sprite = wifiStrengthSprites[currentWifiState];
        }
        else if (wifisignalLevel < -70 && currentWifiState != 2)
        {
            currentWifiState = 2;
            wifiImage.sprite = wifiStrengthSprites[currentWifiState];
        }
#elif UNITY_IOS
#endif
#endif
    }

    #region Loading
    [SerializeField] private LoadingUI loadingUI;

    public void StartLoading(int totalProgress)
    {
        loadingUI.SetActive(true, totalProgress);
    }

    public void EndLoading()
    {
        loadingUI.SetActive(false);
    }

    public void Loading(int progress)
    {
        loadingUI.Loading(progress);
    }
    #endregion

    #region Login
    [Header("Login")]
    [SerializeField] private LoginUI loginUI;

    public void GoToLogin()
    {
        ChangeScene(SceneName.LOGIN);
    }

    public void GoToSignUp()
    {
        ChangeScene(SceneName.SIGN_UP);
    }

    public void GoToHome()
    {
        ChangeScene(SceneName.HOME);
    }

    #region SignUp
    [Space]
    [SerializeField] private GameObject findOrganization;
    [SerializeField] private GameObject successSignup;
    public async void SignUp()
    {
        // 결과에 따라서 Callback
        if (true)
        {
            successSignup.SetActive(true);
            //Alert("Sign Up Complete");
        }/*
        else
        {
            Alert("회원가입", "회원가입에 실패하였습니다.\n관리자에게 문의하세요.");
        }*/
    }

    public void FindOrganization()
    {
        findOrganization.SetActive(true);
    }

    public void CheckID()
    {
        if (true)
        {
            Alert("중복 확인", "사용할 수 있는 아이디 입니다");
        }
        else
        {
            Alert("중복 확인", "이미 사용중인 아이디 입니다");
        }
    }
    #endregion
    public async void SignIn()
    {
        Alert("로그인", "존재하지 않는 계정이거나\n아이디 혹은 비밀번호가 일치하지 않습니다");
        //WebServerManager.Instance.SignIn(loginUI.Email, loginUI.Password);
        // 결과에 따라서 Callback
        if (true)
        {
            GoToHome();
            //Alert("Login Complete");
        }/*
        else
        {
            Alert("로그인", "존재하지 않는 계정이거나\n아이디 혹은 비밀번호가 일치하지 않습니다");
        }*/
    }

    public void SignOut()
    {
        //FirebaseAuthManager.Instance.SignOut();
        ChangeScene(SceneName.LOGIN);
    }

    public void Connect()
    {
        //SocketServerManager.Instance.Connect(ipInputField.text);
    }
    #endregion

    #region Log
    [Header("Log")]

    [SerializeField] private GameObject continueMeasure;
    public void GoToLog()
    {
        ChangeScene(SceneName.LOG);
    }
    #endregion

    #region Video
    [Header("Cam")]
    [SerializeField] private RawImage camRawImage;
    [SerializeField] private List<Image> joints;
    [Space]
    [SerializeField] private List<Image> progressIcons;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject measureExampleTextZone;
    [SerializeField] private GameObject measureTimerTextZone;
    [SerializeField] private TextMeshProUGUI waitRecordingTimerText;
    [Space]
    [SerializeField] private GameObject recordingPopup;
    [SerializeField] private Button continueMeasureButton;
    [SerializeField] private TextMeshProUGUI continueMeasureButtonText;
    private IEnumerator restMeasureCoroutine;
    private int progress = 0;
    private Color progressActiveColor = new Color(54 / 255f, 122 / 255f, 1);
    private Color progressDeactiveColor = new Color(0, 0, 0, 120 / 255f);

    public void GoToMeasure()
    {
        ChangeScene(SceneName.MEASURE);
        progress = 0;
        SetProgress();
        CamRecorder.Instance.ReadyToCamera();
        StartRecord();
    }

    public void SetCam(Texture texture, float camRatio)
    {
        camRawImage.texture = texture;
    }

    public void StartRecord()
    {
        CamRecorder.Instance.StartRecord();
    }

    public void ShowRecordText(bool b)
    {
        measureExampleTextZone.SetActive(!b);
        measureTimerTextZone.SetActive(b);
    }

    public void WaitRecordingTimer(int time)
    {
        if (time == 0) waitRecordingTimerText.text = "";
        else waitRecordingTimerText.text = time.ToString();
    }

    public void SetTimer(int time)
    {
        timerText.text = $"<font-weight=\"700\">{time / 60}:{(time % 60).ToString("D2")}";
    }

    public void NextRecord()
    {
        ClearJoint();
        if (++progress < progressIcons.Count)
        {
            SetProgress();
            recordingPopup.SetActive(true);
            if (restMeasureCoroutine != null)
            {
                StopCoroutine(restMeasureCoroutine);
                restMeasureCoroutine = null;
            }
            restMeasureCoroutine = RestMeasure();
            StartCoroutine(restMeasureCoroutine);
        }
        else
        {
            StopRecord();
            GoToResult();
        }
    }

    private IEnumerator RestMeasure()
    {
        float time = CamRecorder.WAITING_TIME;
        continueMeasureButton.interactable = false;
        while (time > 0)
        {
            continueMeasureButtonText.text = $"<font-weight=\"600\">{Mathf.CeilToInt(time)}";
            yield return null;
            time -= Time.deltaTime;
        }
        continueMeasureButton.interactable = true;
        continueMeasureButtonText.text = $"<font-weight=\"600\">계속 측정";
    }

    private void SetProgress()
    {
        for (int i = 0; i < progressIcons.Count; i++)
        {
            progressIcons[i].color = i <= progress ? progressActiveColor : progressDeactiveColor;
        }
        ShowRecordText(false);
    }

    public void StopRecord()
    {
        CamRecorder.Instance.StopRecord();
    }

    public void ClearJoint()
    {
        foreach (var joint in joints)
        {
            joint.gameObject.SetActive(false);
        }
    }

    public void AddJoint(Vector2 pos, int index)
    {
        var joint = joints[index];
        joint.rectTransform.anchoredPosition = new Vector2(pos.x * camRawImage.rectTransform.sizeDelta.x, pos.y * camRawImage.rectTransform.sizeDelta.y);
        joint.gameObject.SetActive(true);
    }

    public void TemporarySave()
    {
        StopRecord();
        GoToLog();
    }

    #endregion

    #region Result
    public void GoToResult()
    {
        ChangeScene(SceneName.RESULT);
    }
    #endregion
}
