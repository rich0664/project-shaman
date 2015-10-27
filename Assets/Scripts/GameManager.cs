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
	public VillagerManager villagerManager;
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
		villagerManager = GetComponent<VillagerManager>();
		gameCamera = Camera.main.GetComponent<MouseOrbit>();
		ffManager = Camera.main.GetComponent<FrontFaceManager>();

		resourceManager.StartUp ();
		structureManager.StartUp ();
		gridManager.StartUp();
		builderHelper.StartUp();

		saveLoad.StartUp();
		saveLoad.Load(false);

		villagerManager.StartUp();
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

			resourceManager.foodMaster.amount = 0f;
			foreach(ResourceManager.Resource res in resourceManager.resources) {
				res.sumStorage = res.baseStorage;
				res.contributors.activeProduction = 0f;

				if(res.resourceType == ResourceManager.Resource.resourceMode.Food)
					resourceManager.foodMaster.amount += res.amount;
			}
			foreach(StructureManager.Structure struc in structureManager.structures)
				if(!struc.passiveStructure && struc.discovered)
					structureManager.DoTick (struc);
			foreach(ResourceManager.Resource res in resourceManager.resources)
				resourceManager.DoTick(res);
		
			uiManager.UpdateUI();
		}
	}

	public void Forage(){
		resourceManager.AddFood(resourceManager.GetResource("BerriesA"), Random.Range(0.5f, 5f));
		resourceManager.GetResource("Wood").amount += Random.Range(0.5f, 2f);
		resourceManager.GetResource("Stone").amount += Random.Range(0.0f, 0.5f);
	}

	void LoadSequence(){

	}

	//end class
}
