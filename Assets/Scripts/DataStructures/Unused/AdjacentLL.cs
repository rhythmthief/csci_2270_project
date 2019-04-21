using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	!!!WARNING!!!
	THIS IMPLEMENTATION IS CURRENTLY NOT USED IN THE FINAL PROJECT.
	I scrapped it in favor of an adjacency matrix, but decided to keep the code for the time being.
	Moreover, this file has never been compiled.
*/

class AdjNode
{
	internal Vertex adj;
	internal AdjNode next = null;

	internal AdjNode(Vertex _adj)
	{
		adj = _adj;
	}
}

internal class AdjacentLL
{
	AdjNode head = null;

	/* Adds a new node to the beginning of the list */
	internal void addAdj(Vertex _vertex)
	{
		AdjNode newNode = new AdjNode(_vertex);
		newNode.next = head;
		head = newNode;
	}

	internal bool isAdjacent(Vertex _vertex)
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

	internal bool isEmpty()
	{
		bool empty = false;

		if (head == null)
			empty = true;

		return empty;
	}
}