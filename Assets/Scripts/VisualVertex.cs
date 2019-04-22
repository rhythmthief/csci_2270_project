using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class VisualVertex : MonoBehaviour
{
	internal GameObject vertexPrefab; //The GameObject I'll be reproducting from this instance
	internal GameObject driver; //Reference to the game driver
	public int id; //Public access level exposes the variable in the Unity editor, useful for debugging

	internal void Start()
	{
		driver = this.transform.parent.transform.parent.gameObject; //Saves a reference to the object with the driver script attached
	}

	/* Builds a visual representation for the given graph.
	Take the following integers:
	id		-- id of the current element within the Graph data structure
	level		-- level of the pyramid that's currently being built
	levelIndex	-- id of the current element within its pyramid level
	totalCount	-- total number of elements generated so far, passed by reference
	size			-- maximum number of elements to generate
	Technically recursive, although it calls this method attached to another GameObjects */
	internal void buildVisualGraph(int _id, int level, int levelIndex, ref int totalCount, int size, Graph gameGraph)
	{
		GameObject newVertex;
		vertexPrefab = this.gameObject;
		id = _id; //Corresponds with a vertex in the game graph

		gameGraph.setVisual(id, this.gameObject); //Adds a reference to the game object this script is attached to into the graph

		totalCount++;
		this.name = "" + id + " | " + level + "_" + levelIndex;

		/* Algorithm:
		1) Fill up the current level
		2) Jump to the next level and repeat */

		if (level != levelIndex && totalCount < size) //Recursive calls to finish a level will stop once this condition fails
		{
			//Instantiate() clones a passed GameObject
			newVertex = Instantiate(vertexPrefab, transform.position + new Vector3(5, 0, 0), transform.rotation);
			newVertex.transform.SetParent(this.gameObject.transform.parent);
			newVertex.GetComponent<VisualVertex>().buildVisualGraph(totalCount, level, levelIndex + 1, ref totalCount, size, gameGraph);
		}

		if (levelIndex == 0 && totalCount < size) //Index 0 belongs to the first element in a level, we'll use it to "jump" to the next level and start generation again
		{
			newVertex = Instantiate(vertexPrefab, transform.position + new Vector3(-2.5f, 0, 5), transform.rotation);
			newVertex.transform.SetParent(this.gameObject.transform.parent);
			newVertex.GetComponent<VisualVertex>().buildVisualGraph(totalCount, level + 1, 0, ref totalCount, size, gameGraph);
		}

		/* Disables this GameObject unless it's the last two elements added to the graph. */
		if (id < size - 2)
			this.gameObject.SetActive(false);

		/* Setting the color of the object */
		if (id < size - 1)
			setColor(2);
		else
			setColor(1); //The last object is special


		/* Rolling random animation speed for the mesh. */
		transform.GetChild(2).GetComponent<Animator>().speed = Random.Range(0.5f, 1.0f);
	}

	/* Called when a collider of the object registers a mouse click 
	Clicks are processed by the driver. */
	void OnMouseDown() => driver.GetComponent<Driver>().clickVertex(id);

	/* Changes the color of the vertex. Takes in integer and uses it to choose from a preselected list of colors. */
	public void setColor(int i)
	{
		//0 -- Green, complete
		//1 -- Yellow, verified, incomplete
		//2 -- Red, unverified, incomplete

		//This array stores the color data
		Color[] colors = new Color[3] { new Color(0, 1.5f, 0), new Color(1.5f, 1.5f, 0), new Color(1.5f, 0, 0) };

		//Setting the material color
		transform.GetChild(2).GetComponent<Renderer>().material.color = colors[i];
	}
}