using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //Contains StreamReader class

/*
	Notes:
		This is the main driver file used to power the entire project
		C# doesn't use header files, everything is automatically visible to other parts of the project without #inclusion
		I am using "class" to define LL nodes throughout my data structures, since in C# a class is reference type and can be handled like a pointer

	Gameplay:
		The player starts with a single revealed node and sees one random piece of information for every connected node as well as for their own immediate connections, but the exact relationship to the original node is not specified. The goal is to match all names with their respective nodes. Nodes are verified in sets of 2 on demand. There are extra profiles which never show up in the graph. The player works against a timer, verification and tips cost time. The goal is to reveal the entire grid and find the "person of interest", who is described through second-order information ("knows someone who knows a person who owns a dog...")

	Gameplay loop:
		0) The player starts with one revealed profile
		[Before the finale]
		1) The player inspects a revealed profile and its surroundings, requests information from the revealed profile
		2) The player examines all clues and compares them against a larger list of profiles available in section on-screen
		3) The player matches a node with a name based on provided clues
		4) Once the player feels confident about two names, they can request verification and either solidify the guesses or start over
		5) Go to step (1) unless the entire graph has been uncovered

		[Finale]
		6) The player has to look for a person of interest in the graph without any direct information
		7) The player can request a certain finite number of tips to figure out the person of interest through second-order information ("knows someone who knows a person who owns a dog...")

*/

/*
	TODOs:
		Double the number of names on the list

*/

public class driver : MonoBehaviour //MonoBehaviour is the base class of Unity, from which every other script derives
{
	public Helpers help; //I am calling the helper functions I need through this instance, seeing how I can't inherit from both MonoBehavior and Helpers

	StackLL testStack = new StackLL();


	void Start()
	{
		//int test;
		//test = help.readGameData();
		//Application.targetFrameRate = 65; //Just a precaution in case v-sync isn't active on a machine

	}

	// Update is called once per frame
	void Update()
	{
	}
}
