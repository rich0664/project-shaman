using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Villager{

	public string name;
	public float age;
	public float health;
	public PhysicalStructure worksAt;
	public PhysicalStructure livesAt;
	public List<VillagerSkill> skillList;
	public bool experienced = false;
	[HideInInspector] public int headIconIndex;

	Dictionary<string, VillagerSkill> skillDictionary;


	// Use this for initialization
	public void Start () {
		skillDictionary = new Dictionary<string, VillagerSkill> ();
		skillList = new List<VillagerSkill>();
		StructureManager TSM = GameObject.Find("Management").GetComponent<StructureManager>();
		foreach(StructureManager.Structure struc in TSM.structures){
			VillagerSkill tmpSkill = new VillagerSkill();
			tmpSkill.structName = struc.name;
			tmpSkill.skillLevel = 1f;
			skillDictionary [struc.name] = tmpSkill;
			skillList.Add(tmpSkill);

		}
		headIconIndex = Random.Range(0,8);
	}

	public VillagerSkill GetSkill (string skillToGet){
		return skillDictionary [skillToGet];
	}

}
