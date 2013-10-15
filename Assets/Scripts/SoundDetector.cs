using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public class SoundDetector : MonoBehaviour
{
	private float[][] timingGuideList = new float[10][] //preencher somente timingGuideList: a player se completa baseada nela
	{
		new float[3]{0, 0.17f, 0.34f},
		new float[3]{0, 0.17f, 0.34f},
		new float[5]{0, 0.83f, 1, 1.17f, 1.34f},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[3]{0, 0.17f, 0.34f},
		new float[3]{0, 0.17f, 0.34f},
		new float[5]{0, 0.83f, 1, 1.17f, 1.34f},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[2]{0, 0.33f},
	};
	private float[][] timingPlayerList = new float[10][] //não mexer; se completa no Start
	{
		new float[3]{0, 0.17f, 0.34f},
		new float[3]{0, 0.17f, 0.34f},
		new float[5]{0, 0.83f, 1, 1.17f, 1.34f},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[3]{0, 0.17f, 0.34f},
		new float[3]{0, 0.17f, 0.34f},
		new float[5]{0, 0.83f, 1, 1.17f, 1.34f},
		new float[5]{0, 0.33f, 0.66f, 0.82f, 1},
		new float[2]{0, 0.33f},
	};
	public float[] timingGuide; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	public float[] timingPlayer; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	private float[] melodiesDuration = new float[10]{1,1,2,2,2,1,1,2,1.66f,2};
	
	private int[][] notesButtonList = new int[10][] //preencher com o index dos botões das notas
	{
		new int[3]{0,1,2},
		new int[3]{0,1,2},
		new int[5]{3,4,3,5,6},
		new int[5]{6,0,1,7,6},
		new int[5]{6,0,1,7,8},
		new int[3]{0,1,2},
		new int[3]{0,1,2},
		new int[5]{3,4,3,5,6},
		new int[5]{6,0,1,7,6},
		new int[2]{6,0},
	};
	
	private Notes[] notesList = new Notes[9]
	{
		Notes.D,
		Notes.F,
		Notes.D,
		Notes.E,
		Notes.F,
		Notes.D,
		Notes.A,
		Notes.G,
		Notes.E
	};
	private int[] notesHeightList = new int[9]
	{
		0,0,1,1,1,1,0,0,0
	};
	private AudioClip[] melodies;
	
	public float allowedDelay = 0.1f; //desvio máximo do tempo exato para acertar a nota
	
	public Transform[] notesPositions;
	public GameObject playerNotePrefab; //prefabs com as notas do jogador
	public GameObject guideNotePrefab; //prefabs com as notas do guia
	public GameObject wrongNote; //prefab com as notas erradas
	public AudioSource melodyPlayer; //objeto com o audiosource
	
	public AudioClip pauseSound; //som a ser tocado quando o jogo pausa
	
	private bool paused = false;
	private bool gameOver = false;
	public enum GameState
	{
		Start,
		Guide,
		Player,
		WaitingForPlayer,
		WaitingForGuide
	}
	public GameState gameState = GameState.Start; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	
	public int currentStage = 0; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	public int stageInternalCounter = 0; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	
	private int beatLocalCounter = 0;
	private int beatPlayerCounter = 0;
	public float absoluteTime = 0; //é publica apenas para que se possa examinar seu comportamento durante o runtime; não mexer
	private List<float> rightBeats;
	
	List<int> rightNoteIndex = new List<int>();
	List<float> trio = new List<float>();
	
	public UIButton[] musicButtons;

    public void Start()
    {
		paused = true;
		
		Invoke("RealStart", 2);
    }
	
	void RealStart()
	{
		paused = false;
		gameState = GameState.Guide;
		foreach(UIButton b in musicButtons) b.controlIsEnabled = false;
		
		melodies = new AudioClip[10]
		{
			AudioClip.Create("Empty_0", Mathf.RoundToInt(44101 * melodiesDuration[0]), 2, 44100, true, true),
			AudioClip.Create("Empty_1", Mathf.RoundToInt(44101 * melodiesDuration[1]), 2, 44100, true, true),
			AudioClip.Create("Empty_2", Mathf.RoundToInt(44101 * melodiesDuration[2]), 2, 44100, true, true),
			AudioClip.Create("Empty_3", Mathf.RoundToInt(44101 * melodiesDuration[3]), 2, 44100, true, true),
			AudioClip.Create("Empty_4", Mathf.RoundToInt(44101 * melodiesDuration[4]), 2, 44100, true, true),
			AudioClip.Create("Empty_5", Mathf.RoundToInt(44101 * melodiesDuration[5]), 2, 44100, true, true),
			AudioClip.Create("Empty_6", Mathf.RoundToInt(44101 * melodiesDuration[6]), 2, 44100, true, true),
			AudioClip.Create("Empty_7", Mathf.RoundToInt(44101 * melodiesDuration[7]), 2, 44100, true, true),
			AudioClip.Create("Empty_8", Mathf.RoundToInt(44101 * melodiesDuration[8]), 2, 44100, true, true),
			AudioClip.Create("Empty_9", Mathf.RoundToInt(44101 * melodiesDuration[9]), 2, 44100, true, true),
		};
		
		for(int i = 0; i<timingPlayerList.Length; i++)
		{
			for(int f = 0; f<timingPlayerList[i].Length; f++)
			{
				timingPlayerList[i][f] = timingGuideList[i][f] + melodies[i].length;
				//UnityEngine.Debug.Log("TimingPlayerList[" + i + "][" + f + "]: " + (timingGuideList[i][f] + melodies[i].length).ToString());
			}
		}
		
		rightBeats = new List<float>();
		
		timingGuide = timingGuideList[stageInternalCounter];
		timingPlayer = timingPlayerList[stageInternalCounter];
		
		melodyPlayer.clip = melodies[stageInternalCounter];
		melodyPlayer.Play();
	}
	
    public void Update()
    {	
		PauseManager();
		
		if(!paused)
    	{
			FollowSong();
			
			PlayerInput();
		
			VerifyGuide();
		}
    }
	
	public void FollowSong()
	{
		switch(gameState)
		{
			case GameState.Player:
				absoluteTime = melodies[stageInternalCounter].length + (float)melodyPlayer.timeSamples/(float)melodies[currentStage].frequency;
				if(!melodyPlayer.isPlaying)
				{
					rightBeats = new List<float>();
					beatLocalCounter = 0;
					beatPlayerCounter = 0;
					
					if(stageInternalCounter<currentStage)
					{
						//UnityEngine.Debug.Log("proxima etapa do level (player)");
						stageInternalCounter++;
						
						timingGuide = timingGuideList[stageInternalCounter];
						timingPlayer = timingPlayerList[stageInternalCounter];
						melodyPlayer.clip = melodies[stageInternalCounter];
						melodyPlayer.Play();
					}
					else
					{
						//UnityEngine.Debug.Log("proximo level");
						gameState = GameState.WaitingForGuide;
						foreach(UIButton b in musicButtons) b.controlIsEnabled = false;
						currentStage++;
						stageInternalCounter = 0;
						
						timingGuide = timingGuideList[stageInternalCounter];
						timingPlayer = timingPlayerList[stageInternalCounter];
						melodyPlayer.clip = melodies[stageInternalCounter];
					}
				}
			break;
			case GameState.Guide:
				absoluteTime = (float)melodyPlayer.timeSamples/(float)melodies[currentStage].frequency;
				if(absoluteTime >= melodies[stageInternalCounter].length || !melodyPlayer.isPlaying)
				{
					rightBeats = new List<float>();
					beatLocalCounter = 0;
					beatPlayerCounter = 0;
					
					if(stageInternalCounter<currentStage)
					{
						//UnityEngine.Debug.Log("proxima etapa do level (guia)");
						stageInternalCounter++;
					
						timingGuide = timingGuideList[stageInternalCounter];
						timingPlayer = timingPlayerList[stageInternalCounter];
						melodyPlayer.clip = melodies[stageInternalCounter];
					
						melodyPlayer.Play();
					}
					else
					{
						foreach(UIButton b in musicButtons) b.controlIsEnabled = true;
					
						//UnityEngine.Debug.Log("enter player turn");
						gameState = GameState.WaitingForPlayer;
						stageInternalCounter = 0;
					
						timingGuide = timingGuideList[stageInternalCounter];
						timingPlayer = timingPlayerList[stageInternalCounter];
						melodyPlayer.clip = melodies[stageInternalCounter];
					}
				}
			break;
		}
		//UnityEngine.Debug.Log(absoluteTime);	
	}
	
	public void Button1()
	{
		switch(gameState)
		{
			case GameState.Player:
				VerifyPlayer(0);
			break;
			case GameState.WaitingForPlayer:
				melodyPlayer.Play();
				gameState = GameState.Player;
				VerifyPlayer(0);
			break;
			case GameState.Guide:
				UnityEngine.Debug.Log("wait for your turn!");
			break;
		}
		//PlaySoundRight(0);
	}
	
	public void Button2()
	{
		switch(gameState)
		{
			case GameState.Player:
				VerifyPlayer(1);
			break;
			case GameState.WaitingForPlayer:
				melodyPlayer.Play();
				gameState = GameState.Player;
				VerifyPlayer(1);
			break;
			case GameState.Guide:
				UnityEngine.Debug.Log("wait for your turn!");
			break;
		}
		//PlaySoundRight(1);
	}
	
	public void Button3()
	{
		switch(gameState)
		{
			case GameState.Player:
				VerifyPlayer(2);
			break;
			case GameState.WaitingForPlayer:
				melodyPlayer.Play();
				gameState = GameState.Player;
				VerifyPlayer(2);
			break;
			case GameState.Guide:
				UnityEngine.Debug.Log("wait for your turn!");
			break;
		}
		//PlaySoundRight(2);
	}
	
	public void Button4()
	{
		switch(gameState)
		{
			case GameState.Player:
				VerifyPlayer(3);
			break;
			case GameState.WaitingForPlayer:
				melodyPlayer.Play();
				gameState = GameState.Player;
				VerifyPlayer(3);
			break;
			case GameState.Guide:
				UnityEngine.Debug.Log("wait for your turn!");
			break;
		}
		//PlaySoundRight(3);
	}
	
	public void Button5()
	{
		switch(gameState)
		{
			case GameState.Player:
				VerifyPlayer(4);
			break;
			case GameState.WaitingForPlayer:
				melodyPlayer.Play();
				gameState = GameState.Player;
				VerifyPlayer(4);
			break;
			case GameState.Guide:
				UnityEngine.Debug.Log("wait for your turn!");
			break;
		}
		//PlaySoundRight(4);
	}
	
	public void PlayerInput()
	{
		switch(gameState)
		{
			case GameState.Player:
				if(Input.GetKeyDown(KeyCode.A)) VerifyPlayer(0);
				if(Input.GetKeyDown(KeyCode.S)) VerifyPlayer(1);
				if(Input.GetKeyDown(KeyCode.D)) VerifyPlayer(2);
				if(Input.GetKeyDown(KeyCode.F)) VerifyPlayer(3);
				if(Input.GetKeyDown(KeyCode.G)) VerifyPlayer(4);
			break;
			case GameState.Guide:
				if(Input.GetKeyDown(KeyCode.A)) UnityEngine.Debug.Log("wait for your turn!");
				if(Input.GetKeyDown(KeyCode.S)) UnityEngine.Debug.Log("wait for your turn!");
				if(Input.GetKeyDown(KeyCode.D)) UnityEngine.Debug.Log("wait for your turn!");
				if(Input.GetKeyDown(KeyCode.F)) UnityEngine.Debug.Log("wait for your turn!");
				if(Input.GetKeyDown(KeyCode.G)) UnityEngine.Debug.Log("wait for your turn!");
			break;
			case GameState.WaitingForPlayer:
				if(Input.GetKeyDown(KeyCode.A))
				{
					melodyPlayer.Play();
					gameState = GameState.Player;
					VerifyPlayer(0);
				}
				if(Input.GetKeyDown(KeyCode.S))
				{
					melodyPlayer.Play();
					gameState = GameState.Player;
					VerifyPlayer(1);
				}
				if(Input.GetKeyDown(KeyCode.D))
				{
					melodyPlayer.Play();
					gameState = GameState.Player;
					VerifyPlayer(2);
				}
				if(Input.GetKeyDown(KeyCode.F))
				{
					melodyPlayer.Play();
					gameState = GameState.Player;
					VerifyPlayer(3);
				}
				if(Input.GetKeyDown(KeyCode.G))
				{
					melodyPlayer.Play();
					gameState = GameState.Player;
					VerifyPlayer(4);
				}
			break;
			case GameState.WaitingForGuide:
				melodyPlayer.Play();
				gameState = GameState.Guide;
				foreach(UIButton b in musicButtons) b.controlIsEnabled = false;
			break;
		}
	}
	
	public void PauseManager()
	{	
		if(Input.GetButtonDown("Pause"))
    	{
			if(!paused)
	    	{
				UnityEngine.Debug.Log("pausei");
				melodyPlayer.PlayOneShot(pauseSound);
				Time.timeScale = 0;
				paused = true;
				melodyPlayer.Pause();
			}
			else
	    	{
				UnityEngine.Debug.Log("despausei");
				melodyPlayer.PlayOneShot(pauseSound);
				Time.timeScale = 1;
				paused = false;
				melodyPlayer.Play();
			}
		}
	}
	
	public void VerifyGuide()
	{
		//toca as batidas do guia
		if(beatLocalCounter<timingGuide.Length)
		{
			if(timingGuide[beatLocalCounter]-absoluteTime<0 && timingGuide[beatLocalCounter]-absoluteTime>-1)
			{
				PlaySoundGuide(notesButtonList[stageInternalCounter][beatLocalCounter]);
				beatLocalCounter++;
			}
		}
		
		//gerencia o momento em que deveriam entrar as batidas do player
		if(beatPlayerCounter<timingPlayer.Length)
		{
			if(timingPlayer[beatPlayerCounter]-absoluteTime<0 && timingPlayer[beatPlayerCounter]-absoluteTime>-1)
			{
				//PlaySoundRight(notesButtonList[stageInternalCounter][beatPlayerCounter]);
				beatPlayerCounter++;
			}
		}
	}
	
	public void RestartLevel()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
	
	public void NextLevel()
	{
		//mandar pra próxima cena
	}
	
	public void PlaySoundGuide(int noteIndex)
	{
		GameObject g = GameObject.Instantiate(guideNotePrefab, notesPositions[noteIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void PlaySoundRight(int noteIndex)
	{
		testText = (int.Parse(testText)+1).ToString();
		GameObject g = GameObject.Instantiate(playerNotePrefab, notesPositions[noteIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void PlaySoundWrong(int noteIndex)
	{
		GameObject g = GameObject.Instantiate(wrongNote, notesPositions[noteIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void VerifyPlayer(int noteIndex)
	{
		rightNoteIndex = new List<int>();
		trio = new List<float>();
		
		List<float> timingPlayerTransformed = new List<float>();
		foreach(float f in timingPlayer) timingPlayerTransformed.Add(f);
		foreach(float f in timingPlayerTransformed)
		{
			trio.Add(f);
			rightNoteIndex.Add(timingPlayerTransformed.IndexOf(f));
		} //transforma a array timingPlayer na list timingPlayerTransformed (elas são absolutamente iguais)
		
		RemoveByDistance(); //remove notas mais distantes que o delay máximo
		
		RemoveAlreadyPlayed(); //remove notas que o jogador já acertou
		
		float temporaryBeat = 999;
		int tempRightNote = -1;;
		int rightNote = -1;
		
		foreach(float beat in trio) // encontra a nota certa entre as notas restantes, checando qual é a nota mais perto do começo e se acertou a nota
		{
			if(beat<temporaryBeat)
			{
				tempRightNote = notesButtonList[stageInternalCounter][rightNoteIndex[trio.IndexOf(beat)]];
				int modifier = 0;
				
				if(noteIndex == tempRightNote)
				{
					temporaryBeat = beat;
					rightNote = noteIndex;
				}
				else
				{
					bool breakLoop = false;
					while(tempRightNote > 0 && !breakLoop)
					{
						tempRightNote -= 5;
						modifier++;
						if(noteIndex == tempRightNote)
						{
							temporaryBeat = beat;
							rightNote = tempRightNote + modifier*5;
							breakLoop = true;
						}
					}
				}
			}
		}
		
		if(temporaryBeat!=999)
		{
			//UnityEngine.Debug.Log("nota final: " + tempRightNote);
			PlaySoundRight(rightNote);
			
			rightBeats.Add(temporaryBeat); //adiciona na list de notas certas a nota tocada e todas que vieram antes dela
			for(int i = 0; i < timingPlayer.Length; i++)
			{
				if(timingPlayer[i] < temporaryBeat)
				{
					bool doNotAdd = false;
					for(int j = 0; j < rightBeats.Count; j++)
					{
						if(timingPlayer[i] == rightBeats[j])
						{
							doNotAdd = true;
						}
					}
					if(!doNotAdd)
					{
						rightBeats.Add(timingPlayer[i]);
					}
				}
			}
		}
		else
		{
			if(trio.Count==0) UnityEngine.Debug.Log("errei timing");
			else  UnityEngine.Debug.Log("errei nota");
			PlaySoundWrong(noteIndex);
		}
	}
	
	void RemoveByDistance()
	{
		for(int i = 0; i<trio.Count; i++)
		{
			if((trio[i]-absoluteTime)>allowedDelay || (trio[i]-absoluteTime)*-1>allowedDelay)
			{
				trio.RemoveAt(i);
				rightNoteIndex.RemoveAt(i);
				RemoveByDistance();
			}
		}
	}
	
	void RemoveAlreadyPlayed()
	{
		for(int i = 0; i<rightBeats.Count; i++)
		{
			for(int j = 0; j<trio.Count; j++)
			{
				if(rightBeats[i]==trio[j])
				{
					trio.RemoveAt(j);
					rightNoteIndex.RemoveAt(j);
					RemoveAlreadyPlayed();
				}
			}
		}
	}
	
	string testText = "0";
	void OnGUI()
	{
		GUI.Label(new Rect(100,100,300,300), testText);
	}
}

		/* OLD
		//remove notas já tocadas, notas com distância maior que o delay máximo e notas erradas
		for(int i = 0; i<rightBeats.Count; i++)
		{
			for(int j = 0; j<trio.Count; j++)
			{
				if(rightBeats[i]==trio[j])
				{
					trio.RemoveAt(j);
					rightNoteIndex.RemoveAt(j);
				}
				else if(notesButtonList[stageInternalCounter][rightNoteIndex[j]]!=noteIndex)
				{
					bool brokenLoop = false;
					int modifier = 0;
					float temporaryIndex = notesButtonList[stageInternalCounter][rightNoteIndex[j]];
					while(temporaryIndex>=5 && !brokenLoop)
					{
						modifier++;
						temporaryIndex -= 5;
						if(temporaryIndex==noteIndex)
						{
							fivePlus.Add(new Vector2(trio[j], noteIndex + 5 * modifier));
							brokenLoop = true;
						}
						else if(temporaryIndex<0)
						{
							trio.RemoveAt(j);
							rightNoteIndex.RemoveAt(j);
							brokenLoop = true;
						}
					}
				}
				else
				{
					float difference = trio[j]-absoluteTime;
					if(difference<0)
					{
						difference *= -1;
					}
					if(difference>allowedDelay)
					{
						trio.RemoveAt(j);
						rightNoteIndex.RemoveAt(j);
					}
				}
			}
		}
		
		//encontra qual das 3 é a nota mais velha
		float temporaryBeat = 200;
		int temporaryNote = 200;
		foreach(float beat in trio)
		{
			if(beat<temporaryBeat)
			{
				UnityEngine.Debug.Log("diff " + (beat-absoluteTime).ToString() + ", right note " + rightNoteIndex[trio.IndexOf(beat)].ToString());
				temporaryBeat = beat;
				temporaryNote = rightNoteIndex[trio.IndexOf(beat)];
			}
		}
		
		//toca nota certa ou errada, finalmente
		if(trio.Count>0)
		{
			foreach(Vector2 v in fivePlus)
			{
				if(v.x == temporaryBeat)
				{
					rightBeats.Add(temporaryBeat);
					PlaySoundRight(Mathf.RoundToInt(v.y));
					return;
				}
			}
			rightBeats.Add(temporaryBeat);
			PlaySoundRight(noteIndex);
		}
		else
		{
			PlaySoundWrong(noteIndex);
		}*/
		
		/*int temporaryNote = 200;
		float temporaryBeat = 200;
		float distance = 200;
		foreach(float beat in trio)
		{
			//UnityEngine.Debug.Log("index: " + noteIndex + "beat: " + beat);
			float difference = beat-absoluteTime;
			if(difference<0)
			{
				difference *= -1;
			}
			if(difference<distance)
			{
				distance = difference;
				temporaryBeat = beat;
				temporaryNote = rightNoteIndex[trio.IndexOf(beat)];
			}
		}
		//UnityEngine.Debug.Log(distance);
		if(distance<allowedDelay)
		{
			//checar se está tocando a nota correta
			int tempCounter = 0;
			int tempIndex = notesButtonList[stageInternalCounter][temporaryNote];
			if(noteIndex == tempIndex)
			{
				rightBeats.Add(temporaryBeat);
				PlaySoundRight(noteIndex);
			}
			else
			{
				while(tempIndex>0)
				{
					tempIndex -= 5;
					tempCounter++;
					if(noteIndex == tempIndex)
					{
						rightBeats.Add(temporaryBeat);
						PlaySoundRight(noteIndex+tempCounter*5);
						return;
					}
				}
				
				PlaySoundWrong(noteIndex);
			}
			
			//UnityEngine.Debug.Log(rightBeats.ToArray().Length - playerSequence.Length);
		}
		else
		{
			PlaySoundWrong(noteIndex);
		}*/