using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserData : MonoBehaviour
{
    

    public void GoGamesBtn()
    {
        SceneManager.LoadScene("Welcome");
    }
}
