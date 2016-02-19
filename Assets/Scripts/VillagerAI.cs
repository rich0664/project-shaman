using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VillagerAI : MonoBehaviour {

	public Villager villager;
	public PhysicalStructure jobStruct;
	public GameManager GM;
	public float walkingSpeed = 0.2f;
	Animator vAnimator;
	float actionDuration = 0f;
	SpriteRenderer[] bodyParts;
	int jobSwitch = 0;

	// Use this for initialization
	void Start () {
		vAnimator = GetComponentInChildren<Animator>();
		vAnimator.SetBool("Walking", false);
		bodyParts = GetComponentsInChildren<SpriteRenderer>();
		StartCoroutine(FadeIn());
	}

	IEnumerator FadeIn(){
		float opac = 0f;
		while(opac < 0.99f){
			opac = Mathf.SmoothStep(opac, 1f, Time.unscaledDeltaTime * 5.5f);
			foreach(SpriteRenderer sr in bodyParts){
				Color tc = sr.color; tc.a = opac; sr.color = tc;
			}
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(waitRoutine());
	}

	IEnumerator waitRoutine(){
		float waitTime = Random.Range(0.5f, 1.5f);
		yield return new WaitForSeconds(waitTime);
		if(jobStruct.structure.structCategory == StructureManager.Structure.structType.Farming){
			StartCoroutine(Farm());
		}else if(jobStruct.structure.structCategory == StructureManager.Structure.structType.Foraging){
			StartCoroutine(Forage());
		}else{
			waitTime = Random.Range(5.0f, 11.5f);
			yield return new WaitForSeconds(waitTime);
			StartCoroutine(FadeOut());
		}
	}

	IEnumerator Forage(){
		//go to forage
		Vector3 dir = (transform.position - Vector3.zero).normalized;
		float dist = GM.gridManager.FurthestSpotDist(false) + Random.Range(11.5f, 35.5f);
		Vector3 foragePos = dir * dist;
		vAnimator.SetBool("Walking", true);
		while(transform.position != foragePos){
			transform.position += dir * walkingSpeed;
			float tDist = Vector3.Distance(Vector3.zero, transform.position);
			if(tDist >= dist)
				transform.position = foragePos;
			yield return new WaitForEndOfFrame();
		}
		vAnimator.SetBool("Walking", false);

		//would forage here
		float waitTime = Random.Range(5.5f, 9.5f);
		yield return new WaitForSeconds(waitTime);

		//walk back
		dir = (jobStruct.transform.position - transform.position).normalized;
		dist = Vector3.Distance(Vector3.zero, jobStruct.transform.position);
		foragePos = jobStruct.transform.position;
		vAnimator.SetBool("Walking", true);
		while(transform.position != foragePos){
			transform.position += dir * walkingSpeed;
			float tDist = Vector3.Distance(Vector3.zero, transform.position);
			if(tDist <= dist)
				transform.position = foragePos;
			yield return new WaitForEndOfFrame();
		}
		vAnimator.SetBool("Walking", false);
		StartCoroutine(FadeOut());
	}

	IEnumerator Farm(){
		float waitTime = Random.Range(6.5f, 11.5f);
		yield return new WaitForSeconds(waitTime);
		StartCoroutine(FadeOut());
	}
					
	IEnumerator FadeOut(){
		float opac = 1f;
		while(opac > 0.01f){
			yield return new WaitForEndOfFrame();
			opac = Mathf.SmoothStep(opac, 0f, Time.unscaledDeltaTime * 5.5f);
			foreach(SpriteRenderer sr in bodyParts){
				Color tc = sr.color; tc.a = opac; sr.color = tc;
			}
		}
		GM.ffManager.updateList.Remove(transform);
		GM.aiManager.outVillagers.Remove(villager);
		GM.aiManager.ResetCameraTarget();
		GameObject.Destroy(gameObject);
	}

	//end class
}
