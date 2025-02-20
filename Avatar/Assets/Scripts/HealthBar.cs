using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon;

public class HealthBar : Photon.MonoBehaviour {

	void Start() {

	}
		
	void Update () {

	}

	[PunRPC]
	public void UpdateHealthBar(float newHealth) {
		Debug.Log("Updating health bar via RPC for PhotonView " + gameObject.GetPhotonView().viewID);
		// Keep the bar from "displaying" a less-than-zero amount:
		if (newHealth / 300	<= 0f) {
			newHealth = 0;
		}
		Vector3 updateSize = gameObject.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().localScale;
		updateSize.x = newHealth / 300;
		gameObject.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3 (updateSize.x, updateSize.y, updateSize.z);
	}
	
}
