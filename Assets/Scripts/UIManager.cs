﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

	public bool isMenu = false;
	GameManager GM;
	GameObject canvasObj;
	Animator ShamanMenuAnimator;
	bool isShamanMenuOpen;
	bool shouldCheckMenu;
	GameObject ShamanMenu;
	EventSystem MES;

	public void StartUp(){
		MES = GameObject.Find("EventSystem").GetComponent<EventSystem>();
		GM = GetComponent<GameManager>();
		canvasObj = GameObject.Find("UICanvas");
		ShamanMenuAnimator = canvasObj.transform.Find("ShamanMenu").GetComponent<Animator>();

		RectTransform structRekt = canvasObj.transform.Find("Structures/ScrollRect/StructureList").GetComponent<RectTransform>();
		GameObject StrucTemplate = structRekt.Find("Template").gameObject;
		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			GameObject strucGO = GameObject.Instantiate(StrucTemplate);
			strucGO.name = struc.name;
			strucGO.transform.SetParent(structRekt);
			strucGO.transform.localScale = Vector3.one;
			strucGO.SetActive(struc.discovered);
			Button button = strucGO.transform.Find("Struct/Button").GetComponent<Button>();
			button.onClick.AddListener(delegate { StructureTooltip(strucGO.name); });
		}
		ShamanMenu = canvasObj.transform.Find("ShamanMenu").gameObject;
	}

	void Update(){

		if(Input.GetMouseButtonUp(0) && isShamanMenuOpen)
			shouldCheckMenu = true;

		if(shouldCheckMenu){
			shouldCheckMenu = false;
			isShamanMenuOpen = !isShamanMenuOpen;
			ShamanMenuAnimator.SetBool("Open", isShamanMenuOpen);
		}
	}

	public void UpdateUI(){
		UpdateResources();
		UpdateStructures();
		if(toolTip)
			if(toolTip.name == "ResourceTooltip"){
				ResourceTooltip(lastTooltip);
			}else if(toolTip.name == "StructureTooltip"){
				StructureTooltip(lastTooltip);
			}
	}

	public void ToggleMenu(string str){
		isMenu = !isMenu;
		Camera.main.GetComponent<BlurOptimized>().enabled = isMenu;

		canvasObj.transform.Find(str).gameObject.SetActive(isMenu);
		if(str == "Structures"){
			if(isMenu)
				StructureTooltip("Drug Farm");
		}else if(str == "Leadership"){
			MES.SetSelectedGameObject(
				canvasObj.transform.Find(str + "/Tabs/Resources/Button").gameObject);
		}
		ShamanMenu.SetActive(!isMenu); 
	}

	void UpdateResources(){
		Transform resources = canvasObj.transform.Find("Leadership/Resources");
		int items = 0;
		foreach(ResourceManager.Resource res in GM.resourceManager.resources){
			if(!resources.Find(res.name))
				continue;
			GameObject tmpResUI = resources.Find(res.name).gameObject;
			tmpResUI.SetActive(res.discovered);
			if(res.discovered)
				items++;
			Text nameText = tmpResUI.transform.Find("Name").GetComponent<Text>();
			nameText.color = res.dispColor;
			nameText.text = res.name + ": ";
			Text amountText = tmpResUI.transform.Find("Amount").GetComponent<Text>();
			amountText.text = res.amount.ToString("F2") + "/" + res.sumStorage;
			Text rateText = tmpResUI.transform.Find("Rate").GetComponent<Text>();
			rateText.text = res.contributors.ToRate();
		}
		RectTransform vlg = resources.GetComponent<RectTransform>();
		vlg.sizeDelta = new Vector2(275f, 30 * items);
	}

	void UpdateStructures(){
		Transform structures = canvasObj.transform.Find("Structures/ScrollRect/StructureList");
		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			if(!structures.Find(struc.name))
				continue;
			GameObject tmpStrucUI = structures.Find(struc.name).gameObject;
			tmpStrucUI.SetActive(struc.discovered);

			//Button buyBtn = tmpStrucUI.transform.Find("BuyButton").GetComponent<Button>();
			//buyBtn.interactable = GM.structureManager.CanBuyStructure(struc);

			Text nameText = tmpStrucUI.transform.Find("Struct/Text").GetComponent<Text>();
			nameText.text = struc.name + " (" + struc.amount + ")";
		}
	}

	GameObject toolTip;
	string lastTooltip;
	public void ResourceTooltip(string str){
		if(toolTip)
			Destroy(toolTip);
		lastTooltip = str;
		ResourceManager.Resource tmpRes = GM.resourceManager.GetResource(str);
		Transform templates = canvasObj.transform.Find("Templates");
		GameObject resTemplate = templates.Find("ResourceTooltip").gameObject;
		toolTip = GameObject.Instantiate(resTemplate);
		toolTip.transform.SetParent(templates);
		toolTip.name = "ResourceTooltip";
		toolTip.SetActive(true);
		toolTip.transform.localScale = Vector3.one;
		Text tipText = toolTip.transform.Find("Panel/Text").GetComponent<Text>();
		tipText.text = tmpRes.contributors.ToTooltip();

		RectTransform rekt = toolTip.GetComponent<RectTransform>();
		rekt.sizeDelta = new Vector2(150f, 10f + 20f * tmpRes.contributors.activeContributors());
		rekt.position = canvasObj.transform.Find("Resources/" + str + "/Rate").position + new Vector3(10f,0f,0f);
	}

	public void cool(string wat){
		Debug.Log(wat);
	}

	public void StructureTooltip(string str){
		if(str != lastTooltip)
			DestroyImmediate(toolTip);
		if(toolTip){
			Transform lech = toolTip.transform.Find("CostHead/CostInfo");
			Transform leeh = toolTip.transform.Find("EffectHead/EffectInfo");
			for(int i = 0; i < lech.childCount; i++)
				DestroyImmediate(lech.GetChild(1).gameObject);
			for(int i = 0; i < leeh.childCount; i++)
				DestroyImmediate(leeh.GetChild(1).gameObject);
		}else{
			lastTooltip = str;
			Transform structs = canvasObj.transform.Find("Structures");
			GameObject resTemplate = structs.Find("SctructInfoTemplate").gameObject;
			toolTip = GameObject.Instantiate(resTemplate);
			RectTransform rekt = toolTip.GetComponent<RectTransform>();
			toolTip.transform.SetParent(structs);
			toolTip.SetActive(true);
			toolTip.name = "StructureTooltip";
			toolTip.transform.localScale = Vector3.one;
			rekt.offsetMin = new Vector2(rekt.offsetMin.x, 0f);
			rekt.offsetMax = new Vector2(rekt.offsetMax.x, 0f);
			rekt.anchoredPosition = Vector2.zero;
		}

		StructureManager.Structure tmpStruc = GM.structureManager.GetStructure(str);
		int costCount = tmpStruc.costs.Count;
		int effectCount = tmpStruc.effects.Count + tmpStruc.activeEffects.Count + tmpStruc.activeCosts.Count;
		Transform lch = toolTip.transform.Find("CostHead/CostInfo");
		Transform leh = toolTip.transform.Find("EffectHead/EffectInfo");
		GameObject costemplate = lch.Find("Template").gameObject;
		GameObject effectemplate = leh.Find("Template").gameObject;
		Button buyButton = toolTip.transform.Find("StructIcon").GetComponent<Button>();
		buyButton.onClick.RemoveAllListeners();
		buyButton.onClick.AddListener(delegate { GM.structureManager.BuyStructure(str); });
		buyButton.interactable = GM.structureManager.CanBuyStructure(tmpStruc);

		Text NameText = toolTip.transform.Find("NameText").GetComponent<Text>();
		NameText.text = tmpStruc.name;
		Text DescText = toolTip.transform.Find("DescText").GetComponent<Text>();
		DescText.text = tmpStruc.description;
		Text FootText = toolTip.transform.Find("FootText").GetComponent<Text>();
		FootText.text = tmpStruc.footNote;
		foreach(Cost cst in tmpStruc.costs){
			GameObject cstInst = GameObject.Instantiate(costemplate);
			cstInst.transform.SetParent(lch);
			cstInst.SetActive(true);
			cstInst.transform.localScale = Vector3.one;
			Image prcnt = cstInst.transform.Find("Cost/PercentBar").GetComponent<Image>();
			Image costIcon = cstInst.transform.Find("Cost/PercentBar/CostButton").GetComponent<Image>();
			//Debug.Log(Resources.Load("ResourceIcons/" + cst.resource));
			costIcon.sprite = Resources.Load<Sprite>("ResourceIcons/" + cst.resource);

			ResourceManager.Resource tmpRes = GM.resourceManager.GetResource(cst.resource);
			string tmpStr = "";
			float totalCost = (cst.amount * tmpStruc.costMultiplier) + (tmpStruc.amount * cst.scaling);
			if(tmpRes.amount >= totalCost){
				tmpStr = totalCost.ToString("F2");
				prcnt.fillAmount = 1f;
			}else{
				tmpStr = tmpRes.amount.ToString("F2") + " / " + totalCost.ToString("F2");
				prcnt.fillAmount = tmpRes.amount / totalCost;
				prcnt.color = Color.red;
			}
		}
		foreach(Effect effct in tmpStruc.effects){
			int enumIndex = (int)effct.effectType;
			string prefix = "+"; string suffix = "/sec";
			if(enumIndex == 2 || enumIndex == 6)
				suffix = "%";
			if(effct.effectValue < 0)
				prefix = "-";
			GameObject effctInst = GameObject.Instantiate(effectemplate);
			effctInst.transform.SetParent(leh);
			effctInst.SetActive(true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find("Effect/Image").GetComponent<Image>();
			Text amntText = effctInst.transform.Find("Effect/AmountText").GetComponent<Text>();
			Text dscText = effctInst.transform.Find("Effect/DescText").GetComponent<Text>();
			if(effct.effectTarget == Effect.targetMode.Resource){
				Color resCol = GM.resourceManager.GetResource(effct.targetName).dispColor;
				amntText.color = resCol;
				dscText.color = resCol;
				effectIcon.sprite = Resources.Load<Sprite>("ResourceIcons/" + effct.targetName);
			}

			amntText.text = prefix + effct.effectValue.ToString("F2") + suffix;
			dscText.text = effct.targetName;
		}
		foreach(Effect effct in tmpStruc.activeEffects){
			int enumIndex = (int)effct.effectType;
			string prefix = "+"; string suffix = "/sec";
			if(enumIndex == 2 || enumIndex == 6)
				suffix = "%";
			if(enumIndex == 3)
				suffix = "";
			if(effct.effectValue < 0)
				prefix = "-";
			GameObject effctInst = GameObject.Instantiate(effectemplate);
			effctInst.transform.SetParent(leh);
			effctInst.SetActive(true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find("Effect/Image").GetComponent<Image>();
			Text amntText = effctInst.transform.Find("Effect/AmountText").GetComponent<Text>();
			Text dscText = effctInst.transform.Find("Effect/DescText").GetComponent<Text>();
			if(effct.effectTarget == Effect.targetMode.Resource){
				Color resCol = GM.resourceManager.GetResource(effct.targetName).dispColor;
				amntText.color = resCol;
				dscText.color = resCol;
				effectIcon.sprite = Resources.Load<Sprite>("ResourceIcons/" + effct.targetName);
			}


			amntText.text = prefix + effct.effectValue.ToString("F2") + suffix;
			dscText.text = effct.targetName;
			if(enumIndex == 3)
				dscText.text += " Storage";
		}
		foreach(Cost cst in tmpStruc.activeCosts){
			string prefix = "-"; string suffix = "/sec";
			GameObject effctInst = GameObject.Instantiate(effectemplate);
			effctInst.transform.SetParent(leh);
			effctInst.SetActive(true);
			effctInst.transform.localScale = Vector3.one;
			Image effectIcon = effctInst.transform.Find("Effect/Image").GetComponent<Image>();
			Text amntText = effctInst.transform.Find("Effect/AmountText").GetComponent<Text>();
			Text dscText = effctInst.transform.Find("Effect/DescText").GetComponent<Text>();
			Color resCol = GM.resourceManager.GetResource(cst.resource).dispColor;
			amntText.color = resCol;
			dscText.color = resCol;
			effectIcon.sprite = Resources.Load<Sprite>("ResourceIcons/" + cst.resource);


			amntText.text = prefix + cst.amount.ToString("F2") + suffix;
			dscText.text = cst.resource;
		}
	}

	public void KillTooltip(bool str){
		if(str){
			if(toolTip)
				Destroy(toolTip);
		}
	}

	public void ToggleShamanMenu(bool str){
		if(str){
			shouldCheckMenu = true;
		}
	}

}
