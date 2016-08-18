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

				tVill.SetColors();
				outVillagers.Add(tVill);
				GameObject villAInst = GameObject.Instantiate(villagerPref);
				villAInst.name = tVill.name;
				villAInst.transform.parent = villagerParent.transform;
				Vector3 spawnPos = tStruct.transform.position; spawnPos.x += Random.Range(0f, spawnRadius) + spawnRadius;
				villAInst.transform.position = spawnPos;
				villAInst.transform.RotateAround(tStruct.transform.position, Vector3.up, Random.Range(0, 360));
				//villAInst.transform.localScale = new Vector3(villagerSize * tVill.width, villagerSize * tVill.height, 1f);
				villAInst.transform.localScale = new Vector3(villagerSize, villagerSize, 1f);
				VillagerAI instAI = villAInst.GetComponent<VillagerAI>(); instAI.villager = tVill; instAI.jobStruct = tStruct; instAI.GM = GM;
				instAI.StartUp();
				GM.ffManager.updateList.Add(villAInst.transform);
				GM.ffManager.rot = Quaternion.Euler(Vector3.zero);
			}

			i++;
			if(i >= GM.builderHelper.pStructList.Count)
				i = 0;
		}
	}
		
	public float targetDistance = 5.0f;
	public float overZoom = 0f;
	public float x = 0.0f;
	public float y = 0.0f;
	public float distance = 5f;
	public Vector3 targOffset = Vector3.zero;

	public bool resetTarget = false;

	public void SetCameraTarget(Transform target){
		targetDistance = GM.gameCamera.targetDistance;
		overZoom = GM.gameCamera.overZoom;
		x = GM.gameCamera.x;
		y = GM.gameCamera.y;
		distance = GM.gameCamera.distance;
		targOffset = GM.gameCamera.targOffset;

		GM.gameCamera.targetDistance = 90f;
		//GM.gameCamera.distance = 5f;
		//GM.gameCamera.overZoom = 0f;
		GM.gameCamera.transform.position = target.position;
		GM.gameCamera.transform.LookAt(GM.gameCamera.target);
		GM.gameCamera.x = GM.gameCamera.transform.eulerAngles.y;
		GM.gameCamera.y = 15f;
		GM.gameCamera.targOffset = (Vector3.zero - target.position).normalized * 9f;

		GM.uiManager.isMenu = true;
		resetTarget = true;
		GM.gameCamera.target = target;
	}

	public void ResetCameraTarget(){
		Debug.Log(1);

		GM.gameCamera.targetDistance = targetDistance;
		GM.gameCamera.distance = distance;
		GM.gameCamera.overZoom = overZoom;
		GM.gameCamera.x = x;
		GM.gameCamera.y = y;
		GM.gameCamera.targOffset = targOffset;

		GM.uiManager.isMenu = false;
		resetTarget = false;
		GM.gameCamera.target = GameObject.Find("Ground/target").transform;
	}

	//end class
}
