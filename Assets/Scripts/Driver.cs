using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
	Notes:
		This is the main driver file used to power the entire project
		C# doesn't use header files, everything is automatically visible to other parts of the project without #inclusion
		I am using "class" to define LL nodes throughout my data structures, since in C# a class is reference type and can be handled like a pointer
		Default access level within Unity is private

	Gameplay:
		The player starts with a single revealed node and sees one random piece of information for every connected node as well as for their own immediate connections, but the exact relationship to the original node is not specified. The goal is to match all names with their respective nodes. Nodes are verified as name + at least 1 connection. There are extra profiles which never show up in the graph. The player works against a timer, verification and tips cost time. The goal is to reveal the entire grid and find the "person of interest", who is described through second-order information ("knows someone who knows a person who owns a dog...")

	Gameplay loop:
		0) The player starts with one revealed profile
		[Before the finale]
		1) The player inspects a revealed profile and its surroundings, requests information from the revealed profile
		2) The player examines all clues and compares them against a larger list of profiles available in a section on-screen
		3) The player matches a node with a name based on provided clues and connect it
		4) Verification is done automatically in real time. Some nodes might have more than one connection, they will be indicated to be incomplete.
		5) Go to step (1) unless the entire graph has been uncovered

		[Finale]
		6) The player has to look for a person of interest in the graph without any direct information
		7) The player can request a certain finite number of tips to figure out the person of interest through second-order information ("knows someone who knows a person who owns a dog...")

*/

/*
	TODOs:
		Read the file and populate a graph

		Indexes are retrieved by clicking on node visualizations

		Cheat/dev mode

		Linked list for profiles

		Revise all comments -- there have been changes to the graph structure (adjacency list to matrix)

		Expose a vertex's ID within the List and use it to call vertices within the game

		keep track of the indices the player is working with

		track whether tipCache in the graph is full. If it is, disable the button for requesting tips
		cache for the chosen node has to be randomized

		!!!Add more names to minimize the odds of rolling the same name twice

		Don't need to algorithmically connect the visual vertices, the players will do it themselves.

		Visual connections: use light particles. Give a slight push along Y and have them converge on the target vertex



	IMMEDIATE:
	Show profile names on top of the vertices.
	Canvases for profiles and profile info. Use the presentation layout.
	Timer (not necessarily integrated yet)
	Player-driven traversal starts from the LAST node in the graph --> increasing difficulty, easier start

	REVEALING LOGICS: go over the adjacency matrix for a vertex and enable all adjacent vertices (otherwise the game objects are disabled)

	Animation: tween from the bottom

	Particle system: flag each edge to prevent 2 PS doing the same thing


	DESIGN CHANGE -- reveal only nodes one edge away instead of 2. Update the loop info above.
	PARTICLES -- lower quantity (1-2 particles) with trails moving around force fields.
	VERIFICATION: Send node indexes,the function takes value from a dropdown box by itself
	REVISE USAGE OF LINKED LISTS -- NO LONGER USED FOR CLUES

	CHANGE ALL FONTS TO HACK

	MAKE SURE STAGE 2 PASSES

q	TUTORIAL:
	EXPLAIN THAT THE PLAYER CAN'T CONNECT TO UNVERIFIED VERTICES

	UI Modes:
	DESCRIBE UI MODES
	NORMAL
	VERIFICATION

	UI
	AESTHETICS


	UNIMPLEMENTED:
		Graph --> getTip() --> Mode 1

	FULLY IMPLEMENTED:
		StackLL
		LinkedList
		Graph generation

	DS Used:
	Graph
	HashTable
	LinkedList
	StackLL
	List (vector) -- generic
	Queue -- generic
*/

public class Driver : MonoBehaviour
{
	internal Helpers helpers = new Helpers(); //I am calling the helper functions I need through this instance, seeing how I can't inherit from both MonoBehavior and Helpers


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
						    */

