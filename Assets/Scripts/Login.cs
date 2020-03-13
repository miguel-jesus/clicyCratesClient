using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
  

    [SerializeField]
    Player player;
    public InputField inputUser;
    public InputField inputPassword;


    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        player = FindObjectOfType<Player>();
    }

    public void LoginBtn()
    {
        StartCoroutine(TryLogin());
    }


    private IEnumerator GetToken()
    {
        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", inputUser.text);
        data.AddField("password", inputPassword.text);

        using (UnityWebRequest httpClient = UnityWebRequest.Post(player.HttpServerAddress + "/Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }

    
    private IEnumerator GetInfoPlayer()
    {

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/GetPlayer/" + player.Id, "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                PlJson playerJson = JsonUtility.FromJson<PlJson>(jsonResponse);

                Debug.Log("GetInfoPlayer > Info: " + playerJson.NickName);
                player.FirstName = playerJson.FirstName;
                player.LastName = playerJson.LastName;
                player.DateOfBirth = playerJson.DateOfBirth;
                player.NickName = playerJson.NickName;
                player.Email = playerJson.Email;
                player.City = playerJson.City;
                player.DateJoined = playerJson.DateJoined;
                player.BlobUri = playerJson.BlobUri;


            }

        }

    }

  
    private IEnumerator TryLogin()
    {

        if (string.IsNullOrEmpty(player.Token))
        {
            yield return GetToken();
        }

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/UserId"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TryLogin > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
                Debug.Log("TryLogin > Info: Estas logeado");
            }

        }

        yield return GetInfoPlayer();
        SceneManager.LoadScene("Welcome");
       

    }




    public void goRegister()
    {
        SceneManager.LoadScene("Register");
    }
}
