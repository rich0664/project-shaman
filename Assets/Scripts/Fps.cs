using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Fps : MonoBehaviour {

	float deltaTime = 0.0f;
	Text fpsText;

	void Start(){
		fpsText = GetComponent<Text>();
	}

	void Update () {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		fpsText.text = (1.0f / deltaTime).ToString("F1") + "\n";
		fpsText.text += Input.touchCount.ToString() + "\n";
		if(Input.touchCount == 1){
			fpsText.text += Input.GetTouch(0).tapCount.ToString();
		}
	}
}
