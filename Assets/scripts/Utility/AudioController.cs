using UnityEngine;
using System.Collections;

public class AudioController {

	private AudioSource source;
	private AudioClip[] swap, drop, hand, match;

	public AudioController(){
		source = GameObject.Find ("board").GetComponent<AudioSource> ();

		AudioListener.volume = .6f;

		swap = new AudioClip[5];
		swap[0] = (AudioClip)Resources.Load ("Swap-1");
		swap[1] = (AudioClip)Resources.Load ("Swap-2");
		swap[2] = (AudioClip)Resources.Load ("Swap-3");
		swap[3] = (AudioClip)Resources.Load ("Swap-4");
		swap[4] = (AudioClip)Resources.Load ("Swap-5");

		drop = new AudioClip[5];
		drop[0] = (AudioClip)Resources.Load ("Drop-2");
		drop[1] = (AudioClip)Resources.Load ("Drop-3");
		drop[2] = (AudioClip)Resources.Load ("Drop-6");
		drop[3] = (AudioClip)Resources.Load ("Drop-8");
		drop[4] = (AudioClip)Resources.Load ("Drop-9");

		hand = new AudioClip[5];
		hand[0] = (AudioClip)Resources.Load ("Hand-0");
		hand[1] = (AudioClip)Resources.Load ("Hand-1");
		hand[2] = (AudioClip)Resources.Load ("Hand-3");
		hand[3] = (AudioClip)Resources.Load ("Hand-4");
		hand[4] = (AudioClip)Resources.Load ("Hand-5");

		match = new AudioClip[5];
		match[0] = (AudioClip)Resources.Load ("Match-7-1");
		match[1] = (AudioClip)Resources.Load ("Match-7-2");
		match[2] = (AudioClip)Resources.Load ("Match-7-3");
		match[3] = (AudioClip)Resources.Load ("Match-7-4");
		match[4] = (AudioClip)Resources.Load ("Match-7-5");
	}

	public void SwapSound(){
		source.clip = swap [Random.Range (0, 5)];
		source.Play ();
	}	

	public void DropSound(AudioSource source){
		source.clip = drop [Random.Range (0, 5)];
		source.Play ();
	}
	
	public void PickupSound(AudioSource source){
		source.clip = hand [Random.Range (0, 5)];
		source.Play ();
	}

	public void BreakSound(){
		source.clip = match [Random.Range (0, 5)];
		source.Play ();
	}
}
