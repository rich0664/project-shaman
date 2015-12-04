using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {

	public float expandIncrement = 0.3f;
	public float rotationIncrement = 22.5f;
	public int expandAt = 4;
	public int expandInc = 8;
	public int rings;
	public int spots = 8;
	public int foliageCount = 30;
	GameManager GM;
	GameObject spotPref;
	GameObject ringParent;
	GameObject foliageParent;
	float rotat = 0f;
	int spotCount = 0;
	[HideInInspector] public HashSet<Transform> foliages = new HashSet<Transform>();
	
	public void StartUp(){
		//StartCoroutine(TestLoop());
		spotPref = Resources.Load<GameObject>("BuildingPrefabs/Spot");
		ringParent = new GameObject(); ringParent.name = "Rings";
		GM = GetComponent<GameManager>();
		ProcessFoliage();
	}

	public void TryExpand(bool str){
		Expand(str);
	}

	public void AdjustFoliageDensity(float amount){
		foliageCount = (int)(amount + 6 * rings);
		ProcessFoliage();
	}

	public void ProcessFoliage(){
		if(foliageParent)
			Destroy(foliageParent);

		foliages.Clear();
		foliageParent = new GameObject();
		foliageParent.name = "Foliage";
		GameObject treePref = Resources.Load<GameObject>("BuildingPrefabs/Tree");
		GameObject shrubPref = Resources.Load<GameObject>("BuildingPrefabs/Shrub");
		for(int i = 0; i < foliageCount; i++){
			GameObject treeInst = GameObject.Instantiate(shrubPref);
			treeInst.transform.SetParent(foliageParent.transform);
			treeInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + Random.Range(11.5f, 41.5f));
			treeInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / foliageCount) * i + Random.Range(-6.0f, 6.0f));
			treeInst.transform.localScale *= Random.Range(0.4f, 1.1f);
			foliages.Add(treeInst.transform);
		}
		int treeIT = foliageCount/2;
		for(int i = 0; i < treeIT; i++){
			GameObject treeInst = GameObject.Instantiate(treePref);
			treeInst.transform.SetParent(foliageParent.transform);
			treeInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + Random.Range(14f, 34.5f));
			treeInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / treeIT) * i + Random.Range(-6.0f, 6.0f));
			treeInst.transform.localScale *= Random.Range(0.55f, 0.95f);
			foliages.Add(treeInst.transform);
		}
		for(int i = 0; i < foliageCount; i++){
			GameObject treeInst = GameObject.Instantiate(treePref);
			treeInst.transform.SetParent(foliageParent.transform);
			treeInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + Random.Range(35.5f, 60.5f));
			treeInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / foliageCount) * i + Random.Range(-10.0f, 10.0f));
			treeInst.transform.localScale *= Random.Range(0.55f, 0.95f);
			foliages.Add(treeInst.transform);
		}
		//rot = Quaternion.Euler(Vector3.zero);
		GM.ffManager.rot = Quaternion.Euler(Vector3.zero);
		foliageCount += 8;
	}

	void Expand(bool reFoliage){
		if(rings >= expandAt){
			expandAt += expandInc;
			expandInc *= 2;
			spots *= 2;
			rotationIncrement /= 2f;
			rotat = rotationIncrement;
		}
		GameObject Ring = new GameObject();
		Ring.name = "Ring" + (rings + 1); Ring.transform.SetParent(ringParent.transform);
		for(int i = 0; i < spots; i++){
			spotCount++;
			GameObject spotInst = GameObject.Instantiate(spotPref);
			spotInst.transform.SetParent(Ring.transform);
			spotInst.transform.localPosition = new Vector3(0f, 0f, expandIncrement * rings + 7f);
			spotInst.transform.RotateAround(Vector3.zero, Vector3.up, (360f / spots) * i);
			spotInst.name = spotCount.ToString();
			GM.builderHelper.spotList.Add(spotInst.GetComponent<Spot>());
		}
		if(reFoliage){
			ProcessFoliage();
		}else{
			foliageCount += 8;
		}
		Ring.transform.RotateAround(Vector3.zero, Vector3.up, rotat);
		if(rotat == rotationIncrement){
			rotat = 0f;
		}else{ rotat = rotationIncrement; }
		rings++;
		GM.gameCamera.ExpandDistance(52f);
		GameObject.Find("DiscoveredGround").transform.localScale += Vector3.one * 2f;
	}

}
