using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public class SoundDetectorImproved : MonoBehaviour
{
	//aqui deve ser preenchido com todas as músicas, as notas e escalas de cada música e as informações de cada nota de cada level de cada música
	public List<Music> musicsList;
	[System.Serializable] public class Music
	{
		public List<Notes> notesList;
		public List<int> notesHeightList;
		public List<MusicLevel> allLevels;
		
		[System.Serializable] public class MusicLevel
		{
			public List<float> timingGuideList;
			public List<int> notesButtonList;
			public float melodyDuration;
		}
	}
	public int currentMusic = 0; //preencher com o index da música atual
	public float allowedDelay = 0.1f; //desvio máximo do tempo exato para acertar a nota
	public GameObject playerNotePrefab; //prefabs com as notas do jogador
	public GameObject guideNotePrefab; //prefabs com as notas do guia
	public GameObject wrongNote; //prefab com as notas erradas
	public AudioSource melodyPlayer; //objeto com o audiosource
	public AudioClip pauseSound; //som a ser tocado quando o jogo pausa
	public UIButton[] musicButtons; //botões EZGUI que tocam as cinco notas
	public Transform[] notesPositions; //transforms com a posição de cada partícula de cada nota
	
	private List<List<float>> timingGuideList;
	private List<List<float>> timingPlayerList;
	private List<float> timingGuide;
	private List<float> timingPlayer;
	private List<float> melodiesDuration;
	private List<List<int>> notesButtonList;
	private List<Notes> notesList;
	private List<int> notesHeightList;
	private List<AudioClip> melodies;
	private List<float> rightBeats;
	private List<int> rightNoteIndex;
	private List<float> trio;
	
	private bool paused = false;
	private bool gameOver = false;
	private enum GameState
	{
		Start,
		Guide,
		Player,
		WaitingForPlayer,
		WaitingForGuide
	}
	private GameState gameState = GameState.Start;
	
	private int currentStage = 0;
	private int beatLocalCounter = 0;
	private int beatPlayerCounter = 0;
	private float absoluteTime = 0;
	
	public GameObject character;
	
	public void Restart()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

    public void Start()
    {
		//pausa tudo no começo, para ajeitar as arrays
		paused = true;
		
		//começa com os botões desligados
		foreach(UIButton b in musicButtons) b.controlIsEnabled = false;
		
		PrepareCurrentMusic();
		
		Invoke("RealStart", 2);
    }
	
	void PrepareCurrentMusic()
	{
		//povoa notesList com as notas da música atual e notesHeightList com as oitavas das notas da música atual
		notesList = new List<Notes>(musicsList[currentMusic].notesList);
		notesHeightList = new List<int>(musicsList[currentMusic].notesHeightList);
		
		//povoa a melodiesDuration com a duração de cada level
		melodiesDuration = new List<float>();
		foreach(Music.MusicLevel m in musicsList[currentMusic].allLevels) melodiesDuration.Add(m.melodyDuration);
		
		//insere a duração certa em melodiesDuration e cria todos os audioclips
		melodies = new List<AudioClip>();
		for(int i = 0; i<melodiesDuration.Count; i++)
		{
			if(i>0) melodiesDuration[i] += melodiesDuration[i-1];
		}
		for(int i = 0; i<melodiesDuration.Count; i++)
		{
			melodies.Add(AudioClip.Create("Empty_" + i.ToString(), Mathf.RoundToInt(44100 * melodiesDuration[i]), 2, 44100, false, false));
		}
		
		//povoa timingGuideList e notesButtonList com os valores da música atual
		timingGuideList = new List<List<float>>();
		notesButtonList = new List<List<int>>();
		for(int i = 0; i<musicsList[currentMusic].allLevels.Count; i++)
		{
			float tempFloat = 0;
			List<float> tempLevelList = new List<float>();
			List<int> tempNoteList = new List<int>();
			for(int k = 0; k<i; k++)
			{
				UnityEngine.Debug.Log("adicionar todas as notas do level " + k + " no level " + i);
				for(int j = 0; j<musicsList[currentMusic].allLevels[k].timingGuideList.Count; j++)
				{
					tempFloat = musicsList[currentMusic].allLevels[k].timingGuideList[j];
					if(k>0) tempFloat += melodiesDuration[k-1];
					//UnityEngine.Debug.Log("adicionei " + tempFloat + " no level " + i);
					tempLevelList.Add(tempFloat);
					tempNoteList.Add(musicsList[currentMusic].allLevels[k].notesButtonList[j]);
				}
			}
			
			for(int j = 0; j<musicsList[currentMusic].allLevels[i].timingGuideList.Count; j++)
			{
				tempFloat = musicsList[currentMusic].allLevels[i].timingGuideList[j];
				if(i>0) tempFloat += melodiesDuration[i-1];
				
				//UnityEngine.Debug.Log("adicionei " + tempFloat + " no level " + i);
				tempLevelList.Add(tempFloat);
				tempNoteList.Add(musicsList[currentMusic].allLevels[i].notesButtonList[j]);
			}
			
			timingGuideList.Add(tempLevelList);
			notesButtonList.Add(tempNoteList);
		}
		
		//iguala timingPlayerList a timingGuideList
		timingPlayerList = new List<List<float>>();
		for(int i = 0; i<timingGuideList.Count; i++)
		{
			timingPlayerList.Add(new List<float>(timingGuideList[i]));
			for(int f = 0; f<timingPlayerList[i].Count; f++)
			{
				timingPlayerList[i][f] = timingGuideList[i][f] + melodies[i].length;
			}
		}
		
		//zera array de notas que o jogador acertou, array dos indexes certos e array das três notas mais próximas da nota certa
		rightBeats = new List<float>();
		rightNoteIndex = new List<int>();
		trio = new List<float>();
		
		//coloca a sequência de notas do primeiro level nas arrays timingGuide e timingPlayer
		timingGuide = new List<float>(timingGuideList[currentStage]);
		timingPlayer = new List<float>(timingPlayerList[currentStage]);
		
		//coloca o audioclip certo para começar tocando
		melodyPlayer.clip = melodies[currentStage];
	}
	
	void RealStart()
	{
		paused = false;
		gameState = GameState.Guide;
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
				absoluteTime = melodies[currentStage].length + (float)melodyPlayer.timeSamples/(float)melodies[currentStage].frequency;
				if(!melodyPlayer.isPlaying)
				{
					rightBeats = new List<float>();
					beatLocalCounter = 0;
					beatPlayerCounter = 0;
				
					//UnityEngine.Debug.Log("proximo level");
					gameState = GameState.WaitingForGuide;
					foreach(UIButton b in musicButtons) b.controlIsEnabled = false;
					currentStage++;
					
					timingGuide = new List<float>(timingGuideList[currentStage]);
					timingPlayer = new List<float>(timingPlayerList[currentStage]);
					melodyPlayer.clip = melodies[currentStage];
				}
			break;
			case GameState.Guide:
				absoluteTime = (float)melodyPlayer.timeSamples/(float)melodies[currentStage].frequency;
				if(absoluteTime >= melodies[currentStage].length || !melodyPlayer.isPlaying)
				{
					rightBeats = new List<float>();
					beatLocalCounter = 0;
					beatPlayerCounter = 0;
				
					foreach(UIButton b in musicButtons) b.controlIsEnabled = true;
				
					//UnityEngine.Debug.Log("enter player turn");
					gameState = GameState.WaitingForPlayer;
				
					timingGuide = new List<float>(timingGuideList[currentStage]);
					timingPlayer = new List<float>(timingPlayerList[currentStage]);
					melodyPlayer.clip = melodies[currentStage];
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
		if(beatLocalCounter<timingGuide.Count)
		{
			if(timingGuide[beatLocalCounter]-absoluteTime<0 && timingGuide[beatLocalCounter]-absoluteTime>-1)
			{
				PlaySoundGuide(notesButtonList[currentStage][beatLocalCounter]);
				beatLocalCounter++;
			}
		}
		
		//gerencia o momento em que deveriam entrar as batidas do player
		if(beatPlayerCounter<timingPlayer.Count)
		{
			if(timingPlayer[beatPlayerCounter]-absoluteTime<0 && timingPlayer[beatPlayerCounter]-absoluteTime>-1)
			{
				//PlaySoundRight(notesButtonList[currentStage][beatPlayerCounter]);
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
		int positionIndex = noteIndex;
		while(positionIndex>=musicButtons.Length)
		{
			positionIndex -= musicButtons.Length;
		}
		
		GameObject g = GameObject.Instantiate(guideNotePrefab, notesPositions[positionIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void PlaySoundRight(int noteIndex)
	{
		int positionIndex = noteIndex;
		while(positionIndex>=musicButtons.Length)
		{
			positionIndex -= musicButtons.Length;
		}
		
		testText = (int.Parse(testText)+1).ToString();
		GameObject g = GameObject.Instantiate(playerNotePrefab, notesPositions[positionIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void PlaySoundWrong(int noteIndex)
	{
		int positionIndex = noteIndex;
		while(positionIndex>=musicButtons.Length)
		{
			positionIndex -= musicButtons.Length;
		}
		
		GameObject g = GameObject.Instantiate(wrongNote, notesPositions[positionIndex].position, Quaternion.identity) as GameObject;
		g.GetComponent<PitchManager>().Go(notesList[noteIndex], notesHeightList[noteIndex], noteIndex);
	}
	
	public void VerifyPlayer(int noteIndex)
	{
		rightNoteIndex = new List<int>();
		trio = new List<float>();
		
		foreach(float f in timingPlayer)
		{
			trio.Add(f);
			rightNoteIndex.Add(timingPlayer.IndexOf(f));
		}
		
		RemoveByDistance(); //remove notas mais distantes que o delay máximo
		
		RemoveAlreadyPlayed(); //remove notas que o jogador já acertou
		
		float temporaryBeat = 999;
		int tempRightNote = -1;;
		int rightNote = -1;
		
		foreach(float beat in trio) // encontra a nota certa entre as notas restantes, checando qual é a nota mais perto do começo e se acertou a nota
		{
			if(beat<temporaryBeat)
			{
				tempRightNote = notesButtonList[currentStage][rightNoteIndex[trio.IndexOf(beat)]];
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
			for(int i = 0; i < timingPlayer.Count; i++)
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
			
			character.animation.CrossFadeQueued("guitar"+(noteIndex+1).ToString(), 0.1f, QueueMode.PlayNow);
		}
		else
		{
			if(trio.Count==0) UnityEngine.Debug.Log("errei timing");
			else  UnityEngine.Debug.Log("errei nota");
			PlaySoundWrong(noteIndex);
			
			character.animation.CrossFadeQueued("guitar"+(noteIndex+1).ToString(), 0.1f, QueueMode.PlayNow);
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