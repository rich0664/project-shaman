using UnityEngine;
using System.Collections;

public class PhysicalStructure : MonoBehaviour {

	Renderer rnder;

	// Use this for initialization
	void Start () {
		rnder = GetComponentInChildren<Renderer> ();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.rotation = Camera.main.transform.rotation;
	}
}
