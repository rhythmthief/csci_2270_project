using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	!!!WARNING!!!
	THIS IMPLEMENTATION IS CURRENTLY NOT USED IN THE FINAL PROJECT.
	I scrapped it in favor of an adjacency matrix, but decided to keep the code for the time being.
*/

class AdjNode
{
	public Vertex adj;
	public AdjNode next = null;

	public AdjNode(Vertex _adj)
	{
		adj = _adj;
	}
}

public class AdjacentLL
{
	AdjNode head = null;

	/* Adds a new node to the beginning of the list */
	public void addAdj(Vertex _vertex)
	{
		AdjNode newNode = new AdjNode(_vertex);
		newNode.next = head;
		head = newNode;
	}

	public bool isAdjacent(Vertex _vertex)
	{
		bool pass = false; //Turns to true if we determine that two vertices are adjacent
		AdjNode walker = head;

		while (walker != null)
		{
			if (walker.adj == _vertex)
			{
				pass = true;
				break;
			}
			walker = walker.next;
		}

		return pass;
	}

	public bool isEmpty()
	{
		bool empty = false;

		if (head == null)
			empty = true;

		return empty;
	}
}