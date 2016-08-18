using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
	[System.Serializable]
	public class Resource {
		public string name;
		public string displayName;
		public enum resourceMode{
			Resource = 1,
			Food = 2
		}
		public List<FoodEffect> foodEffects;
		public resourceMode resourceType;
		public float amountEaten;
		public float amount;
		public Contributors contributors;
		public float sumStorage;
		public float baseStorage;
		public SerColor dispColor = Color.black;
		public bool discovered;
	}

	public void DoTick (Resource res){
		float tmpAmnt = (res.contributors.combinedProduction() / GM.ticks) * res.contributors.combinedMult();
		int enumIndex = (int)res.resourceType;
		switch (enumIndex) {
		case 1: //Resource
			res.amount += tmpAmnt;
			res.amount = Mathf.Clamp(res.amount, 0f, res.sumStorage);
			break;
		case 2: //Food
			AddFood(res, tmpAmnt);
			break;
		}
	}

	public void AddFood(Resource foodSource, float amount){
		if(foodMaster.amount + amount <= foodMaster.sumStorage){
			foodSource.amount += amount;
			foodMaster.amount += amount;
			foodSource.amount = Mathf.Clamp(foodSource.amount, 0f, foodMaster.sumStorage);
		}else{
			foodSource.amount += foodMaster.sumStorage - foodMaster.amount;
			foodMaster.amount += foodMaster.sumStorage - foodMaster.amount;
		}
	}

	public List<Resource> resources;
	[HideInInspector] public Resource foodMaster;

	GameManager GM;
	Dictionary<string, Resource> resourceDictionary;

	public Resource GetResource(string resToGet){
		return resourceDictionary[resToGet];
	}

	public void StartUp (){
		GM = GetComponent<GameManager>();
		//Add each resource name to a dictionary for easy lookup;
		resourceDictionary = new Dictionary<string, Resource>();
		foreach(Resource res in resources)
			resourceDictionary[res.name] = res;
		foodMaster = GetResource("Food");
	}


	//end class
}
