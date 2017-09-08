
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
//using System.Diagnostics;

public class GameManager : MonoBehaviour {

	//Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)
	public static GameManager instance=null;

	//The reference to the script managing the board (interface/canvas).
	private BoardManager boardScript;

	//Current Scene
	public static int escena;

	//Time spent so far on this scene
	public static float tiempo;

	//Total time for these scene
	public static float totalTime;

	//Current trial initialization
	public static int trial = 0;

	//Current block initialization
	public static int block = 0;

	//Total trial (As if no blocks were used)
	public static int generalTrial=0;

	private static bool showTimer;


	//Modifiable Variables:
	//Minimum and maximum for randomized interperiod Time
	public static float timeRest1min=5;
	public static float timeRest1max=9;

	//InterBlock rest time
	public static float timeRest2=10;

	//public static float timeRest1;

	//Time given for each trial (The total time the items are shown -With and without the question-)
	public static float timeTrial=10;

	//Total number of trials in each block
	private static int numberOfTrials = 30;

	//Total number of blocks
	private static int numberOfBlocks = 3;

	//Number of instance file to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances = 3;

	//The order of the instances to be presented
	public static int[] instanceRandomization;

	//This is the string that will be used as the file name where the data is stored. DeCurrently the date-time is used.
	public static string participantID = "Empty";

	public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

	private static string identifierName;

	//Is the question shown on scene scene 1?
	private static int questionOn;

	//Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/DATAinf/Input/";
	private static string inputFolderInstances = "/DATAinf/Input/";
	private static string outputFolder = "/DATAinf/Output/";

	// Stopwatch to calculate time of events.
	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	// Time at which the stopwatch started. Time of each event is calculated according to this moment.
	private static string initialTimeStamp;

	private static bool soundON =false;


	//A structure that contains the parameters of each instance
	public struct Instance
	{
		public string question;
		public string solution;
	}

	//An array of all the instances to be uploaded form .txt files.
	public static Instance[] instances;

	// Use this for initialization
	void Awake () {

		//Makes the Gama manager a Singleton
		if (instance == null) {
			instance = this;
		}
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		//Initializes the game
		boardScript = instance.GetComponent<BoardManager> ();

		InitGame();

	}


	//Initializes the scene. One scene is setup, other is trial, other is Break....
	void InitGame(){

		/*
		Scene Order: escena
		0=setup
		1=trial game
		2=trial game answer
		3= intertrial rest
		4= interblock rest
		5= end
		*/
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.buildIndex;
		Debug.Log ("escena" + escena);
		if (escena == 0) {
			//Only uploads parameters and instances once.
			block++;
			loadParameters ();
			loadInstances ();
			boardScript.setupInitialScreen ();

		} else if (escena == 1) {
			trial++;
			generalTrial = trial + (block - 1) * numberOfTrials;
			showTimer = true;
			boardScript.SetupScene (1);

			tiempo = timeTrial;
			totalTime = timeTrial;

		} else if (escena == 2) {
			showTimer = false;
			tiempo = Random.Range (timeRest1min, timeRest1max);
			totalTime = tiempo;
		} else if (escena == 3) {
			trial = 0;
			block++;
			showTimer = true;
			tiempo = timeRest2;
			totalTime = tiempo;
		}

	}

	// Update is called once per frame
	void Update () {

		if (escena != 0) {
			startTimer ();
			pauseManager ();
		}
	}

	//To pause press alt+p
	//Pauses/Unpauses the game. Unpausing take syou directly to next trial
	//Warning! When Unpausing the following happens:
	//If paused/unpaused in scene 1 or 2 (while items are shown or during answer time) then saves the trialInfo with an error: "pause" without information on the items selected.
	//If paused/unpaused on ITI or IBI then it generates a new row in trial Info with an error ("pause"). i.e. there are now 2 rows for the trial.
	private void pauseManager(){
		if (( Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) && Input.GetKeyDown (KeyCode.P) ){
			Time.timeScale = (Time.timeScale == 1) ? 0 : 1;
			if(Time.timeScale==1){
				errorInScene("Pause");
			}
		}
	}

	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(string answer, float timeSpent, int submitted , string error) {

		//Get the instance n umber for this trial and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
		int instanceNum = instanceRandomization [generalTrial - 1] + 1;

		string solution = instances [instanceNum - 1].solution;
		string question = instances [instanceNum - 1].question;

		string dataTrialText = block + ";" + trial + ";" + instanceNum + ";" + question + ";" + solution + ";" + answer + ";" + submitted + ";" + timeSpent + ";" + error;
		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName +"TrialInfo.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}
	}

