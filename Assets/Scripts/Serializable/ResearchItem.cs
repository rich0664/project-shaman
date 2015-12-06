using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ResearchItem : MonoBehaviour{

	public string name;
	public string dispName;
	public string quote;
	public float researchProgress = 0f;
	public float researchRequired = 100f;
	public bool researched;
	public bool persistUnlocked;
	public bool beingResearched;
	public Color displayColor;
	public Color barColor = new Color(200f/255f, 160f/255f, 245f/255f , 1f);
	public ResearchItem[] inputNodes;
	public unlockMode unlockType;
	[HideInInspector] public Image progressBar;


	public enum unlockMode{
		Any = 1, 
		All = 2
	}


}
