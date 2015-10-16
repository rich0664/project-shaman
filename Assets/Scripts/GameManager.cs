using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public bool paused = false;
	public float ticks = 10f;

	public SaveLoad saveLoad;
	public ResourceManager resourceManager;
	public StructureManager structureManager;
	public UIManager uiManager;
	public GridManager gridManager;
	public UnlockManager unlockManager;
	public BuilderHelper builderHelper;
	public MouseOrbit gameCamera;
	public FrontFaceManager ffManager;

	void Start (){
		QualitySettings.vSyncCount = 0;  // VSync must be disabled
		Application.targetFrameRate = 30;

		saveLoad = GetComponent<SaveLoad> ();
		resourceManager = GetComponent<ResourceManager> ();
		structureManager = GetComponent<StructureManager> ();
		uiManager = GetComponent<UIManager>();
		gridManager = GetComponent<GridManager>();
		unlockManager = GetComponent<UnlockManager>();
		builderHelper = GetComponent<BuilderHelper>();
		gameCamera = Camera.main.GetComponent<MouseOrbit>();
		ffManager = Camera.main.GetComponent<FrontFaceManager>();

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
		while(true) {
			yield return new WaitForSeconds (delay);
			if(paused)
				continue;
			foreach(ResourceManager.Resource res in resourceManager.resources) {
				res.sumStorage = res.baseStorage;
				res.contributors.activeProduction = 0f;
			}
			foreach(StructureManager.Structure struc in structureManager.structures)
				if(!struc.passiveStructure)
					structureManager.DoTick (struc);
			foreach(ResourceManager.Resource res in resourceManager.resources)
				resourceManager.DoTick(res);

		
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
