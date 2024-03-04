using UnityEngine;
using System.Collections;

public class BaseMenu : MonoBehaviour
{
	// public GUISkin CustomSkin;
	// public GUIStyle TextStyle = new GUIStyle();

	protected bool isVisible = false;

	public void ShowMenu()
	{
		isVisible = true;
		gameObject.SetActive(true);
		OnShowMenu();
	}

	public void HideMenu()
	{
		isVisible = false;
		gameObject.SetActive(false);
		OnHideMenu();
	}

	protected virtual void OnShowMenu()
	{

	}

	protected virtual void OnHideMenu()
	{

	}

	protected void DrawBackground()
	{
		// Texture2D Background = (Texture2D)UnityEngine.Resources.Load( "BlackTransparent" );
		// GUI.DrawTexture( new Rect( 0, 0, Screen.width, Screen.height ), Background, ScaleMode.StretchToFill, true );
	}
}