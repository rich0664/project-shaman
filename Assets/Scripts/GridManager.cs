using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour {

	public float expandIncrement = 0.3f;
	public float rotationIncrement = 22.5f;
	public int expandAt = 4;
	public int expandInc = 8;
	public int rings;
	public int spots = 8;
	GameManager GM;
	GameObject spotPref;
	GameObject ringParent;
	float rotat = 0f;


	void Start(){
		//StartCoroutine(TestLoop());
		spotPref = Resources.Load<GameObject>("BuildingPrefabs/Spot");
		ringParent = new GameObject(); ringParent.name = "Rings";
		GM = GetComponent<GameManager>();
		Expand();
	}

	public void TryExpand(bool str){
		if(str){
			Expand();
		}
	}

	void Expand(){
		if(rings >= expandAt){
			expandAt += expandInc;
			expandInc *= 2;
			spots *= 2;
			rotationIncrement /= 2f;
			rotat = rotationIncrement;
		}
		GameObject Ring = new GameObject();
		Ring.name = "Ring" + (rings + 1); Ring.transform.SetParent(ringParent.transform);
		for(int i = 0; i < spots; i++){
			GameObject spotInst = GameObject.Instantiate(spotPref);
			spotInst.transform.SetParent(Ring.transform);
			spotInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + 7f);
			spotInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / spots) * i);
			GM.builderHelper.spotList.Add(spotInst.GetComponent<Spot>());
		}
		Ring.transform.RotateAround(Vector3.zero, Vector3.up, rotat);
		if(rotat == rotationIncrement){
			rotat = 0f;
		}else{ rotat = rotationIncrement; }
		rings++;

	}

	IEnumerator TestLoop(){
		while(true){
			yield return new WaitForSeconds(0.4f);
			Expand();
		}
	}

}
