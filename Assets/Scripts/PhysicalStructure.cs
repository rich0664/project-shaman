using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PhysicalStructure : MonoBehaviour {

	[HideInInspector] public StructureManager.Structure structure;
	[HideInInspector] public GameManager GM;
	[HideInInspector] public GameObject constructTimer;
	[HideInInspector] public float constructTime;
	[HideInInspector] public bool built = false;
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
		if(structure.hasExt){
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f);
			int sameCount = 0;
			foreach(Collider c in hitColliders){
				PhysicalStructure tcps = c.GetComponentInChildren<PhysicalStructure>();
				if(tcps){
					if(tcps.built && tcps.structure == structure)
						sameCount++;
				}
			}
			if(sameCount > 0 && sameCount < structure.extOverflow){
				transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> ("BuildingSprites/" + structure.name + "_Ext");
			}else
				transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> ("BuildingSprites/" + structure.name);
		}else
			transform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> ("BuildingSprites/" + structure.name);
		built = true;
	}

}
