using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StructureManager : MonoBehaviour {

	[System.Serializable]
	public class Structure {
		public string name;
		public string displayName;
		public float amount;
		public structType structCategory;
		public bool isHouse;
		public List<Effect> effects;
		public List<Cost> costs;

		[Header("Misc Settings")]
		public float constructTime = 1f;
		public float costMultiplier = 1f;
		public float multiplier = 1f;
		public bool discovered;
		public string description;
		public string footNote;

		[Header("Active Settings")]
		public bool passiveStructure = true;
		public float activeMult = 1f;
		public float activeCostMult = 1f;
		public List<Effect> activeEffects;
		public List<Cost> activeCosts;

		[Header("Worker Settings")]
		public int workerCapacity = 1;
		public float skillLearnRate = 1.1f;
		public float demandAmount;
		public float activeAmount;
		public float fullyActiveAmount;
		public float averageCapacity;
		public float averageSkill;
		public List<Villager> workers;
		//[System.NonSerialized] 
		//public List<PhysicalStructure> pStructs;
		public enum structType{
			House = 1,
			Farming = 2
		}
	}
	
	public void DoTick(Structure struc){

		//fire the worst people to keep up with demand
		if(struc.demandAmount < struc.activeAmount){
			if(struc.workers.Count > 0){
				float worst = 100f;
				Villager worstVill = new Villager();
				foreach(Villager vill in struc.workers){
					float tSkill = vill.GetSkill(struc.name).skillLevel;
					if(tSkill < worst){
						worst = tSkill;
						worstVill = vill;
					}
				}
				GM.villagerManager.FireVillager(worstVill, struc.isHouse);
				GM.shouldRefreshStructs = true;
				CalculateAverages(struc);
			}
		}//else if(struc.activeAmount < struc.demandAmount){
		//	RefreshHiringStructures();
		//}

		foreach(Cost cst in struc.activeCosts){
			ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (cst.resource);
			if(tmpRes.amount < (cst.amount * struc.activeAmount / GM.ticks) * struc.activeCostMult)
				return;
			tmpRes.amount -= (cst.amount * struc.activeAmount / GM.ticks) * struc.activeCostMult;
			tmpRes.contributors.activeProduction -= (cst.amount * struc.activeAmount) * struc.activeCostMult;
		}
		
		foreach(Effect effct in struc.activeEffects) {
			int enumIndex = (int)effct.effectType;
			if(effct.effectTarget == Effect.targetMode.Resource) {
				ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (effct.targetName);
				switch (enumIndex) {
					case 1: //Produce
						//tmpRes.amount += (effct.effectValue * struc.activeAmount / GM.ticks) * struc.activeMult;
						tmpRes.contributors.activeProduction += effct.effectValue * struc.amount * struc.activeMult * struc.averageCapacity * struc.averageSkill;
						break;
					case 3: //SumStorage
						tmpRes.sumStorage += (effct.effectValue * struc.activeAmount) * struc.activeMult;
						break;
				}
			}
		}
	}

	[HideInInspector] public List<Structure> hiringStructures;
	public void RefreshHiringStructures(){
		hiringStructures = structures.Where(x => !x.isHouse && x.fullyActiveAmount < x.demandAmount).ToList();
		GM.builderHelper.hiringPStructList = GM.builderHelper.pStructList.Where(x => x.structure.workerCapacity != x.employeeList.Count).ToHashSet();
		//Debug.Log(hiringStructures.Count);
		//Debug.Log(GM.builderHelper.hiringPStructList.Count);
		/*hiringStructures.Clear();
		GM.builderHelper.pStructList.Where();
		foreach(Structure struc in structures){
			if(struc.fullyActiveAmount < struc.demandAmount)
				hiringStructures.Add(struc);
		}
		*/
	}

	public void CalculateAverages(Structure struc){
		float totalSkill = 0f;
		if(struc.workers.Count != 0){
			foreach(Villager vill in struc.workers){
				totalSkill += vill.GetSkill(struc.structCategory.ToString()).skillLevel;
			}
			struc.averageSkill = totalSkill / struc.workers.Count;
		}else{
			struc.averageSkill = totalSkill;
		}
		struc.averageCapacity = struc.workers.Count / (struc.workerCapacity * struc.amount);
	}

	public Structure GetStructure (string structToGet){
		return structureDictionary [structToGet];
	}

	public bool CanBuyStructure (Structure struc){
		bool tmpCanBuy = true;
		foreach(Cost cst in struc.costs)
			if(GM.resourceManager.GetResource (cst.resource).amount < (cst.amount * struc.costMultiplier) + (struc.amount * cst.scaling))
				tmpCanBuy = false;
		return tmpCanBuy;
	}

	public int CountCanBuy(Structure struc){
		int count = 0; bool first = true; 
		int maxCount = GM.builderHelper.emptySpots.Count;
		foreach(Cost cst in struc.costs){
			if(cst.scaling != 0f){
				float fakeAmount = GM.resourceManager.GetResource (cst.resource).amount;
				bool hasRes = true; int buyCount = 0; int actCount = (int)struc.amount;
				while(hasRes){
					if(buyCount >= maxCount)
						break;
					fakeAmount -= cst.amount * struc.costMultiplier + actCount * cst.scaling;
					if(fakeAmount <= 0)
					{hasRes = false;}
					else {buyCount++; actCount++;}
				}
				count = buyCount;
			}else{
				int tmpC = Mathf.FloorToInt(GM.resourceManager.GetResource (cst.resource).amount
					/ cst.amount * struc.costMultiplier);
				if(first){ first = false; count = tmpC; }
				else if(tmpC < count) count = tmpC;
			}
			if(count > maxCount)
				count = maxCount;
		} return count;
	}

	public void BuyStructure (string structToBuy){
		Structure struc = GetStructure (structToBuy);
		foreach(Cost cst in struc.costs)
			GM.resourceManager.GetResource (cst.resource).amount -= (cst.amount * struc.costMultiplier) + (struc.amount * cst.scaling);
		struc.amount++;
	}

	public void BuildStructure(string structToBuy){
		Structure struc = GetStructure (structToBuy);
		struc.demandAmount++;
		struc.demandAmount = Mathf.Clamp(struc.demandAmount, 0f, struc.amount);
		foreach(Effect effct in struc.effects) {
			int enumIndex = (int)effct.effectType;
			float tmpEffectValue = effct.effectValue;
			if(effct.effectTarget == Effect.targetMode.Resource) {
				ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (effct.targetName);
				switch (enumIndex) {
				case 1: //Produce
					tmpRes.contributors.baseProduction += tmpEffectValue * struc.multiplier;
					break;
				case 2: //Multiply
					tmpRes.contributors.structureMult += tmpEffectValue * struc.multiplier;
					break;
				}
			} else if(effct.effectTarget == Effect.targetMode.Structure) {
				StructureManager.Structure tmpStruc = GM.structureManager.GetStructure (effct.targetName);
				switch (enumIndex) {
				case 2: //Multiply
					tmpStruc.multiplier += tmpEffectValue * struc.multiplier;
					break;
				case 4: //Cost
					tmpStruc.costMultiplier += tmpEffectValue * struc.multiplier;
					break;
				case 5: //ActiveCost
					tmpStruc.activeCostMult += tmpEffectValue * struc.multiplier;
					break;
				case 6: //ActiveMult
					tmpStruc.activeMult += tmpEffectValue * struc.multiplier;
					break;
				}
			}
		}
		RefreshHiringStructures();
	}

	public List<Structure> structures;
	GameManager GM;
	Dictionary<string, Structure> structureDictionary;

	public void StartUp (){
		//Add each resource name to a dictionary for easy lookup;
		structureDictionary = new Dictionary<string, Structure> ();
		foreach(Structure struc in structures)
			structureDictionary [struc.name] = struc;

		GM = GetComponent<GameManager> ();
	}

	//end class
}
