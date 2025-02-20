using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OG_HeadsetRaycast : MonoBehaviour {

	public LayerMask layerMask;
	public float raycastDistance;
	public GameObject myWall;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Raycasting ();
	}


	void Raycasting(){
		Vector3 fwd = transform.TransformDirection (Vector3.forward); //what is the direction in front of us
		RaycastHit hit = new RaycastHit ();

		if(Physics.Raycast(transform.position, fwd, out hit, raycastDistance, layerMask)){
			//Debug.Log ("hit object: " + hit.collider.gameObject.name);

			if(hit.collider.gameObject.tag == "Wall"){
				Debug.Log("Hello, the headset sees a wall!");
				myWall = hit.collider.gameObject;
			}
		}
	}
}
