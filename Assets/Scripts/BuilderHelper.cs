using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuilderHelper : MonoBehaviour {

	GameManager GM;
	bool wasClick = false;
	Vector3 prevPos;
	[HideInInspector] public Spot lastSpot;
	[HideInInspector] public List<Spot> spotList;
	[HideInInspector] public List<Spot> emptySpots;
	[HideInInspector] public List<PhysicalStructure> pStructList;
	[HideInInspector] public HashSet<PhysicalStructure> hiringPStructList;
	[HideInInspector] public HashSet<pStructInfo> pStructData;

	// Use this for initialization
	public void StartUp () {
		GM = GetComponent<GameManager>();
		pStructList = new List<PhysicalStructure>();
		pStructData = new HashSet<pStructInfo>();
	}
	
	// Update is called once per frame
	void Update () {
		CheckClick();
		CheckTouchHold();
	}

	public PhysicalStructure QuickBuild(string typeToBuild, bool instant){
		if(!instant)
			GM.structureManager.BuyStructure(typeToBuild);
		lastSpot.filled = true;
		if(GM.uiManager.toolTip)
		if(GM.uiManager.toolTip.name == "QuickBuildTooltip"){
			GM.uiManager.KillTooltip(true);
		}
		return BuildOnSpot(typeToBuild, instant);
	}

	public PhysicalStructure BuildOnSpot(string typeToBuild, bool instant){
		lastSpot.transform.localScale = Vector3.one * 0.725f;
		GameObject buildInst = GameObject.Instantiate(Resources.Load<GameObject>("BuildingPrefabs/Construction"));
		GameObject timerInst = GameObject.Instantiate(GameObject.Find("WorldCanvasOcc").transform.Find("Templates/ConstructionTimer").gameObject);
		timerInst.transform.SetParent(GameObject.Find("WorldCanvasOcc").transform);
		timerInst.transform.localScale = Vector3.one;
		timerInst.transform.position = lastSpot.transform.position;
		buildInst.transform.SetParent(lastSpot.transform);
		buildInst.transform.localPosition = Vector3.zero;
		buildInst.transform.rotation = Camera.main.transform.rotation;
		PhysicalStructure pStruct = buildInst.GetComponent<PhysicalStructure>();
		pStruct.structure = GM.structureManager.GetStructure(typeToBuild);
		lastSpot.transform.localScale *= pStruct.structure.sizeScale;

		//push
		StartCoroutine(AnimPush(lastSpot, pStruct.structure.sizeScale, instant));

		pStruct.GM = GM;
		pStruct.constructTimer = timerInst;
		//pStructList.Add(pStruct);
		GM.ffManager.updateList.Add(pStruct.transform);
		if(instant){
			pStruct.constructTime = 0f;
		}else{
			pStruct.constructTime = pStruct.structure.constructTime;
		}
		pStruct.spotIndex = pStruct.transform.parent.name;
		pStruct.StartConstruct();
		return pStruct;
	}

	IEnumerator AnimPush(Spot lastS, float sizeScale, bool instant = false){
		float maxDist = 20f * sizeScale;
		Collider[] hitColliders = Physics.OverlapSphere(lastSpot.transform.position, maxDist);
		List<Vector3> endPositions = new List<Vector3>();
		List<Collider> endColliders = new List<Collider>();
		foreach(Collider hc in hitColliders){
			if(hc.transform == lastSpot.transform)
				continue;
			Vector3 heading = hc.transform.position - lastSpot.transform.position;
			float dist = heading.magnitude;
			if(dist >= maxDist)
				continue;
			Vector3 dir = heading / dist;
			//Debug.Log(dir);
			dist = maxDist - dist;
			dist /= maxDist;
			dist *= sizeScale - 1f;
			//Debug.Log(dist);
			endColliders.Add(hc);
			endPositions.Add(hc.transform.position + (dir * dist * 2f));
		}
		if(!instant){
			float currentT = 0f;
			float animT = 0.2f;
			//Debug.Log(endColliders.Count + " : " + endPositions.Count);
			while(currentT < animT){
				currentT += Time.deltaTime;
				for(int i = 0; i < endColliders.Count; i++)
					endColliders[i].transform.position = Vector3.Lerp(endColliders[i].transform.position, endPositions[i], currentT / animT);			
				yield return new WaitForEndOfFrame();
			}
		}
		for(int i = 0; i < endColliders.Count; i++)
			endColliders[i].transform.position = endPositions[i];
		
	}

	bool isHeld = false;
	float startTouchTime = 0f;
	public void InitialTouch(){
		isHeld = true;
		startTouchTime = Time.time;
	}

	public void ExitTouch (){
		isHeld = false;
	}

	public void ClickBuild(){
		isHeld = false;
		StartCoroutine(BatchBuild(GM.uiManager.lastTooltip, 
		           (int)GM.uiManager.toolTip.transform.Find ("Slider").GetComponent<Slider> ().value,
		           Vector3.zero));
	}

	void CheckTouchHold(){
		if(isHeld)
			if(Time.time - startTouchTime > 1f){				
				isHeld = false;
				if(GM.uiManager.toolTip.transform.Find ("Slider").GetComponent<Slider> ().value == 0f)
					return;
				StartCoroutine(ManualPlace());
			}
	}

	[HideInInspector] public bool isPlacing = false;
	IEnumerator ManualPlace(){
		int tmpAmount = (int)GM.uiManager.toolTip.transform.Find ("Slider").GetComponent<Slider> ().value;
		isPlacing = true; GM.uiManager.ToggleMenu("Structures"); GameManager.paused = true;
		GM.gameCamera.targetDistance = 1000f;	GM.gameCamera.ReCenterCamera();
		GameObject targeter = GameObject.Find("UICanvas").transform.Find("BuildTargeter").gameObject;
		targeter.SetActive(true); string toBuild = GM.uiManager.lastTooltip;
		targeter.transform.Find("StructIcon").GetComponent<Image>().sprite = Resources.Load<Sprite> ("BuildingIcons/" + toBuild);
		while(Input.GetMouseButton(0)){
			yield return new WaitForEndOfFrame();
			Vector3 modMousePos = Input.mousePosition + new Vector3(0f,60f,0f);
			Ray rayy = Camera.main.ScreenPointToRay(modMousePos); RaycastHit hitt;
			if(Physics.Raycast(rayy, out hitt))
				targeter.transform.position = modMousePos;
				//targeter.transform.position = hitt.point;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition + new Vector3(0f,60f,0f)); RaycastHit hit;
		if(Physics.Raycast(ray, out hit))
			StartCoroutine(BatchBuild(toBuild, tmpAmount, hit.point));
		isPlacing = false; GameManager.paused = false; targeter.SetActive(false);
	}

	IEnumerator BatchBuild(string toBuild, int buildCount, Vector3 buildCenter){
		RefreshEmptySpots();
		List<Spot> batchSpots = new List<Spot>();
		for(int i = 0; i < buildCount; i++){
			GM.structureManager.BuyStructure(toBuild);
			Spot tmpSpot = GetClosestSpot(buildCenter);
			batchSpots.Add(tmpSpot);
			tmpSpot.filled = true;
		}
		for(int i = 0; i < buildCount; i++){
			yield return new WaitForSeconds(0.005f);
			lastSpot = batchSpots[i];
			BuildOnSpot(toBuild, false);
		}
	}

	public void RefreshEmptySpots(){
		emptySpots = new List<Spot>();
		foreach(Spot spt in spotList)
			if(!spt.filled)
				emptySpots.Add(spt);
	}

	public Spot GetClosestSpot(Vector3 refPoint){
		Spot closeSpot = emptySpots[0];
		float dist = 9999999f;
		int spotIndex = 0;
		for(int i = 0; i < emptySpots.Count; i++){
			float tmpDist = Vector3.Distance(refPoint, emptySpots[i].transform.position);
			if(tmpDist < dist){
				closeSpot = emptySpots[i];
				dist = tmpDist;
				spotIndex = i;
			}
		}
		emptySpots.RemoveAt(spotIndex);
		return closeSpot;
	}

	void CheckClick(){
		if(GM.uiManager.isMenu && GM.uiManager.lastTooltip != "VillagerInfo")
			return;
		if(!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonDown(0))
			return;
		if(Input.GetMouseButtonDown(0)){
			wasClick = true;
			prevPos = Input.mousePosition;
			return;
		}
		if(wasClick && Input.GetMouseButtonUp(0)){
			wasClick = false; float leniency = 6f;
			if(Mathf.Clamp(prevPos.x, Input.mousePosition.x - leniency, Input.mousePosition.x + leniency) != prevPos.x){
				return;
			}else if(Mathf.Clamp(prevPos.y, Input.mousePosition.y - leniency, Input.mousePosition.y + leniency) != prevPos.y){
				return;
			}
		}

		if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
			if(GM.uiManager.toolTip){
				GM.uiManager.KillTooltip(true);
				if(GM.aiManager.resetTarget){
					GM.aiManager.ResetCameraTarget();
				}
				return;
			}
		}else{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			if(hit.transform.GetComponent<Spot>()){
				lastSpot = hit.transform.GetComponent<Spot>();
				//Debug.Log(hit.collider.gameObject);
				if(!lastSpot.filled){
					GM.uiManager.QuickBuildTooltip();
				}else{
					GM.uiManager.StructInfo(lastSpot.GetComponentInChildren<PhysicalStructure>());
				}
			}
			if(hit.transform.parent == GM.aiManager.villagerParent.transform){
				//GM.aiManager.SetCameraTarget(hit.transform);
				VillagerAI tvai = hit.transform.GetComponent<VillagerAI>();
				GM.uiManager.VillagerInfo(tvai.villager);
			}
		}
	}
}
