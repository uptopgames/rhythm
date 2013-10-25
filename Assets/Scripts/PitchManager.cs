using UnityEngine;
using System.Collections;

public enum Notes
{
	C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B, G_Major, D_Major
}

public class PitchManager : MonoBehaviour
{
	public Color[] colors;

	public void Go (Notes newNote, float newHeight, int newColor)
	{
		Debug.Log ("newColor: " + newColor);
		
		// GLA Up Top Fix Me
		if (newColor == 12) newColor = 11;
		
		particleSystem.startColor = colors[newColor];
		
		Debug.Log ("newNote: " + newNote);
		
		if((int)newNote>11)
		{
			switch(newNote)
			{
				case Notes.G_Major:
					PlayChord(Notes.C, 0);
					PlayChord(Notes.E, 1);
					PlayChord(Notes.C, 2);
				break;
				case Notes.D_Major:
					PlayChord(Notes.Gb, 0);
					PlayChord(Notes.D, 1);
					PlayChord(Notes.A, 1);
				break;
			}
		}
		else
		{
			audio.pitch =  Mathf.Pow(2, (12 * newHeight + (int)newNote)/12.0f);
			audio.Play();
		}
		
		Invoke("Death", audio.clip.length);
	}
	
	void Death()
	{
		GameObject.Destroy(gameObject);
	}
	
	void PlayChord(Notes newNote, float newHeight)
	{
		GameObject chord = new GameObject("Chord");
		chord.AddComponent("AudioSource");
		chord.audio.clip = audio.clip;
		chord.audio.volume = audio.volume;
		chord.audio.playOnAwake = audio.playOnAwake;
		chord.audio.priority = audio.priority;
		chord.audio.loop = audio.loop;
		
		chord.audio.pitch = Mathf.Pow(2, (12 * newHeight + (int)newNote)/12.0f);
		chord.audio.Play();
	}
}