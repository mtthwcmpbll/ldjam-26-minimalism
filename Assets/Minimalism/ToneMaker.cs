using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ToneMaker : MonoBehaviour {
	
	public List<AudioClip> Tones;
	public AudioClip Finale;
	
	private System.Random rnd;
	
	public void Setup() {
		rnd = new System.Random();
	}

	public void PlayToneAt(int i) {
		if (i < 0 || i >= Tones.Count) {
			//play a random tone, we don't have one for this index
			AudioClip randomClip = Tones[rnd.Next(Tones.Count)];
			AudioSource.PlayClipAtPoint(randomClip, Vector3.zero);
		} else {
			AudioSource.PlayClipAtPoint(Tones[i], Vector3.zero);
		}
	}
	
	public void PlayFinale() {
		AudioSource.PlayClipAtPoint(Finale, Vector3.zero);
	}
}
