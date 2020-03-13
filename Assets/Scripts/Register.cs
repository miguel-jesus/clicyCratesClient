using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    [SerializeField]
    Player player;
    public InputField inputEmail;
    public InputField inputPassword;
    public InputField inputCPassword;
    public InputField inputFirstName;
    public InputField inputLastName;
    public InputField inputBirthDate;
    public InputField inputNickName;
    public InputField inputCity;

    private Image pruebaImagen;

    void Start()
    {
        player = FindObjectOfType<Player>();
        Screen.orientation = ScreenOrientation.Portrait;
    }


    public void RegisterBtn()
    {
        StartCoroutine(RegisterNewPlayer());
    }

    private IEnumerator RegisterNewPlayer()
    {
        // Comprobamos que la contraseña sea igual que el confirmar contraseña
        if (inputPassword.text == inputCPassword.text)
        {
            // Comprobamos que hay información en el campo del email
            if (!string.IsNullOrEmpty(inputEmail.text))
            {
                yield return RegistrarAspNetUser();
                yield return GetAuthenticationToken();
                yield return GetAspNetUserId();
                yield return InsertPlayer();
                goBack();
                //yield return LoadImage();
            }
        }

    }

    private IEnumerator RegistrarAspNetUser()
    {

        AspNetUser aspUser = new AspNetUser();

        aspUser.Email = inputEmail.text;
        aspUser.Password = inputPassword.text;
        aspUser.ConfirmPassword = inputCPassword.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/Register", "POST"))
        {

            string bodyJson = JsonUtility.ToJson(aspUser);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.SetRequestHeader("Content-type", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("RegistrarAspNetUser > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegistrarAspNetUser > Info: " + httpClient.responseCode);
            }
        }


    }

    private IEnumerator GetAuthenticationToken()
    {

        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", inputEmail.text);
        data.AddField("password", inputPassword.text);

        using (UnityWebRequest httpClient = UnityWebRequest.Post(player.HttpServerAddress + "/Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAuthenticationToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }

    private IEnumerator GetAspNetUserId()
    {

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/UserId", "GET"))
        {

            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAspNetUserId > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
                Debug.Log("GetAspNetUserId > Info: " + player.Id);
            }

        }

    }

    private IEnumerator InsertPlayer()
    {
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;
        playerJson.Email = inputEmail.text;
        playerJson.FirstName = inputFirstName.text;
        playerJson.LastName = inputLastName.text;
        playerJson.NickName = inputNickName.text;
        playerJson.City = inputCity.text;
        playerJson.BirthDate = "1998-09-29";
        playerJson.BlobUri = "https://i.pinimg.com/originals/6b/7a/ac/6b7aacd389dca6133eb076eec7108652.jpg";
        //player.BlobUri = playerJson.BlobUri;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/InsertNewPlayer", "POST"))
        {

            string playerData = JsonUtility.ToJson(playerJson);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertPlayer > Info: " + httpClient.responseCode);
            }

        }
    }

    public void goBack()
    {
        SceneManager.LoadScene("Main");
    }
}
