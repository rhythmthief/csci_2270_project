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
	/* In-game data */
	internal string name;
	internal string country;
	internal string[] misc; //Holds strings of miscellaneous information
	internal bool[] fieldAccess = { false, false, false, false, false }; //Tracks whether a particular field from above can be accessed by the player

	/* Clues are cached within the vertices */
	internal StackLL clueCache; //This stack stores all clues that can be accessible via this vertex
	internal bool clueCacheComplete = false;
	internal string clueString; //Contains all popped clues

	internal bool status = false; //Determines whether the vertex has been completely guessed in-game (name + all edges). True -- the vertex is complete
	internal int edgeCount = 0;   //Total number of edges this vertex has
	internal int edgesFoundCount = 0; //The number of edges found by the player
	internal HashTable edgesFoundHT; //A Hash Table with the edges which the player has already discovered. Prevents repeated connections.

	internal GameObject visual; //Visual in-game representation of the vertex

	/* Parameterized constructor for a vertex */
	internal Vertex(string _name, string _country, string[] _misc)
	{
		name = _name;
		country = _country;
		misc = _misc;
		clueCache = new StackLL();
		edgesFoundHT = new HashTable(7);
		clueString = "Clues about acquaintances:";
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
		
		0	1	1	1	0	Where 0 -- no edge
		1	0	1	0	1	and 1 -- there is an edge
		1	1	0	1	0
		1	0	1	0	1
		0	1	0	1	0
	*/

	/* Methods expected in a graph implementation */
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
			vertices[vx0].edgeCount++; //Increments edge counters within vertices
			vertices[vx1].edgeCount++;

			matrix[vx0, vx1] = true;
			matrix[vx1, vx0] = true; //The matrix has to be symmetrical relative to the main diagonal, since my edges are undirected
		}
	}

	/* Checks whether two vertices share an edge. Since my graph is undirected, I'll only check it one way */
	internal bool findEdge(int vx0, int vx1)
	{
		bool adjacent = false;

		if (vx0 > -1 && vx0 < size && vx1 > -1 && vx1 < size) //Making sure we're not out of bounds
			if (matrix[vx0, vx1] == true) //I only need to check one side, since edges are undirected
				adjacent = true;

		return adjacent;
	}

	/* Returns a reference to the vertex under a given index */
	internal Vertex getVertex(int _id)
	{
		Vertex vx = null;
		if (_id < vertices.Count)
			vx = vertices[_id];
		else
			Debug.Log("Graph.getVertex(): index exceeds the number of vertices.");

		return vx;
	}

	/* Returns the maximum size of the graph */
	internal int getSize() => size;

	/* Returns the current number of elements in the graph */
	internal int getCount() => vertices.Count;

	/* Returns the number of edges the vertex shares with other vertices */
	internal int getVertexEdgeCount(int index) => vertices[index].edgeCount;

	/* Traverses to a certain depth from a given vertex and enables GameObjects. One of two overloads of BFT. The second overload is found in the debug section below. */
	internal void BFT(int _vx, int depth)
	{
		Queue<int> vxQueue = new Queue<int>(); //I'll use a generic queue for BFT
		int vx = _vx;

		vxQueue.Enqueue(vx);

		//Traverses the edges and activates corresponding GameObjects for a visual component of an adjacent vertex
		while (vxQueue.Count != 0 && depth != 0)
		{
			vx = vxQueue.Dequeue();
			depth--;

			for (int i = 0; i < size; i++)
			{
				if (matrix[vx, i] == true && vertices[i].visual.gameObject.activeSelf == false) //activeSelf returns true if the game object is active
				{
					vertices[i].visual.gameObject.SetActive(true); //Activating the visuals
					vxQueue.Enqueue(i); //Technically unnecessary, since with the current game logics I am not traversing deeper than 1 edge, but it might change in the future
				}
			}
		}
	}
	#endregion

	/* GAME-SPECIFIC METHODS */
	#region GAME
	/* Making sure the edge the player is attempting to draw hasn't already been discovered */
	internal bool edgeUnique(int _id0, int _id1) => !(getVertex(_id0).edgesFoundHT.searchItem(_id1));

	/* Sets the status of a vertex to true (complete) if requirements are met. 
	Returns the current completion status */
	internal bool setStatus(int _id)
	{
		bool state = false;

		//The vertex is complete if the number of discovered edges matches the number recorded during generation
		if (getVertex(_id).edgeCount == getVertex(_id).edgesFoundCount)
		{
			vertices[_id].status = true;
			state = true;
		}

		return state;
	}

	/* Assigns a visual component of the vertex.
	Takes an index of a vertex and GameObject to assign. */
	internal void setVisual(int _id, GameObject _visual) => vertices[_id].visual = _visual;

	/* Makes data of a vertex accessible to the player.
	Takes the index of a vertex and the index of a misc string.
	Passing -1 as the latter makes all misc strings accessible. */
	internal void openField(int _id, int _fieldIndex)
	{
		/* Fields:
			0 -- name
			1 -- country
			2-4 -- misc */

		if (_fieldIndex != -1)
		{
			vertices[_id].fieldAccess[_fieldIndex] = true;
		}
		else
		{
			//Makes all data accessible
			for (int i = 0; i < vertices[_id].fieldAccess.Length; i++)
			{
				vertices[_id].fieldAccess[i] = true;
			}
		}
	}

	/* Checks whether a field is accessible through UI */
	internal bool checkField(int _id, int field) => getVertex(_id).fieldAccess[field];

	/* Increments the number of edges found by 1 for both vertices, adds vertices to hash tables to be looked up later */
	internal void setEdgesFound(int _id0, int _id1)
	{
		Vertex vx0 = getVertex(_id0);
		Vertex vx1 = getVertex(_id1);

		//Setting the cache flags to false so the player can generate a new one and avoid fetching data about a discovered vertex
		vx0.clueCacheComplete = false;
		vx1.clueCacheComplete = false;

		//These counters are used to track completion of a vertex
		vx0.edgesFoundCount++;
		vx1.edgesFoundCount++;

		//Inserts the discovered edge into a hash table of a vertex for fast look-up (so the player can't duplicate the same edge)
		vx0.edgesFoundHT.insertItem(_id1);
		vx1.edgesFoundHT.insertItem(_id0);
	}

	/* Returns the string with clues for a vertex. */
	internal string getClue(int _id) => getVertex(_id).clueString;

	/* Looks up data from adjacent nodes and returns it as a string.
	Takes the index of a focused vertex and a boolean for execution mode.
	Mode 0 -- pulls up the data one edge away
	Mode 1 -- goes two edges away, used for the final stage of the game */
	internal void newClue(int _id, bool mode)
	{
		Vertex vx = getVertex(_id);

		if (!mode) //Mode 0, used for the majority of the game
		{
			//I am caching all clues in a stack for ease of use. If this statement passes, it means that the cache hasn't been built yet.
			if (!vx.clueCacheComplete)
			{
				vx.clueCache.deleteStack(); //Clears out the old cache
				vx.clueString += "\n"; //Inserting a line break into the clueString string to separate old from new clues

				vx.clueCacheComplete = true;

				for (int i = 0; i < size - 1; i++) //Going to size-1 because the last vertex operates by special rules and therefore its misc doesn't get collected
				{
					if (matrix[_id, i] == true && edgeUnique(_id, i)) //Makes sure the edge hasn't already been discovered, and if so, skips the clueString generation for the checked vertex
					{
						//Cashes the misc data
						for (int j = 0; j < 3; j++)
							vx.clueCache.push(vertices[i].misc[j]);

						//Caches the country
						vx.clueCache.push(vertices[i].country);
					}

				}
			}

			//Once the player requests a clueString, it gets popped from a cache stack and added to a linked list, which is then displayed in-game
			if (!vx.clueCache.isEmpty())
				vx.clueString += "\n" + vx.clueCache.pop();
		}
		else
		{
			//Mode 1 has been deprecated due to time constaints, but it might be back in the future
		}
	}
	#endregion

	/* DEBUG METHODS */
	#region DEBUGGING

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

	/* Traverses the graph in breadth-first fashion and marks vertices as visited. One of two overloads of BFT. */
	internal void BFT(bool[] visited)
	{
		Queue<int> vxQueue = new Queue<int>(); //I'll use a generic queue for BFT
		int vx = 0;
		//BFT will start with index 0

		vxQueue.Enqueue(vx);
		visited[0] = true;

		//Standard BFT
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

	#endregion
};