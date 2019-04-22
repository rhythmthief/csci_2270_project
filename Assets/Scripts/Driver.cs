using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/*
	Notes:
		This is the main driver file used to power the entire project
		C# doesn't use header files, everything is automatically visible to other parts of the project without #inclusion
		I am using "class" to define LL nodes throughout my data structures, since in C# a class is reference type and can be handled like a pointer
		Default access level within Unity is private

	Gameplay:
		The player starts with a single revealed node and sees one random piece of information for every connected node as well as for their own immediate connections, but the exact relationship to the original node is not specified. The goal is to match all names with their respective nodes. Nodes are verified as name + at least 1 connection. The player works against a clock, verification and clues cost time. The goal is to reveal the entire graph.

	Gameplay loop:
		0) The player starts with one revealed profile
		1) The player inspects a revealed profile and its surroundings, requests information from the revealed profile
		2) The player examines all clues and compares them against a list of profiles available in a section on-screen
		3) The player matches a node with a name based on provided clues and connects it
		4) The player verifies their guess
		5) Go to step (1) unless the entire graph has been uncovered



	Used Data Structures:
		Graph -- the underlying data structure of this project
		HashTable -- heavily used during graph generation in Helpers
		StackLL -- A stack implemented using a linked list

	Implemented, but deprecated:
		LinkedList -- I fully implemented a linked list, but dropped its use in favor of a better-optimized approach without the use of a data structure
		AdjacencyLL -- I initially planned to use an adjacency list for my graph and wrote code for it, but ended up using an adjacency matrix instead
		
	Generic (from the libraries):
		List (vector)
		Queue
*/

public class Driver : MonoBehaviour
{
	/* Graph */
	internal int size;
	internal Graph gameGraph;

	/* Helper */
	internal Helpers helpers = new Helpers(); //I am calling the helper functions I need through this instance, seeing how I can't inherit from both MonoBehavior and Helpers

	/* Scoring */
	internal int cluesUsed = 0;
	internal int verticesComplete = 0; //Counter for the total number of vertices fully guessed by the player

	/* Game logics */
	internal float timer;
	internal bool gameFinished = false;
	internal int[] edgeCheck; //Stores the indexes of two vertices we want to check for an existing edge
	internal bool verifying = false; //Determines whether the UI is in verification mode

	/* Visuals */
	internal Text timerDisplay;
	internal GameObject marker0; //Used as a visual indicator for a selected node
	internal GameObject marker1; //Used as a visual indicator for a selected node
	internal GameObject particlePrefab; //Used to spawn particle systems
	internal GameObject[] menu; //GameObject which collects the menu components for vertex interaction
						   /* Menu elements:
							   0 -- data text box
							   1 -- dropdown with names
							   2 -- verify button
							   3 -- clue button
							   4 -- submit button
							   5 -- cancel button
							   6 -- text box with all names
							   7 -- clue text box
							   8 -- timer
							   9 -- restart
							   10 -- quit
							   11 -- tutorial button
							   12 -- tutorial text box
							   13 -- message text box
						    */

	/* The code below is executed before the first frame is rendered by the Unity engine */
	void Start()
	{
		Application.targetFrameRate = 60; //Sets the target framerate for this application

		/* Graph generation */
		size = 10; //Size of the graph to generate. Has to be at least 2
		gameGraph = helpers.buildGraph(size); //Generates a graph
		int temp = 0; //Used as a counter for building a visual graph below
		gameGraph.printVertices(); //Quick debug function, prints all generated vertices in the engine log

		/* This call goes to the pre-exiting GameObject with an attached visualVertex script and builds up the graph from there. */
		transform.GetChild(0).GetChild(0).gameObject.GetComponent<VisualVertex>().buildVisualGraph(0, 0, 0, ref temp, gameGraph.getSize(), gameGraph);

		/* Visuals */
		particlePrefab = transform.GetChild(2).gameObject; //Located a particle GameObject to make copies of
		marker0 = transform.GetChild(3).gameObject; //Visual markers displayed to the player
		marker1 = transform.GetChild(4).gameObject;
		menu = new GameObject[14];
		timer = 300f; //Default time

		for (int i = 0; i < 14; i++) //Sets references to in-game menu elements (GameObjects)
			menu[i] = transform.GetChild(1).GetChild(i).gameObject;

		timerDisplay = menu[8].transform.GetChild(0).GetComponent<Text>(); //A text box which will be used as a timer

		edgeCheck = new int[2] { size - 1, -1 }; //Setting default values for the game start

		//Updating menu elements
		menuUpdateStatic();
		menuUpdateActive(gameGraph.getCount() - 1);

		//the last vertex needs special rules -- I'll be showing its name from the start
		gameGraph.getVertex(size - 1).visual.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = gameGraph.getVertex(size - 1).name;
	}

