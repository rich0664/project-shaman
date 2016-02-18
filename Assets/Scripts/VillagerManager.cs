using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VillagerManager : MonoBehaviour {

	public float skillRate = 0.01f;
	public float MoodMult = 5f;
	public float foodMult = 0.01f;
	public float foodConsumptionRate = 1f;
	public float averageMood = 100f;
	public HashSet<Villager> villagers = new HashSet<Villager>();
	public List<Villager> villagersDebug = new List<Villager>();
	GameManager GM;
	int iDentifier = 0;
	
	public void StartUp () {
		GM = GetComponent<GameManager>();
		StartCoroutine(FreeWill());
		foreach(Villager vill in villagers){
			if(vill.skillList.Count == 0){
				vill.Start();
				vill.uniqueID = iDentifier;
				iDentifier++;
			}
		}
		GM.structureManager.RefreshHiringStructures();
	}

	int villagerI = 0;
	IEnumerator FreeWill(){
		while(true){
			if(!GM.paused && villagers.Count > 0){

				villagersDebug = villagers.ToList();

				if(villagerI >= villagers.Count)
					villagerI = 0;
				Villager tmpVillager = villagers.ElementAt(villagerI);
				tmpVillager.mood = 100f;

				//skill progression
				if(tmpVillager.worksAt != null){
					float deltaSkillTime = (Time.time - tmpVillager.timeStamp) * skillRate;
					try{
						string jobGroup = PStructFromIndex(tmpVillager.worksAt).structure.structCategory.ToString();
						if(jobGroup == tmpVillager.talentGroup)
							deltaSkillTime *= tmpVillager.talentednes;
						tmpVillager.skillList.Find(x => x.skillGroup == jobGroup).skillLevel += deltaSkillTime;
					}catch{
						//yield return new WaitForEndOfFrame();
						//continue;
					}
				}

				VillagerMunch(tmpVillager);
				tmpVillager.timeStamp = Time.time;

				//housing stuff
				if(tmpVillager.livesAt == null){
					StructureManager.Structure housingStruc = GM.structureManager.structures.Find(x => x.isHouse && x.fullyActiveAmount < x.demandAmount);
					if(housingStruc != null){
						HireVillager(tmpVillager, housingStruc);
						GM.structureManager.RefreshHiringStructures();
						GM.structureManager.CalculateAverages(housingStruc);
					}
				}

				//Homeless
				if(tmpVillager.livesAt == null){
					FireVillager(tmpVillager);
					//tmpVillager.worksAt = null;
					tmpVillager.mood -= 20f; //homeless debuff
				}else{
					//job stuff
					if(GM.structureManager.hiringStructures.Count != 0){
						int hiringCount = GM.structureManager.hiringStructures.Count;
						int bestSkill = Random.Range(0, hiringCount);
						if(tmpVillager.experienced){
							float prevSkillLevel = 1f;
							prevSkillLevel = tmpVillager.GetSkill(GM.structureManager.hiringStructures[bestSkill].structCategory.ToString()).skillLevel;
							for(int i = 0; i < hiringCount; i++){
								VillagerSkill tSkill = tmpVillager.GetSkill(GM.structureManager.hiringStructures[i].structCategory.ToString());
								if(tSkill.skillLevel > prevSkillLevel){
									bestSkill = i;
									prevSkillLevel = tSkill.skillLevel;
							}
							}
						}
						StructureManager.Structure optimalStruct = GM.structureManager.GetStructure(GM.structureManager.hiringStructures[bestSkill].name);
						if(tmpVillager.worksAt == null || PStructFromIndex(tmpVillager.worksAt).structure.name != optimalStruct.name){
							if(tmpVillager.worksAt != null)
								FireVillager(tmpVillager);
							HireVillager(tmpVillager, optimalStruct);
							GM.structureManager.RefreshHiringStructures();
							GM.structureManager.CalculateAverages(optimalStruct);
							tmpVillager.experienced = true;
						}
					}
					if(tmpVillager.worksAt == null){
						tmpVillager.mood += 20f; //enemployed but housed buff
					}
				}


				//end algorithm and ++ to next villager
				villagerI++;

			}
			yield return new WaitForEndOfFrame();
		}
	}

	PhysicalStructure PStructFromIndex(string index){
		IEnumerable<PhysicalStructure> tps = null;
		try{
			tps = GM.builderHelper.pStructList.Where(x => x.transform.parent.name == index);
			//Debug.Log(index + " : " + tps.Count());
			return tps.First();
		}catch(System.Exception e){
			Debug.Log(index + " - WAT - " + e);
		}
		return null;
	}


	public void VillagerMunch(Villager eatVillager){
		int wat = eatVillager.foodList.Count;
		wat -= wat / 2;
		float deltaFood = (Time.time - eatVillager.timeStamp) * foodMult * foodConsumptionRate;
		foreach(ResourceManager.Resource foodle in eatVillager.foodList){
			if(foodle.amount <= 0f){
				wat--;
				continue;
			}
			ResourceManager.Resource realFoodle = GM.resourceManager.GetResource(foodle.name);
			float tAmount = realFoodle.amount; //amount eaten
			tAmount -= deltaFood;
			if(tAmount < 0f)
				tAmount = 0f;
			tAmount = realFoodle.amount - tAmount;
			realFoodle.amountEaten += tAmount;
			eatVillager.mood += (tAmount / deltaFood)* MoodMult * wat;
			deltaFood -= tAmount;
			wat--;
			foreach(FoodEffect fect in realFoodle.foodEffects){
				int enumIndex = (int)fect.foodEffectType;
				switch (enumIndex){
				case 1: //HealthBuff
					
					break;
				case 2: //MoodBuff
					
					break;
				}
			}
			if(deltaFood <= 0f)
				break;
		}
		if(deltaFood > 0f){
			eatVillager.health -= deltaFood;
		}
	}

	public float MoodAverage(){
		//float tMood = 0f;
		//foreach(Villager mVill in villagers)
			//tMood += mVill.mood;
		float tMood = villagers.Sum(x => x.mood);
		return tMood / (float)villagers.Count;
	}

	public void HireVillager(Villager villager, StructureManager.Structure struc){
		IEnumerable<PhysicalStructure> hiringList = GM.builderHelper.hiringPStructList.Where(x => x.structure == struc);
		if(hiringList.Count() > 0){
			PhysicalStructure hiringStruc = hiringList.First();
			if(struc.isHouse){
				villager.livesAt = hiringStruc.spotIndex;
			}else{
				villager.worksAt = hiringStruc.spotIndex;
			}
			struc.workers.Add(villager);
			if(hiringStruc.employeeList.Count == 0)
				struc.activeAmount++;
			hiringStruc.employeeList.Add(villager);
			if(hiringStruc.employeeList.Count == struc.workerCapacity)
				struc.fullyActiveAmount++;
		}
	}

	public void FireVillager(Villager villager, bool housing = false){
		PhysicalStructure tmpPStruct;

		if(!housing){
			if(villager.worksAt == null)
				return;
			tmpPStruct = PStructFromIndex(villager.worksAt);
		}else
			tmpPStruct = PStructFromIndex(villager.livesAt);

		if(tmpPStruct == null)
			return;
		StructureManager.Structure struc = tmpPStruct.structure;
		struc.workers.Remove(villager);
		if(tmpPStruct.employeeList.Count == struc.workerCapacity)
			struc.fullyActiveAmount--;
		tmpPStruct.employeeList.Remove(villager);
		if(tmpPStruct.employeeList.Count == 0)
			struc.activeAmount--;
		if(housing){
			villager.livesAt = null;
		}else{
			villager.worksAt = null;
		}
	}

	public void AddVillagers(int amount){
		for(int i = 0; i < amount; i++){
			Villager tmpVillager = new Villager();
			villagers.Add(tmpVillager);
			tmpVillager.name = "Villager " + villagers.Count.ToString();
			tmpVillager.GM = GM;
			tmpVillager.Start();
			tmpVillager.uniqueID = iDentifier;
			iDentifier++;
		}
	}

}
