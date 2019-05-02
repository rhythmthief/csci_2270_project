# csci_2270_project

Dennis Windham  
Student ID: 109304752  
Section 303 (Thursdays at 9:30AM)  

Runs on Unity engine.


The goal of the game is to reconstruct a pre-generated graph through progressive traversal of vertices.

Gameplay loop:

0) The player starts with a named vertex.
1) They inspect the named vertex and its surroundings, request clues about vertices its adjacent to.
2) The player examines all clues and compares them against a list of vertices available in a section on-screen.
3) They match an unnamed vertex with a name based on provided clues and connect to the original named vertex.
4) The player verifies their guess, the previously unnamed profile is named and displays an edge if the guess was right.
5) Go to step (0) unless the entire graph has been reconstructed.

Used Data Structures:
Graph -- the underlying data structure of this project.
HashTable -- heavily used during graph generation in Helpers.
StackLL -- A stack implemented using a linked list, exists in every graph vertex.
BST -- used for the scoring.


![picture](https://raw.githubusercontent.com/rhythmthief/csci_2270_project/master/screenshot.png "In-game view.")
