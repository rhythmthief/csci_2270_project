using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //Contains the StreamReader class

/*
	Notes:
		This file contains definitions for helper functions used in the driver file
*/

public class Helpers : MonoBehaviour
{
	/* A helper function which reads a section of a stream into a linked list until encountering a given stopword */
	void readSection(StreamReader stream, LinkedList ll)
	{
		string line = stream.ReadLine();

		if (line != "#stop")
		{
			ll.addNode(line);
			readSection(stream, ll);
		}
	}

	/*
		Rolls elements for a new vertex and adds it to the graph
	*/
	void generateProfile(Graph gameGraph, LinkedList names, LinkedList countries, LinkedList misc)
	{
	//	walker
	}


	/*
		Can be accessed from outside of this class

		Reads graph data from a file, distributes it among vertices, randomly connects the vertices
		Takes a reference to a graph object and the number of vertices to generate
	*/
	public void buildGraph(Graph gameGraph, int vertexCount)
	{
		StreamReader stream = new StreamReader("Assets/DataFiles/graphData.cyb"); //Initializes a stream and opens a predefined data file

		string line;
		LinkedList names = new LinkedList();
		LinkedList countries = new LinkedList();
		LinkedList misc = new LinkedList();


		/* I will be storing contents of my file in linked lists for later manipulation */

		while ((line = stream.ReadLine()) != null)
		{
			switch (line)
			{
				/*
					Looks for starting points of sections in my file, skips to the next line unless one is located
				*/
				case "#NameStart":
					readSection(stream, names);
					break;

				case "#CountryStart":
					readSection(stream, countries);
					break;

				case "#MiscStart":
					readSection(stream, misc);
					break;
				default:
					stream.ReadLine();
					break;
			}
		}
	}

	//WHEN CONNECTING EDGES, MAKE SURE THAT THE ENTIRE GRAPH IS CONNECTED (THERE ARE NO DISCONNECTED ELEMENTS)
	//SHOULD I VERIFY ALGORITHMICALLY BY TRAVERSING THE ENTIRE GRAPH?
}