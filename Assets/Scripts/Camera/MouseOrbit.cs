using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit : MonoBehaviour
{
	
	public Transform target;
	public float zoomSpeed = 5f;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	public float distanceMin = .5f;
	public float distanceMax = 15f;
	public float maxZoom = 500f;
	public float offsetLimit = 1f;
	float overZoom = 0f;
	Vector3 targOffset = Vector3.zero;
	float x = 0.0f;
	float y = 0.0f;
	GameManager GM;
	Toggle invertPan;
	
	// Use this for initialization
	void Start ()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().freezeRotation = true;

		GM = GameObject.Find ("Management").GetComponent<GameManager> ();
		invertPan = GameObject.Find ("UICanvas").transform.Find("GameMenu/InvertPan").GetComponent<Toggle>();
	}

	public void ExpandDistance (float expInc)
	{
		if (distanceMax < maxZoom) {
			distanceMax += expInc;
		} else {
			distanceMax = maxZoom;
			overZoom += expInc;
		}
		offsetLimit += GM.gridManager.expandIncrement;
		distance = distanceMax;
	}

	/*
	void Update(){
		if (Input.GetMouseButtonDown (2)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;			
			if (Physics.Raycast (ray, out hit)) {
				if(hit.collider.transform.parent.name == "Targets"){
					target = hit.collider.transform;
				}
			}
		}
	}
	*/

	public void ReCenterCamera(){
		targOffset = Vector3.zero;
	}

	void FixOffset ()
	{
		float limit = offsetLimit;
		float percent = (distance - distanceMin) / (overZoom + distanceMax - distanceMin);
		limit = (1 - percent) * limit + 8f;
		targOffset.y = 0f;
		targOffset.x = Mathf.Clamp (targOffset.x, -limit, limit);
		targOffset.z = Mathf.Clamp (targOffset.z, -limit, limit);
	}
	
	void LateUpdate ()
	{
		if (target) {

			float zoomMult = distance / distanceMin;
			if (!GM.uiManager.isMenu) {
				if (Input.GetMouseButton (1) && Input.touchCount == 0) {
					x += Input.GetAxis ("Mouse X") * xSpeed * distance * 0.02f;
					y -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
					y = ClampAngle (y, yMinLimit, yMaxLimit);
				}
				if(!GM.builderHelper.isPlacing){
					if (Input.GetMouseButton (0) && Input.touchCount == 0) {
						float dirMult = 1f;
						if(invertPan.isOn)
							dirMult = -dirMult;
						float panSpeed = 0.2f * zoomMult;
						targOffset += transform.right * -Input.GetAxis ("Mouse X") * panSpeed * dirMult;
						targOffset += transform.forward * -Input.GetAxis ("Mouse Y") * panSpeed * dirMult;
					}
					if(Input.touchCount == 1){
						Vector2 tdp = Input.GetTouch (0).deltaPosition;
						x += (tdp.x / Screen.width) * xSpeed * distance * 12.75f;
						y -= (tdp.y / Screen.height) * ySpeed * 12.75f;
						y = ClampAngle (y, yMinLimit, yMaxLimit);
					}
					if (Input.touchCount == 2) {
						float panSpeed = 0.4f * zoomMult;
						float zoomSpeed = 2.75f * zoomMult;
						float dirMult = 1f;
						if(invertPan.isOn)
							dirMult = -dirMult;
						Touch touchZero = Input.GetTouch (0);
						Touch touchOne = Input.GetTouch (1);
						Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
						Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
						float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
						float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
						float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
						Vector2 midTouch = (touchZero.position + touchOne.position) * 0.5f;//midpoints
						Vector2 prevMidTouch = (touchZeroPrevPos + touchOnePrevPos) * 0.5f;
						Vector2 deltaMid = midTouch - prevMidTouch;
					
						targOffset += transform.right * -deltaMid.x * panSpeed * dirMult;
						targOffset += transform.forward * -deltaMid.y * panSpeed * dirMult;
						distance = Mathf.Clamp (distance + deltaMagnitudeDiff * zoomSpeed, distanceMin, distanceMax);
					}
				}else{
					if(Input.touchCount == 1){
						float panSpeed = 0.4f * zoomMult;
						Touch touchZero = Input.GetTouch (0);
						targOffset += transform.right * touchZero.deltaPosition.x * panSpeed;
						targOffset += transform.forward * touchZero.deltaPosition.y * panSpeed;
					}
					if (Input.GetMouseButton (0) && Input.touchCount == 0) {
						float panSpeed = 0.2f * zoomMult;
						targOffset += transform.right * Input.GetAxis ("Mouse X") * panSpeed;
						targOffset += transform.forward * Input.GetAxis ("Mouse Y") * panSpeed;
					}
				}
			}
			
			Quaternion rotation = Quaternion.Euler (y, x, 0);
			
			if (!GM.uiManager.isMenu){
				float zS = zoomSpeed * zoomMult;
				distance = Mathf.Clamp (distance - Input.GetAxis ("Mouse ScrollWheel") * zS, distanceMin, distanceMax);
			}
				
			FixOffset ();
			//RaycastHit hit;
			/*if (Physics.Linecast (target.position, transform.position, out hit)) {
					distance -=  hit.distance;
				}*/
			Vector3 negDistance = new Vector3 (0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position + targOffset;
				
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.smoothDeltaTime * 18f);
			transform.position = Vector3.Lerp (transform.position, position, Time.smoothDeltaTime * 18f);
			
		}
		
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
	
	
}