using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class SciTreeManager : MonoBehaviour {

	public Transform SciTree;
	public bool open = false;
	public float baseResearch = 1f;
	public float distanceThreshHold = 300f;
	public float smoothSpeed = 10f;
	public float friction = 6f;

	[HideInInspector] public ResearchItem[] branchActive = new ResearchItem[3];
	[HideInInspector] public float[] deltaResearchBranches = new float[3];

	Image popupBar;
	GameObject popup;
	ResearchItem popupItem;

	GameManager GM;
	HashSet<Transform> nodeForms = new HashSet<Transform>();
	public HashSet<ResearchItem> nodeList = new HashSet<ResearchItem>();
	public List<NodeDataObj> nodeData = new List<NodeDataObj>();
	Transform treeParent;

	Vector3 startPos;
	Quaternion startRot;
	public void StartUp () {
		GM = GetComponent<GameManager>();
		GameObject lineTemplate = SciTree.parent.parent.Find("TreeLineT").gameObject;
		Transform parentLine = SciTree.parent.Find("Lines");
		treeParent = SciTree.parent;
		popup = treeParent.parent.Find("TreePopup").gameObject;
		popup.SetActive(false);
		startPos = treeParent.position;
		startRot = treeParent.rotation;

		int scc = SciTree.childCount;
		bool hasSave = false;

		if(PlayerPrefs.HasKey("SciTreeData"))
			hasSave = true;
		int nodeCounter = 0;
		for(int q = 0; q < scc; q++){
			Transform branch = SciTree.GetChild(q);
			int bcc = branch.childCount;			
			for(int i = 0; i < bcc; i++){
				Transform node = branch.GetChild(i);
				ResearchItem nodeItem = node.GetComponent<ResearchItem>();

				//assigning
				if(hasSave){
					nodeItem.researchProgress = nodeData[nodeCounter].researchProgress;
					nodeItem.researched = nodeData[nodeCounter].researched;
					nodeItem.persistUnlocked = nodeData[nodeCounter].persistUnlocked;
					nodeItem.beingResearched = nodeData[nodeCounter].beingResearched;
				}
				nodeItem.progressBar = node.Find("ProgBar").GetComponent<Image>();
				nodeItem.progressBar.fillAmount = nodeItem.researchProgress / nodeItem.researchRequired;
				if(nodeItem.beingResearched){
					branchActive[node.parent.GetSiblingIndex()] = nodeItem;
				}
				nodeItem.dispName = i.ToString() + nodeItem.transform.parent.name[0].ToString();//temp


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
					if(dist > distanceThreshHold){
						tLine.transform.SetAsFirstSibling();
						tLine.transform.Find("TreeLine").GetComponent<Image>().color *= 0.3f;
					}
					tLine.transform.localScale = new Vector3(dist,1f,1f);
				}

				//sprite and color stuff
				Image icon = node.Find("Icon").GetComponent<Image>();
				Button butt = icon.GetComponent<Button>();
				//Destroy(icon.transform.Find("Text").gameObject);
				int tmpBSint = nodeCounter;
				butt.onClick.AddListener(delegate {
					CreatePopup(tmpBSint);
				});
				icon.sprite = Resources.Load<Sprite> ("SciTreeIcons/" + nodeItem.name);
				icon.color = nodeItem.displayColor;
				nodeList.Add(nodeItem);
				nodeForms.Add(node);
				nodeCounter++;
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
					ClosePopup();
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

			if(popup.activeInHierarchy)
				PopupUpdate();

			count++;
		}
	}

	public void ResearchTick(){
		int tCount = 0;
		foreach(ResearchItem activeB in branchActive){
			ResourceManager.Resource branchRes = GM.resourceManager.GetResource(SciTree.GetChild(tCount).name);
			deltaResearchBranches[tCount] = branchRes.amount + baseResearch;
			tCount++;
			if(activeB){
				if(activeB.researched)
					continue;
				activeB.researchProgress += branchRes.amount + baseResearch;
				branchRes.amount = 0f;
				activeB.researchProgress = Mathf.Clamp(activeB.researchProgress, 0, activeB.researchRequired);
				if(activeB.researchProgress >= activeB.researchRequired){
					activeB.researched = true;
					activeB.persistUnlocked = true;
					if(popup.activeInHierarchy){
						ClosePopup();
						CreatePopup(lastPopupIndex);
					}
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
				tItem.transform.Find("Icon/Text").GetComponent<Text>().text = "";
				tItem.persistUnlocked = true;
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
						tItem.transform.Find("Icon/Text").GetComponent<Text>().text = "";
						tImg.sprite = Resources.Load<Sprite> ("SciTreeIcons/Unknown");
					}
				}else{
					Image tImg = tItem.transform.Find("Icon").GetComponent<Image>();
					tImg.color = tItem.displayColor;
					tItem.transform.Find("Icon/Text").GetComponent<Text>().text = "";
					tImg.sprite = Resources.Load<Sprite> ("SciTreeIcons/" + tItem.name);
				}				
			}
		}
	}

	public void UpdateProgressBars(){
		foreach(ResearchItem activeB in branchActive)
			if(activeB)
				activeB.progressBar.fillAmount = activeB.researchProgress / activeB.researchRequired;

	}

	int lastPopupIndex = 0;
	public void CreatePopup(int nodeIndex){
		popup.SetActive(true);
		lastPopupIndex = nodeIndex;
		if(popupBar)
			Destroy(popupBar.transform.parent.gameObject);
		popupItem = nodeList.ElementAt(nodeIndex);
		GameObject nodeInst = GameObject.Instantiate(treeParent.parent.Find("NodeT").gameObject);
		Vector3 tScale = treeParent.parent.Find("NodeT").localScale;
		popupBar = nodeInst.transform.Find("ProgBar").GetComponent<Image>();
		nodeInst.transform.SetParent(popup.transform.Find("NodeClone"));
		nodeInst.transform.localPosition = Vector3.zero;
		nodeInst.transform.localScale = tScale;
		float halfScreen = (float)Screen.width / 2f;
		float fourthScreen = (float)Screen.width / 4f;
		int branchIndex = popupItem.transform.parent.GetSiblingIndex();
		if(popupItem.transform.position.x > halfScreen)
			popup.transform.position = new Vector3(halfScreen - fourthScreen, popup.transform.position.y, 0f);
		else
			popup.transform.position = new Vector3(halfScreen + fourthScreen, popup.transform.position.y, 0f);
		angOff = Quaternion.Euler(new Vector3(0f, 0f, branchIndex * 120f));

		Button popButt = popup.transform.Find("Button").GetComponent<Button>();
		Text popButtText = popup.transform.Find("Button/Text").GetComponent<Text>();

		if(popupItem.transform.Find("Icon").GetComponent<Image>().sprite.name != "Unknown"){
			popup.transform.Find("PopupTitle").GetComponent<Text>().text = popupItem.dispName;
			popup.transform.Find("PopupQuote").GetComponent<Text>().text = "''" + popupItem.quote + "''";

			Image popIcon = nodeInst.transform.Find("Icon").GetComponent<Image>();
			popIcon.sprite = Resources.Load<Sprite> ("SciTreeIcons/" + popupItem.name);
			popIcon.color = popupItem.displayColor;		

			if(popupItem.researched){
				popup.transform.Find("PopupTime").GetComponent<Text>().text = "Done.";
				popButtText.text = "Researched";
				popButt.interactable = false;
				return;
			}

			int completeTime = (int)(((popupItem.researchRequired - popupItem.researchProgress)/deltaResearchBranches[branchIndex]) / GM.ticks);

			if(popupItem.beingResearched){
				popButtText.text = "Researching...";
				popButt.interactable = false;
			}else{
				popButtText.text = "Research";
				popButt.interactable = true;
				popButt.onClick.RemoveAllListeners();
				popButt.onClick.AddListener(delegate {
					SwitchResearch(popupItem);
				});
			}
		}else{
			string ttitle = CensorString(popupItem.dispName);
			string tquote = CensorString(popupItem.quote);
			popup.transform.Find("PopupTitle").GetComponent<Text>().text = ttitle;
			popup.transform.Find("PopupTime").GetComponent<Text>().text = "???? ?? ?? ???";
			popup.transform.Find("PopupQuote").GetComponent<Text>().text = tquote;

			Image popIcon = nodeInst.transform.Find("Icon").GetComponent<Image>();
			popIcon.sprite = Resources.Load<Sprite> ("SciTreeIcons/Unknown");
			popIcon.color = popupItem.displayColor;
			popButtText.text = "?????????";
			popButt.interactable = false;
		}
	}

	string CensorString(string inStr){
		string tStr = "";
		for(int i = 0; i < inStr.Length; i++){
			if(inStr[i].ToString() == " ")
				tStr += " ";
			else
				tStr += "?";
		}
		return tStr;
	}

	void SwitchResearch(ResearchItem itemSwitch){
		int branchIndex = itemSwitch.transform.parent.GetSiblingIndex();
		ResearchItem prevItem = branchActive[branchIndex];
		if(prevItem){
			prevItem.beingResearched = false;
		}
		itemSwitch.beingResearched = true;
		branchActive[branchIndex] = itemSwitch;
		ClosePopup();
		CreatePopup(lastPopupIndex);
		UpdateUINodes();
	}

	public List<NodeDataObj> SerializableNodes(){
		List<NodeDataObj> tList = new List<NodeDataObj>();
		foreach(ResearchItem tItem in nodeList){
			NodeDataObj tObj = new NodeDataObj();
			tObj.researchProgress = tItem.researchProgress;
			tObj.researched = tItem.researched;
			tObj.persistUnlocked = tItem.persistUnlocked;
			tObj.beingResearched = tItem.beingResearched;
			tList.Add(tObj);
		}
		return tList;
	}

	public void ClosePopup(){
		if(popupBar)
			Destroy(popupBar.transform.parent.gameObject);
		popup.SetActive(false);
	}

	public void PopupUpdate(){
		if(popupItem.transform.Find("Icon").GetComponent<Image>().sprite.name != "Unknown"){
			if(popupItem.researched){
				popup.transform.Find("PopupTime").GetComponent<Text>().text = "Done.";
			}else{
				int branchIndex = popupItem.transform.parent.GetSiblingIndex();
				int completeTime = (int)(((popupItem.researchRequired - popupItem.researchProgress)/deltaResearchBranches[branchIndex]) / GM.ticks);
				//Debug.Log(deltaResearchBranches[1]);
				completeTime++;
				if(popupItem.beingResearched){
					popup.transform.Find("PopupTime").GetComponent<Text>().text = "Done in " + completeTime + " seconds";
				}else{
					popup.transform.Find("PopupTime").GetComponent<Text>().text = "Would take about " + completeTime + " seconds";
				}
			}
			popupBar.fillAmount = popupItem.researchProgress / popupItem.researchRequired;
		}
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
