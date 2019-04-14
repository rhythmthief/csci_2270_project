using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Notes:
		Edges are unweighted and undirected
*/

/* The class for vertices in the graph. 
The reason I decided to use a class instead of a struct is that my compiler flags pointers as "unsafe" code and refuses to compile unless I explicitly allow it in the project settings, which in turn makes the built-in .net garbage collector useless as it would not be able to track and deallocate unused memory. So, I would've had to pass a struct by reference for some operations, which in essence is what happens by default when I use a class.*/
internal class Vertex
{
	internal string name;
	internal string country;
	internal string[] misc; //Holds strings of miscellaneous information
	internal bool[] fieldAccess; //Tracks whether a particular field from above can be accessed by the player
	internal StackLL tipCache; //This stack stores all tips that can be accessible via this vertex
	internal LinkedList tipShown; //A linked list which contains tips which have been popped from the tipCache stack



	/* Parameterized constructor for a vertex */
	internal Vertex(string _name, string _country, string[] _misc, bool[] _miscAccess)
	{
		name = _name;
		country = _country;
		misc = _misc;
		fieldAccess = _miscAccess;
	}
};

/* Graph implentation */
internal class Graph
{
	private List<Vertex> vertices;
	private int[,] matrix;
	private int size; //Represents the size of a single dimension

	/*
		The adjacency matrix stores information about edges between vertices of the graph. I am using a multidimensional array [,] as opposed to a regular array-of-arrays [][] since I can get away without the extra functionality the latter method would otherwise provide.
		
		0	1	0	0	1	Where 0 -- no edge
		1	0	1	1	1	and 1 -- there is an edge
		1	1	0	1	0
		1	0	0	0	1
		0	0	0	1	0
	*/

	internal Graph(int _size)
	{
		size = _size;
		matrix = new int[size, size];

		//I need to zero out the adjacency matrix first
		for (int i = 0; i < size; i++)
			for (int j = 0; j < size; j++)
				matrix[i, j] = 0;
	}

	/* Inserts a new vertex at the end of the <Vertex> list */
	internal void addVertex(string _name, string _country, string[] _misc, bool[] _miscOpen)
	{
		Vertex newVx = new Vertex(_name, _country, _misc, _miscOpen);
	}

	/* Inserts an edge between two vertices.
	Takes indexes of corresponding vertices.
	0 -- no edge; 1 -- edge is present */
	internal void insertEdge(int vx0, int vx1)
	{
		matrix[vx0, vx1] = 1;
		matrix[vx1, vx0] = 1; //The matrix has to be symmetrical relative to the main diagonal, since my edges are undirected
	}

	/* Checks whether two vertices share an edge. Since my graph is undirected, I'll only check it one way */
	internal bool checkAdjacency(int vx0, int vx1)
	{
		bool adjacent = false;

		if (matrix[vx0, vx1] == 1) //I only need to check one side, since edges are undirected
			adjacent = true;

		return adjacent;
	}

	/* Returns a reference to the vertex under a given index */
	internal Vertex findVertex(int _index)
	{
		Vertex vx = null;
		if (_index < vertices.Count)
		{
			vx = vertices[_index];
		}
		else
		{
			Debug.Log("Graph.findVertex(): index exceeds the number of vertices.");
		}

		return vx;

	}


	/* GAME-SPECIFIC METHODS */

	/* Makes data of a vertex accessible to the player.
	Takes the index of a vertex and the index of a misc string.
	Passing -1 as the latter makes all misc strings accessible. */
	internal void setMisc(int _index, int _miscIndex)
	{
		/* Fields:
			0 -- name
			1 -- country
			2-5 -- misc */

		if (_miscIndex != -1)
		{
			vertices[_index].fieldAccess[_miscIndex] = true;
		}
		else
		{
			//Makes all data accessible
			for (int i = 0; i < vertices[_index].fieldAccess.Length; i++)
			{
				vertices[_index].fieldAccess[i] = true;
			}
		}
	}

	/* Looks up data from adjacent nodes and returns it as a string.
	Takes the index of a focused vertex and a boolean for execution mode.
	Mode 0 -- pulls up the data one edge away
	Mode 1 -- goes two edges away, used for the final stage of the game */
	internal string getTip(int _index, bool mode)
	{
		if (!mode) //Mode 0, used for the majority of the game
		{
			//I am caching all tips in a stack for ease of use. If this statement passes, it means that the cache hasn't been built yet.
			if (vertices[_index].tipCache.isEmpty() && vertices[_index].tipShown.isEmpty())
			{
				for (int i = 0; i < size; i++)
				{
					if (matrix[_index, i] == 1)
					{
						//Pushes country first as it will be the last tip a user can receive about a node
						vertices[_index].tipCache.push(vertices[i].country);

						//And then pushes the misc data
						for (int j = 0; j < 3; j++)
							vertices[_index].tipCache.push(vertices[i].misc[j]);
					}

				}
			}

			//Once the player requests a tip, it gets popped from a cache stack and added to a linked list, which is then displayed in-game
			if (!vertices[_index].tipCache.isEmpty())
			{
				vertices[_index].tipShown.addNode(vertices[_index].tipCache.pop());
			}
		}
		else
		{
			//Mode 1, activated in the latter half of the game
		}

		return null;
	}
};