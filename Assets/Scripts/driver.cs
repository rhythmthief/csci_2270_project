﻿using System.Collections;
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

public class driver : MonoBehaviour //MonoBehaviour is the base class of Unity, from which every other script derives
{
	internal Helpers help = new Helpers(); //I am calling the helper functions I need through this instance, seeing how I can't inherit from both MonoBehavior and Helpers

	StackLL testStack = new StackLL();
	LinkedList testList = new LinkedList(); //not tested yet
	Graph testGraph = new Graph(15);
	HashTable testTable = new HashTable(7);

	void Start()
	{
		help.buildGraph(testGraph);

		testGraph.printVertices();
		testGraph.graphValid();
	}

	// Update is called once per frame
	void Update()
	{
	}
}