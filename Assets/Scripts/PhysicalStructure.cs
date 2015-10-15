using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhysicalStructure : MonoBehaviour {

	[HideInInspector] public StructureManager.Structure structure;
	[HideInInspector] public GameManager GM;
	[HideInInspector] public GameObject constructTimer;

	public void StartConstruct(){
		StartCoroutine(Construct());
	}

	Image timer;
	IEnumerator Construct(){
		float delay = 0.1f;
		delay = 0.05f;
		float conTime = 0f;
		timer = constructTimer.transform.Find("Image").GetComponent<Image>();
		while(conTime < structure.constructTime){
			conTime += delay;
			timer.fillAmount = conTime / structure.constructTime;
			yield return new WaitForSeconds(delay);
		}
		Destroy(constructTimer);
		transform.Find("BSprite").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> ("BuildingSprites/" + structure.name);
		GM.structureManager.BuildStructure(structure.name);
	}

}
