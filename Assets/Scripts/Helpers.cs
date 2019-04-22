using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO; //Contains the StreamReader class

/*
	Notes:
		This file contains definitions for helper functions used in the driver file
*/

internal class Helpers
{
	/* A helper function which reads a section of a stream into a List until encountering a given stopword. Takes a StreamReader object and a List of strings to insert elements into. */
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
	void generateVertex(Graph gameGraph, HashTable usedNames, List<string> names, List<string> countries, List<string> misc, int count)
	{
		HashTable usedMisc = new HashTable(7); //A table which saves rolled misc for each user
		string[] newMisc = new string[3]; //This array will store rolled misc
		int miscIndex = Random.Range(0, misc.Count);
		int miscCount = 0; //I'll roll a total of 3 misc items
		int nameIndex = Random.Range(0, names.Count); //I'll need to save indices for names

		if (usedNames == null) //I'll initialize the hash table in case if this is the first call of generateVertex()
			usedNames = new HashTable(7); //I'll be using this hash table to save indexes of used names in order to avoid duplicates in the graph during generation

		//The last element won't actually be rolled
		if (count != gameGraph.getSize() - 1)
		{
			/* Rolling a name.
					I'll keep rolling until I get a unique name that's not already in a hash table. The number of available names will generally be much larger than the number of vertices we can generate, so long streaks of bad rolls (repeats) are unlikely. */
			while (usedNames.searchItem(nameIndex))
				nameIndex = Random.Range(0, names.Count);

			usedNames.insertItem(nameIndex); //I'll be saving the index of a used name in a hash table which gets passed down to the next call

			/* Rolling misc */
			while (miscCount != 3)
			{
				while (usedMisc.searchItem(miscIndex)) //Same as usedNames above, shouldn't affect performance
					miscIndex = Random.Range(0, misc.Count);

				usedMisc.insertItem(miscIndex);
				newMisc[miscCount] = misc[miscIndex];
				miscCount++;
			}

			//Now that all elements have been rolled, we can finally add a new vertex
			gameGraph.addVertex(names[nameIndex], countries[Random.Range(0, countries.Count)], newMisc);
		}
		else
		{
			//The last element is the starting vertex of the player
			gameGraph.addVertex("[YOU]", "[CLASSIFIED]", new string[] { "[CLASSIFIED]", "[CLASSIFIED]", "[CLASSIFIED]" });
		}

		//Some fields have to be accessible to the player through UI
		if (count < gameGraph.getSize() - 2)
		{
			gameGraph.openField(count, Random.Range(1, 4)); //We'll also make a random non-name field accessible the player
		}
		else if (count == gameGraph.getSize() - 2)
		{
			//If we are generating the second-to-last element of the graph, we need to reveal every field except for the name -- this node will serve as a tutorial
			for (int i = 1; i < 5; i++)
				gameGraph.openField(count, i);
		}
		else
		{
			//The last element is the starting vertex (the player), and it's supposed to be completely known from the start
			gameGraph.openField(count, -1);
		}

		count++; //Counts how many profiles have been generated so far

		/* Recursive call to generate the next graph node */
		if (count < gameGraph.getSize())
		{
			generateVertex(gameGraph, usedNames, names, countries, misc, count);
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

		if (index != gameGraph.getSize() - 1) //Generating an edge for every element other than the last one
		{
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
				gameGraph.insertEdge(index, Random.Range(0, index - 1)); //The bias for connecting nodes with smaller index is counteracted by increasing chance of connection to nodes of greater index down below
				connected.insertItem(index); //And now I'll add this element to the hash table
									    //vxValid = true;
			}

			//Now it's time to roll other edges
			for (int i = 0; i < gameGraph.getSize(); i++)
			{
				if (i != index) //Vertices are not allowed to be connected to themselves
				{
					roll = Random.Range(i / 2, gameGraph.getSize() - 1); //Higher chance to connect to nodes of a greater index

					if (roll == i) //If I rolled the current index, I'm connecting
					{
						gameGraph.insertEdge(index, i); //If the edge is already present, insertEdge won't add it again. This roll is still valid though
						connected.insertItem(i); //The newly connected element is now connected to the rest of the graph, so I save it in the hash table
					}

					//A limit on the maximum number of edges a node can accumulate through these rolls
					if (gameGraph.getVertexEdgeCount(index) == 2)
						break;
				}
			}
		}
		else
		{
			//The last vertex always shares an edge with second-to-last
			gameGraph.insertEdge(index, index - 1);
		}

		index++;

		/* Recursive call to roll edges for the next vertex */
		if (index < gameGraph.getSize())
		{
			generateEdge(gameGraph, connected, index);
		}
	}

