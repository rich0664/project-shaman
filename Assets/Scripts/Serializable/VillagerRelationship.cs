using UnityEngine;
using System.Collections;

[System.Serializable]
public class VillagerRelationship {

	public int targetVillagerID;
	public float relationshipValue = 1f;

	public VillagerRelationship(int villID){
		targetVillagerID = villID;
	}

}
