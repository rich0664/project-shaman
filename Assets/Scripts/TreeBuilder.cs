using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TreeBuilder : MonoBehaviour {

	public Transform parentLine;
	public GameObject lineTemplate;

	bool placing = false;
	GameObject tLine;

	// Update is called once per frame
	void Update () {

		if(Input.GetMouseButtonDown(0)){
			tLine = GameObject.Instantiate(lineTemplate);
			tLine.transform.SetParent(parentLine);
			tLine.transform.localScale = Vector3.one;
			tLine.transform.position = Input.mousePosition;
			placing = true;
		}

		if(placing){
			Vector3 moveDirection=Input.mousePosition - tLine.transform.position;
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			tLine.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			float dist = Vector3.Distance(tLine.transform.position, Input.mousePosition);
			tLine.transform.localScale = new Vector3(dist,1f,1f);
		}

		if(Input.GetMouseButtonUp(0)){
			placing = false;
		}

		if(tLine && Input.GetKeyDown(KeyCode.Z))
			Destroy(tLine);

	}


}
