using UnityEngine;
using System.Collections;
using UnityEditor;

public class TilePickerWindow : EditorWindow {

	public enum Scale
	{
		x1,x2,x3,x4,x5
	}
	Scale scale;
	public Vector2 scrollPosition = Vector2.zero;
	public Vector2 currentSelection = Vector2.zero;

	[MenuItem("Window/Tile Picker")]
	public static void OpenTilePickerWindow() {
		TilePickerWindow window = EditorWindow.GetWindow (typeof(TilePickerWindow)) as TilePickerWindow;
		GUIContent title = new GUIContent ();
		title.text = "Tile Picker";
		window.titleContent = title;
	}

	void OnGUI() {
		if (Selection.activeGameObject == null)
			return;
		Board selection = Selection.activeGameObject.GetComponent<Board> ();
		if (selection != null) {
			Texture2D texture2D = selection.texture2D;
			if (texture2D != null) {
				scale = (Scale)EditorGUILayout.EnumPopup ("Zoom",scale);
				//int newScale = ((int)scale) + 1;
				float newScale = 0.4f;
				Vector2 newTextureSize = new Vector2 (texture2D.width,texture2D.height)*newScale;
				Vector2 offset = new Vector2 (10,25);
				Rect viewport = new Rect (0,0,position.width-5,position.height-5);
				Rect contentSize = new Rect (0,0,newTextureSize.x+offset.x,newTextureSize.y+offset.y);
				scrollPosition = GUI.BeginScrollView (viewport,scrollPosition,contentSize);
				GUI.DrawTexture (new Rect(offset.x,offset.y,newTextureSize.x,newTextureSize.y),texture2D);
				Vector2 tile = selection.tileSize * newScale;
				tile.x += selection.tilePadding.x * newScale;
				tile.y += selection.tilePadding.y * newScale;
				Vector2 grid = new Vector2 (newTextureSize.x/tile.x,newTextureSize.y/tile.y);
				Vector2 selectionPos = new Vector2 (tile.x*currentSelection.x+offset.x,tile.y*currentSelection.y+offset.y);
				Texture2D boxTex = new Texture2D (1,1);
				boxTex.SetPixel (0,0,new Color(0,0.5f,1f,0.4f));
				boxTex.Apply ();
				GUIStyle style = new GUIStyle (GUI.skin.customStyles[0]);
				style.normal.background = boxTex;
				GUI.Box (new Rect(selectionPos.x,selectionPos.y,tile.x,tile.y),"",style);
				Event cEvent = Event.current;
				Vector2 mousePos = new Vector2 (cEvent.mousePosition.x,cEvent.mousePosition.y);
				if (cEvent.type==EventType.MouseDown && cEvent.button==0) {
					currentSelection.x = Mathf.Floor ((mousePos.x+scrollPosition.x)/tile.x);
					currentSelection.y = Mathf.Floor ((mousePos.y+scrollPosition.y)/tile.y);
					if (currentSelection.x > grid.x - 1)
						currentSelection.x = grid.x - 1;
					if (currentSelection.y > grid.y - 1)
						currentSelection.y = grid.y - 1;
					selection.tileID = (int)(currentSelection.x+(currentSelection.y*grid.x)+1);
					Repaint ();
				}
				GUI.EndScrollView ();
			}
		}
	}
}
