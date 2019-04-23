using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Basic node of a linked list */
internal class StackNode
{
	internal string key;
	internal StackNode next = null;

	/* Uses a parameterized constructor */
	internal StackNode(string _key) => key = _key;

	/* For debugging only. Prints a message proving that memory has been deallocated by .net garbage collector */
	// ~StackNode()
	// {
	// 	Debug.Log("Deleting " + key);
	// }
}

/* Stack implemented using a linked list */
internal class StackLL
{
	private StackNode head;

	/* Default constructor */
	internal StackLL() => head = null;

	/* Deletes the entire stack. 
		Deallocation is unnecessary as .net garbage collector will spot unreferenced nodes and deallocate them for me.
		All I really have to do is null the reference to the beginning of the linked list. */
	internal void deleteStack() => head = null;

	/* Pushes an element to the top of the stack */
	internal void push(string key)
	{
		StackNode newNode = new StackNode(key);

		newNode.next = head;
		head = newNode;
	}

	/* Removes an element from the top of the stack and returns its key. */
	internal string pop()
	{
		string key = null;
		if (head != null)
		{
			/* Note:
				.net garbage collection will take care of unreferenced memory for me as soon as I "unlink" an element. */

			key = head.key;
			head = head.next;
		}

		return key;
	}

	/* Checks whether the stack is empty. */
	internal bool isEmpty()
	{
		bool empty = false;

		if (head == null)
			empty = true;

		return empty;
	}

	/* Prints out all elements of the stack in the Unity Editor console. */
	internal void print()
	{
		StackNode walker = head;

		Debug.Log("Printing all elements of the stack:");
		while (walker != null)
		{
			Debug.Log(walker.key);
			walker = walker.next;
		}
	}
}