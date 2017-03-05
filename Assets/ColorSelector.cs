using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ColorSelector : MonoBehaviour, IPointerClickHandler {

	public GameObject whiteBoard;
	//private WhiteboardController wc;
	private WhiteboardOptionsController woc;
	// Use this for initialization
	void Start () {
		//wc = whiteBoard.GetComponent<WhiteboardController> ();
		woc = GetComponentInParent<WhiteboardOptionsController> ();
	}

	// Update is called once per frame
	void Update () {

	}

	public void OnPointerClick(PointerEventData eventData){
		woc.UpdateColor (GetComponent<Renderer> ().material.color);
		//wc.markerColor = GetComponent<Renderer> ().material.color;
	}
}
