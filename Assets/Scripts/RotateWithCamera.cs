using UnityEngine;
using System.Collections;

public class RotateWithCamera : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
		transform.rotation = Camera.main.transform.rotation;
	}
}
