using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* BST Node class */
internal class BSTNode
{
	internal float key;
	internal string name;
	internal BSTNode leftChild;
	internal BSTNode rightChild;

	internal BSTNode(float _key, string _name)
	{
		key = _key;
		name = _name;
	}
};

/* BST implementation */

internal class BST
{
	internal BSTNode root; //Root of the binary search tree

	/* Default constructor */
	internal BST() => root = null;

	/* Deletes the entire binary search tree. Memory is deallocated automatically. */
	internal void deleteTree() => root = null;

	/* Inserts a new element into the BST. 
	Recursively traverses the BST to find the right spot. */
	private BSTNode insertHelper(BSTNode root, BSTNode newNode)
	{
		if (root == null) //Base case
		{
			root = newNode; //Inserting the new element
		}
		else
		{
			//Determines the direction we want to traverse next
			if (root.key > newNode.key)
				root.leftChild = insertHelper(root.leftChild, newNode);
			else
				root.rightChild = insertHelper(root.rightChild, newNode);
		}

		return root;
	}

	/* Inserts a new mode into the BST */
	internal void insert(float _key, string _name) => root = insertHelper(root, new BSTNode(_key, _name));

	/* Helper function for getScore().
	Recursive. Takes the root of a subtree and a result string (by reference) and a place integer to keep track of the current node's position on the leaderboard (by reference). The passed boolean determines the output: 
	false -- for the leaderboards, inverse inorder (R*L)
	true -- for the writable file, preorder (*LR) */
	private string getScoreHelper(BSTNode root, string result, ref int place, bool mode)
	{
		if (root != null) //Implicit base case where root == null
		{
			if (!mode) //Concatenating for the leaderboards, inverse inorder
			{
				result = getScoreHelper(root.rightChild, result, ref place, mode); //Recursive call to the right side

				result += place + "] " + root.key + "	|	" + root.name + "\n"; //Concatenates this node's data to the string
				place++; //Incrementing the place counter

				result = getScoreHelper(root.leftChild, result, ref place, mode); //Recursive call to the left side

			}
			else //For the file, preorder
			{
				result += root.key + "," + root.name + "\n";
				result = getScoreHelper(root.leftChild, result, ref place, mode); //Recursive call to the left side
				result = getScoreHelper(root.rightChild, result, ref place, mode); //Recursive call to the right side
			}
		}

		return result;
	}

	/* Returns the sorted scores stored in the BST. The passed boolean determines the execution mode:
	false -- returns a string for the in-game leaderboards, inorder
	true -- returns a string for the writable file, preorder */
	internal string getScore(bool mode)
	{
		int place = 1;
		return getScoreHelper(root, "", ref place, mode);
	}
}