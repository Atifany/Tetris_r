using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class AudioFade
{
	public static IEnumerator FadeVolumeAudioSource(AudioSource audioSource, float target, float duration){
		if (!audioSource.isPlaying)
			yield break;
		float current = 0f;
		float start = audioSource.volume;
		while (current < duration){
			current += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(start, target, current / duration);
			yield return null;
		}
		yield break;
	}
	public static IEnumerator FadePitchAudioSource(AudioSource audioSource, float target, float duration){
		if (!audioSource.isPlaying)
			yield break ;
		float current = 0f;
		float start = audioSource.pitch;
		while (current < duration){
			current += Time.deltaTime;
			audioSource.pitch = Mathf.Lerp(start, target, current / duration);
			yield return null; // <- makes the while loop run with each frame, not in a single one
		}
		yield break ;
	}
}
