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
	public bool show = false;
	public LLNode next = null; //Object reference to the next element

	/* Parameterized constructor */
	public LLNode(string _key)
	{
		key = _key;
	}
}

public class LinkedList
{
	private LLNode head;

	public LinkedList()
	{
		head = null;
	}

	/* Adding a new node to the top of the linked list */
	public void addNode(string _key)
	{
		LLNode newNode = new LLNode(_key);
		newNode.next = head;
		head = newNode;
	}

	public void deleteList()
	{
		/* Deletion is unnecessary as .net garbage collector will spot unlinked nodes and deallocate them for me */
		while (head != null)
			head = head.next;
	}
}