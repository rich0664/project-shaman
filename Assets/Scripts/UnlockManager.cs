using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnlockManager : MonoBehaviour {

	[System.Serializable]
	public class Unlock {
		public string targetName;
		public enum targetMode{
			Resource = 1,
			Structure = 2
		}
		public targetMode targetType;
		public string targetResource;
		public enum costMode{
			Resource = 1,
			Structure = 2
		}
		public costMode costType;
		public float unlockAt;
	}

	public List<Unlock> unlocks;
	GameManager GM;

	public void StartUp(){
		GM = GetComponent<GameManager>();
		StartCoroutine(UnlockLoop());
	}

	IEnumerator UnlockLoop(){
		while(!GM.paused){
			yield return new WaitForSeconds(1.0f);
			CheckUnlocks();
		}
	}

	public void CheckUnlocks(){
		foreach(Unlock unlck in unlocks){
			int enumIndex = (int)unlck.targetType;
			switch (enumIndex) {
				case 1: //resource
					ResourceManager.Resource targetRes = GM.resourceManager.GetResource(unlck.targetName);
					if(targetRes.amount >= unlck.unlockAt)
						targetRes.discovered = true;
					break;
				case 2: //structure
					int tmpEnumIndex = (int)unlck.costType;
					float tmpAmount = 0f;
					switch(tmpEnumIndex){
						case 1: //resource
							tmpAmount = GM.resourceManager.GetResource(unlck.targetResource).amount;
							break;
						case 2: //structure
							tmpAmount = GM.structureManager.GetStructure(unlck.targetResource).amount;
							break;
					}
					StructureManager.Structure targetStruc = GM.structureManager.GetStructure(unlck.targetName);
					if(tmpAmount >= unlck.unlockAt)
						targetStruc.discovered = true;
					break;
			}
		}
	}


	//end class
}
