using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrontFaceManager : MonoBehaviour {

	public int refreshRate = 2;
	[HideInInspector] public List<Transform> updateList = new List<Transform>();
	int delayer = 0;
	Quaternion rot = Quaternion.Euler(Vector3.zero);
	Transform camT;
	GameManager GM;

	void Start(){
		camT = Camera.main.transform;
		GM = GameObject.Find ("Management").GetComponent<GameManager> ();
	}

	public void GetTrans(List<PhysicalStructure> pStructs){
		updateList.Clear();
		foreach(PhysicalStructure pS in pStructs)
			updateList.Add(pS.transform.GetChild(0));
	}

	// Update is called once per frame
	void LateUpdate () {
		if(camT.rotation != rot){
			delayer++;
			if(delayer % refreshRate == 0){
				rot = camT.rotation; delayer = 0;
				foreach(Transform tr in updateList)
					tr.rotation = rot;
			}
		}
	}
}
