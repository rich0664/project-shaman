using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
				VillagerSkill optimalSkill = tmpVillager.GetSkill(GM.structureManager.hiringStructures[bestSkill].name);
				if(tmpVillager.worksAt.structure.name != optimalSkill.structName){
					//if(job)
						//quit job
					//hire this villager
				}

			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void AddVillagers(int amount){
		for(int i = 0; i < amount; i++){
			Villager tmpVillager = new Villager();
			villagers.Add(tmpVillager);
			tmpVillager.Start();
		}
	}

}
