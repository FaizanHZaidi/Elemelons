using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon;

public class BoulderBehavior : Photon.MonoBehaviour {

    //public AudioSource rumbling;
    public GameObject healthUpPrefab;
    public AudioSource rumbling;
	public AudioSource explode;
	public ParticleSystem explodeParticles;
	public float lowPitch = 0.45f;
	public float highPitch = 0.85f;
	public float maxSize = 0.02f;
	public float minSize = 0.008f;
	public float health = 1.7f;
	float randomPitch;

	// Set audio starts to 0, apply a random scale and pitch to this boulder:
	void Start () {
		rumbling.time = 0f;
		explode.time = 0f;
		float randomScale = Random.Range (minSize, maxSize); // Scale the size of the boulder.
		randomPitch = Map (minSize, maxSize, highPitch, lowPitch, randomScale); // Set the pitch based on the size.
		gameObject.transform.localScale += new Vector3 (randomScale, randomScale, randomScale);
	}
	
	void Update () {
		// Flamethrowers are the only things that affect health, but if the destroy the boulder:
		if (health <= 0) {
			PhotonView.Get(this).RPC("PlayExplosion", PhotonTargets.All, true);
		}
		// If the boulder falls off the map, destroy it silently:
		if (gameObject.transform.position.y < -20f) {
			// Call the NetworkDestroy RPC via the PhotonView component to destroy ON NETWORK:
			PhotonView.Get(this).RPC("NetworkDestroy", PhotonTargets.All);
		}
	}

	// Listen for collisions (things the boulder is hitting):
	void OnCollisionEnter(Collision col){
		// Rumble on each collision if silent:
		if (!rumbling.isPlaying) {
			rumbling.pitch = randomPitch;
			rumbling.Play ();
		}
		// If the boulder hits a neworked player, call that PhotonView owner to take damage:
		if (col.gameObject.GetPhotonView () != null && col.gameObject.CompareTag ("Player")) {
			col.gameObject.GetPhotonView().RPC("TakeDamage", col.gameObject.GetPhotonView().owner, 15f);
			PhotonView.Get(this).RPC("PlayExplosion", PhotonTargets.All, false);
		}
		// Boulders should bounce off of: the environment, other boulders, and shields. Explode otherwise.
		else if (!col.gameObject.CompareTag("Environment") && !col.gameObject.CompareTag("boulder") && !col.gameObject.CompareTag("defense") && !col.gameObject.CompareTag("Untagged") && !col.gameObject.CompareTag("SkillStone") && !col.gameObject.CompareTag("healthUp")) {
			PhotonView.Get(this).RPC("PlayExplosion", PhotonTargets.All, true);
		}
	}

	// Trigger explosion audio, "hide" the main boulder, call network to destroy once audio is done:
	[PunRPC]
	public void PlayExplosion(bool withExplosionParticles) {
		// If the clip is not playing (this is SelfDestruct's first call), play it,
		// turn off the Renderer/Collider, and turn on the explosion particle effect:
		if (!explode.isPlaying) {
			if (withExplosionParticles) {
				explodeParticles.Play();
                if(PhotonNetwork.isMasterClient)
                {
                    int powerUp = Random.Range(0, 5);
                    if (powerUp == 3)
                    {
                        PhotonNetwork.Instantiate(healthUpPrefab.name, gameObject.transform.position, Quaternion.identity, 0);
                    }
                }
            }
			gameObject.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
			gameObject.GetComponent<MeshRenderer> ().enabled = false;
			gameObject.GetComponent<SphereCollider> ().enabled = false;
			explode.pitch = randomPitch;
			explode.Play ();
		}
		StartCoroutine ("SelfDestruct", explode.clip.length);
	}

	IEnumerator SelfDestruct(float clipLength) {
		yield return new WaitForSeconds(clipLength);
		PhotonView.Get(this).RPC("NetworkDestroy", PhotonTargets.All);
	}
		
	// Remote Procedure Calls happen indirectly on set network clients as follows:
	// PhotonView.Get(this).RPC("NetworkDestroy", PhotonTargets.All);
	// In this case the PhotonTarget is MasterClient, reducing network traffic by only telling the master to delete the object from the network's scene.
	[PunRPC]
	void NetworkDestroy() {
		if (gameObject.GetPhotonView().isMine) {
			GameObject.Find ("GameManager").GetPhotonView ().RPC ("BoulderCountUpdate", PhotonTargets.MasterClient);
			PhotonNetwork.RemoveRPCs(gameObject.GetPhotonView());
			PhotonNetwork.Destroy (gameObject.GetPhotonView());
		}
	}

	[PunRPC]
	void NetworkDestroyNoCount() {
		if (gameObject.GetPhotonView().isMine) {
			PhotonNetwork.RemoveRPCs(gameObject.GetPhotonView());
			PhotonNetwork.Destroy (gameObject.GetPhotonView());
		}
	}

	[PunRPC]
	public void TakeDamage(float damage) {
		health -= damage;
		//Debug.Log ("Boulder health: " + health);
	}

	float Map (float oldMin, float oldMax, float newMin, float newMax, float val){
		float oldRange = (oldMax - oldMin);
		float newRange = (newMax - newMin);
		float newVal = (((val - oldMin) * newRange) / oldRange) + newMin;

		return(newVal);
	}

}