	/* 	Reads graph data from a file, distributes it among vertices, randomly connects the vertices. 
		Takes the size of a graph to generate. Returns a populated Graph object.
		Additionally, takes a string from the driver and saves rolled names into it. */
	internal Graph buildGraph(int _size)
	{
		Graph gameGraph = new Graph(_size);
		StreamReader stream = new StreamReader(Application.dataPath + "/Resources/graphData.txt"); //Initializes a stream and opens a predefined data file

		//I considered using a linked list to store names for generation, but ultimately decided ot use standard Lists instead because I intend to roll random indexes later, so lookup would've been inefficient compared to a List
		List<string> names = new List<string>();
		List<string> countries = new List<string>();
		List<string> misc = new List<string>();
		string line;

		if (_size > 1) //Makes sure there's at least 2 elements to insert
		{
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

			generateVertex(gameGraph, null, names, countries, misc, 0); //Generating new vertices for the graph, recursive
			generateEdge(gameGraph, null, 0); //Rolling edges for the graph, recursive

			/* As a final step, I am going to verify that the newly generated graph is connected. There's logics in generateEdge() method to ensure that the graph does turn out to be connected, but it never hurts to make sure. If the graph is somehow not connected (which should never be the case), I am going to recursively run this function again.
			Note: running again WOULD technically do unnecessary work to read the file again, but since the graph is guaranteed to be connected unless some unforeseen logical error I didn't account for takes place, it is highly unlikely that the recursive call will ever take place. Still, the functionality for the game graph to heal itself is there! */
			if (gameGraph.graphConnected())
			{
				Debug.Log("The Graph is connected. Generation successful.");
			}
			else
			{
				Debug.Log("The generated graph is disconnected. Running a recursive call...");
				return buildGraph(_size);
			}
		}

		return gameGraph; //Once the generation is complete and the graph is connected, we'll return it
	}

	/* Returns a string will all available data about a vertex. Switches between two modes:
	False -- generates a profile view for the active vertex menu with [UNKNOWN] elements
	True -- generates complete profiles, but without the available clues. Shown on the left.
	The second mode (true) also fills up a namesList passed by reference*/
	internal string concatenateData(Vertex vx, bool mode, List<string> namesList)
	{
		string data = "";

		//It's rather tedious to check every field and concatenate, but it's a tradeoff for better code readability in the rest of the project
		/* Completion status displayed on the right */
		if (mode == false)
		{
			data += "Status:\t\t";
			if (vx.status)
				data += "COMPLETE\n";
			else
				data += "INCOMPLETE\n";
		}

		if (vx.fieldAccess[0] || mode == true)
		{
			data += "Name:\t\t" + vx.name + "\n";

			if (mode == true && namesList != null)
				namesList.Add(vx.name); //Adding names to a list passed by reference. I'll use this list in the dropdown menu for name selection
		}
		else
			data += "Name:\t\t" + "[UNKNOWN]\n";

		if (vx.fieldAccess[1] || mode == true)
			data += "Country:\t" + vx.country + "\n";
		else
			data += "Country:\t" + "[UNKNOWN]\n";

		data += "\nDetails:\n";
		for (int i = 2; i < 5; i++)
		{
			if (vx.fieldAccess[i] || mode == true)
				data += vx.misc[i - 2] + "\n";
			else
				data += "[UNKNOWN]\n"; ;
		}

		return data;
	}
}