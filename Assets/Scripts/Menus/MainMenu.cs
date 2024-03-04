using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : BaseMenu
{
    [SerializeField] private GameObject m_Cell;

    void Start()
    {
        ShowMenu();

        //Draw all songs in the playlist
        SongData[] playlist = GuitarGameplay.Instance.GetPlaylist();
        for (int i = 0; i < playlist.Length; i++)
        {
            var index = i;
            var go = GameObject.Instantiate(m_Cell);
            go.SetActive(true);
            go.transform.SetParent(m_Cell.transform.parent);
            string buttonLabel = playlist[i].Band + " - " + playlist[i].Name;
            go.GetComponentInChildren<TextMeshProUGUI>().text = buttonLabel;
            go.GetComponent<Button>().onClick.AddListener(() => { OnStartClick(index); });
        }
    }

    // void OnGUI()
    // {
    // 	if( isVisible == false )
    // 	{
    // 		return;
    // 	}
    //
    // 	if( CustomSkin )
    // 	{
    // 		GUI.skin = CustomSkin;
    // 	}
    //
    // 	DrawBackground();
    // 	DrawText();
    // 	DrawSongButtons();
    // }
    //
    // void DrawText()
    // {
    // 	TextStyle.fontSize = 106;
    // 	GUI.Label( new Rect( 0, 50, Screen.width, 110 ), "Guitar Unity", TextStyle );
    //
    // 	TextStyle.fontSize = 48;
    // 	GUI.Label( new Rect( 0, 230, Screen.width, 50 ), "Select your Song", TextStyle );
    // }
    //
    // void DrawSongButtons()
    // {
    // 	GUILayout.BeginArea( new Rect( Screen.width / 2f - 250, Screen.height / 2f - 20, 500, 400 ) );
    //
    // 	//Draw all songs in the playlist
    // 	SongData[] playlist = GetComponent<GuitarGameplay>().GetPlaylist();
    //
    // 	for( int i = 0; i < playlist.Length; ++i )
    // 	{
    // 		string buttonLabel = playlist[ i ].Band + " - " + playlist[ i ].Name;
    //
    // 		if( GUILayout.Button( buttonLabel ) )
    // 		{
    // 			GetComponent<GuitarGameplay>().StartPlaying( i );
    // 			HideMenu();
    // 		}
    // 	}
    //
    // 	GUILayout.EndArea();
    // }

    void OnStartClick(int index)
    {
        GuitarGameplay.Instance.StartPlaying(index);
        HideMenu();
    }
}