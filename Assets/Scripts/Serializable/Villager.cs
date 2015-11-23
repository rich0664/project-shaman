using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Villager{

	public string name;
	public float age;
	public float health;
	public string worksAt;
	public string livesAt;
	public string talentGroup;
	public float talentednes;
	public List<VillagerSkill> skillList;
	public bool experienced = false;
	[HideInInspector] public float skillTimeStamp = 0f;
	[HideInInspector] public int headIconIndex;
	[HideInInspector] public int uniqueID;

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
		return skillDictionary [skillToGet];
	}

}
