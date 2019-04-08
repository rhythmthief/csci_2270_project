using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Notes:
		.net automatically collects unused memory so I don't have to deallocate manually
		This linked list is somewhat barebones as I am not implementing the functionality I won't use in the project
		Default protection level of a node is "internal" -- can be accessed by any code in the same assembly (project)
*/


class LLNode
{
	public string key;
	public LLNode next = null; //Object reference to the next element

	/* Parameterized constructor */
	public LLNode(string _key)
	{
		key = _key;
	}

	//I used this destructor to make sure that memory was being deallocated when I removed elements
	// ~LLNode()
	// {
	// 	Debug.Log("DELETED NODE " + key);
	// }
}

public class LinkedList
{
	private LLNode head;
	private int length = 0;

	/* Default constructor */
	public LinkedList()
	{
		head = null;
	}

	public bool isEmpty()
	{
		bool empty = false;
		
		if (head == null)
			empty = true;

		return empty;
	}

	/* Adding a new node to the top of the linked list */
	public void addNode(string _key)
	{
		length++;
		LLNode newNode = new LLNode(_key);
		newNode.next = head;
		head = newNode;
	}

	/* Deletes the entire linked list */
	public void deleteList()
	{
		/* Deallocation is unnecessary as .net garbage collector will spot unreferenced nodes and deallocate them for me */

		head = null; //All I really have to do is null the reference to the beginning of the linked list
	}

	/* Returns the length of the linked list */
	public int listLength()
	{
		return length;
	}

	/* Traverses the linked list and looks for the node under a given index, then returns its key.
	Returns "-1" if the index is out of bounds. */
	public string getKey(int index)
	{
		LLNode walker = null;
		int i = 0;

		if (index < length && index > -1) //Making sure we're not out of bounds
		{
			walker = head;

			while (i != index)
				walker = walker.next;

			return walker.key;
		}
		else
		{
			return "-1";
		}
	}
}