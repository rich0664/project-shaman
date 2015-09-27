using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	GameManager GM;
	GameObject canvasObj;

	public void StartUp(){
		GM = GetComponent<GameManager>();
		canvasObj = GameObject.Find("UICanvas");
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

	void UpdateResources(){
		Transform resources = canvasObj.transform.Find("Resources");
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
		Transform structures = canvasObj.transform.Find("Structures");
		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			if(!structures.Find(struc.name))
				continue;
			GameObject tmpStrucUI = structures.Find(struc.name).gameObject;
			tmpStrucUI.SetActive(struc.discovered);

			Button buyBtn = tmpStrucUI.transform.Find("BuyButton").GetComponent<Button>();
			buyBtn.interactable = GM.structureManager.CanBuyStructure(struc);

			Text nameText = tmpStrucUI.transform.Find("BuyButton/Text").GetComponent<Text>();
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

	public void StructureTooltip(string str){
		if(toolTip)
			Destroy(toolTip);
		lastTooltip = str;
		StructureManager.Structure tmpStruc = GM.structureManager.GetStructure(str);
		Transform templates = canvasObj.transform.Find("Templates");
		GameObject resTemplate = templates.Find("StructureTooltip").gameObject;
		toolTip = GameObject.Instantiate(resTemplate);
		toolTip.transform.SetParent(templates);
		toolTip.SetActive(true);
		toolTip.name = "StructureTooltip";
		toolTip.transform.localScale = Vector3.one;

		int costCount = tmpStruc.costs.Count;
		int effectCount = tmpStruc.effects.Count + tmpStruc.activeEffects.Count + tmpStruc.activeCosts.Count;
		RectTransform rekt = toolTip.GetComponent<RectTransform>();
		rekt.sizeDelta = new Vector2(200f, 90f + 20f * (costCount + effectCount));
		LayoutElement lec = toolTip.transform.Find("Panel/CostHead").GetComponent<LayoutElement>();
		LayoutElement lee = toolTip.transform.Find("Panel/EffectsHead").GetComponent<LayoutElement>();
		lec.minHeight += 20f * costCount;
		lee.minHeight += 20f * effectCount;

		Text DescText = toolTip.transform.Find("Panel/DescHead/DescriptionText").GetComponent<Text>();
		DescText.text = tmpStruc.description;
		Text FootText = toolTip.transform.Find("Panel/EffectsHead/FootnoteText").GetComponent<Text>();
		FootText.text = tmpStruc.footNote;
		Text CostName = toolTip.transform.Find("Panel/CostHead/CostName").GetComponent<Text>();
		CostName.text = "";
		Text CostAmount = toolTip.transform.Find("Panel/CostHead/CostAmount").GetComponent<Text>();
		CostAmount.text = "";
		Text EffectsText = toolTip.transform.Find("Panel/EffectsHead/EffectsText").GetComponent<Text>();
		EffectsText.text = "";
		foreach(Cost cst in tmpStruc.costs){
			ResourceManager.Resource tmpRes = GM.resourceManager.GetResource(cst.resource);
			string tmpStr = "";
			if(tmpRes.amount >= (cst.amount * tmpStruc.costMultiplier) + (tmpStruc.amount * cst.scaling)){
				tmpStr = ((cst.amount * tmpStruc.costMultiplier) + (tmpStruc.amount * cst.scaling)).ToString("F2");
			}else{
				tmpStr = tmpRes.amount.ToString("F2") + " / " + ((cst.amount * tmpStruc.costMultiplier) + (tmpStruc.amount * cst.scaling)).ToString("F2");
				CostAmount.color = Color.red;
			}
			CostName.text += cst.resource + "\n";
			CostAmount.text += tmpStr + "\n";
		}
		foreach(Effect effct in tmpStruc.effects){
			EffectsText.text += effct.targetName + ": " + effct.effectValue + "\n";
		}
		foreach(Effect effct in tmpStruc.activeEffects){
			EffectsText.text += effct.targetName + ": " + effct.effectValue + "\n";
		}
		foreach(Cost cst in tmpStruc.activeCosts){
			EffectsText.text += cst.resource + ": " + -cst.amount + "\n";
		}

		toolTip.transform.position = canvasObj.transform.Find("Structures/" + str + "/InfoButton").position + new Vector3(10f,0f,0f);
	}

	public void KillTooltip(bool str){
		if(str){
			if(toolTip)
				Destroy(toolTip);
		}
	}

}
