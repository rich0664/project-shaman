using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
	[System.Serializable]
	public class Resource {
		public string name;
		public float amount;
		public Contributors contributors;
		public float sumStorage;
		public float baseStorage;
		public Color dispColor = Color.white;
		public bool discovered;
	}

	public void DoTick (Resource res){
		res.amount += (res.contributors.combinedProduction() / GM.ticks) * res.contributors.combinedMult();
		res.amount = Mathf.Clamp(res.amount, 0f, res.sumStorage);
	}

	public List<Resource> resources;
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

	}	





	//end class
}
