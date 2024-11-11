using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerManager : MonoBehaviour
{
    public struct LoginInfo
    {
        public string username;
        public string password;
    }

    public static WebServerManager Instance { get { return instance; } }
    private static WebServerManager instance;

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

    public string serverUrl = "https://localhost:7074/login";

    public void SignUp()
    {

    }

    public async void SignIn(string user, string pw)
    {
        // 로그인 한 번에 여러번 못하도록 해야함.
        string jsonData = JsonUtility.ToJson(new LoginInfo { username = user, password = pw });

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 성공
                string token = request.downloadHandler.text;
                Debug.Log($"Complete {token}");
            }
            else
            {
                // 실패
                Debug.LogWarning("Error: " + request.error);
            }
        }
    }

    public void SignOut()
    {

    }
}
