using UnityEngine;
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
	float x = 0.0f;
	float y = 0.0f;
	UIManager UIM;
	
	// Use this for initialization
	void Start ()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().freezeRotation = true;

		UIM = GameObject.Find ("Management").GetComponent<UIManager> ();
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

	void LateUpdate ()
	{
		if (target) {

			if (!UIM.isMenu)
			if (Input.touchCount > 0 || Input.GetMouseButton (0)) {

				float xinput = Input.GetAxis ("Mouse X");
				if (Input.touchCount > 0)
					xinput += Input.GetTouch (0).deltaPosition.x;
				float yinput = Input.GetAxis ("Mouse Y");
				if (Input.touchCount > 0)
					yinput += Input.GetTouch (0).deltaPosition.y;

				x += xinput * xSpeed * distance * 0.02f;
				y -= yinput * ySpeed * 0.02f;
			
				y = ClampAngle (y, yMinLimit, yMaxLimit);
				
			}
				
			Quaternion rotation = Quaternion.Euler (y, x, 0);

			if (!UIM.isMenu)
				distance = Mathf.Clamp (distance - Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);
				
			//RaycastHit hit;
			/*if (Physics.Linecast (target.position, transform.position, out hit)) {
					distance -=  hit.distance;
				}*/
			Vector3 negDistance = new Vector3 (0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + target.position;
				
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