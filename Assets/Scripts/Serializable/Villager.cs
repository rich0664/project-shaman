using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Villager{

	public string name;
	public PhysicalStructure worksAt;
	public PhysicalStructure livesAt;
	public List<VillagerSkill> skillList;
	public bool experienced = false;

	Dictionary<string, VillagerSkill> skillDictionary;


	// Use this for initialization
	public void Start () {
		skillList.Clear();
		StructureManager TSM = GameObject.Find("Management").GetComponent<StructureManager>();
		foreach(StructureManager.Structure struc in TSM.structures){
			VillagerSkill tmpSkill = new VillagerSkill();
			tmpSkill.structName = struc.name;
			tmpSkill.skillLevel = 1f;
			skillDictionary [struc.name] = tmpSkill;
		}
	}

	public VillagerSkill GetSkill (string skillToGet){
		return skillDictionary [skillToGet];
	}

}
