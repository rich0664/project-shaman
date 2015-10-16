using UnityEngine;
using System.Collections;

public class SaveLoad : MonoBehaviour {

	GameManager GM;

	// Use this for initialization
	void Start () {
		GM = GetComponent<GameManager>();
	}

	public void Save(bool quit){

		if(quit)
			Application.Quit();
	}

	public void Load(bool firstLoad){

	}

}
