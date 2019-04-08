using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Notes:
		Edges are unweighted and undirected

	TODOs:
		Function to retrieve tips through the graph
*/

public class Vertex
{
	public int index;
	public string name;
	public string country;
	public string[] misc;
	public Vertex next;

	/* Parameterized constructor */
	public Vertex(int _index, string _name, string _country, string[] _misc)
	{
		index = _index;
		name = _name;
		country = _country;
		misc = _misc;
	}

	//The default constructor is empty and gets used only to initialize the head of an empty linked list of vertices
	public Vertex() { }
}

/* Graph implentation */
public class Graph
{
	private int numVertices = 0;
	private Vertex head = new Vertex();
	private int[,] matrix;

	/*
		The adjacency matrix stores information about edges between vertices of the graph. I am using a multidimensional array [,] as opposed to a regular array-of-arrays [][] since I can get away without the extra functionality the latter method would otherwise provide.
		
		0	1	0	0	1	Where 0 -- no edge
		1	0	1	1	1	and 1 -- there is an edge
		1	1	0	1	0
		1	0	0	0	1
		0	0	0	1	0
	*/

	public Graph(int size)
	{
		matrix = new int[size, size];

		//I need to zero out the adjacency matrix first
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				matrix[i, j] = 0;
			}
		}
	}

	/* Inserts a new vertex into the linked list */
	public void insertVertex(int _index, string _name, string _country, string[] _misc)
	{
		Vertex newVertex = new Vertex(numVertices, _name, _country, _misc);
		newVertex.next = head;
		head = newVertex;

		numVertices++;
	}

	/* 
		Inserts an edge into linked lists of both vertices
		Edges are undirected
		Since I'm using an adjacency matrix, I'll pass indexes of vertices
		0 -- no edge; 1 -- edge is present
	*/
	public void insertEdge(int vx0, int vx1)
	{
		matrix[vx0, vx1] = 1;
		matrix[vx1, vx0] = 1; //The matrix technically has to be symmetrical relative to the main diagonal, since my edges are undirected
	}

	/* Checks whether two vertices share an edge. Since my graph is undirected, I'll only check it one way */
	public bool checkAdjacency(int vx0, int vx1)
	{
		bool adjacent = false;

		if (matrix[vx0, vx1] == 1) //I only need to check one side, since edges are undirected
			adjacent = true;

		return adjacent;
	}

	/* Returns a reference to the vertex under a given index */
	public Vertex findVertex(int _index)
	{
		Vertex walker = null;
		if (_index < numVertices)
		{
			walker = head;

			while (walker != null) //Traverses the linked list and looks for the right element
			{
				if (walker.index == _index)
					break;

				walker = walker.next;
			}
		}
		else
		{
			Debug.Log("Error: index exceeds the number of vertices.");
		}

		return walker;
	}

	/* Looks up information from adjacent nodes and returns it as a string */
	public string generateTip(int _index)
	{
		



		return null;
	}
}