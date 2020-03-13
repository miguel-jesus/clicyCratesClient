using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InfoPlayer : MonoBehaviour
{
    [SerializeField]
    GameObject playerGO;
    private Player player;
    public InputField inputName;
    public InputField inputLastName;
    public InputField inputNickName;
    public InputField inputEmail;
    public InputField inputBlobUri;
    public Image imagenAvatar;
   

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
       
        playerGO = GameObject.Find("Player");
        player = playerGO.GetComponent<Player>();
        StartCoroutine(LoadImage());
        GetInfoPlayer();

    }

 
    public void GoGamesBtn()
    {
        SceneManager.LoadScene("Welcome");
    }

    
    private void GetInfoPlayer()
    {
        inputName.text = player.FirstName;
        inputLastName.text = player.LastName;
        inputNickName.text = player.NickName;
        inputEmail.text = player.Email;
        inputBlobUri.text = player.BlobUri;
    }

    
    private IEnumerator LoadImage()
    {

        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobUri))
        {

            httpClient.downloadHandler = new DownloadHandlerTexture();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("LoadImage > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                Debug.Log("LoadImage > Info: " + texture);

                imagenAvatar.sprite = Sprite.Create(texture,
                                                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                                                    new Vector2(0.5f, 0.5f),
                                                    100.0f);
            }

        }

    }

   
    public void SaveChangesBtn()
    {
        StartCoroutine(UpdateInfoPlayer());
    }


   
    private IEnumerator UpdateInfoPlayer()
    {
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;
        playerJson.FirstName = inputName.text;
        playerJson.LastName = inputLastName.text;
        playerJson.NickName = inputNickName.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/UpdatePlayer", "POST"))
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
                throw new System.Exception("UpdateInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("UpdateInfoPlayer > Info: " + httpClient.responseCode);
                player.FirstName = playerJson.FirstName;
                player.LastName = playerJson.LastName;
                player.NickName = playerJson.NickName;

                yield return UpdateInfoOnline();
            }
        }

    }


    private IEnumerator UpdateInfoOnline()
    {
        ConnectedSerializable connected = new ConnectedSerializable();
        connected.Id = player.Id;
        connected.NickName = player.NickName;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Connected/UpdateConnectedPlayer", "POST"))
        {
            string playerData = JsonUtility.ToJson(connected);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("UpdateInfoOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("UpdateInfoOnline > Info: " + httpClient.responseCode);

            }

        }

    }


    public void DeleteAccountBtn()
    {
        StartCoroutine(DeleteAccount());
    }

    private IEnumerator DeleteAccount()
    {
        yield return TryCerrarSesion();
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;


        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/DeleteAccount", "POST"))
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
                throw new System.Exception("EliminarCuenta > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("EliminarCuenta > Info: " + httpClient.responseCode);


                player.Id = string.Empty;
                player.Token = string.Empty;
                player.FirstName = string.Empty;
                player.LastName = string.Empty;
                player.Email = string.Empty;
                player.NickName = string.Empty;
                player.BlobUri = string.Empty;
                player.City = string.Empty;

                SceneManager.LoadScene("Welcome");

            }
        }
    }


    private IEnumerator TryCerrarSesion()
    {
        ConnectedSerializable connected = new ConnectedSerializable();
        connected.Id = player.Id;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Connected/DeleteConnected", "POST"))
        {
            string playerData = JsonUtility.ToJson(connected);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TryCerrarSesion > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("TryCerrarSesion > Info: " + httpClient.responseCode);

                player.Login = false;

            }

        }

    }
}
