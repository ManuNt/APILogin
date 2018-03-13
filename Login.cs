using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class Login : MonoBehaviour
{

    private HttpHandler m_Connection;  // The HttpHandler was done by my lecturer.

    public Text m_TxtError;
    private string m_ErrorMsg;

    public Button m_BtnLogin, m_BtnLeaderboard, m_BtnPlay;

    public Text m_TxtUsername, m_TxtPassword, m_TxtLeaders, m_TxtHighscores;

    public GameObject m_LeaderboardPanel;

    


    private void Start()
    {
        if (InfoStatic.token == null)
        {
            m_BtnLeaderboard.interactable = false;
            m_BtnPlay.interactable = false;
        }
        m_Connection = HttpHandler.Instance;
        

        m_LeaderboardPanel.SetActive(false);
    }


    public void LogIn()
    {
        if (m_TxtUsername.text == "" || m_TxtPassword.text == "")
        {
            m_ErrorMsg = "Make sure the username and password fields are not empty!";
            StartCoroutine(Error());
        }
        else
        {
            WWWForm form = new WWWForm();

            form.AddField("username", m_TxtUsername.text);
            form.AddField("password", m_TxtPassword.text);

            m_Connection.HttpRequest(this, form, "http://localhost/TP2/login.php", InterpretResult);
        }
        
    }

    private IEnumerator Error()
    {
        m_TxtError.text = m_ErrorMsg;
        yield return new WaitForSeconds(10f);
        m_TxtError.text = "";
    }

    private void InterpretResult(object result)
    {
        string res = result.ToString();
        if (res == " wrong")
        {
            m_ErrorMsg = "Wrong user information!";
            StartCoroutine(Error());
        }
        else
        {
            string json = (string)result;
            UserInfo[] final = JsonConvert.DeserializeObject<UserInfo[]>(json);

            InfoStatic.id = final[0].Id;
            InfoStatic.token = final[0].Value;
            InfoStatic.highscore = final[0].HighScore;
            InfoStatic.userLogged = final[0].Username;

            m_BtnLogin.interactable = false;
            m_BtnPlay.interactable = true;
            m_BtnLeaderboard.interactable = true;

            
        }
    }

    public void VerifyToken()
    {
        if (InfoStatic.token == null || InfoStatic.token == "")
        {
            m_ErrorMsg = "Session expired. Please, log in again.";
            StartCoroutine(Error());
        }
        else
        {
            WWWForm form = new WWWForm();

            form.AddField("token", InfoStatic.token);


            m_Connection.HttpRequest(this, form, "http://localhost/TP2/token.php", ShowLeaderboard);
        }
    }

    private void ShowLeaderboard(object result)
    {
        if ((string)result == " no")
        {
            m_ErrorMsg = "Your token has expired, Sorry!";
            StartCoroutine(Error());
        }
        else
        {
            m_LeaderboardPanel.SetActive(true);
            string json = (string)result;
            Leaderboard[] learderboard = JsonConvert.DeserializeObject<Leaderboard[]>(json);

            for (int i = 0; i < learderboard.Length; i++)
            {
                m_TxtLeaders.text += learderboard[i].username + "\n";
                m_TxtHighscores.text += learderboard[i].highscore.ToString() + "\n";
            }
        }
    }

    public void PlayGame()
    {
        WWWForm form = new WWWForm();

        form.AddField("token", InfoStatic.token);


        m_Connection.HttpRequest(this, form, "http://localhost/TP2/token.php", Play);

    }

    private void Play(object result)
    {
        if ((string)result == " no")
        {
            m_ErrorMsg = "Your token has expired, Sorry!";
            StartCoroutine(Error());
        }
        else
        {
            GameManager.Instance.StartGame();
        }
    }

    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }

}
