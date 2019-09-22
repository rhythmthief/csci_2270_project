# csci_2270_project

Dennis Windham  

Runs on Unity Engine.  
This game was inspired by detective/mystery board games. The goal of the game is to reconstruct a pre-generated graph through use of clues and deduction.

Gameplay loop:  

1) The player starts with a named vertex.
2) The named vertex and surrounding unnamed vertices are inspected by the player, who then uses the named vertex from (1) to request clues about unnamed vertices it's adjacent to.
3) The player examines all clues from (2) and compares them against a list of vertices available in an on-screen window.
4) One of the vertices from the list is discovered to match clues from (2) and available data of an unnamed vertex, the player attempts to ascertain the unnamed vertex's identity by giving it a name from the list and connecting it to the named vertex from (1).
5) The player verifies their guess, the hitherto unnamed profile is named and displays an edge if the guess was right.
6) Go to step (0) unless the entire graph has been reconstructed, in which case the player wins the game.

Used Data Structures:  
Graph — the underlying data structure of this project.  
HashTable — heavily used during graph generation in Helpers.  
StackLL — A stack implemented using a linked list, exists in every graph vertex.  
BST — used for scoring.  


![picture](https://raw.githubusercontent.com/rhythmthief/csci_2270_project/master/screenshot.png "In-game view.")
