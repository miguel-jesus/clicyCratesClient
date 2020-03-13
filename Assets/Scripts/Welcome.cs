using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Welcome : MonoBehaviour
{
    [SerializeField]
    Player player;
    public Text textInfoJugador;
    public Text text;
    public UnityEngine.UI.Image imagenAvatar;
    public GameObject content;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        player = FindObjectOfType<Player>();
        Debug.Log(player.Token);
        InfoPlayer();
        StartCoroutine(LoadImage());
        StartCoroutine(GetPartidasJugadas());
        if (player.Login)
        {
            Debug.Log("Connected");
        }
        else
        {
            StartCoroutine(InsertOnlinePlayer());
        }

    }


    public void GoToJugarButton()
    {
        StartCoroutine(IrAlJuego());
    }


    public void GoToPerfilButton()
    {
        SceneManager.LoadScene("InfoPlayer");
    }

    private void InfoPlayer()
    {
        textInfoJugador.text = player.NickName;
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


    private IEnumerator IrAlJuego()
    {


        ConnectedSerializable online = new ConnectedSerializable();
        online.Id = player.Id;
        online.Estado = "Jugando";

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Connected/UpdateConnectedPlayer", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);

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
                SceneManager.LoadScene("Prototype 5");
            }

        }


    }


    private IEnumerator InsertOnlinePlayer()
    {
        ConnectedSerializable online = new ConnectedSerializable();
        online.Id = player.Id;
        online.NickName = player.NickName;
        online.ImageUser = player.BlobUri;
        

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Connected/InsertConnected", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertOnlinePlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertOnlinePlayer > Info: " + httpClient.responseCode);

                player.Login = true;

            }

        }
    }


    private IEnumerator GetPartidasJugadas()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Games/GetGames/" + player.Id, "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetPartidasJugadas > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"miLista\":" + jsonResponse + "}";
                ListGameSerializable lista = JsonUtility.FromJson<ListGameSerializable>(response);
             

                foreach (GameSerializable p in lista.miLista)
                {
                   
                    Text textoPartida = Instantiate(text, Vector3.zero, Quaternion.identity);
                   
                    textoPartida.text = "Id Player: " + p.IdPlayer + " \n Inicio: " + p.Inicio + " \n Final: " + p.Final + " \n Dificultad: " + p.Difficulty + " \n Puntos: " + p.Points;
                    textoPartida.fontSize = 30;
                    textoPartida.resizeTextForBestFit = true;
                    textoPartida.color = Color.white;
                    textoPartida.alignment = TextAnchor.MiddleCenter;
                  
                    textoPartida.transform.SetParent(content.transform);

                   }
            }
        }
    }
}
            