	internal Graph gameGraph;
	public bool verifying = false; //Determines whether the UI is in verification mode
	public int[] edgeCheck; //Stores the indexes of two nodes we want to check for an edge
	internal Vertex activeVertex;
	internal int verticesComplete = 0; //Counter for the total number of vertices fully guessed by the player

	internal GameObject particlePrefab;


	int size;

	void Start()
	{
		//	QualitySettings.vSyncCount = 0;

		Application.targetFrameRate = 60;


		size = 7;
		gameGraph = helpers.buildGraph(size);

		int temp = 0; //Used as a counter for building a visual graph
				    /* This call goes to the pre-exiting GameObject with an attached visualVertex script and builds up the graph from there. All vertices are added into the visualVertices List */
		transform.GetChild(0).GetChild(0).gameObject.GetComponent<VisualVertex>().buildVisualGraph(0, 0, 0, ref temp, gameGraph.getSize(), gameGraph);

		//Quick debug function, prints all generated vertices in the engine log
		gameGraph.printVertices();

		menu = new GameObject[8];

		particlePrefab = transform.GetChild(2).gameObject;

		edgeCheck = new int[2] { -1, -1 };

		for (int i = 0; i < 8; i++)
		{
			//Sets references to in-game menu elements (GameObjects)
			menu[i] = transform.GetChild(1).GetChild(i).gameObject;
		}

		menuUpdateStatic();
		menuUpdateActive(gameGraph.getCount() - 1);
		edgeCheck[0] = size - 1;

		//the last vertex needs special rules -- I'll be showing its name from the start
		gameGraph.getVertex(size - 1).visual.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = gameGraph.getVertex(size - 1).name;
	}

	/* Updates the active vertex menu based on a selected vertex */
	internal void menuUpdateActive(int _id)
	{
		//Updates the text box with data info (on the right)
		menu[0].transform.GetChild(0).GetComponent<Text>().text = helpers.concatenateData(gameGraph.getVertex(_id), false, null);

		//Updates the clues window
		menu[7].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gameGraph.getClue(_id);
	}

	/* Updates static components of the menu:
	loads all profiles into the list located on the left of the screen
	loads names into the dropdown menu */
	internal void menuUpdateStatic()
	{
		string data = "";
		Vertex vx;
		List<string> namesList = new List<string>();
		for (int i = 0; i < gameGraph.getCount(); i++)
		{
			vx = gameGraph.getVertex(i);
			data += "---------------------------------------------\n" +
			helpers.concatenateData(vx, true, namesList) +
			"---------------------------------------------\n";
		}

		//Loading all generated text into a visual text field on the left
		menu[6].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data;

		//Loading the namesList into a dropdown used for verification
		menu[1].GetComponent<Dropdown>().AddOptions(namesList);
	}

	/* Flips the UI button states. Takes a new state and execution mode.
	_mode == true -- default mode.
	_mode == false -- the function was called by clickVertex and we only need to update one button. */
	internal void menuUpdateButtons(bool _mode, bool _change)
	{
		if (_mode)
		{
			//True -- verification mode, false -- default mode

			//Making sure that the name hasn't been guessed before activating a dropdown menu
			if (_change == true && !gameGraph.checkField(edgeCheck[0], 0))
				menu[1].SetActive(true);
			else
				menu[1].SetActive(false);

			menu[2].SetActive(!_change);
			menu[3].SetActive(!_change);
			for (int i = 4; i < 6; i++)
				menu[i].SetActive(_change);
		}
		else
		{
			if (gameGraph.checkField(edgeCheck[0], 0)) //Makes sure the vertex has been guessed before displaying the Clue button
				menu[3].SetActive(true);
			else
				menu[3].SetActive(false);
		}
	}

	/* Displays a message for the player on-screen */
	internal void displayMessage(string message)
	{
		//Fades out over time
	}

	/* Instantiates particle systems under given vertices targeted at one another */
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

