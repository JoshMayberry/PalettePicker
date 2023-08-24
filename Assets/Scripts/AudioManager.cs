using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance { get; private set; }
	void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one AudioManager in the scene.");
		}

		instance = this;
	}

	public void PlayOneShot(EventReference sound, Vector3 worldPosition) {
		RuntimeManager.PlayOneShot(sound, worldPosition);
	}
}
