using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit : MonoBehaviour
{
	
	public Transform target;
	public float zoomSpeed = 5f;
	public float targetDistance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	public float distanceMin = .5f;
	public float distanceMax = 15f;
	public float maxZoom = 500f;
	public float offsetLimit = 1f;
	public float GPanSpeed = 80f;
	public float overZoom = 0f;
	public Vector3 targOffset = Vector3.zero;
	public float x = 0.0f;
	public float y = 0.0f;
	GameManager GM;
	Toggle invertPan;
	public float distance = 5f;
	
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
		GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;
	}

	public void SetSensitivity (float sensitivity){
		GPanSpeed = sensitivity;
	}

	public void ExpandDistance (float expInc)
	{
		if (distanceMax < maxZoom) {
			distanceMax += expInc;
		} else {
			distanceMax = maxZoom;
			overZoom += expInc;
		}
		offsetLimit += 3.87f;
		targetDistance = distanceMax;
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
		float percent = (targetDistance - distanceMin) / (overZoom + distanceMax - distanceMin);
		limit = (1 - percent) * limit + 8f;
		targOffset.y = 0f;
		targOffset = Vector3.ClampMagnitude(targOffset, limit);
	}

	bool hasMode = false;
	bool panOrZoom = false;
	int frameWait = 0;
	Vector2 pzDiff = Vector2.zero;
	void LateUpdate ()
	{
		if (target) {

			float zoomMult = targetDistance / distanceMin;
			if (!GM.uiManager.isMenu) {
				if (Input.GetMouseButton (1) && Input.touchCount == 0) {
					x += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
					y -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
				}
				if(!GM.builderHelper.isPlacing){
					if (Input.GetMouseButton (0) && Input.touchCount == 0) {
						float dirMult = 1f;
						if(invertPan.isOn)
							dirMult = -dirMult;
						float panSpeed = (GPanSpeed / 550f) * zoomMult;
						targOffset += transform.right * -Input.GetAxis ("Mouse X") * panSpeed * dirMult;
						targOffset += transform.forward * -Input.GetAxis ("Mouse Y") * panSpeed * dirMult;
					}
					if(Input.touchCount == 1){
						Vector2 tdp = Input.GetTouch (0).deltaPosition;
						x += (tdp.x / Screen.width) * xSpeed * 13.25f;
						y -= (tdp.y / Screen.height) * ySpeed * 12.75f;
					}
					if (Input.touchCount == 2) {
						float panSpeed = GPanSpeed * zoomMult;
						float zoomSpeed = 2.25f * zoomMult;
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
						Debug.Log(deltaMagnitudeDiff.ToString("F2") + " : " + deltaMid.magnitude.ToString("F2"));
						if(!hasMode){
							if(frameWait < 3){
								frameWait++;
								pzDiff.x += deltaMid.magnitude;
								pzDiff.y += deltaMagnitudeDiff * 0.6f;
							}else{
								if(pzDiff.x != 0f || pzDiff.y != 0f){
									hasMode = true;
									if(Mathf.Abs(pzDiff.y) > Mathf.Abs(pzDiff.x)){
										panOrZoom = true;
									}else{
										panOrZoom = false;
									}
								}else{
									frameWait = 0;
								}
							}
						}else{
							if(panOrZoom){
								targetDistance = Mathf.Clamp (targetDistance + deltaMagnitudeDiff * zoomSpeed, distanceMin, distanceMax);
							}else{
								targOffset += transform.right * (-deltaMid.x / (float)Screen.width) * panSpeed * dirMult;
								targOffset += transform.forward * (-deltaMid.y / (float)Screen.height) * panSpeed * dirMult;
							}
						}
					}else{
						hasMode = false;
						frameWait = 0;
						pzDiff = Vector2.zero;
					}
				}else{
					if(Input.touchCount == 1){
						float panSpeed = GPanSpeed * zoomMult * 1.2f;
						Touch touchZero = Input.GetTouch (0);
						targOffset += transform.right * (touchZero.deltaPosition.x / (float)Screen.width) * panSpeed;
						targOffset += transform.forward * (touchZero.deltaPosition.y / (float)Screen.height) * panSpeed;
					}
					if (Input.GetMouseButton (0) && Input.touchCount == 0) {
						float panSpeed = (GPanSpeed / 400f) * zoomMult;
						targOffset += transform.right * Input.GetAxis ("Mouse X") * panSpeed;
						targOffset += transform.forward * Input.GetAxis ("Mouse Y") * panSpeed;
					}
				}
			}
			yMinLimit = Mathf.SmoothStep(yMinLimit, 2f * zoomMult + 0.1f, Time.deltaTime * 10f);
			y = ClampAngle (y, yMinLimit, yMaxLimit);
			Quaternion rotation = Quaternion.Euler (y, x, 0);
			
			if (!GM.uiManager.isMenu){
				float zS = zoomSpeed * zoomMult;
				targetDistance = Mathf.Clamp (targetDistance - Input.GetAxis ("Mouse ScrollWheel") * zS, distanceMin, distanceMax);
			}
				
			FixOffset ();
			//RaycastHit hit;
			/*if (Physics.Linecast (target.position, transform.position, out hit)) {
					distance -=  hit.distance;
				}*/

			distance = Mathf.SmoothStep(distance, targetDistance, Time.deltaTime * 14f);
			Vector3 negDistance = new Vector3 (0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position + targOffset;
				
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 12f);
			transform.position = Vector3.Lerp (transform.position, position, Time.deltaTime * 12f);
			
		}else{
			GM.aiManager.ResetCameraTarget();
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