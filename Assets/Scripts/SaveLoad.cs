using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class SaveLoad : MonoBehaviour
{

	GameManager GM;
	Transform canvasObj;
	string savePref = "SaveData";

	// Use this for initialization
	public void StartUp (){
		GM = GetComponent<GameManager> ();
		canvasObj = GameObject.Find ("UICanvas").transform;
		//savePref = "DefaultData";
		//Save(false);
		savePref = "SaveData";
	}
	
	public void Save (bool quit){
		string dataString = "";

		//Villagers
		if(GM.villagerManager.villagers.Count > 0)
			Serializer.SaveObj("VillagerData", GM.villagerManager.villagers.ToList());

		//SciTree
		//Debug.Log(GM.sciTreeManager.nodeList.Count);
		if(GM.sciTreeManager.nodeList.Count > 0)
			Serializer.SaveObj("SciTreeData", GM.sciTreeManager.SerializableNodes());

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
			dataString += SaveString(struc.name + "ActiveMult", struc.activeMult.ToString());
			dataString += SaveString(struc.name + "ActiveCostMult", struc.activeCostMult.ToString());
			dataString += SaveString(struc.name + "Discovered", struc.discovered.ToString());
			dataString += SaveString(struc.name + "ActiveAmount", struc.activeAmount.ToString());
			dataString += SaveString(struc.name + "FullyActiveAmount", struc.fullyActiveAmount.ToString());
			dataString += SaveString(struc.name + "DemandAmount", struc.demandAmount.ToString());
			foreach(Cost cst in struc.costs){
				dataString += SaveString(struc.name + cst.resource + "ActiveCostAmount", cst.amount.ToString());
				dataString += SaveString(struc.name + cst.resource + "ActiveCostScaling", cst.scaling.ToString());
			}
		}

		//structureworkers 
		Serializer.SaveObj("WorkerData", GM.structureManager.structures.ToList());

		//Grid
		dataString += SaveString("GridRings", GM.gridManager.rings.ToString());

		/* Physicsal Structures
		dataString += SaveString("PStructCount", GM.builderHelper.pStructList.Count.ToString());
		int tC = 0;
		foreach(PhysicalStructure pS in GM.builderHelper.pStructList){
			dataString += SaveString("PStructType" + tC.ToString(), pS.structure.name);
			dataString += SaveString("PStructIndex" + tC.ToString(), pS.transform.parent.name);  tC++;
		}
		*/
		//if(GM.builderHelper.pStructData.ToList().Count > 0)
			//Debug.Log(GM.builderHelper.pStructData.ToList()[0].eList.Count);
		GM.builderHelper.pStructData.Clear();
		foreach(PhysicalStructure ps in GM.builderHelper.pStructList){
			pStructInfo psi = new pStructInfo();
			psi.eList = ps.employeeList;
			psi.pIndex = ps.spotIndex;
			psi.type = ps.structure.name;
			GM.builderHelper.pStructData.Add(psi);
		}
		if(GM.builderHelper.pStructList.Count > 0)
			Serializer.SaveObj("pStructData", GM.builderHelper.pStructData.ToList());

		//Game Settings
		dataString += SaveString("TiltShift", canvasObj.Find("GameMenu/Tilt").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("Vignette", canvasObj.Find("GameMenu/Vignette").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("InvertPan", canvasObj.Find("GameMenu/InvertPan").GetComponent<Toggle>().isOn.ToString());
		dataString += SaveString("Sensitivity", canvasObj.Find("GameMenu/SensitivitySlider").GetComponent<Slider>().value.ToString());
		dataString += SaveString("FoliageDensity", canvasObj.Find("GameMenu/FoliageSlider").GetComponent<Slider>().value.ToString());

		PlayerPrefs.SetString(savePref, dataString);
		PlayerPrefs.Flush();

		if (quit)
			Application.Quit ();
	}

	public void Load(bool loadDefault){
		if(!PlayerPrefs.HasKey("SaveData"))
			Save(false);

		//Villagers
		if(PlayerPrefs.HasKey("VillagerData"))
			GM.villagerManager.villagers = ((List<Villager>)Serializer.LoadObj("VillagerData")).ToHashSet();

		//scitree
		if(PlayerPrefs.HasKey("SciTreeData"))
			GM.sciTreeManager.nodeData = (List<NodeDataObj>)Serializer.LoadObj("SciTreeData");

		//worker data
		List<StructureManager.Structure> sList = (List<StructureManager.Structure>)Serializer.LoadObj("WorkerData");
		//structure settings
		foreach(StructureManager.Structure struc in GM.structureManager.structures){
			//StructureWorkers
			if(PlayerPrefs.HasKey("WorkerData")){
				StructureManager.Structure serialStruc = sList.Find(x => x.name == struc.name);
				if(serialStruc == null)
					continue;
				foreach(Villager serialVill in serialStruc.workers)
					struc.workers.Add(GM.villagerManager.villagers.Where(x => x.uniqueID == serialVill.uniqueID).First());				
			}
			float.TryParse(LoadString(struc.name + "Amount"), out struc.amount);
			foreach(Cost cst in struc.costs){
				float.TryParse(LoadString(struc.name + cst.resource + "CostAmount"), out cst.amount);
				float.TryParse(LoadString(struc.name + cst.resource + "CostScaling"), out cst.scaling);
			}
			float.TryParse(LoadString(struc.name + "ConstructTime"), out struc.constructTime);
			float.TryParse(LoadString(struc.name + "CostMultiplier"), out struc.costMultiplier);
			float.TryParse(LoadString(struc.name + "Multiplier"), out struc.multiplier);
			float.TryParse(LoadString(struc.name + "ActiveMult"), out struc.activeMult);
			float.TryParse(LoadString(struc.name + "ActiveCostMult"), out struc.activeCostMult);
			bool.TryParse(LoadString(struc.name + "Discovered"), out struc.discovered);;
			float.TryParse(LoadString(struc.name + "ActiveAmount"), out struc.activeAmount);
			float.TryParse(LoadString(struc.name + "FullyActiveAmount"), out struc.fullyActiveAmount);
			float.TryParse(LoadString(struc.name + "DemandAmount"), out struc.demandAmount);
			foreach(Cost cst in struc.costs){
				float.TryParse(LoadString(struc.name + cst.resource + "ActiveCostAmount"), out cst.amount);
				float.TryParse(LoadString(struc.name + cst.resource + "ActiveCostScaling"), out cst.scaling);
			}
			GM.structureManager.CalculateAverages(struc);
		}
		//resource settings
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
		if(tmpRings > 0){
			for(int i = 0; i < tmpRings - 1; i++)
				GM.gridManager.TryExpand(false);
			GM.gridManager.TryExpand(true);
		}

		//physical structures
		if(PlayerPrefs.HasKey("pStructData")){
			List<pStructInfo> pSI = (List<pStructInfo>)Serializer.LoadObj("pStructData");
			foreach(pStructInfo ps in pSI){
				int spotIndex = 0; int.TryParse(ps.pIndex, out spotIndex);
				GM.builderHelper.lastSpot = GM.builderHelper.spotList[spotIndex - 1];
				PhysicalStructure tmpPStruct = GM.builderHelper.QuickBuild(ps.type, true);
				foreach(Villager vill in ps.eList)
					tmpPStruct.employeeList.Add(GM.villagerManager.villagers.Where(x => x.uniqueID == vill.uniqueID).First());
			}
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


	public void RESET(bool hardReset = false){
		if(!hardReset){
			GM.sciTreeManager.ResetTreeProgress();
			Serializer.SaveObj("SciTreeData", GM.sciTreeManager.SerializableNodes());
		}else
			PlayerPrefs.DeleteKey("SciTreeData");
		PlayerPrefs.DeleteKey("SaveData");
		PlayerPrefs.DeleteKey("VillagerData");
		PlayerPrefs.DeleteKey("pStructData");
		PlayerPrefs.DeleteKey("WorkerData");
		PlayerPrefs.Flush();
		Application.LoadLevel("Main");
	}

	string SaveString(string name, string value){
		return "<" + name + ">" + value + "</" + name + ">";
	}

	string LoadString(string varToGet){
		string tmpData = PlayerPrefs.GetString(savePref);
		string sString = "<" + varToGet + ">";

		string eString = "</" + varToGet + ">";
		int sI = tmpData.IndexOf(sString);
		if(sI == -1)
			return null;
		sI += sString.Length;
		int eI = tmpData.IndexOf(eString); 
		return tmpData.Substring(sI, eI - sI);
	}


}
