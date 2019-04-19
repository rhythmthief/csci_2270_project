using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class visualVertex : MonoBehaviour
{
	internal GameObject vertexPrefab;
	public int id;


	/* Builds a visual representation for the given graph.
	Take the following integers:
	id		-- id of the current element within the Graph data structure
	level		-- level of the pyramid that's currently being built
	levelIndex	-- id of the current element within its pyramid level
	totalCount	-- total number of elements generated so far, passed by reference
	size			-- maximum number of elements to generate
	Technically recursive, although it calls this method attached to other GameObjects */
	internal void buildVisualGraph(int _id, int level, int levelIndex, ref int totalCount, int size, ref List<GameObject> vertices)
	{
		vertexPrefab = this.gameObject;
		GameObject newVertex;

		id = _id; //Corresponds with a vertex in the game graph
		totalCount++;
		vertices.Add(this.gameObject);
		this.name = "" + id + " | " + level + "_" + levelIndex; 

		/* Algorithm:
		1) Fill up the current level
		2) Jump to the next level and repeat */

		if (level != levelIndex && totalCount < size) //Recursive calls to finish a level will stop once this condition fails
		{
			newVertex = Instantiate(vertexPrefab, transform.position + new Vector3 (5,0,0), transform.rotation);
			newVertex.transform.SetParent(this.gameObject.transform.parent);
			newVertex.GetComponent<visualVertex>().buildVisualGraph(totalCount, level, levelIndex+1, ref totalCount, size, ref vertices);
		}

		if (levelIndex == 0 && totalCount < size) //Index 0 belongs to the first element in a level, we'll use it to "jump" to the next level and start generation again
		{
			newVertex = Instantiate(vertexPrefab, transform.position + new Vector3 (-2.5f, 0, 5), transform.rotation);
			newVertex.transform.SetParent(this.gameObject.transform.parent);
			newVertex.GetComponent<visualVertex>().buildVisualGraph(totalCount, level+1, 0, ref totalCount, size, ref vertices);
		}
	}

	/* Called when a collider of the object registers a mouse click */
	void OnMouseDown()
	{
	}
}