	/* Calculates the endgame score and enables the buttons to restart or quit 
	True -- the player won. */
	internal void gameOver(bool mode)
	{
		string message;
		float score = 0;

		if (mode)
			message = "You won! Final score:\n";
		else
			message = "You lost. Final score:\n";

		message += "Time left: + " + Mathf.Floor(timer) + " x 5\n"
				+ "Vertices matched: + " + verticesComplete + " x 15\n"
				+ "Clues used: - " + cluesUsed + " x 10\n";

		score = Mathf.Floor(timer) * 5 + verticesComplete * 15 - cluesUsed * 10;
		message += "Total: " + score;
		displayMessage(message, 120f);
	}

	/* Displays a message for the player on-screen */
	internal void displayMessage(string _message, float _time)
	{
		//Launches a coroutine, which is executed per-frame similar to Update()
		StartCoroutine(displayTimeout(_message, _time));
	}

	/* Displays a message and keeps it up for _time seconds */
	internal IEnumerator displayTimeout(string _message, float _time)
	{
		//Assigns the new message and activates a game object
		menu[13].transform.GetChild(0).GetComponent<Text>().text = _message;
		menu[13].SetActive(true);

		//Sits idly for a given number of seconds
		yield return new WaitForSeconds(_time);

		//Disables the game object and shuts down the coroutine once we reach the end
		menu[13].SetActive(false);
	}

	/* Updates the active vertex menu based on a selected vertex */
	internal void menuUpdateActive(int _id)
	{
		//Updates the text box with active vertex info
		menu[0].transform.GetChild(0).GetComponent<Text>().text = "ACTIVE VERTEX:\n" + helpers.concatenateData(gameGraph.getVertex(_id), false, null);

		//Updates the clues window
		menu[7].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gameGraph.getClue(_id);
	}

	/* Updates static components of the menu:
	loads all profiles into the list located on the left side of the screen
	loads names into the dropdown menu */
	internal void menuUpdateStatic()
	{
		string data = "";
		Vertex vx;
		List<string> namesList = new List<string>(); //The dropdown menu takes data in the form of a list

		//Generating a string for the profile list
		for (int i = 0; i < gameGraph.getCount(); i++)
		{
			vx = gameGraph.getVertex(i);
			data += "---------------------------------\n" +
			helpers.concatenateData(vx, true, namesList) +
			"---------------------------------\n\n";
		}

		//Loading all generated text into a visual text field on the left
		menu[6].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data;

		//Loading the namesList into a dropdown used for verification
		menu[1].GetComponent<Dropdown>().AddOptions(namesList);
	}

	/* Flips the UI button states. Takes a new state and execution mode.
	_mode == true -- default mode.
	_mode == false -- the function was called by clickVertex and we only need to update one button. */
	internal void menuUpdateButtons(bool _mode, bool _state)
	{
		if (_mode)
		{
			//True -- verification state, false -- default state

			//Making sure that the name hasn't been guessed before activating a dropdown menu
			if (_state == true && !gameGraph.checkField(edgeCheck[0], 0))
				menu[1].SetActive(true);
			else
				menu[1].SetActive(false);

			//Setting appropriate buttons (see reference before Start())
			menu[2].SetActive(!_state);
			menu[3].SetActive(!_state);
			for (int i = 4; i < 6; i++)
				menu[i].SetActive(_state);
		}
		else
		{
			//Called by a selected vertex, toggling clue button
			if (gameGraph.checkField(edgeCheck[0], 0)) //Makes sure the vertex has been guessed before displaying the Clue button
				menu[3].SetActive(true);
			else
				menu[3].SetActive(false);
		}
	}

