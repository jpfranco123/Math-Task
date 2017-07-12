using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour {

	//Timer width
	//public static float timerWidth =400;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	private String question;

	//Should the key be working?
	public static bool keysON = false;

	//Shows the question on the screen
	public void setQuestion(){
		int randInstance = GameManager.instanceRandomization[GameManager.trial-1];
		question = GameManager.instances [randInstance].question;// + " = ?";
		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		Quest.text = question;
	}

	/// Macro function that initializes the Board
	/// 1: Trial / 2: trial game answer
	public void SetupScene(int sceneToSetup)
	{
		if (sceneToSetup == 1) {
			setQuestion ();

			InputField ansField = GameObject.Find("Answer1").GetComponent<InputField>();

			InputField.SubmitEvent se = new InputField.SubmitEvent();
			se.AddListener((value)=>submitAnswer(value, ""));
			ansField.onEndEdit = se;
		}
	}

	//Updates the timer rectangle size accoriding to the remaining time.
	public void updateTimer(){
		// timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
		// timer.sizeDelta = new Vector2 (timerWidth * (GameManager.tiempo / GameManager.totalTime), timer.rect.height);
		Image timer = GameObject.Find ("Timer").GetComponent<Image> ();
		timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
	}

	//Sets the triggers for pressing the corresponding keys
	//123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	//changeToNextScene(answer) on game manager
	private void setKeyInput(){

	 if (GameManager.escena == 0) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				GameManager.setTimeStamp ();
				GameManager.changeToNextScene("", 2);
			}
		}
	}
		

	public void setupInitialScreen(){

		//Button 
		GameObject start = GameObject.Find("Start") as GameObject;
		start.SetActive (false);

		InputField pID = GameObject.Find ("ParticipantID").GetComponent<InputField>();

		InputField.SubmitEvent se = new InputField.SubmitEvent();
		se.AddListener((value)=>submitPID(value,start));
		pID.onEndEdit = se;

	}

	private void submitPID(string pIDs, GameObject start){

		//Debug.Log (pIDs);

		GameObject pID = GameObject.Find ("ParticipantID");
		pID.SetActive (false);

		//Set Participant ID
		GameManager.participantID=pIDs;

		//Activate Start Button and listener
		start.SetActive (true);
		keysON = true;

	}

	private void submitAnswer(string answer, string other){
		//Save
		if(Input.GetButtonDown("Submit")){
			GameManager.changeToNextScene(answer,1);
		}

	}

	public string getAnswer(){
//		GameObject answerGO = GameObject.Find ("Answer");//.gameObject
//		string answer = answerGO.GetComponentInChildren<Text>().text;

		GameObject answerGO = GameObject.Find ("Answer1");
		string answer = answerGO.GetComponent<InputField>().text;


		//InputField answerIF = GameObject.Find ("Answer1").GetComponent<InputField>();
		//string answer = answerIF.GetComponentInChildren<Text>().text;
		//string answer = answerIF.text;
		//Debug.Log (answer);

		return answer;
		//return "";

	}

	// Use this for initialization
	void Start () {
		//GameManager.saveTimeStamp(GameManager.escena);
	}

	// Update is called once per frame
	void Update () {
		
		if (keysON) {
			setKeyInput ();
		}

	}
		
}