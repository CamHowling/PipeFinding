using System.Collections.Generic;
using UnityEngine;

//TODO; Interface, 

/// <summary>
/// Represents the Maze problem in the context of connecting two points
/// </summary>
public class Maze
{
    public MazeCell[,,] mazeGrid;
    public Vector3 startingPosition;
    public Vector3 targetPosition;
    public Vector3Int pipeStartIndex;
    public List<MazeCell> searchCells;
    public bool isSearchComplete;

    public override bool Equals(object obj)
    {
        var otherMaze = obj as Maze;
        var isEqual = this.startingPosition.Equals(otherMaze.startingPosition) && this.targetPosition.Equals(otherMaze.targetPosition);
        return isEqual;
    }
}

/// <summary>
/// Represents a 3D cell of the maze object
/// </summary>
public class MazeCell
{
    public Vector3 position;
    public Vector3Int index;
    public int stepsFromTarget;
    public bool hasCollision;

    public override bool Equals(object obj)
    {
        var otherMaze = obj as MazeCell;
        var isEqual = this.position.Equals(otherMaze.position);
        return isEqual;
    }
}

/// <summary>
/// Represents a search step's distance and index in the maze array
/// </summary>
public class MazeStep
{
    public Vector3 deltaPosition;
    public Vector3Int deltaIndex;
}

/// <summary>
/// Pipe object, including it's index and direction. Note only directions parallel to the axis are valid for this implementation.
/// </summary>
public class MazePipe
{
    public Vector3 position;
    public Vector3 prevPipe;
    public Vector3 nextPipe;
    public Vector3 rotation; 
    public PipeType type;
    public bool IsRendered; //might not need
}

public enum PipeType
{
    Base = 0,
    RightAngle = 1,
    TSection =2
}