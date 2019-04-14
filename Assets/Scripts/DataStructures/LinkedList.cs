using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	Notes:
		.net automatically collects unused memory so I don't have to deallocate manually.
*/

class LLNode
{
	internal string key;
	internal LLNode next = null; //Object reference to the next element
	internal LLNode prev = null; //Previous element

	/* Parameterized constructor */
	internal LLNode(string _key)
	{
		key = _key;
	}

	/* For debugging only */
	~LLNode()
	{
		//Debug.Log("Deleting " + key);
	}
}

/* Doubly lineked list */
class LinkedList
{
	private LLNode head;
	private int length = 0;

	/* Default constructor */
	internal LinkedList()
	{
		head = null;
	}

	/* Checks whether the linked list is empty. */
	internal bool isEmpty()
	{
		bool empty = false;

		if (head == null)
			empty = true;

		return empty;
	}

	/* Adds a new node at the beginning of the linked list */
	internal void addNode(string _key)
	{
		length++;
		LLNode newNode = new LLNode(_key);

		if (head != null)
			head.prev = newNode;

		newNode.next = head;
		head = newNode;
	}

	/* Deletes the entire linked list */
	internal void deleteList()
	{
		/* Deallocation is unnecessary as .net garbage collector will spot unreferenced nodes and deallocate them for me. */
		head = null; //All I really have to do is null the reference to the beginning of the linked list
	}

	/* Returns the length of the linked list */
	internal int listLength()
	{
		return length;
	}

	/* Traverses the linked list and looks for the node under a given index, then returns its key.
	Returns null if the index is out of bounds. */
	internal string getKey(int _index)
	{
		LLNode walker = null;
		int i = 0;

		if (_index < length && _index > -1) //Making sure we're not out of bounds
		{
			walker = head;

			while (i != _index)
				walker = walker.next;

			return walker.key;
		}
		else
		{
			return null;
		}
	}

	/* Deletes a node under the given index */
	internal void deleteNode(int _index)
	{
		LLNode walker = head;
		int pos = 0; //Current position of the walker

		if (head != null && _index < length)
		{
			//Reaches the right node
			while (pos != _index)
			{
				walker = walker.next;
				pos++;
			}

			//Once I remove all references to a node, the .net garbage collector will realize that memory can be deallocated and immediately do so
			if (walker == head) //Deleting the head
			{
				head.next.prev = null;
				head = head.next;
			}
			else //Deleting any other node
			{
				walker.prev.next = walker.next;

				if (walker.next != null)
					walker.next.prev = walker.prev;
			}
		}
		else
		{
			Debug.Log("LinkedList.deleteKey(): out of bounds.");
		}
	}
}