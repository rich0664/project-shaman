using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VillagerManager : MonoBehaviour {

	public List<Villager> villagers;
	GameManager GM;

	public void StartUp () {
		GM = GetComponent<GameManager>();
		StartCoroutine(FreeWill());
		for(int i = 0; i < villagers.Count; i++){
			Villager tmpVillager = villagers[i];
			tmpVillager.Start();
		}
		GM.structureManager.RefreshHiringStructures();
	}

	int villagerI = 0;
	IEnumerator FreeWill(){
		while(true){
			if(!GM.paused){
				if(villagerI >= villagers.Count)
					villagerI = 0;
				Villager tmpVillager = villagers[villagerI];
				if(GM.structureManager.hiringStructures.Count != 0){
					int hiringCount = GM.structureManager.hiringStructures.Count;
					int bestSkill = Random.Range(0, hiringCount);
					if(tmpVillager.experienced){
						float prevSkillLevel = 1f;
						prevSkillLevel = tmpVillager.GetSkill(GM.structureManager.hiringStructures[bestSkill].name).skillLevel;
						for(int i = 0; i < hiringCount; i++){
							VillagerSkill tSkill = tmpVillager.GetSkill(GM.structureManager.hiringStructures[i].name);
							if(tSkill.skillLevel > prevSkillLevel){
								bestSkill = i;
								prevSkillLevel = tSkill.skillLevel;
							}
						}
					}

					StructureManager.Structure optimalStruct = GM.structureManager.GetStructure(GM.structureManager.hiringStructures[bestSkill].name);
					if(!tmpVillager.worksAt || tmpVillager.worksAt.structure.name != optimalStruct.name){
						if(tmpVillager.worksAt)
							FireVillager(tmpVillager);
						HireVillager(tmpVillager, optimalStruct);
						GM.structureManager.RefreshHiringStructures();
						GM.structureManager.CalculateAverages(optimalStruct);
						tmpVillager.experienced = true;
					}
				}
			}
			villagerI++;
			yield return new WaitForEndOfFrame();
		}
	}


	public void HireVillager(Villager villager, StructureManager.Structure struc){
		IEnumerable<PhysicalStructure> hiringList = GM.builderHelper.hiringPStructList.Where(x => x.structure == struc);
		if(hiringList.Count() > 0){
			PhysicalStructure hiringStruc = hiringList.First();
			villager.worksAt = hiringStruc;
			struc.workers.Add(villager);
			if(hiringStruc.employeeList.Count == 0)
				struc.activeAmount++;
			hiringStruc.employeeList.Add(villager);
			if(hiringStruc.employeeList.Count == struc.workerCapacity)
				struc.fullyActiveAmount++;
		}
	}
	public void FireVillager(Villager villager){
		StructureManager.Structure struc = villager.worksAt.structure;
		struc.workers.Remove(villager);
		if(villager.worksAt.employeeList.Count == struc.workerCapacity)
			struc.fullyActiveAmount--;
		villager.worksAt.employeeList.Remove(villager);
		if(villager.worksAt.employeeList.Count == 0)
			struc.activeAmount--;
		villager.worksAt = null;
	}


	public void AddVillagers(int amount){
		for(int i = 0; i < amount; i++){
			Villager tmpVillager = new Villager();
			villagers.Add(tmpVillager);
			tmpVillager.Start();
		}
	}

}
