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
	public Color[] BodyColors; 
	public Color[] EyeColors; 
	
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
			if(!GameManager.paused && villagers.Count > 0){

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

				if(tmpVillager.relationships == null)
					tmpVillager.relationships = new List<VillagerRelationship>();

				//Homeless
				if(tmpVillager.livesAt == null){
					if(tmpVillager.worksAt != null)
						FireVillager(tmpVillager);
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

					//mood/relationship stuff
					PhysicalStructure tmpLiveStruct = PStructFromIndex(tmpVillager.livesAt);
					if(tmpLiveStruct == null)
						continue;
					foreach(Villager tv in tmpLiveStruct.employeeList)
						if(tv != tmpVillager)
							CheckRelationship(tmpVillager, tv);
					
					if(tmpVillager.worksAt == null){
						tmpVillager.mood += 20f; //enemployed but housed buff
					}else{
						PhysicalStructure tmpWorkStruct = PStructFromIndex(tmpVillager.worksAt);
						if(tmpWorkStruct == null)
							continue;
						foreach(Villager tv in tmpWorkStruct.employeeList)
							if(tv != tmpVillager)
								CheckRelationship(tmpVillager, tv);						
					}
				}

                float avgRelationMood = 1;
                if(tmpVillager.relationships.Count > 0)
                    avgRelationMood = tmpVillager.relationships.Average(x => x.relationshipValue) * 10f - 10f;
				//Debug.Log(avgRelationMood);
				tmpVillager.mood += avgRelationMood;


				//end algorithm and ++ to next villager
				villagerI++;

			}
			yield return new WaitForEndOfFrame();
		}
	}

	public static void CheckRelationship(Villager vill1, Villager vill2){
		if(vill2.relationships == null)
			vill2.relationships = new List<VillagerRelationship>();
		VillagerRelationship relVill = vill1.relationships.Find(x => x.targetVillagerID == vill2.uniqueID);
		VillagerRelationship relVill2 = vill2.relationships.Find(x => x.targetVillagerID == vill1.uniqueID);
		float interestRating = InterestRating(vill1, vill2) * 2f / 10000f - 0.0001f;
		//Debug.Log(interestRating);
		if(relVill == null){
			relVill = new VillagerRelationship(vill2.uniqueID);
			relVill.relationshipValue = Mathf.Clamp(relVill.relationshipValue + interestRating, 0f, 2f);
			vill1.relationships.Add(relVill);
		}else{
			relVill.relationshipValue = Mathf.Clamp(relVill.relationshipValue + interestRating, 0f, 2f);
		}
		if(relVill2 == null){
			relVill2 = new VillagerRelationship(vill1.uniqueID);
			relVill2.relationshipValue = Mathf.Clamp(relVill2.relationshipValue + interestRating, 0f, 2f);
			vill2.relationships.Add(relVill2);
		}else{
			relVill2.relationshipValue = Mathf.Clamp(relVill2.relationshipValue + interestRating, 0f, 2f);
		}
	}

	static float InterestRating(Villager vill1, Villager vill2){
		float Similarness = 0;
		Similarness += ListPercentSimilar(vill1.foodList, vill2.foodList);
		//Similarness += ListPercentSimilar(vill1.skillList, vill2.skillList);
		Similarness /= 1f;
		return Similarness;
	}

	public static float ListPercentSimilar(List<ResourceManager.Resource> list1, List<ResourceManager.Resource> list2){
		int diffCount = 0;
		for(int i = 0; i < list1.Count; i++){
			var item1 = list1[i];
			int ti = -1;
			ResourceManager.Resource tr = list2.Find(x => x.name == item1.name);
			if(tr != null)
				ti = list2.IndexOf(tr);
			//Debug.Log(ti);
			if(ti != -1){
				diffCount += Mathf.Abs(i - ti);
			}
		}
		float difference = (float)diffCount / (float)MaxDiff(list1.Count);
		difference = 1f - difference;
		//Debug.Log(difference);
		return difference;
	}
	public static float ListPercentSimilar(List<VillagerSkill> list1, List<VillagerSkill> list2){
		int diffCount = 0;
		for(int i = 0; i < list1.Count; i++){
			var item1 = list1[i];
			int ti = -1;
			VillagerSkill tr = list2.Find(x => x.skillGroup == item1.skillGroup);
			if(tr != null)
				ti = list2.IndexOf(tr);
			Debug.Log(ti);
			if(ti != -1){
				diffCount += Mathf.Abs(i - ti);
			}
		}
		float difference = (float)diffCount / (float)MaxDiff(list1.Count);
		difference = 1f - difference;
		//Debug.Log(difference);
		return difference;
	}

	static int MaxDiff(int listCount){
		int counter = listCount - 1;
		for(int i = listCount - 2; i > 0; i--)
			counter += i;
		if(counter == 0)
			counter = 1;
		return counter;
	}

	public PhysicalStructure PStructFromIndex(string index){
		PhysicalStructure tps = null;
		try{
			tps = GM.builderHelper.pStructList.Find(x => x.transform.parent.name == index);
			//Debug.Log(index + " : " + tps.Count());
			return tps;
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
        float avg = 0;
        if(villagers.Count > 0) {
            try {
                avg = villagers.Average(x => x.mood);
            } catch(System.Exception ex) {
                GameManager.Print(villagers.Count);
                GameManager.PrintException(ex, "avg error");
            }
        } else {
            return 0;
        }

        return avg;
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