		//Setting a color of the new particle
		ParticleSystem.MainModule settings = newParticle.transform.GetComponent<ParticleSystem>().main;
		settings.startColor = new ParticleSystem.MinMaxGradient(_color); //There appears to be no faster way of swapping out colors for a particle system

		//Activating the GameObject
		newParticle.SetActive(true);
	}

	#region CLICK PROCESSING

	/* Processes verify button clicks */
	public void clickVerify()
	{
		//Changes the button layour and enters verification mode
		verifying = true;
		menuUpdateButtons(true, true);
	}

	/* Processes verify button clicks */
	public void clickClue()
	{
		gameGraph.newClue(edgeCheck[0], false);
		menu[7].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = gameGraph.getClue(edgeCheck[0]);
	}

	/* Processes submit button clicks */
	public void clickSubmit()
	{
		Vertex vx = gameGraph.getVertex(edgeCheck[0]); //Saving a reference to the graph vertex for code readability (as opposed to calling getVertex all the time)

		if (gameGraph.edgeUnique(edgeCheck[0], edgeCheck[1])) //Making sure the player is not trying to connect the same pair twice
		{
			//Makes sure the name selected by the player matches the actual name of the vertex, or checks whether the name has already been unlocked
			if (menu[1].transform.GetChild(0).GetComponent<Text>().text == vx.name || gameGraph.checkField(edgeCheck[0], 0))
			{
				//Making sure the vertices share an edge
				if (gameGraph.findEdge(edgeCheck[0], edgeCheck[1]))
				{
					//At this point the player has successfully guessed the vertex name and one of its edges


					//PARTICLE SYSTEM INSTANTIATION GOES HERE?

					gameGraph.openField(edgeCheck[0], -1); //Opening all data of the vertex
					menuUpdateActive(edgeCheck[0]); //Updating the active vertex info to reflect the changes
					gameGraph.BFT(edgeCheck[0], 1); //We'll traverse the graph from the verified vertex up to one edge away and activate inactive vertices
					clickCancel(); //I'll just call this function to reset the UI


					gameGraph.setEdgesFound(edgeCheck[0], edgeCheck[1]);

					//Checking and recording completion status for vertices
					if (gameGraph.setStatus(edgeCheck[0]))
						verticesComplete++;
					if (gameGraph.setStatus(edgeCheck[1]))
						verticesComplete++;


					//Sets the name for the visual representation of the vertex
					vx.visual.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = vx.name;

					Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
					addTrail(edgeCheck[0], edgeCheck[1], color);
					addTrail(edgeCheck[1], edgeCheck[0], color);


					displayMessage("Success.");



					//Completion condition for stage 1
					if (verticesComplete == size)
					{
						//MOVE ON TO STAGE 2
					}
				}
			}
			else
			{
				//Error message displayed to the player in case the guess is wrong.
				displayMessage("Wrong.");
			}
		}
	}


	/* Processes cancel button clicks */
	public void clickCancel()
	{
		//Changes the button layour and exits verification mode
		verifying = false;
		menuUpdateButtons(true, false);
	}

	/* Processes user clicks on vertices. 
	Takes the index of the vertex the user has just clicked on. */
	internal void clickVertex(int _id)
	{
		if (verifying == true) //Verification mode
		{
			//CHANGE THE COLOR OF THE LAST SELECTED EDGE?

			if (gameGraph.checkField(_id, 0)) //Making sure the node we're connecting to is already verified
				edgeCheck[1] = _id; //The last object clicked during verification is stored
		}
		else
		{
			edgeCheck[0] = _id; //The object most recently clicked outside of verification mode is stored for an edge check
			activeVertex = gameGraph.getVertex(_id);

			//Updates the active menu component if we're currently not verifying any vertices
			menuUpdateActive(_id);
			menuUpdateButtons(false, false); //Switches the Clue button on and off. The first parameter doesn't matter.
		}
	}
	#endregion
}
