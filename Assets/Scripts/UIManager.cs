using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject userGameUI;
   


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        ClearScreen();
        loginUI.SetActive(true);
        
        

    }
    public void RegisterScreen() // Register button
    {
        ClearScreen();
        registerUI.SetActive(true);
        
		
    }

    public void UserGameScreen()//cuando se encuentra logueado
    {
        ClearScreen();
        userGameUI.SetActive(true);
    }
    
    //todas las screen se ponen en falso para que no esten visibles
    public void ClearScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        userGameUI.SetActive(false);
    }
}
