using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrontFaceManager : MonoBehaviour {

	public int refreshRate = 2;
	[HideInInspector] public HashSet<Transform> updateList = new HashSet<Transform>();
	[HideInInspector] public Quaternion rot = Quaternion.Euler(Vector3.zero);
	int delayer = 0;
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
				foreach(Transform tr in GM.gridManager.foliages)
					tr.rotation = rot;
			}
		}
	}

	//end class
}