	/* Instantiates particle systems under vertex of _id0 targeted at vertex of _id1
	Takes color of a particle as a Color object */
	internal void addTrail(int _id0, int _id1, Color _color)
	{
		Vertex vx0 = gameGraph.getVertex(_id0);
		Vertex vx1 = gameGraph.getVertex(_id1);

		GameObject newParticle;

		//Instantiating a new particle and setting the first vertex as its parent
		newParticle = Instantiate(particlePrefab, vx0.visual.transform.position, transform.rotation);
		newParticle.transform.SetParent(vx0.visual.transform);

		//Setting an external force of the second vertex to affect the particles (attractice)
		newParticle.GetComponent<ParticleSystem>().externalForces.SetInfluence(0, vx1.visual.transform.GetChild(1).GetComponent<ParticleSystemForceField>());

		//Setting a kill trigger to be the collider of the second vertex
		newParticle.GetComponent<ParticleSystem>().trigger.SetCollider(0, vx1.visual.GetComponent<Collider>());

		//Renaming and setting the scale of the new particle system
		newParticle.name = "PS: " + _id0 + " --> " + _id1;
		newParticle.gameObject.transform.localScale = new Vector3(2, 2, 2);

		//Setting a color of the new particle
		ParticleSystem.MainModule settings = newParticle.transform.GetComponent<ParticleSystem>().main;
		settings.startColor = new ParticleSystem.MinMaxGradient(_color); //There appears to be no faster way of swapping out colors for a particle system

		//Activating the particle system GameObject
		newParticle.SetActive(true);
	}

	#region CLICK PROCESSING
	/* Reloads the scene to restart the entire game. */
	public void clickRestart() => SceneManager.LoadScene("Cyberspace");

	/* Quits the game. */
	public void clickQuit() => Application.Quit();

	/* Toggles a tutorial text box. */
	public void clickTutorial() => menu[12].SetActive(!menu[12].activeSelf);

	/* Processes verify button clicks. */
	public void clickVerify()
	{
		//Changes the button layout and enters verification mode
		verifying = true;
		menuUpdateButtons(true, true);
	}

	/* Processes clue button clicks. */
	public void clickClue()
	{
		gameGraph.newClue(edgeCheck[0], false); //Requests the graph to generate new clues for a vertex
		menu[7].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gameGraph.getClue(edgeCheck[0]); //Assigns a string of clues

		/* The code below is used for scoring */
		timer -= 10f;
		cluesUsed++;
		displayMessage("-10 seconds", 3f);
	}

