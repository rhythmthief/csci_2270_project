using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Basic node of a linked list */
class StackNode
{
	public int key;
	public StackNode next = null;

	/* Uses a parameterized constructor */
	public StackNode(int _key)
	{
		key = _key;
	}
}

/* Stack implemented using a linked list */
public class StackLL
{
	private StackNode head;

	/* Default constructor */
	public StackLL()
	{
		head = null;
	}

	/* Pushes an element to the top of the stack */
	public void push(int key)
	{
		StackNode newNode = new StackNode(key);

		newNode.next = head;
		head = newNode;
	}

	/* Removes an element from the top of the stack */
	public void pop()
	{
		if (head != null)
		{
			/* 
				Note:
				It would appear that .net garbage collection will take care of unreferenced memory for me as soon as I "unlink" an element.
			*/

			head = head.next;
		}
	}

	/* Prints out all elements of the stack in the Unity Editor console */
	public void print()
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