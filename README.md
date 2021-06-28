# Hierarchical-PathFinding

My attempt on the hierarchical pathfinding based on the Near Optimal Hierarchical Path-Finding by Adi Botea, Martin Muller, Jonathan Schaeffer. 

The paper can be found on http://webdocs.cs.ualberta.ca/~mmueller/ps/hpastar.pdf

This solution have modified methods and do not use Edge calculation/storage.
The cluster have fixed ammount of subCluster (4) to make it easy
This is 100% working.

How to use:
 - create an object and attach the CreateGrid.cs on it -> grid object 
    - attach the AbstractGraph.cs on it and set the level
 - create an object that will move on the grid 
    - attach the following scripts:
      - PlayerController.cs
      - HierarchicalPathfinding.cs
      - Movement.cs
 The input to set path is the mouse left click
 I suggest you to click only on the grid region, since I didn't iterate on the other possibilities
 
 Feel free to make adjustments        