	/* Processes submit button clicks */
	public void clickSubmit()
	{
		Vertex[] vx = new Vertex[2] { gameGraph.getVertex(edgeCheck[0]), gameGraph.getVertex(edgeCheck[1]) }; //Saving references to the graph vertices for code readability

		if (gameGraph.edgeUnique(edgeCheck[0], edgeCheck[1])) //Making sure the player is not trying to connect the same pair twice
		{
			//Makes sure the name selected by the player matches the actual name of the vertex, or checks whether the name has already been unlocked
			if (menu[1].transform.GetChild(0).GetComponent<Text>().text == vx[0].name || gameGraph.checkField(edgeCheck[0], 0))
			{
				//Making sure the vertices share an edge
				if (gameGraph.findEdge(edgeCheck[0], edgeCheck[1]))
				{
					//At this point the player has successfully guessed the vertex name and one of its edges

					gameGraph.openField(edgeCheck[0], -1); //Opening all data of the vertex
					menuUpdateActive(edgeCheck[0]); //Updating the active vertex info to reflect the changes
					gameGraph.BFT(edgeCheck[0], 1); //We'll traverse the graph from the verified vertex up to one edge away and activate inactive vertices
					clickCancel(); //I'll just call this function to reset the UI

					gameGraph.setEdgesFound(edgeCheck[0], edgeCheck[1]); //Informs the actual data structure that an edge has been found

					//Checking and recording completion status for vertices. Setting new colors for vertices
					for (int i = 0; i < 2; i++)
					{
						if (gameGraph.setStatus(edgeCheck[i]))
						{
							//The name and all edges have been guessed for this vertex
							verticesComplete++;
							vx[i].visual.GetComponent<VisualVertex>().setColor(0); //Changing the color to indicate completion
						}
						else
							vx[i].visual.GetComponent<VisualVertex>().setColor(1);
					}

					//Sets the name for the visual representation of the guessed vertex
					vx[0].visual.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = vx[0].name;

					//Rolling a new color and adding a new trail of particles to connect vertices
					Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
					addTrail(edgeCheck[0], edgeCheck[1], color);

					timer += 20; //Extra time

					//Completion condition for stage 1
					if (verticesComplete == size)
					{
						//Stage 2 has bee deprecated, this is a game over condition
						gameFinished = true;
						gameOver(true);
					}
					else
					{
						//Notifying the player about successful connection
						displayMessage("Success: " + vx[0].name + " <--> " + vx[1].name, 3f);
					}
				}
			}
			else
			{
				//Error message displayed to the player in case the guess is wrong.
				displayMessage("Wrong, -10 seconds", 3f);
				timer -= 10;
			}
		}
	}

	/* Processes cancel button clicks */
	public void clickCancel()
	{
		//Changes the button layout and exits verification mode
		verifying = false;
		menuUpdateButtons(true, false);

		//Disables the second marker since we are no longer verifying a vertex
		marker1.SetActive(false);
	}

	/* Processes user clicks on vertices. 
	Takes the index of the vertex the user has just clicked on.
	The calls are received from VisualVertex attached to in-game vertices. */
	internal void clickVertex(int _id)
	{
		if (verifying == true) //Verification mode
		{
			if (gameGraph.checkField(_id, 0)) //Making sure the node we're connecting to is already verified
			{
				edgeCheck[1] = _id; //The last object clicked during verification is stored

				//Placing the second marker on top of the second focused object
				marker1.transform.position = gameGraph.getVertex(_id).visual.transform.position + new Vector3(0, 3, 0);
				marker1.SetActive(true);
			}
		}
		else
		{
			edgeCheck[0] = _id; //The object most recently clicked outside of verification mode is stored for an edge check

			//Placing the first marker on top of the focused object
			marker0.transform.position = gameGraph.getVertex(_id).visual.transform.position + new Vector3(0, 3, 0);
			marker0.SetActive(true);

			//Updates the active menu component if we're currently not verifying any vertices
			menuUpdateActive(_id);
			menuUpdateButtons(false, false); //Switches the Clue button on and off based on whether the name of this vertex has been guessed
		}
	}
	#endregion

	/* Update() runs code once per frame, and the only reason I'm using it is that I want to have a countdown timer in place */
	void Update()
	{
		if (gameFinished == false)
		{
			/* This section of code is only responsible for the in-game timer and one game-over state (player runs out of time) */
			timer -= Time.deltaTime; //Getting the time out of the engine
			float seconds = Mathf.Floor(timer % 60); //Mathf.Floor() appears to be the "official" way of getting integer-like values from floating points within Unity

			/* Putting together a string */
			timerDisplay.text = Mathf.Floor(timer / 60) + ":";

			if (seconds < 10)
			{
				timerDisplay.text += "0";
			}

			timerDisplay.text += Mathf.Floor(timer % 60); //Assigning the new time string

			//Game over condition
			if (timer < 0)
			{
				timerDisplay.text = "00:00"; //One last update so that the clock doesn't display an invalid value
				timer = 0;
				gameFinished = true;
				gameOver(false); //Notifying the game that it's over
			}
		}
	}
}