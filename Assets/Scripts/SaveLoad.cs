using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SaveLoad : MonoBehaviour
{

	GameManager GM;
	Transform canvasObj;
	string savePref = "SaveData";

	// Use this for initialization
	public void StartUp (){
		GM = GetComponent<GameManager> ();
		canvasObj = GameObject.Find ("UICanvas").transform;
		savePref = "DefaultData";
		Save(false);
		savePref = "SaveData";
	}
	
	public void Save (bool quit){
		string dataString = "";

		foreach(ResourceManager.Resource res in GM.resourceManager.resources){
			dataString += SaveString(res.name + "Amount", res.amount.ToString());
			dataString += SaveString(res.name + "BaseProduct", res.contributors.baseProduction.ToString());
			dataString += SaveString(res.name + "WorkerProduct", res.contributors.workerProduction.ToString());
			dataString += SaveString(res.name + "UpgradeMult", res.contributors.upgradeMult.ToString());
			dataString += SaveString(res.name + "StructureMult", res.contributors.structureMult.ToString());
			dataString += SaveString(res.name + "ActiveMult", res.contributors.activeProduction.ToString());
			dataString += SaveString(res.name + "Discovered", res.discovered.ToString());
		}
		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			dataString += SaveString(struc.name + "Amount", struc.amount.ToString());
			foreach(Cost cst in struc.costs){
				dataString += SaveString(struc.name + cst.resource + "CostAmount", cst.amount.ToString());
				dataString += SaveString(struc.name + cst.resource + "CostScaling", cst.scaling.ToString());
			}
			dataString += SaveString(struc.name + "ConstructTime", struc.constructTime.ToString());
			dataString += SaveString(struc.name + "CostMultiplier", struc.costMultiplier.ToString());
			dataString += SaveString(struc.name + "Multiplier", struc.multiplier.ToString());
			dataString += SaveString(struc.name + "ActiveAmount", struc.activeAmount.ToString());
			dataString += SaveString(struc.name + "ActiveMult", struc.activeMult.ToString());
			dataString += SaveString(struc.name + "ActiveCostMult", struc.activeCostMult.ToString());
			dataString += SaveString(struc.name + "Discovered", struc.discovered.ToString());
			foreach(Cost cst in struc.costs){
				dataString += SaveString(struc.name + cst.resource + "ActiveCostAmount", cst.amount.ToString());
				dataString += SaveString(struc.name + cst.resource + "ActiveCostScaling", cst.scaling.ToString());
			}
		}

		//Grid
		dataString += SaveString("GridRings", GM.gridManager.rings.ToString());

		//Physicsal Structures
		dataString += SaveString("PStructCount", GM.builderHelper.pStructList.Count.ToString());
		int tC = 0;
		foreach(PhysicalStructure pS in GM.builderHelper.pStructList){
			dataString += SaveString("PStructType" + tC.ToString(), pS.structure.name);
			dataString += SaveString("PStructIndex" + tC.ToString(), pS.transform.parent.name);  tC++;
		}
		//for(int i = 0; i < GM.builderHelper.pStructList.Count; i++){
		//	PhysicalStructure pS = GM.builderHelper.pStructList[i];
		//	dataString += SaveString("PStructType" + i.ToString(), pS.structure.name);
		//	dataString += SaveString("PStructIndex" + i.ToString(), pS.transform.parent.name);
		//}

		//Game Settings
		dataString += SaveString("TiltShift", canvasObj.Find("GameMenu/Tilt").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("Vignette", canvasObj.Find("GameMenu/Vignette").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("InvertPan", canvasObj.Find("GameMenu/InvertPan").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("Sensitivity", canvasObj.Find("GameMenu/SensitivitySlider").GetComponent<Slider>().value.ToString());
		dataString += SaveString("FoliageDensity", canvasObj.Find("GameMenu/FoliageSlider").GetComponent<Slider>().value.ToString());

		PlayerPrefs.SetString(savePref, dataString);
		PlayerPrefs.Save();

		if (quit)
			Application.Quit ();
	}

	public void Load(bool loadDefault){
		if(!PlayerPrefs.HasKey("SaveData"))
			Save(false);

		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			float.TryParse(LoadString(struc.name + "Amount"), out struc.amount);
			foreach(Cost cst in struc.costs){
				float.TryParse(LoadString(struc.name + cst.resource + "CostAmount"), out cst.amount);
				float.TryParse(LoadString(struc.name + cst.resource + "CostScaling"), out cst.scaling);
			}
			float.TryParse(LoadString(struc.name + "ConstructTime"), out struc.constructTime);
			float.TryParse(LoadString(struc.name + "CostMultiplier"), out struc.costMultiplier);
			float.TryParse(LoadString(struc.name + "Multiplier"), out struc.multiplier);
			float.TryParse(LoadString(struc.name + "ActiveAmount"), out struc.activeAmount);
			float.TryParse(LoadString(struc.name + "ActiveMult"), out struc.activeMult);
			float.TryParse(LoadString(struc.name + "ActiveCostMult"), out struc.activeCostMult);
			bool.TryParse(LoadString(struc.name + "Discovered"), out struc.discovered);;
			foreach(Cost cst in struc.costs){
				float.TryParse(LoadString(struc.name + cst.resource + "ActiveCostAmount"), out cst.amount);
				float.TryParse(LoadString(struc.name + cst.resource + "ActiveCostScaling"), out cst.scaling);
			}
		}
		foreach(ResourceManager.Resource res in GM.resourceManager.resources){
			float.TryParse(LoadString(res.name + "Amount"), out res.amount);
			float.TryParse(LoadString(res.name + "BaseProduct"), out res.contributors.baseProduction);
			float.TryParse(LoadString(res.name + "WorkerProduct"), out res.contributors.workerProduction);
			float.TryParse(LoadString(res.name + "UpgradeMult"), out res.contributors.upgradeMult);
			float.TryParse(LoadString(res.name + "StructureMult"), out res.contributors.structureMult);
			float.TryParse(LoadString(res.name + "ActiveMult"), out res.contributors.activeProduction);
			bool.TryParse(LoadString(res.name + "Discovered"), out res.discovered);
		}


		//Grid
		GM.gridManager.rings = 0;
		int tmpRings = 0; int.TryParse(LoadString("GridRings"), out tmpRings);
		for(int i = 0; i < tmpRings; i++){
			GM.gridManager.TryExpand(true);
		}

		//Physicsal Structures
		int psCount = 0; int.TryParse(LoadString("PStructCount"), out psCount);
		for(int i = 0; i < psCount; i++){
			int spotIndex = 0; int.TryParse(LoadString("PStructIndex" + i.ToString()), out spotIndex);
			GM.builderHelper.lastSpot = GM.builderHelper.spotList[spotIndex - 1];
			GM.builderHelper.QuickBuild(LoadString("PStructType" + i.ToString()), true);
		}

		//Game Settings
		bool tmpTlt = false; bool.TryParse(LoadString("TiltShift") , out tmpTlt);
		bool tmpVig = false; bool.TryParse(LoadString("Vignette") , out tmpVig);
		bool tmpInP = false; bool.TryParse(LoadString("InvertPan") , out tmpInP);
		float tmpPaS = -1f; float.TryParse(LoadString("Sensitivity") , out tmpPaS);
		float tmpFoD = -1f; float.TryParse(LoadString("FoliageDensity") , out tmpFoD);
		canvasObj.Find("GameMenu/Tilt").GetComponent<Toggle>().isOn = tmpTlt;
		canvasObj.Find("GameMenu/Vignette").GetComponent<Toggle>().isOn = tmpVig;
		canvasObj.Find("GameMenu/InvertPan").GetComponent<Toggle>().isOn = tmpInP;
		if(tmpPaS != -1f)
			canvasObj.Find("GameMenu/SensitivitySlider").GetComponent<Slider>().value = tmpPaS;
		if(tmpFoD != -1f)
			canvasObj.Find("GameMenu/FoliageSlider").GetComponent<Slider>().value = tmpFoD;
	}


	public void RESET(){
		PlayerPrefs.DeleteKey("SaveData");
		PlayerPrefs.Save();
		Application.LoadLevel("Main");
	}

	string SaveString(string name, string value){
		return "<" + name + ">" + value + "</" + name + ">";
	}

	string LoadString(string varToGet){
		string tmpData = PlayerPrefs.GetString(savePref);
		string sString = "<" + varToGet + ">";
		string eString = "</" + varToGet + ">";
		int sI = tmpData.IndexOf(sString) + sString.Length;
		int eI = tmpData.IndexOf(eString); 
		return tmpData.Substring(sI, eI - sI);
	}


}
