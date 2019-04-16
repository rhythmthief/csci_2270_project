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
	internal bool[] fieldAccess = { false, false, false, false, false }; //Tracks whether a particular field from above can be accessed by the player
	internal StackLL tipCache; //This stack stores all tips that can be accessible via this vertex
	internal LinkedList tipShown; //A linked list which contains tips which have been popped from the tipCache stack
	internal int edgeCount = 0;
	/* Parameterized constructor for a vertex */
	internal Vertex(string _name, string _country, string[] _misc)
	{
		name = _name;
		country = _country;
		misc = _misc;
	}
};

/* Graph implentation */
internal class Graph
{
	private List<Vertex> vertices;
	private bool[,] matrix;
	private int size; //Represents the size of a single dimension

	/*
		The adjacency matrix stores information about edges between vertices of the graph. I am using a multidimensional array [,] as opposed to a regular array-of-arrays [][] since I can get away without the extra functionality the latter method would otherwise provide.
		
		0	1	0	0	1	Where 0 -- no edge
		1	0	1	1	1	and 1 -- there is an edge
		1	1	0	1	0
		1	0	0	0	1
		0	0	0	1	0
	*/

	#region STANDARD
	internal Graph(int _size)
	{
		size = _size;
		matrix = new bool[size, size];
		vertices = new List<Vertex>();

		//I need to zero out the adjacency matrix first
		for (int i = 0; i < size; i++)
			for (int j = 0; j < size; j++)
				matrix[i, j] = false;
	}

	/* Inserts a new vertex at the end of the <Vertex> list */
	internal void addVertex(string _name, string _country, string[] _misc)
	{
		Vertex newVx = new Vertex(_name, _country, _misc);
		vertices.Add(newVx);
	}

	/* Inserts an edge between two vertices.
	Takes indexes of corresponding vertices.
	false -- no edge; true -- edge is present */
	internal void insertEdge(int vx0, int vx1)
	{
		if (!findEdge(vx0, vx1)) //Making sure the elements aren't already connected
		{
			vertices[vx0].edgeCount++;
			vertices[vx1].edgeCount++;
			matrix[vx0, vx1] = true;
			matrix[vx1, vx0] = true; //The matrix has to be symmetrical relative to the main diagonal, since my edges are undirected
		}
	}

	/* Checks whether two vertices share an edge. Since my graph is undirected, I'll only check it one way */
	internal bool findEdge(int vx0, int vx1)
	{
		bool adjacent = false;

		if (matrix[vx0, vx1] == true) //I only need to check one side, since edges are undirected
			adjacent = true;

		return adjacent;
	}

	/* Returns a reference to the vertex under a given index */
	internal Vertex getVertex(int _index)
	{
		Vertex vx = null;
		if (_index < vertices.Count)
		{
			vx = vertices[_index];
		}
		else
		{
			Debug.Log("Graph.getVertex(): index exceeds the number of vertices.");
		}

		return vx;

	}

	/* Returns the maximum size of the graph */
	internal int getSize()
	{
		return size;
	}

	/* Returns the current number of elements in the graph */
	internal int getCount()
	{
		return vertices.Count;
	}

	internal int getVertexEdgeCount(int index)
	{
		return vertices[index].edgeCount;
	}
	#endregion

	#region GAME
	/* GAME-SPECIFIC METHODS */

	/* Makes data of a vertex accessible to the player.
	Takes the index of a vertex and the index of a misc string.
	Passing -1 as the latter makes all misc strings accessible. */
	internal void openField(int _index, int _fieldIndex)
	{
		/* Fields:
			0 -- name
			1 -- country
			2-4 -- misc */

		if (_fieldIndex != -1)
		{
			vertices[_index].fieldAccess[_fieldIndex] = true;
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
					if (matrix[_index, i] == true)
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
	#endregion

	#region DEBUGGING
	/* DEBUG METHODS */

	/* Prints out all node data and edges as a console entry. */
	internal void printVertices()
	{
		int index = 0;
		string edges;

		foreach (Vertex vx in vertices)
		{
			edges = "";
			for (int i = 0; i < size; i++)
			{
				//Concatenates edges in a string
				edges = edges + " " + matrix[index, i];
			}

			//All data related to a node is neatly packed in a console entry
			Debug.Log(
				index + " " + vx.name + "\n" +
				vx.country + "\n" +
				vx.misc[0] + "\n" +
				vx.misc[1] + "\n" +
				vx.misc[2] + "\n" +
				"Access: " + vx.fieldAccess[0] + " " + vx.fieldAccess[1] + " " + vx.fieldAccess[2] + " " + vx.fieldAccess[3] + " " + vx.fieldAccess[4] + "\n" +
				"Edges: " + vx.edgeCount + " (" + edges + ")\n"
			);
			index++;
		}
	}

	internal void BFT(bool[] visited)
	{
		Queue<int> vxQueue = new Queue<int>(); //I'll use a generic queue for BFT
		int vx = 0;
		//BFT will start with index 0

		vxQueue.Enqueue(0);
		visited[0] = true;

		while (vxQueue.Count != 0)
		{
			vx = vxQueue.Dequeue();

			for (int i = 0; i < size; i++)
			{
				if (matrix[vx, i] == true && visited[i] == false)
				{
					visited[i] = true;
					vxQueue.Enqueue(i);
				}
			}
		}
	}

	/* Validates the graph through BFT */
	internal bool graphConnected()
	{
		bool valid = true;
		bool[] visited = new bool[size];

		for (int i = 0; i < size; i++)
			visited[i] = false;


		BFT(visited);

		//Time to check whether every vertex has been visited
		for (int i = 0; i < size; i++)
		{
			if (!visited[i])
				valid = false;
		}

		return valid;
	}
	#endregion
};


