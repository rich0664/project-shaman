using UnityEngine;
using System.Collections;

public class BuilderHelper : MonoBehaviour {

	GameManager GM;
	bool wasClick = false;
	Vector3 prevPos;
	Spot lastSpot;

	// Use this for initialization
	void Start () {
		GM = GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		CheckClick();
	}

	void QuickBuild(){
		GM.uiManager.QuickBuildTooltip();
	}


	void CheckClick(){
		if(GM.uiManager.isMenu)
			return;
		if(Input.touchCount != 1 && !Input.GetMouseButtonUp(0) && !Input.GetMouseButtonDown(0))
			return;
		if(Input.GetMouseButtonDown(0)){
			wasClick = true;
			prevPos = Input.mousePosition;
			return;
		}
		if(wasClick && Input.GetMouseButtonUp(0)){
			wasClick = false;
			if(prevPos != Input.mousePosition)
				return;
		}
		if(Input.touchCount == 1)
			if(Input.touches[0].tapCount != 1)
				return;
		Vector3 rayPos = Vector3.zero;
		if(Input.GetMouseButtonUp(0)){
			rayPos = Input.mousePosition;
		}else{
			rayPos = new Vector3(Input.touches[0].position.x,
			                     Input.touches[0].position.y, 0f);
		}
		Ray ray = Camera.main.ScreenPointToRay(rayPos);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			if(!hit.transform.GetComponent<Spot>()){
				return;
			}else{
				lastSpot = hit.transform.GetComponent<Spot>();
			}
			Debug.Log(hit.collider.gameObject);
			if(!lastSpot.filled){
				QuickBuild();
			}
		}
	}
}
