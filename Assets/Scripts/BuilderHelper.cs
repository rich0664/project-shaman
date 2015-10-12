﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuilderHelper : MonoBehaviour {

	GameManager GM;
	bool wasClick = false;
	Vector3 prevPos;
	Spot lastSpot;
	public List<Spot> spotList;
	public List<PhysicalStructure> pStructList;

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

	public void BuildOnSpot(string typeToBuild){
		GameObject buildInst = GameObject.Instantiate(Resources.Load<GameObject>("BuildingPrefabs/" + typeToBuild));
		buildInst.transform.SetParent(lastSpot.transform);
		buildInst.transform.localPosition = Vector3.zero;
		buildInst.transform.localScale = Vector3.one;
		buildInst.GetComponent<PhysicalStructure>().structure = GM.structureManager.GetStructure(typeToBuild);
		pStructList.Add(buildInst.GetComponent<PhysicalStructure>());
		GM.structureManager.BuyStructure(typeToBuild);
		lastSpot.filled = true;
		if(GM.uiManager.toolTip)
		if(GM.uiManager.toolTip.name == "QuickBuildTooltip"){
			GM.uiManager.KillTooltip(true);
		}
	}

	void CheckClick(){
		if(GM.uiManager.isMenu )
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
		if(GM.uiManager.toolTip)
		if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
			GM.uiManager.KillTooltip(true);
			return;
		}else{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(rayPos);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			if(!hit.transform.GetComponent<Spot>()){
				return;
			}else{
				lastSpot = hit.transform.GetComponent<Spot>();
			}
			//Debug.Log(hit.collider.gameObject);
			if(!lastSpot.filled){
				QuickBuild();
			}else{
				GM.uiManager.StructInfo(lastSpot.GetComponentInChildren<PhysicalStructure>().structure.name);
			}
		}
	}
}