using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerManager : MonoBehaviour
{
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

    private async void Start()
    {/*
        await SignIn("student1234", "potato1234!");
        Debug.Log(token);
        {
            var task = await CheckIDDuplication("student1234");
            Debug.Log(task);
        }

        {
            var str = "";
            var task = await GetOrganizations();
            foreach (var item in task.orgList)
            {
                str += $"{item.id}, {item.name} \n";
            }
            Debug.Log(str);
        }

        {
            var task = await GetUserInfo();
            Debug.Log($"{task.id}, {task.name}, {task.role}, {task.userName}, {task.orgId}, {task.expireTime}");
        }

        {
            var task = await GetMeasureHistory();
            Debug.Log($"{task.historyList.Length}");
        }

        {
            var str = "";
            var task = await GetMeasureItems();
            foreach (var item in task.items)
            {
                str += $"{item.id}, {item.name}, {item.number}, {item.type}, {item.unit} \n";
            }
            Debug.Log(str);
        }

        {
            var task = await GetMeasureValues();
        }
        */
    }

    private string serverUrl = "";
    private string token = "";

    #region Auth
    #region struct
    [System.Serializable]
    public class SignUpInfo
    {
        public string userName;
        public string password;
        public string name;
        public string gender;
        public string birth;
        public int organizationId;
    }

    [System.Serializable]
    public struct TokenResponse
    {
        public string accessToken;
    }

    [System.Serializable]
    public class DuplicateID
    {
        public bool isDuplicate;
    }

    [System.Serializable]

    public class OrganizationList
    {
        public Organization[] orgList;
    }

    [System.Serializable]
    public struct Organization
    {
        public int id;
        public string name;
    }

    public async Task<bool> SignUp(SignUpInfo info, string type = "student")
    {
        // organization, teacher, student
        // 로그인 한 번에 여러번 못하도록 해야함.
        
        string jsonData = JsonUtility.ToJson(info);
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "v1.0/auth/sign-up/" + type, "POST"))
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
                return true;
            }
            else
            {
                // 실패
                return false;
            }
        }
    }

    public async Task<bool> CheckIDDuplication(string userName)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + $"v1.0/auth/sign-up/duplication?userName={userName}"))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<DuplicateID>(request.downloadHandler.text).isDuplicate;
            }
            else
            {
                return true;
            }
        }
    }

    public async Task<OrganizationList> GetOrganizations()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "v1.0/auth/sign-up/organizations"))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<OrganizationList>(request.downloadHandler.text);
            }
            else
            {
                return null;
            }
        }
    }
    [System.Serializable]
    public struct LoginInfo
    {
        public string userName;
        public string password;
    }

    [System.Serializable]
    public class UserInfo
    {
        public int id;
        public string userName;
        public string name;
        public string role;
        public int orgId;
        public ulong expireTime;
    }
    #endregion
    public async Task<bool> SignIn(string user, string pw)
    {
        // 로그인 한 번에 여러번 못하도록 해야함.
        string jsonData = JsonUtility.ToJson(new LoginInfo { userName = user, password = pw });
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "v1.0/auth/sign-in", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            //request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 성공
                token = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text).accessToken;
                return true;
            }
            else
            {
                // 실패
                return false;
            }
        }
    }

    public void SignOut()
    {
        token = "";
    }

    public async Task<UserInfo> GetUserInfo()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "v1.0/member/info"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<UserInfo>(request.downloadHandler.text);
            }
            else
            {
                // 실패
                return null;
            }
        }
    }
    #endregion

    #region Measure
    #region struct
    [System.Serializable]
    public class MeasureHistory
    {
        public MeasureLog[] historyList;
    }

    [System.Serializable]
    public struct MeasureLog
    {
        public int id;
        public string date;
    }

    [System.Serializable]
    public class MeasureItems
    {
        public MeasureItem[] items;
    }


    [System.Serializable]
    public struct MeasureItem
    {
        public int id;
        public string type;
        public string name;
        public int number;
        public string unit;
    }

    [System.Serializable]
    public class MeasureResult
    {
        public Measurement[] measurements;
    }

    [System.Serializable]
    public struct Measurement
    {
        public string name;
        public string status;
        public int value;
        public string unit;
    }
    #endregion
    public async Task<MeasureHistory> GetMeasureHistory()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "v1.0/ai-measurement/history"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<MeasureHistory>(request.downloadHandler.text);
            }
            else
            {
                // 실패
                return null;
            }
        }
    }
    public async Task<MeasureItems> GetMeasureItems()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "v1.0/ai-measurement/items"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 성공
                return JsonUtility.FromJson<MeasureItems>(request.downloadHandler.text);

            }
            else
            {
                // 실패
                return null;
            }
        }
    }
    public async Task<MeasureResult> GetMeasureValues(int id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + $"v1.0/ai-measurement/{id}"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 성공
                return JsonUtility.FromJson<MeasureResult>(request.downloadHandler.text);
            }
            else
            {
                // 실패
                return null;
            }
        }
    }

    public async Task<bool> MeasureSubmit(MeasureResult result)
    {
        // 로그인 한 번에 여러번 못하도록 해야함.
        string jsonData = JsonUtility.ToJson(result);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "v1.0/ai-measurement/submit", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("Authorization", "Bearer " + token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success");
                // 성공
                return true;
            }
            else
            {
                Debug.Log("Fail");
                // 실패
                return false;
            }
        }
    }
    /*
    private void OnGUI()
    {
        if (token != "" && GUI.Button(new Rect(0, 0, 200, 200), "TEST"))
        {
            MeasureSubmit(true);
        }

        if (token != "" && GUI.Button(new Rect(0, 200, 200, 200), "TEST2"))
        {
            MeasureSubmit(false);
        }
    }*/
    #endregion
}
