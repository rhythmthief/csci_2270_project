using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

		Spotlight to makes vertices project shadows

		Don't need to algorithmically connect the visual vertices, the players will do it themselves.




	IMMEDIATE:
	Show profile names on top of the vertices.
	Canvases for profiles and profile info. Use the presentation layout.
	More names





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

public class driver : MonoBehaviour
{
	internal Helpers helpers = new Helpers(); //I am calling the helper functions I need through this instance, seeing how I can't inherit from both MonoBehavior and Helpers
	public List<GameObject> visualVertices;
	internal Graph gameGraph;
	int size;

	void Start()
	{
		size = 30;
		visualVertices = new List<GameObject>();
		gameGraph = helpers.buildGraph(size);
		int temp = 0; //Used as a counter for building a visual graph

		/* This call goes to the pre-exiting GameObject with an attached visualVertex script and builds up the graph from there. All vertices are added into the visualVertices List */
		transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<visualVertex>().buildVisualGraph(0, 0, 0, ref temp, gameGraph.getSize(), ref visualVertices);

		//VISUAL NODES ARE JUST BUTTONS WHICH UPDATE VERTEX VIEW WINDOW AND SHOW VISUAL CLUES BASED ON COMPLETION


		gameGraph.printVertices();
	}

	// Update is called once per frame
	void Update()
	{
	}
}
