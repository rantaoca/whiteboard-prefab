using UnityEngine;
using System.Collections;
//using UnityEngine.EventSystems;

public class WhiteboardOptionsController : MonoBehaviour {
	public GameObject thin;
	public GameObject middle;
	public GameObject thick;
	public GameObject whiteBoard;

	//public GameObject wc;

	private Color currentSelectedColor;
	private WhiteboardController wc;
	//private WhiteboardController wc;

	// Use this for initialization
	void Start () {
		wc = whiteBoard.GetComponent<WhiteboardController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void UpdateColor (Color selectedColor) {
		currentSelectedColor = selectedColor;
		Color currentColor = selectedColor;
		thin.GetComponent<Renderer> ().material.color = currentColor;
		middle.GetComponent<Renderer> ().material.color = currentColor;
		thick.GetComponent<Renderer> ().material.color = currentColor;
		wc.markerColor = selectedColor;

	}
}
