using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

	public bool isMenu = false;
	GameManager GM;
	GameObject canvasObj;
	Animator ShamanMenuAnimator;
	bool isShamanMenuOpen;
	bool shouldCheckMenu;
	GameObject ShamanMenu;
	EventSystem MES;

	public void StartUp ()
	{
		MES = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();
		GM = GetComponent<GameManager> ();
		canvasObj = GameObject.Find ("UICanvas");
		ShamanMenuAnimator = canvasObj.transform.Find ("ShamanMenu").GetComponent<Animator> ();

		RectTransform structRekt = canvasObj.transform.Find ("Structures/ScrollRect/StructureList").GetComponent<RectTransform> ();
		GameObject StrucTemplate = structRekt.Find ("Template").gameObject;
		foreach (StructureManager.Structure struc in GM.structureManager.structures) {
			GameObject strucGO = GameObject.Instantiate (StrucTemplate);
			strucGO.name = struc.name;
			strucGO.transform.SetParent (structRekt);
			strucGO.transform.localScale = Vector3.one;
			strucGO.SetActive (struc.discovered);
			Sprite strucIcon = Resources.Load<Sprite> ("BuildingIcons/" + strucGO.name);
			strucGO.transform.Find ("Struct/Image").GetComponent<Image> ().sprite = strucIcon;
			Button button = strucGO.transform.Find ("Struct/Button").GetComponent<Button> ();
			button.onClick.AddListener (delegate {
				StructureTooltip (strucGO.name);
			});
		}
		ShamanMenu = canvasObj.transform.Find ("ShamanMenu").gameObject;
	}

	void Update ()
	{

		if (Input.GetMouseButtonUp (0) && isShamanMenuOpen)
			shouldCheckMenu = true;

		if (shouldCheckMenu) {
			shouldCheckMenu = false;
			isShamanMenuOpen = !isShamanMenuOpen;
			ShamanMenuAnimator.SetBool ("Open", isShamanMenuOpen);
		}
	}

	public void UpdateUI ()
	{
		if (toolTip)
		if (toolSwitch == 3)
			QuickBuildTooltip ();

		if (!isMenu)
			return;

		UpdateResources ();
		UpdateStructures ();

		if (toolTip)
			switch (toolSwitch) {
			case 1: // resourece
				ResourceTooltip (lastTooltip);				
				break;
			case 2: //structure
				StructureTooltip (lastTooltip);
				break;
			}
	}

	public void ToggleMenu (string str)
	{
		isMenu = !isMenu;
		Camera.main.GetComponent<Blur> ().enabled = isMenu;

		canvasObj.transform.Find (str).gameObject.SetActive (isMenu);
		if (str == "Structures") {
			if (isMenu){
				StructureTooltip ("Drug Farm");
			}else{
				KillTooltip(true);
			}
		} else if (str == "Leadership") {
			MES.SetSelectedGameObject (
				canvasObj.transform.Find (str + "/Tabs/Resources/Button").gameObject);
				KillTooltip(true);
		} else if (str == "GameMenu") {
			KillTooltip(true);
		}
		ShamanMenu.SetActive (!isMenu); 
	}

	void UpdateResources ()
	{
		Transform resources = canvasObj.transform.Find ("Leadership/Resources");
		int items = 0;
		foreach (ResourceManager.Resource res in GM.resourceManager.resources) {
			if (!resources.Find (res.name))
				continue;
			GameObject tmpResUI = resources.Find (res.name).gameObject;
			tmpResUI.SetActive (res.discovered);
			if (res.discovered)
				items++;
			Text nameText = tmpResUI.transform.Find ("Name").GetComponent<Text> ();
			nameText.color = res.dispColor;
			nameText.text = res.name + ": ";
			Text amountText = tmpResUI.transform.Find ("Amount").GetComponent<Text> ();
			amountText.text = res.amount.ToString ("F2") + "/" + res.sumStorage;
			Text rateText = tmpResUI.transform.Find ("Rate").GetComponent<Text> ();
			rateText.text = res.contributors.ToRate ();
		}
		RectTransform vlg = resources.GetComponent<RectTransform> ();
		vlg.sizeDelta = new Vector2 (275f, 30 * items);
	}

	void UpdateStructures ()
	{
		Transform structures = canvasObj.transform.Find ("Structures/ScrollRect/StructureList");
		foreach (StructureManager.Structure struc in GM.structureManager.structures) {
			if (!structures.Find (struc.name))
				continue;
			GameObject tmpStrucUI = structures.Find (struc.name).gameObject;
			tmpStrucUI.SetActive (struc.discovered);

			//Button buyBtn = tmpStrucUI.transform.Find("BuyButton").GetComponent<Button>();
			//buyBtn.interactable = GM.structureManager.CanBuyStructure(struc);

			Text nameText = tmpStrucUI.transform.Find ("Struct/Text").GetComponent<Text> ();
			nameText.text = struc.name + " (" + struc.amount + ")";
		}
	}

	[HideInInspector]
	public GameObject toolTip;
	public string lastTooltip;
	public int toolSwitch = 0;
	List<string> couldBuyList;

	public void StructInfo(string structType){
		if(GM.builderHelper.isPlacing)
			return;
		if(toolTip)
			Destroy(toolTip);
		lastTooltip = "StructInfo";
		toolSwitch = 4;
		Transform templates = canvasObj.transform.Find ("Templates");
		GameObject sInfoTemp = templates.Find ("StructInfo").gameObject;
		toolTip = GameObject.Instantiate (sInfoTemp);
		toolTip.transform.SetParent (templates);
		toolTip.name = "StructInfoTooltip";
		toolTip.SetActive (true);
		toolTip.transform.localScale = Vector3.one;
		toolTip.transform.localPosition = Vector3.zero;
		Sprite strucIcon = Resources.Load<Sprite> ("BuildingIcons/" + structType);
		toolTip.transform.Find ("StructIcon").GetComponent<Image> ().sprite = strucIcon;
	}

	public void QuickBuildTooltip ()
	{
		if(GM.builderHelper.isPlacing)
			return;
		if (toolTip)
		if (toolTip.name == "QuickBuildTooltip") {
			List<string> canBuyList = new List<string> ();
			for (int i = 0; i < GM.structureManager.structures.Count; i++) {
				StructureManager.Structure tmpStruc = GM.structureManager.structures [i];
				if (GM.structureManager.CanBuyStructure (tmpStruc) && tmpStruc.discovered)
					canBuyList.Add (tmpStruc.name);
			}
			if (canBuyList.Count == couldBuyList.Count) {
				bool shouldReset = false;
				for (int q = 0; q < canBuyList.Count; q++) {
					//Debug.Log(canBuyList[q] + " : " + couldBuyList[q]);
					if (canBuyList [q] != couldBuyList [q]) {
						shouldReset = true;
						break;
					}
				}
				if (!shouldReset)
					return;
			}
			DestroyImmediate (toolTip);
		}else{
			Destroy(toolTip);
		}
		lastTooltip = "QuickBuild";
		toolSwitch = 3;
		Transform templates = canvasObj.transform.Find ("Templates");
		GameObject quickTemp = templates.Find ("QuickBuild").gameObject;
		toolTip = GameObject.Instantiate (quickTemp);
		toolTip.transform.SetParent (templates);
		toolTip.name = "QuickBuildTooltip";
		toolTip.SetActive (true);
		toolTip.transform.localScale = Vector3.one;
		toolTip.transform.localPosition = Vector3.zero;
		Transform container = toolTip.transform.Find ("ScrollSnap/Container");
		GameObject bTemp = toolTip.transform.Find ("Buildings").gameObject;
		GameObject icoTemp = toolTip.transform.Find ("Buildings/Holder").gameObject;
		int count = 4;
		GameObject bInst = null;
		couldBuyList = new List<string> ();
		for (int i = 0; i < GM.structureManager.structures.Count; i++) {
			if (count >= 4) {
				count = 0;
				bInst = GameObject.Instantiate (bTemp);
				bInst.transform.SetParent (container);
				bInst.SetActive (true);
				bInst.transform.localScale = Vector3.one;
				toolTip.transform.Find ("ScrollSnap").GetComponent<ScrollSnapRect> ().Reset ();
			}
			StructureManager.Structure tmpStruc = GM.structureManager.structures [i];
			if (GM.structureManager.CanBuyStructure (tmpStruc) && tmpStruc.discovered) {
				GameObject icoInst = GameObject.Instantiate (icoTemp);
				icoInst.transform.SetParent (bInst.transform);
				icoInst.SetActive (true);
				icoInst.transform.localScale = Vector3.one;
				Sprite strucIcon = Resources.Load<Sprite> ("BuildingIcons/" + tmpStruc.name);
				icoInst.transform.Find ("Image").GetComponent<Image> ().sprite = strucIcon;
				Button button = icoInst.transform.Find ("Image").GetComponent<Button> ();
				button.onClick.AddListener (delegate {
					GM.builderHelper.BuildOnSpot (tmpStruc.name);
				});
				couldBuyList.Add (tmpStruc.name);
				count++;
			}
		}
	}

	public void ResourceTooltip (string str)
	{
		if (toolTip)
			Destroy (toolTip);
		lastTooltip = str;
		toolSwitch = 1;
		ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (str);
		Transform templates = canvasObj.transform.Find ("Templates");
		GameObject resTemplate = templates.Find ("ResourceTooltip").gameObject;
		toolTip = GameObject.Instantiate (resTemplate);
		toolTip.transform.SetParent (templates);
		toolTip.name = "ResourceTooltip";
		toolTip.SetActive (true);
		toolTip.transform.localScale = Vector3.one;
		Text tipText = toolTip.transform.Find ("Panel/Text").GetComponent<Text> ();
		tipText.text = tmpRes.contributors.ToTooltip ();

		RectTransform rekt = toolTip.GetComponent<RectTransform> ();
		rekt.sizeDelta = new Vector2 (150f, 10f + 20f * tmpRes.contributors.activeContributors ());
		rekt.position = canvasObj.transform.Find ("Leadership/Resources/" + str + "/Rate").position + new Vector3 (10f, 0f, 0f);
	}

	public void cool (string wat)
	{
		Debug.Log (wat);
	}

	public void StructureTooltip (string str)
	{
		if (str != lastTooltip)
			DestroyImmediate (toolTip);
		toolSwitch = 2;
		if (toolTip) {
			Transform lech = toolTip.transform.Find ("CostHead/CostInfo");
			Transform leeh = toolTip.transform.Find ("EffectHead/EffectInfo");
			for (int i = 0; i < lech.childCount; i++)
				DestroyImmediate (lech.GetChild (1).gameObject);
			for (int i = 0; i < leeh.childCount; i++)
				DestroyImmediate (leeh.GetChild (1).gameObject);
		} else {
			lastTooltip = str;
			Transform structs = canvasObj.transform.Find ("Structures");
			GameObject resTemplate = structs.Find ("SctructInfoTemplate").gameObject;
			toolTip = GameObject.Instantiate (resTemplate);
			RectTransform rekt = toolTip.GetComponent<RectTransform> ();
			toolTip.transform.SetParent (structs);
			toolTip.SetActive (true);
			toolTip.name = "StructureTooltip";
			toolTip.transform.localScale = Vector3.one;
			rekt.offsetMin = new Vector2 (rekt.offsetMin.x, 0f);
			rekt.offsetMax = new Vector2 (rekt.offsetMax.x, 0f);
			rekt.anchoredPosition = Vector2.zero;
			Image strucIcon = toolTip.transform.Find ("StructIcon").GetComponent<Image> ();
			strucIcon.sprite = Resources.Load<Sprite> ("BuildingSprites/" + str);
			GM.builderHelper.RefreshEmptySpots();
		}
		StructureManager.Structure tmpStruc = GM.structureManager.GetStructure (str);
		Slider batchSlider = toolTip.transform.Find ("Slider").GetComponent<Slider> ();
		batchSlider.maxValue = GM.structureManager.CountCanBuy(tmpStruc);
		if(batchSlider.maxValue == 0){
			batchSlider.minValue = 0;}else
			batchSlider.minValue = 1;
		toolTip.transform.Find ("Slider/ValueText").GetComponent<Text> ().text = batchSlider.value.ToString();
		toolTip.transform.Find ("Slider/MaxText").GetComponent<Text> ().text = batchSlider.maxValue.ToString();
		int costCount = tmpStruc.costs.Count;
		int effectCount = tmpStruc.effects.Count + tmpStruc.activeEffects.Count + tmpStruc.activeCosts.Count;
		Transform lch = toolTip.transform.Find ("CostHead/CostInfo");
		Transform leh = toolTip.transform.Find ("EffectHead/EffectInfo");
		GameObject costemplate = lch.Find ("Template").gameObject;
		GameObject effectemplate = leh.Find ("Template").gameObject;
		Button buyButton = toolTip.transform.Find ("StructIcon").GetComponent<Button> ();
		if(GM.builderHelper.emptySpots.Count == 0){
			buyButton.interactable = false;
		}else{
			buyButton.interactable = GM.structureManager.CanBuyStructure (tmpStruc);
		}

		Text NameText = toolTip.transform.Find ("NameText").GetComponent<Text> ();
		NameText.text = tmpStruc.name;
		Text DescText = toolTip.transform.Find ("DescText").GetComponent<Text> ();
		DescText.text = tmpStruc.description;
		Text FootText = toolTip.transform.Find ("FootText").GetComponent<Text> ();
		FootText.text = tmpStruc.footNote;
		foreach (Cost cst in tmpStruc.costs) {
			GameObject cstInst = GameObject.Instantiate (costemplate);
			cstInst.transform.SetParent (lch);
			cstInst.SetActive (true);
			cstInst.transform.localScale = Vector3.one;
			Image prcnt = cstInst.transform.Find ("Cost/PercentBar").GetComponent<Image> ();
			Image costIcon = cstInst.transform.Find ("Cost/PercentBar/CostButton").GetComponent<Image> ();
			//Debug.Log(Resources.Load("ResourceIcons/" + cst.resource));
			costIcon.sprite = Resources.Load<Sprite> ("ResourceIcons/" + cst.resource);

			ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (cst.resource);
			string tmpStr = "";
			float totalCost = (cst.amount * tmpStruc.costMultiplier) + (tmpStruc.amount * cst.scaling);
			if (tmpRes.amount >= totalCost) {
				tmpStr = totalCost.ToString ("F2");
				prcnt.fillAmount = 1f;
			} else {
				tmpStr = tmpRes.amount.ToString ("F2") + " / " + totalCost.ToString ("F2");
				prcnt.fillAmount = tmpRes.amount / totalCost;
				prcnt.color = Color.red;
			}
		}
		foreach (Effect effct in tmpStruc.effects) {
			int enumIndex = (int)effct.effectType;
			string prefix = "+";
			string suffix = "/sec";
			if (enumIndex == 2 || enumIndex == 6)
				suffix = "%";
			if (effct.effectValue < 0)
				prefix = "-";
			GameObject effctInst = GameObject.Instantiate (effectemplate);
			effctInst.transform.SetParent (leh);
			effctInst.SetActive (true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find ("Effect/Image").GetComponent<Image> ();
			Text amntText = effctInst.transform.Find ("Effect/AmountText").GetComponent<Text> ();
			Text dscText = effctInst.transform.Find ("Effect/DescText").GetComponent<Text> ();
			if (effct.effectTarget == Effect.targetMode.Resource) {
				Color resCol = GM.resourceManager.GetResource (effct.targetName).dispColor;
				amntText.color = resCol;
				dscText.color = resCol;
				effectIcon.sprite = Resources.Load<Sprite> ("ResourceIcons/" + effct.targetName);
			}

			amntText.text = prefix + effct.effectValue.ToString ("F2") + suffix;
			dscText.text = effct.targetName;
		}
		foreach (Effect effct in tmpStruc.activeEffects) {
			int enumIndex = (int)effct.effectType;
			string prefix = "+";
			string suffix = "/sec";
			if (enumIndex == 2 || enumIndex == 6)
				suffix = "%";
			if (enumIndex == 3)
				suffix = "";
			if (effct.effectValue < 0)
				prefix = "-";
			GameObject effctInst = GameObject.Instantiate (effectemplate);
			effctInst.transform.SetParent (leh);
			effctInst.SetActive (true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find ("Effect/Image").GetComponent<Image> ();
			Text amntText = effctInst.transform.Find ("Effect/AmountText").GetComponent<Text> ();
			Text dscText = effctInst.transform.Find ("Effect/DescText").GetComponent<Text> ();
			if (effct.effectTarget == Effect.targetMode.Resource) {
				Color resCol = GM.resourceManager.GetResource (effct.targetName).dispColor;
				amntText.color = resCol;
				dscText.color = resCol;
				effectIcon.sprite = Resources.Load<Sprite> ("ResourceIcons/" + effct.targetName);
			}


			amntText.text = prefix + effct.effectValue.ToString ("F2") + suffix;
			dscText.text = effct.targetName;
			if (enumIndex == 3)
				dscText.text += " Storage";
		}
		foreach (Cost cst in tmpStruc.activeCosts) {
			string prefix = "-";
			string suffix = "/sec";
			GameObject effctInst = GameObject.Instantiate (effectemplate);
			effctInst.transform.SetParent (leh);
			effctInst.SetActive (true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find ("Effect/Image").GetComponent<Image> ();
			Text amntText = effctInst.transform.Find ("Effect/AmountText").GetComponent<Text> ();
			Text dscText = effctInst.transform.Find ("Effect/DescText").GetComponent<Text> ();
			Color resCol = GM.resourceManager.GetResource (cst.resource).dispColor;
			amntText.color = resCol;
			dscText.color = resCol;
			effectIcon.sprite = Resources.Load<Sprite> ("ResourceIcons/" + cst.resource);


			amntText.text = prefix + cst.amount.ToString ("F2") + suffix;
			dscText.text = cst.resource;
		}
	}

	public void KillTooltip (bool str)
	{
		if (str) {
			if (toolTip)
				Destroy (toolTip);
		}
	}

	public void ToggleShamanMenu (bool str)
	{
		if (str) {
			shouldCheckMenu = true;
		}
	}

}
