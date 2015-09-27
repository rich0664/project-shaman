using UnityEngine;
using System.Collections;

[System.Serializable]
public class Effect{
	public string targetName;
	public enum targetMode{
		Resource = 1,
		Structure = 2
	}
	public enum effectMode{
		Produce = 1,
		Multiply = 2,
		StorageSum = 3,
		Cost = 4,
		ActiveCost = 5,
		ActiveMult = 6
	}
	public targetMode effectTarget;
	public effectMode effectType;
	public float effectValue;
}
