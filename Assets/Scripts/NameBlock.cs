using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

[ExecuteInEditMode]

public class NameBlock : MonoBehaviour {
	public string text = "";
	public int textSize = 40;
	private Font _textFont;
	public Color textColor = new Color(0.1215f, 0.1568f, 0.2117f);
	public float textHeight = 1.65f;
	public float textWeidth = 9f;
	public bool showShadow = true;
	public Color shadowColor = new Color(0, 0, 0, 0.5f);
	public Vector2 shadowOffset = new Vector2(1, 1);
	private string textShadow;

	public float getY() {
		return gameObject.transform.position.y;
	}

	public Font textFont {
		get {
			return _textFont;
		}
		set {
			_textFont = value;
		}
	}

	void Awake() {
		enabled = false;
		TextShadowReady();
	}

	void TextShadowReady() {
		textShadow = Regex.Replace(text, "<color[^>]+>|</color>", string.Empty);
	}

	void OnGUI() {
		GUI.depth = 9999;

		GUIStyle style = new GUIStyle();
		style.fontSize = textSize;
		style.richText = true;
		if (textFont)
			style.font = textFont;
		style.normal.textColor = textColor;
		style.alignment = TextAnchor.MiddleCenter;

		GUIStyle shadow = new GUIStyle();
		shadow.fontSize = textSize;
		shadow.richText = true;
		if (textFont)
			shadow.font = textFont;
		shadow.normal.textColor = shadowColor;
		shadow.alignment = TextAnchor.MiddleCenter;

		Vector3 worldPosition = new Vector3(transform.position.x + textWeidth, transform.position.y + textHeight, transform.position.z);
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
		screenPosition.y = Screen.height - screenPosition.y;

		if (showShadow)
			GUI.Label(new Rect(screenPosition.x + shadowOffset.x, screenPosition.y + shadowOffset.y, 0, 0), textShadow, shadow);
		GUI.Label(new Rect(screenPosition.x, screenPosition.y, 0, 0), text, style);
	}

	void OnBecameVisible() {
		enabled = true;
	}

	void OnBecameInvisible() {
		enabled = false;
	}
}
