using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    public GameObject inGameMenu;
    public GameObject mainMenu;
    public GameObject endOfSongMenu;
    
    void Awake()
    {
        Instance = this;
    }

    //InGameMenu
    public void ShowIngameMenu()
    {
        inGameMenu.SetActive(true);
        inGameMenu.GetComponent<InGameMenu>().ShowMenu();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        mainMenu.GetComponent<MainMenu>().ShowMenu();
    }

    public void ShowEndOfSong()
    {
        endOfSongMenu.SetActive(true);
        endOfSongMenu.GetComponent<EndOfSongMenu>().ShowMenu();
    }
}
