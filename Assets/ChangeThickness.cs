using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ChangeThickness : MonoBehaviour, IPointerClickHandler {
	public GameObject whiteBoard;
	public float markerWidthInPixels = 3f;


	private WhiteboardController wc;

	// Use this for initialization
	void Start () {
		wc = whiteBoard.GetComponent<WhiteboardController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerClick(PointerEventData eventData){
		wc.markerWidthInPixels = markerWidthInPixels;
	}

}
