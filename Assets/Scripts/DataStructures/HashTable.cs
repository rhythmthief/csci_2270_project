using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class HTNode
{
	internal int key;
	internal HTNode next;

	internal HTNode(int _key, HTNode _next)
	{
		key = _key;
		next = _next;
	}
};

/* A basic hash table implementation */
internal class HashTable
{
	private int tableSize;
	private HTNode[] table;

	/* Parameterized constructor, takes the number of buckets to produce */
	internal HashTable(int _size)
	{
		tableSize = _size;
		table = new HTNode[tableSize];

		for (int i = 0; i < tableSize; i++)
		{
			table[i] = null; //I need to set all buckets as null first
		}
	}

	/* Inserts an item into the hash table */
	internal void insertItem(int _key)
	{
		if (!searchItem(_key)) //Won't accept duplicates
		{
			int index = hashFunction(_key); //Calculating the index of a bucket

			table[index] = new HTNode(_key, table[index]); //Pushes a new node to the beginning of the bucket
		}
	}

	/* Returns a hash of the passed key. */
	internal int hashFunction(int _key) => (_key % tableSize);

	/* Checks whether the key is present in the hash table */
	internal bool searchItem(int _key)
	{
		bool found = false;
		int index = hashFunction(_key);
		HTNode walker = table[index]; //Getting a pointer to the first element of a bucket under a given index

		//Traversing and looking for the right key
		while (walker != null)
		{
			if (walker.key == _key)
			{
				found = true;
				break;
			}

			walker = walker.next;
		}

		return found;
	}
};