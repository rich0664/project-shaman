using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour {

	public float expandIncrement = 0.3f;
	public float rotationIncrement = 22.5f;
	public int expandAt = 4;
	public int expandInc = 8;
	public int rings;
	public int spots = 8;
	public GameObject BasePref;
	float rotat = 0f;


	void Start(){
		StartCoroutine(TestLoop());
	}

	public void Expand(){
		if(rings >= expandAt){
			expandAt += expandInc;
			expandInc *= 2;
			spots *= 2;
			rotationIncrement /= 2f;
			rotat = rotationIncrement;
		}
		GameObject Ring = new GameObject();
		Ring.name = "Ring";
		for(int i = 0; i < spots; i++){
			GameObject spotInst = GameObject.Instantiate(BasePref);
			spotInst.transform.SetParent(Ring.transform);
			spotInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + 6f);
			spotInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / spots) * i);
		}
		Ring.transform.RotateAround(Vector3.zero, Vector3.up, rotat);
		rotat += rotationIncrement;
		rings++;

	}

	IEnumerator TestLoop(){
		while(true){
			yield return new WaitForSeconds(0.4f);
			Expand();
		}
	}

}
