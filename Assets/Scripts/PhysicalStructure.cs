using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhysicalStructure : MonoBehaviour {

	[HideInInspector] public StructureManager.Structure structure;
	[HideInInspector] public GameManager GM;
	[HideInInspector] public GameObject constructTimer;
	[HideInInspector] public float constructTime;
	[HideInInspector] public string spotIndex;
	public List<Villager> employeeList;

	public void StartConstruct(){
		StartCoroutine(Construct());
		spotIndex = transform.parent.name;
	}

	Image timer;
	IEnumerator Construct(){
		if(constructTime != 0f){
			float delay = 0.1f;
			delay = 0.05f;
			float conTime = 0f;
			timer = constructTimer.transform.Find("Image").GetComponent<Image>();
			while(conTime < constructTime){
				conTime += delay;
				timer.fillAmount = conTime / constructTime;
				yield return new WaitForSeconds(delay);
			}
			GM.builderHelper.pStructList.Add(this);
			GM.structureManager.BuildStructure(structure.name);
		}else{
			GM.builderHelper.pStructList.Add(this);
		}
		//structure.pStructs.Add(this);
		Destroy(constructTimer);
		transform.Find("BSprite").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> ("BuildingSprites/" + structure.name);
	}

}
