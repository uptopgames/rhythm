using UnityEngine;
using System.Collections;

public class Suicide : MonoBehaviour
{
	public GameObject[] victims;
	public float countDownToDeath = 1;

	// Use this for initialization
	void Start ()
	{
		Invoke("DeathEvent", countDownToDeath);
		foreach(GameObject g in victims) GameObject.Destroy(g, countDownToDeath);
		GameObject.Destroy(gameObject, countDownToDeath);
		
	}
	
	void DeathEvent()
	{
		
	}
}