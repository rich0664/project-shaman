using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public bool paused = false;
	public float ticks = 10f;


	public ResourceManager resourceManager;
	public StructureManager structureManager;
	public UIManager uiManager;
	public UnlockManager unlockManager;
	public BuilderHelper builderHelper;

	void Start (){
		QualitySettings.vSyncCount = 0;  // VSync must be disabled
		Application.targetFrameRate = 30;

		resourceManager = GetComponent<ResourceManager> ();
		structureManager = GetComponent<StructureManager> ();
		uiManager = GetComponent<UIManager>();
		unlockManager = GetComponent<UnlockManager>();
		builderHelper = GetComponent<BuilderHelper>();

		LoadSequence();

		resourceManager.StartUp ();
		structureManager.StartUp ();
		unlockManager.StartUp();
		uiManager.StartUp();
		uiManager.UpdateUI();

		delay = 1f / ticks;
		StartCoroutine (GameLoop ());
	}

	float delay = 0.1f;
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

	public void Forage(float value){
		ResourceManager.Resource tmpDough = resourceManager.GetResource("Dough");
		tmpDough.amount += value;
	}

	void LoadSequence(){

	}

	//end class
}