	private void playSound(){
		if (soundON) {
			//int samplerate = 44100;
			//AudioClip myClip = AudioClip.Create("MySinusoid", samplerate * 2, 1, samplerate, true);
			AudioSource aud = GetComponent<AudioSource> ();
			//aud.clip = myClip;
			aud.Play ();
		}
	}

	/// <summary>
	/// Saves the headers for both files (Trial Info and Time Stamps)
	/// In the trial file it saves:  1. The participant ID. 2. Instance details.
	/// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
	/// </summary>
	private static void saveHeaders(){

		identifierName = participantID + "_" + dateID + "_" + "Math" + "_";
		string folderPathSave = Application.dataPath + outputFolder;

		// Trial Info file headers
		string[] lines = new string[2];
		lines[0]="PartcipantID:" + participantID;
		lines [1] = "block" + ";trial" + ";instanceNumber" + ";question" + ";solution" + ";answer" + ";submitted" + ";timeSpent" + ";error";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}
	}

	/*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt
	 * Structure of each file is the following:
	 * weights:[2,5,8,10,11,12]
	 * values:[10,8,3,9,1,4]
	 * capacity:15
	 * profit:16
	 */
	public static void loadInstances(){
		string folderPathLoad = Application.dataPath + inputFolderInstances;
		instances = new Instance[numberOfInstances];
		try {   // Open the text file using a stream reader.
			using (StreamReader sr = new StreamReader (folderPathLoad + "instances.csv")) {
				string line;
				int i=-1;
				while (!string.IsNullOrEmpty((line = sr.ReadLine())))
				{
					if (i==-1){
						i=i+1;
					} else {
						string[] tmp = line.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
						instances[i].question=tmp[0];
						instances[i].solution=tmp[1];
						//int.Parse(dict[tmp[1]]);
						i=i+1;
					}
				}
			}
		} catch (Exception e) {
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
		}
	}

	//Loads the parameters form the text files: param.txt and layoutParam.txt
	void loadParameters(){
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
		string folderPathLoad = Application.dataPath + inputFolder;
		var dict = new Dictionary<string, string>();

		try {   // Open the text file using a stream reader.

			using (StreamReader sr1 = new StreamReader (folderPathLoad + "param.txt")) {

				// (This loop reads every line until EOF or the first blank line.)
				string line1;
				while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
 				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line1.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

			using (StreamReader sr2 = new StreamReader (folderPathLoad + "param2.txt")) {

				// (This loop reads every line until EOF or the first blank line.)
				string line2;
				while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line2.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

		} catch (Exception e) {
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
		}

		assignVariables(dict);

	}

	//Assigns the parameters in the dictionary to variables
	void assignVariables(Dictionary<string,string> dictionary){

		//Assigns Parameters
		string timeRest1minS;
		string timeRest1maxS;
		string timeRest2S;
		string timeTrialS;
		string numberOfTrialsS;
		string numberOfBlocksS;
		string numberOfInstancesS;
		string instanceRandomizationS;

		dictionary.TryGetValue ("timeRest1min", out timeRest1minS);
		dictionary.TryGetValue ("timeRest1max", out timeRest1maxS);
		dictionary.TryGetValue ("timeRest2", out timeRest2S);

		dictionary.TryGetValue ("timeTrial", out timeTrialS);

		dictionary.TryGetValue ("numberOfTrials", out numberOfTrialsS);

		dictionary.TryGetValue ("numberOfBlocks", out numberOfBlocksS);

		dictionary.TryGetValue ("numberOfInstances", out numberOfInstancesS);

		timeRest1min=Convert.ToSingle (timeRest1minS);
		timeRest1max=Convert.ToSingle (timeRest1maxS);
		timeRest2=Convert.ToSingle (timeRest2S);
		timeTrial=Int32.Parse(timeTrialS);
		numberOfTrials=Int32.Parse(numberOfTrialsS);
		numberOfBlocks=Int32.Parse(numberOfBlocksS);
		numberOfInstances=Int32.Parse(numberOfInstancesS);

		dictionary.TryGetValue ("instanceRandomization", out instanceRandomizationS);
		//If instanceRandomization is not included in the parameters file. It generates a randomization.
		if (!dictionary.ContainsKey("instanceRandomization")){
			RandomizeKSInstances();
		} else{
			int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring (1, instanceRandomizationS.Length - 2).Split (','), int.Parse);
			instanceRandomization = new int[instanceRandomizationNo0.Length];
			for (int i = 0; i < instanceRandomizationNo0.Length; i++){
				instanceRandomization[i] = instanceRandomizationNo0 [i] - 1;
			}
		}

	}


	//Randomizes the sequence of Instances to be shown to the participant adn stores it in: instanceRandomization
	void RandomizeKSInstances(){

		instanceRandomization = new int[numberOfTrials*numberOfBlocks];

		List<int> randomizationTemp = new List<int> ();

		for (int i = 0; i < numberOfTrials*numberOfBlocks; i++){
			randomizationTemp.Add(i);
		}

		for (int i = 0; i < numberOfTrials*numberOfBlocks; i++) {
			int randomIndex = Random.Range (0, randomizationTemp.Count);
			instanceRandomization [i] = randomizationTemp [randomIndex];
			randomizationTemp.RemoveAt (randomIndex);
		}

	}
		


	//Takes care of changing the Scene to the next one (Except for when in the setup scene)
	public static void changeToNextScene(String answer, int submitted){
		//BoardManager.keysON = false;
		if (escena == 0) {
			saveHeaders ();
			SceneManager.LoadScene (1);
		}
		else if (escena == 1) {
			save(answer, timeTrial - tiempo, submitted , "");
			SceneManager.LoadScene (2);
		} else if (escena == 2) {
			changeToNextTrial ();
		} else if (escena == 3) {
			SceneManager.LoadScene (1);
		}

	}

	//Redirects to the next scene depending if the trials or blocks are over.
	private static void changeToNextTrial(){
		//Checks if trials are over
		if (trial < numberOfTrials) {
			SceneManager.LoadScene (1);
		} else if (block < numberOfBlocks) {
			SceneManager.LoadScene (3);
		} else {
			SceneManager.LoadScene (4);
		}
	}

	/// <summary>
	/// In case of an error: Skip trial and go to next one.
	/// Example of error: Not enough space to place all items
	/// </summary>
	/// Receives as input a string with the errorDetails which is saved in the output file.
	public static void errorInScene(string errorDetails){
		Debug.Log (errorDetails);

		//BoardManager.keysON = false;
		string answer = "";
		save (answer, timeTrial, 0, errorDetails);
		changeToNextTrial();
	}


	/// <summary>
	/// Starts the stopwatch. Time of each event is calculated according to this moment.
	/// Sets "initialTimeStamp" to the time at which the stopwatch started.
	/// </summary>
	public static void setTimeStamp(){
		initialTimeStamp=@System.DateTime.Now.ToString("HH-mm-ss-fff");
		stopWatch.Start ();
		Debug.Log (initialTimeStamp);
	}

	/// <summary>
	/// Calculates time elapsed
	/// </summary>
	/// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
	private static string timeStamp(){
		long milliSec = stopWatch.ElapsedMilliseconds;
		string stamp = milliSec.ToString();
		return stamp;
	}


	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene.
	void startTimer(){
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);
		if (showTimer) {
			boardScript.updateTimer();
//			RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
//			timer.sizeDelta = new Vector2 (timerWidth * (tiempo / timeTrial), timer.rect.height);
		}

		//When the time runs out:
		if(tiempo < 0)
		{	
			string answer = "";
			if (escena == 1) {
				answer = boardScript.getAnswer ();
			} 
			changeToNextScene(answer,0);


		}
	}


}
