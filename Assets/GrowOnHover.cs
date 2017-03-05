using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GrowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public float zoomScale = 1.5f;

	private Vector3 defaultScale; 
	// Use this for initialization
	void Start () {
		defaultScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerEnter(PointerEventData eventData) {
		transform.localScale = defaultScale * zoomScale;
	}

	public void OnPointerExit(PointerEventData eventData) {
		transform.localScale = defaultScale;
	}

}
