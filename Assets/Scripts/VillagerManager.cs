using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VillagerManager : MonoBehaviour {

	public float skillRate = 0.01f;
	public float foodMult = 0.01f;
	public float foodConsumptionRate = 1f;
	public float averageMood = 100f;
	public HashSet<Villager> villagers;
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

				if(villagerI >= villagers.Count)
					villagerI = 0;
				Villager tmpVillager = villagers.ElementAt(villagerI);
				tmpVillager.mood = 100f;

				//skill progression
				if(tmpVillager.worksAt != null){
					float deltaSkillTime = (Time.time - tmpVillager.timeStamp) * skillRate;
					string jobGroup = PStructFromIndex(tmpVillager.worksAt).structure.structCategory.ToString();
					if(jobGroup == tmpVillager.talentGroup)
						deltaSkillTime *= tmpVillager.talentednes;
					tmpVillager.skillList.Find(x => x.skillGroup == jobGroup).skillLevel += deltaSkillTime;
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
					tmpVillager.worksAt = null;
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
		return GM.builderHelper.pStructList.Where(x => x.transform.parent.name == index).First();
	}


	public void VillagerMunch(Villager eatVillager){
		int wat = eatVillager.foodList.Count / 2;
		float deltaFood = (Time.time - eatVillager.timeStamp) * foodMult * foodConsumptionRate;
		foreach(ResourceManager.Resource foodle in eatVillager.foodList){
			wat--;
			if(foodle.amount <= 0)
				continue;
			float tAmount = foodle.amount; //amount eaten
			GM.resourceManager.AddFood(foodle, -deltaFood);
			tAmount -= foodle.amount;
			deltaFood -= tAmount;
			eatVillager.mood += tAmount * wat;
			foreach(FoodEffect fect in foodle.foodEffects){
				int enumIndex = (int)fect.foodEffectType;
				switch (enumIndex){
				case 1: //HealthBuff
					
					break;
				case 2: //MoodBuff
					
					break;
				}
			}
			if(deltaFood <= 0)
				break;
		}
		if(deltaFood > 0){
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
		if(!housing)
			tmpPStruct = PStructFromIndex(villager.worksAt);
		else
			tmpPStruct = PStructFromIndex(villager.livesAt);

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
