using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Contributors{
	public float baseProduction;
	public float workerProduction;
	public float upgradeMult;
	public float structureMult;
	public float activeProduction;

	public float combinedMult(){
		return 1 + upgradeMult + structureMult;
	}

	public float combinedProduction(){
		return baseProduction + workerProduction + activeProduction;
	}

	public string ToRate(){
		string tmpStr = "(";
		float perSec = combinedProduction() * combinedMult();
		if(perSec == 0)
			return "";
		if(perSec >= 0)
			tmpStr += "+";
		tmpStr += perSec.ToString("F2") + "/sec)";

		return tmpStr;
	}

	public string ToTooltip(){
		string tmpStr = "";
		if(baseProduction != 0)
			tmpStr += "Base: " + baseProduction + "/s\n";
		if(workerProduction != 0)
			tmpStr += "Workers: " + workerProduction + "/s\n";
		if(upgradeMult != 0)
			tmpStr += "Upgrades: " + upgradeMult * 100f + "%\n";
		if(structureMult != 0)
			tmpStr += "Structures: " + structureMult * 100f + "%\n";
		if(activeProduction != 0)
			tmpStr += "Active: " + activeProduction + "/s\n";

		return tmpStr;
	}

	public int activeContributors(){
		int tmpInt = 0;
		if(baseProduction != 0)
			tmpInt++;
		if(workerProduction != 0)
			tmpInt++;
		if(upgradeMult != 0)
			tmpInt++;
		if(structureMult != 0)
			tmpInt++;
		if(activeProduction != 0)
			tmpInt++;

		return tmpInt;
	}
}
