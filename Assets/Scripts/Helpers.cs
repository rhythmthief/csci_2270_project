using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //Contains the StreamReader class

/*
	Notes:
		This file contains definitions for helper functions used in the driver file
*/

internal class Helpers
{
	/* A helper function which reads a section of a stream into a linked list until encountering a given stopword. Takes a StreamReader object and a List of strings to insert elements into. */
	void readSection(StreamReader stream, List<string> list)
	{
		string line = stream.ReadLine();

		if (line != "#stop")
		{
			list.Add(line);
			readSection(stream, list); //Recursive call
		}
	}

	/* Rolls elements for a new vertex and adds it to the graph.
	Takes the primary Graph object of the game, Lists for read data and count for the current number of generated profiles.
	Recursive. */
	void generateProfile(Graph gameGraph, HashTable usedNames, List<string> names, List<string> countries, List<string> misc, int count)
	{
		HashTable usedMisc = new HashTable(7); //A table which saves rolled misc for each user
		string[] newMisc = new string[3]; //This array will store rolled misc
		int miscIndex = Random.Range(0, misc.Count);
		int miscCount = 0; //I'll roll a total of 3 misc items
		int nameIndex = Random.Range(0, names.Count); //I'll need to save indices for names

		if (usedNames == null) //I'll initialize the hash table in case if this is the first call of generateProfile()
			usedNames = new HashTable(7); //I'll be using this hash table to save indexes of used names in order to avoid duplicates in the graph during generation

		/* Rolling a name */
		while (usedNames.searchItem(nameIndex)) //I'll keep rolling until I get a unique name that's not already in a hash table
			nameIndex = Random.Range(0, names.Count);

		usedNames.insertItem(nameIndex); //I'll be saving the index of a used name in a hash table which gets passed down to the next call

		/* Rolling misc */
		while (miscCount != 3)
		{
			while (usedMisc.searchItem(miscIndex)) //Same as usedNames above
				miscIndex = Random.Range(0, misc.Count);

			usedMisc.insertItem(miscIndex);
			newMisc[miscCount] = misc[miscIndex];
			miscCount++;
		}

		//Now that all elements have been rolled, we can finally add a new vertex
		gameGraph.addVertex(names[nameIndex], countries[Random.Range(0, countries.Count)], newMisc);
		gameGraph.openField(gameGraph.getCount() - 1, Random.Range(1, 4)); //We'll also make a random non-name field accessible the player


		count++; //Counts how many profiles have been generated so far
			    /* Recursive call to generate the next graph node */
		if (count < gameGraph.getSize())
		{
			generateProfile(gameGraph, usedNames, names, countries, misc, count);
		}
	}

	/* Rolls and inserts new edges into the graph.
	Takes the primary game Graph and the index of the current vertex.
	Recursive. */
	void generateEdge(Graph gameGraph, HashTable connected, int index)
	{
		bool vxValid = false;
		int roll;

		/*	I am building a connected graph, so I need make sure that all elements can be reached from the starting point.
			In order to guarantee connectivity, I am going to have indices of all currently connected nodes to a hash table, and ensure that a vertex I am currently rolling is connected to at least one of them. */

		/* Creates a new hash table if this is the first call */
		if (connected == null)
		{
			connected = new HashTable(7);
			connected.insertItem(index); //The first element gets inserted into the table of connected elements
			vxValid = true;
		}
		else if (connected.searchItem(index)) //If this isn't the first element, I need to check if it's already a valid element of the graph (connected to the rest)
		{
			vxValid = true;
		}

		/* First, I need to make sure the vertex is connected to the big graph I am building. If the check above failed and this vertex is not present in the hash table yet, I will make one random roll and connect it to one of the previously processed elements (which are guaranteed to be connected) */
		if (!vxValid)
		{
			gameGraph.insertEdge(index, Random.Range(0, index-1)); //The bias for connecting nodes with smaller index is counteracted by increasing chance of connection to nodes of greater index down below
			connected.insertItem(index); //And now I'll add this element to the hash table
			//vxValid = true;
		}

		//Now it's time to roll other edges
		for (int i = 0; i < gameGraph.getSize(); i++)
		{
			if (i != index) //Vertices are not allowed to be connected to themselves
			{
				roll = Random.Range(i/2, gameGraph.getSize()); //Higher chance to connect to nodes of a greater index

				if (roll == i)
				{
					gameGraph.insertEdge(index, i); //If the edge is already present, insertEdge won't add it again. This roll is still valid though
					connected.insertItem(i); //The newly connected element is now connected to the rest of the graph
				}

				//A limit on the maximum number of edges a node can accumulate through these rolls
				if (gameGraph.getVertexEdgeCount(index) == 4)
					break;
			}
		}

		index++;

		/* Recursive call to roll edges for the next vertex */
		if (index < gameGraph.getSize())
		{
			generateEdge(gameGraph, connected, index);
		}
	}


	/* 	Can be accessed from outside of this class
		Reads graph data from a file, distributes it among vertices, randomly connects the vertices.
		Takes a reference to a graph object */
	internal void buildGraph(Graph gameGraph)
	{
		StreamReader stream = new StreamReader("Assets/DataFiles/graphData.cyb"); //Initializes a stream and opens a predefined data file

		//I considered using a linked list to store names for generation, but ultimately decided ot use standardLists instead because I intend to roll random indexes later, so lookup would've been inefficient compared to a list
		List<string> names = new List<string>();
		List<string> countries = new List<string>();
		List<string> misc = new List<string>();
		string line;

		/* I will be storing contents of my file in lists for later manipulation */
		while ((line = stream.ReadLine()) != null) //StreamReader returns a null once it reaches the end of an input
		{
			switch (line)
			{
				/* Looks for starting points of sections in the data file. */
				case "#names":
					readSection(stream, names);
					break;

				case "#countries":
					readSection(stream, countries);
					break;

				case "#misc":
					readSection(stream, misc);
					break;
			}
		}

		generateProfile(gameGraph, null, names, countries, misc, 0);
		generateEdge(gameGraph, null, 0);
		//NOW DISTRIBUTE THE EDGES
	}


	//WHEN CONNECTING EDGES, MAKE SURE THAT THE ENTIRE GRAPH IS CONNECTED (THERE ARE NO DISCONNECTED ELEMENTS)
	//SHOULD I VERIFY ALGORITHMICALLY BY TRAVERSING THE ENTIRE GRAPH?
}