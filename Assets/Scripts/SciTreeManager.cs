using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SciTreeManager : MonoBehaviour {

	public Transform SciTree;
	public bool open = false;
	public float baseResearch = 1f;
	public float distanceThreshHold = 300f;
	public float smoothSpeed = 10f;
	public float friction = 6f;

	public ResearchItem[] branchActive = new ResearchItem[3];

	GameManager GM;
	HashSet<Transform> nodeForms = new HashSet<Transform>();
	HashSet<ResearchItem> nodeList = new HashSet<ResearchItem>();
	Transform treeParent;


	Vector3 startPos;
	Quaternion startRot;
	public void StartUp () {
		GM = GetComponent<GameManager>();
		GameObject lineTemplate = SciTree.parent.parent.Find("TreeLineT").gameObject;
		Transform parentLine = SciTree.parent.Find("Lines");
		treeParent = SciTree.parent;
		startPos = treeParent.position;
		startRot = treeParent.rotation;

		int scc = SciTree.childCount;
		bool hasSave = false;
		if(nodeList.Count > 0)
			hasSave = true;
		for(int q = 0; q < scc; q++){
			Transform branch = SciTree.GetChild(q);
			int bcc = branch.childCount;			
			for(int i = 0; i < bcc; i++){
				Transform node = branch.GetChild(i);
				ResearchItem nodeItem = node.GetComponent<ResearchItem>();

				//assigning
				if(hasSave){
					nodeItem = nodeList.ElementAt(i);
				}
				nodeItem.progressBar = node.Find("ProgBar").GetComponent<Image>();
				nodeItem.progressBar.fillAmount = nodeItem.researchProgress / nodeItem.researchRequired;
				if(nodeItem.beingResearched){
					branchActive[node.parent.GetSiblingIndex()] = nodeItem;
				}


				//Draw lines between nodes
				foreach(ResearchItem inNode in nodeItem.inputNodes){
					GameObject tLine = GameObject.Instantiate(lineTemplate);
					tLine.transform.SetParent(parentLine);
					tLine.transform.localScale = Vector3.one;
					tLine.transform.position = node.position;
					Vector3 moveDirection = inNode.transform.position - tLine.transform.position;
					float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
					tLine.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
					float dist = Vector3.Distance(tLine.transform.position, inNode.transform.position);
					tLine.transform.localScale = new Vector3(dist,1f,1f);
					if(dist > distanceThreshHold){
						tLine.transform.SetAsFirstSibling();
						tLine.GetComponentInChildren<Image>().color *= 0.3f;
					}
				}

				//sprite and color stuff
				Image icon = node.Find("Icon").GetComponent<Image>();
				icon.color = nodeItem.displayColor;
				nodeList.Add(nodeItem);
				nodeForms.Add(node);

			}
		}
		UpdateUINodes();
	}
	

	Vector3 angMomentum = Vector3.zero;
	Vector3 panMomentum = Vector3.zero;
	Vector3 prevMouse = Vector3.zero;

	int count = 0;
	bool swtich = false;
	Vector3 panOff = Vector3.zero;
	Quaternion angOff = Quaternion.Euler(Vector3.zero);
	Vector3 prevRot = Vector3.one;
	void Update(){
		if(open){
			if(Input.GetMouseButtonDown(0)){
				prevMouse = Input.mousePosition;
				count = 0;
				angMomentum = Vector3.zero;
				panMomentum = Vector3.zero;
			}
			if(Input.GetMouseButton(0)){
				Vector3 deltaMouse = Input.mousePosition - prevMouse;
				angMomentum.z += deltaMouse.x * -0.1f;
				panMomentum.y += deltaMouse.y * 0.25f;
				if(count == 6){
					if(Mathf.Abs(angMomentum.z) > Mathf.Abs(panMomentum.y)){
						swtich = false;
						panOff = Vector3.zero;
					}else{
						swtich = true;
						angOff = Quaternion.Euler(new Vector3(0f, 0f, SnappedAngle(treeParent.eulerAngles.z)));
					}
				}

				prevMouse = Input.mousePosition;
			}
			if(swtich){
				panOff  = Vector3.Lerp(panOff, panOff + panMomentum * 4f, Time.deltaTime * smoothSpeed);
			}else{
				angOff = Quaternion.Lerp(angOff, angOff * Quaternion.Euler(angMomentum), Time.deltaTime * smoothSpeed);
			}

			treeParent.position  = Vector3.Lerp(treeParent.position, startPos + panOff, Time.deltaTime * smoothSpeed);
			treeParent.rotation  = Quaternion.Lerp(treeParent.rotation, startRot * angOff, Time.deltaTime * smoothSpeed);
			panMomentum.y = Mathf.SmoothStep(panMomentum.y, 0f, Time.deltaTime * friction);
			angMomentum.z = Mathf.SmoothStep(angMomentum.z, 0f, Time.deltaTime * (friction / 2f));

			if(treeParent.eulerAngles != prevRot && count % 3 == 0){
				prevRot = treeParent.eulerAngles;
				foreach(Transform tr in nodeForms)
					tr.eulerAngles = Vector3.zero;
			}

			count++;
		}
	}

	public void ResearchTick(){
		foreach(ResearchItem activeB in branchActive){
			if(activeB){
				if(activeB.researched)
					continue;
				ResourceManager.Resource branchRes = GM.resourceManager.GetResource(activeB.transform.parent.name);
				activeB.researchProgress += branchRes.amount + baseResearch;
				branchRes.amount = 0f;
				activeB.researchProgress = Mathf.Clamp(activeB.researchProgress, 0, activeB.researchRequired);
				if(activeB.researchProgress >= activeB.researchRequired){
					activeB.researched = true;
					activeB.persistUnlocked = true;
					UpdateUINodes();
				}
			}		
		}
	}

	public void UpdateUINodes(){
		foreach(ResearchItem tItem in nodeList){
			if(tItem.researched){
				tItem.progressBar.color = tItem.barColor * 0.6f;
				tItem.progressBar.color += Color.black;
				tItem.progressBar.fillAmount = 1f;
				tItem.researchProgress = tItem.researchRequired;
			}else{
				bool locked = false;
				if(tItem.unlockType == ResearchItem.unlockMode.All){
					foreach(ResearchItem inNode in tItem.inputNodes){
						if(!inNode.researched)
							locked = true;
					}
				}else{
					locked = true;
					foreach(ResearchItem inNode in tItem.inputNodes){
						if(inNode.researched){
							locked = false;
							break;
						}
					}
				}
				if(locked){
					Image tImg = tItem.transform.Find("Icon").GetComponent<Image>();
					tImg.color = tItem.displayColor * 0.6f;
					tImg.color += Color.black;
					if(!tItem.persistUnlocked){
						tItem.transform.Find("Icon/Text").GetComponent<Text>().text = "?";
					}
				}else{
					Image tImg = tItem.transform.Find("Icon").GetComponent<Image>();
					tImg.color = tItem.displayColor;
					tItem.transform.Find("Icon/Text").GetComponent<Text>().text = tItem.dispName;
				}
				
			}

		}
	}


	public void UpdateProgressBars(){
		foreach(ResearchItem activeB in branchActive)
			if(activeB)
				activeB.progressBar.fillAmount = activeB.researchProgress / activeB.researchRequired;

	}

	float SnappedAngle(float inAngle){
		return Mathf.RoundToInt(inAngle / 120f) * 120;
	}

	public void ResetTreeProgress(){
		foreach(ResearchItem tItem in nodeList){
			tItem.researchProgress = 0f;
			tItem.researched = false;
			tItem.beingResearched = false;
		}
	}

}
