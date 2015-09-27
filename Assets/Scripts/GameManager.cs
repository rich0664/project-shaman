using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public bool paused = false;
	public float ticks = 10f;
	float delay = 0.1f;

	public ResourceManager resourceManager;
	public StructureManager structureManager;
	public UIManager uiManager;
	public UnlockManager unlockManager;

	void Start (){
		resourceManager = GetComponent<ResourceManager> ();
		structureManager = GetComponent<StructureManager> ();
		uiManager = GetComponent<UIManager>();
		unlockManager = GetComponent<UnlockManager>();

		//LoadHere
		resourceManager.StartUp ();
		structureManager.StartUp ();
		unlockManager.StartUp();
		uiManager.StartUp();
		uiManager.UpdateUI();

		delay = 1f / ticks;
		StartCoroutine (GameLoop ());
	}

	IEnumerator GameLoop (){
		while(!paused) {
			yield return new WaitForSeconds (delay);
			
			foreach(ResourceManager.Resource res in resourceManager.resources) {
				res.sumStorage = res.baseStorage;
				res.contributors.activeProduction = 0f;
			}
			foreach(ResourceManager.Resource res in resourceManager.resources)
				resourceManager.DoTick(res);
			foreach(StructureManager.Structure struc in structureManager.structures)
				if(!struc.passiveStructure)
					structureManager.DoTick (struc);

			uiManager.UpdateUI();
		}
	}

	public void WerkHard(float value){
		ResourceManager.Resource tmpDough = resourceManager.GetResource("Dough");
		tmpDough.amount += value;
	}


	//end class
}
