using UnityEngine;
using System.Collections;

[System.Serializable]
public class FoodEffect {

	public enum foodEffectMode{
		HealthBuff = 1,
		MoodBuff = 2,
		SkillBuff = 3
	}

	public foodEffectMode foodEffectType;
	public StructureManager.Structure.structType foodSkillTarget;
	public float foodEffectStrength;
}
