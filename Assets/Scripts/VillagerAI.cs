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
	public void StartUp () {
		vAnimator = GetComponentInChildren<Animator>();
		vAnimator.SetBool("Walking", false);
		bodyParts = GetComponentsInChildren<SpriteRenderer>();

		//looks
		foreach(SpriteRenderer sr in bodyParts){
			if(sr.name == "eye")
				sr.color = villager.eyeColor;
			else
				sr.color = villager.bodyColor;
		}

		//start fade
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
		}else if (jobStruct.structure.structCategory == StructureManager.Structure.structType.House){
			StartCoroutine(WalkToPointAndBack(Vector3.zero, 3.25f, 0.5f, "Pray", true, 7f,16f, true)); 
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
		float tmpWalkSpeed = 0f;
		float walkAcc = 0.01f;
		while(transform.position != foragePos){
			if(tmpWalkSpeed < walkingSpeed)
				tmpWalkSpeed += walkAcc;
			transform.position += dir * tmpWalkSpeed;
			float tDist = Vector3.Distance(Vector3.zero, transform.position);
			if(tDist >= dist - 4f){
				vAnimator.SetBool("Walking", false);
				if(tmpWalkSpeed > 0.06f)
					tmpWalkSpeed -= walkAcc;
			}
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
		tmpWalkSpeed = 0f;
		while(transform.position != foragePos){
			if(tmpWalkSpeed < walkingSpeed)
				tmpWalkSpeed += walkAcc;
			transform.position += dir * tmpWalkSpeed;
			float tDist = Vector3.Distance(Vector3.zero, transform.position);
			if(tDist <= dist + 4f){
				vAnimator.SetBool("Walking", false);
				if(tmpWalkSpeed > 0.04f)
					tmpWalkSpeed -= walkAcc;
			}
			if(tDist <= dist)
				transform.position = foragePos;
			yield return new WaitForEndOfFrame();
		}
		vAnimator.SetBool("Walking", false);
		StartCoroutine(FadeOut());
	}

	IEnumerator WalkToPointAndBack(Vector3 point, float stopWithin, float walkSpeedMult, string animName, bool animState, float minWaitTime, float maxWaitTime, bool fadeOut){
		//go to forage
		Vector3 dir = (point - transform.position).normalized;

		Vector3 startPos = transform.position;
		Vector3 stopPos = point;
		vAnimator.SetBool("Walking", true);
		float tmpWalkSpeed = 0f;
		float walkAcc = 0.02f;
		float walkSpeed = walkingSpeed * walkSpeedMult;
		int walkFrames = 0;
		while(transform.position != stopPos){
			if(tmpWalkSpeed < walkSpeed)
				tmpWalkSpeed += walkAcc;
			transform.position += dir * tmpWalkSpeed;
			float tDist = Vector3.Distance(stopPos, transform.position);
			if(tDist <= stopWithin + 2f && walkFrames >= 30){
				vAnimator.SetBool("Walking", false);
				if(tmpWalkSpeed > 0.06f)
					tmpWalkSpeed -= walkAcc;
			}
			if(tDist <= stopWithin){
				stopPos= transform.position;
				transform.position = stopPos;
			}
			walkFrames++;
			yield return new WaitForEndOfFrame();
		}
		vAnimator.SetBool("Walking", false);


		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
		foreach(Collider hc in hitColliders){
			if(hc.transform == transform)
				continue;
			VillagerAI cAi = hc.GetComponent<VillagerAI>();
			if(cAi)
				VillagerManager.CheckRelationship(villager, cAi.villager);			
		}
		//would forage here
		vAnimator.SetBool(animName, animState);
		float waitTime = Random.Range(minWaitTime, maxWaitTime);
		yield return new WaitForSeconds(waitTime);

		//walk back
		dir = (startPos - transform.position).normalized;
		vAnimator.SetBool("Walking", true);
		tmpWalkSpeed = 0f;
		walkFrames = 0;
		while(transform.position != startPos){
			if(tmpWalkSpeed < walkSpeed)
				tmpWalkSpeed += walkAcc;
			transform.position += dir * tmpWalkSpeed;
			float tDist = Vector3.Distance(startPos, transform.position);
			if(tDist <= stopWithin + 2f && walkFrames >= 30){
				vAnimator.SetBool("Walking", false);
				if(tmpWalkSpeed > 0.04f)
					tmpWalkSpeed -= walkAcc;
			}
			if(tDist <= stopWithin){
				startPos = transform.position;
				transform.position = startPos;
			}
			walkFrames++;
			yield return new WaitForEndOfFrame();
		}
		vAnimator.SetBool("Walking", false);
		if(fadeOut)
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
		//GM.aiManager.ResetCameraTarget();
		GameObject.Destroy(gameObject);
	}

	//end class
}
