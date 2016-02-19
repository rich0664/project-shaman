using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

	public float spawnChance = 0.1f;
	public float routineDelay = 0.5f;
	public float spawnRadius = 2.5f;
	public float villagerSize = 0.4f;
	public int spawnCap = 20;

	public List<Villager> outVillagers = new List<Villager>();
	public GameObject villagerParent;
	GameManager GM;

	// Use this for initialization
	public void StartUp () {
		GM = GetComponent<GameManager>();
		villagerParent = new GameObject(); villagerParent.name = "VillagerAiParent";
		StartCoroutine(AiRoutine());
	}

	IEnumerator AiRoutine(){
		int i = 0;
		GameObject villagerPref = Resources.Load<GameObject>("Villagers/Villager");
		while(true){
			yield return new WaitForSeconds(routineDelay);
			if(GM.builderHelper.pStructList.Count == 0)
				continue;
			if(GameManager.paused)
				continue;

			float randChance = Random.Range(0.0f, 1.0f);
			if(randChance < spawnChance){
				PhysicalStructure tStruct = GM.builderHelper.pStructList[i];
				int eCount = tStruct.employeeList.Count;
				if(eCount == 0)
					continue;
				int chooseVill = Random.Range(0, eCount);
				Villager tVill = tStruct.employeeList[chooseVill];
				if(outVillagers.Contains(tVill))
					continue;

				outVillagers.Add(tVill);
				GameObject villAInst = GameObject.Instantiate(villagerPref);
				villAInst.name = tVill.name;
				villAInst.transform.parent = villagerParent.transform;
				Vector3 spawnPos = tStruct.transform.position; spawnPos.x += Random.Range(-spawnRadius, spawnRadius); spawnPos.z += Random.Range(-spawnRadius, spawnRadius);
				villAInst.transform.position = spawnPos;
				villAInst.transform.localScale *= villagerSize;
				VillagerAI instAI = villAInst.GetComponent<VillagerAI>(); instAI.villager = tVill; instAI.jobStruct = tStruct; instAI.GM = GM;
				GM.ffManager.updateList.Add(villAInst.transform);
				GM.ffManager.rot = Quaternion.Euler(Vector3.zero);
			}

			i++;
			if(i >= GM.builderHelper.pStructList.Count)
				i = 0;
		}
	}



	public void ResetCameraTarget(){
		GM.gameCamera.target = GameObject.Find("Ground/target").transform;
	}

	//end class
}
