using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructureManager : MonoBehaviour {

	[System.Serializable]
	public class Structure {
		public string name;
		public float amount;
		public List<Effect> effects;
		public List<Cost> costs;

		[Header("MiscSettings")]
		public float costMultiplier = 1f;
		public float multiplier = 1f;
		public bool discovered;
		public string description;
		public string footNote;

		[Header("ActiveSettings")]
		public bool passiveStructure = true;
		public float activeAmount;
		public float activeMult = 1f;
		public float activeCostMult = 1f;
		public List<Effect> activeEffects;
		public List<Cost> activeCosts;

	}

	public void DoTick(Structure struc){
		foreach(Cost cst in struc.activeCosts){
			ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (cst.resource);
			if(tmpRes.amount < (cst.amount * struc.amount / GM.ticks) * struc.activeCostMult)
				return;
			tmpRes.amount -= (cst.amount * struc.amount / GM.ticks) * struc.activeCostMult;
			tmpRes.contributors.activeProduction -= (cst.amount * struc.amount) * struc.activeCostMult;
		}
		
		foreach(Effect effct in struc.activeEffects) {
			int enumIndex = (int)effct.effectType;
			if(effct.effectTarget == Effect.targetMode.Resource) {
				ResourceManager.Resource tmpRes = GM.resourceManager.GetResource (effct.targetName);
				switch (enumIndex) {
					case 1: //Produce
						tmpRes.amount += (effct.effectValue * struc.activeAmount / GM.ticks) * struc.activeMult;
						tmpRes.contributors.activeProduction += (effct.effectValue * struc.activeAmount) * struc.activeMult;
						break;
					case 3: //SumStorage
						tmpRes.sumStorage += (effct.effectValue * struc.activeAmount) * struc.activeMult;
						break;
				}
			}
		}
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

	public void BuyStructure (string structToBuy){
		Structure struc = GetStructure (structToBuy);
		foreach(Cost cst in struc.costs)
			GM.resourceManager.GetResource (cst.resource).amount -= (cst.amount * struc.costMultiplier) + (struc.amount * cst.scaling);
		struc.amount++;
		struc.activeAmount++;
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
