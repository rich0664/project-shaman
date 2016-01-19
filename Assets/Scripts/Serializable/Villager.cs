using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Villager{

	public string name;
	public float age;
	public float health = 100f;
	public float mood = 100f;
	public string worksAt;
	public string livesAt;
	public string talentGroup;
	public float talentednes;
	public List<VillagerSkill> skillList;
	public List<ResourceManager.Resource> foodList = new List<ResourceManager.Resource>();
	public bool experienced = false;
	[System.NonSerialized] public float timeStamp = 0f;
	[HideInInspector] public int headIconIndex;
	[HideInInspector] public int uniqueID;
	[System.NonSerialized][HideInInspector] public GameManager GM;

	//[System.NonSerialized]
	Dictionary<string, VillagerSkill> skillDictionary;


	// Use this for initialization
	public void Start () {
		skillDictionary = new Dictionary<string, VillagerSkill> ();
		skillList = new List<VillagerSkill>();
		foreach(StructureManager.Structure.structType enumValue in System.Enum.GetValues(typeof(StructureManager.Structure.structType))){
			VillagerSkill tmpSkill = new VillagerSkill();
			tmpSkill.skillGroup = enumValue.ToString();
			tmpSkill.skillLevel = 1f;
			skillDictionary[tmpSkill.skillGroup] = tmpSkill;
			skillList.Add(tmpSkill);
		}
		foreach(ResourceManager.Resource res in GM.resourceManager.resources)
			if(res.resourceType == ResourceManager.Resource.resourceMode.Food)
				foodList.Add(res);
		System.Random rnd = new System.Random();
		var tfoodList = foodList.OrderBy(x => rnd.Next());
		foodList = tfoodList.ToList();

		float rand = Random.Range(0.0f, 1.0f);
		if(rand <= 0.3f){
			int randint = Random.Range(2,System.Enum.GetValues(typeof(StructureManager.Structure.structType)).Length + 1);
			StructureManager.Structure.structType tEnum = (StructureManager.Structure.structType)randint;
			talentGroup = tEnum.ToString();
		}else
			talentGroup = "None";
		talentednes = Random.Range(3.0f, 6.0f);

		headIconIndex = Random.Range(0,8);
	}

	public VillagerSkill GetSkill (string skillToGet){
		//Debug.Log(skillToGet);
		return skillDictionary [skillToGet];
	}

}
