using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MazeMapper : MonoBehaviour
{
    //TODO move into dedicated class..

    /// <summary>
    /// Represents the Maze problem in the context of connecting two points
    /// </summary>
    [HideInInspector]
    public class Maze
    {
        public MazeCell[,,] mazeGrid;
        public Vector3 startingPosition;
        public Vector3 targetPosition;
        public List<MazeCell> searchCells;
        public bool isSearchComplete;

        //TODO cover GetHashCode()
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
    [HideInInspector]
    public class MazeCell
    {
        public Vector3 position;
        public Vector3Int index;
        public int stepsFromTarget;
        public bool hasCollision;

        //TODO cover GetHashCode()
        public override bool Equals(object obj)
        {
            var otherMaze = obj as MazeCell;
            var isEqual = this.position.Equals(otherMaze.position);
            return isEqual;
        }
    }

    public class MazeStep
    {
        public Vector3 deltaPosition;
        public Vector3Int deltaIndex;
    }

    //UI and Scene input
    public GameObject maze;
    public GameObject input;
    public GameObject sprinkler1;
    public GameObject sprinkler2;
    public GameObject debuggerPrefab;
    public bool debuggerEnabled;
    public float stepValue;

    public List<Maze> mazes = new List<Maze>();

    //Private fields
    private bool activeMapping = false;
    private Maze activeMaze;
    private List<MazeStep> axisMovementSteps;
    private Vector3 collisionBoxScale;
    private Vector3Int maximumIndexVector;

    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 10; //Debuggin only
        debuggerPrefab.transform.localScale = new Vector3(stepValue, stepValue, stepValue);

        var mazeBox = maze.GetComponent<Collider>().bounds.size;
        maximumIndexVector = GetIndexVector(mazeBox);
        
        var upperXBound = (int)Math.Ceiling(mazeBox.x / stepValue) - 1;
        var upperYBound = (int)Math.Ceiling(mazeBox.y / stepValue) - 1;
        var upperZBound = (int)Math.Ceiling(mazeBox.z / stepValue) - 1;    

        UpdateAxisMovementVectors();
        var sprinkler1ToSprinkler2 = new Maze
        {
            startingPosition = sprinkler1.gameObject.transform.position,
            targetPosition = sprinkler2.gameObject.transform.position,
            mazeGrid = new MazeCell[upperXBound, upperYBound, upperZBound],
            searchCells = new List<MazeCell>()
        };

        var sprinkler2Index = GetIndexVector(sprinkler1ToSprinkler2.targetPosition);
        var originCell = new MazeCell
        {
            position = sprinkler1ToSprinkler2.targetPosition,
            stepsFromTarget = 0,
            index = sprinkler2Index
        };

        sprinkler1ToSprinkler2.searchCells.Add(originCell);
        var inputToSprinkler1 = new Maze
        {
            startingPosition = input.gameObject.transform.position,
            targetPosition = sprinkler2.gameObject.transform.position,
            mazeGrid = new MazeCell[upperXBound, upperYBound, upperZBound],
            searchCells = new List<MazeCell>()
        };

        var inputIndex = GetIndexVector(inputToSprinkler1.targetPosition);
        var originCell2 = new MazeCell
        {
            position = inputToSprinkler1.startingPosition, //review
            stepsFromTarget = 0,
            //index = searchDirectionVector.magnitude >= 0 ? inputIndex : maximumIndexVector
            index = inputIndex
        };

        inputToSprinkler1.searchCells.Add(originCell2);

        mazes.Add(sprinkler1ToSprinkler2);
        mazes.Add(inputToSprinkler1);
    }

    private Vector3Int GetIndexVector(Vector3 vector)
    {
        var xPosition = (int)Math.Ceiling(vector.x / stepValue) - 1;
        var yPosition = (int)Math.Ceiling(vector.y / stepValue) - 1;
        var zPosition = (int)Math.Ceiling(vector.z / stepValue) - 1;
        var indexVector = new Vector3Int(xPosition, yPosition, zPosition);
        return indexVector;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeMapping)
        {
            var cellsToSearch = new List<MazeCell>();

            foreach (var cell in activeMaze.searchCells)
            {
                var newCells = SearchNearbyCells(cell);
                if (activeMaze.isSearchComplete)
                {
                    LogGridToConsole();
                    SetActiveMaze();
                    break;
                }

                cellsToSearch.AddRange(newCells);
            }

            activeMaze.searchCells = cellsToSearch.Distinct().ToList();
        }
    }

    //May need to remove annotation
    [HideInInspector]
    public void SetActiveMaze()
    {
        activeMapping = false;
        activeMaze = mazes.FirstOrDefault(x => !x.isSearchComplete);
        if (activeMaze != null) 
        {
            activeMapping = true;
        }
    }

    [HideInInspector]
    public void UpdateAxisMovementVectors()
    {
        var collisionHalfStep = stepValue;
        collisionBoxScale = new Vector3(collisionHalfStep, collisionHalfStep, collisionHalfStep);

        axisMovementSteps = new List<MazeStep>();
        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(stepValue, 0.0f, 0.0f),
            deltaIndex = new Vector3Int(1, 0, 0)
        });

        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(-stepValue, 0.0f, 0.0f),
            deltaIndex = new Vector3Int(-1, 0, 0)
        });

        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(0.0f, stepValue, 0.0f),
            deltaIndex = new Vector3Int(0, 1, 0)
        });

        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(0.0f, -stepValue, 0.0f),
            deltaIndex = new Vector3Int(0, -1, 0)
        });

        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(0.0f, 0.0f, stepValue),
            deltaIndex = new Vector3Int(0, 0, 1)
        });

        axisMovementSteps.Add(new MazeStep
        {
            deltaPosition = new Vector3(0.0f, 0.0f, -stepValue),
            deltaIndex = new Vector3Int(0, 0, -1)
        });
    }

    private List<MazeCell> SearchNearbyCells(MazeCell cell)
    {
        var newCells = new List<MazeCell>();

        foreach (var searchStep in axisMovementSteps)
        {
            
            var searchIndex = cell.index + searchStep.deltaIndex;
            
            var validSearch = IsValidSearchIndex(searchIndex);
            if (!validSearch)
            {
                continue;
            }

            //check if the search index has already been searched
            var gridCell = activeMaze.mazeGrid[searchIndex.x, searchIndex.y, searchIndex.z];
            if (gridCell != null)
            {
                continue;
            }

            var searchLocation = cell.position + searchStep.deltaPosition;
            if (debuggerEnabled)
            {
                Instantiate(debuggerPrefab, searchLocation, Quaternion.identity);
            }
            
            var searchedCell = new MazeCell
            {
                position = searchLocation,
                index = searchIndex,
                stepsFromTarget = cell.stepsFromTarget + 1
            };

            var targetReached = IsTargetWithinStep(cell, searchLocation);        
            if (targetReached)
            {
                Debug.Log("Searched: " + searchLocation.ToString() + ", Found: " + targetReached.ToString());
                activeMaze.mazeGrid[searchIndex.x, searchIndex.y, searchIndex.z] = searchedCell;
                LogGridToConsole();
                //newCells.Add(searchedCell);
                //Starting point reached
                break;
            }

            var originColliders = Physics.OverlapBox(cell.position, collisionBoxScale);
            var colliders = Physics.OverlapBox(searchLocation, collisionBoxScale);
            searchedCell.hasCollision = colliders.Length > 0 ? true : false;

            activeMaze.mazeGrid[searchIndex.x, searchIndex.y, searchIndex.z] = searchedCell;

            if (!searchedCell.hasCollision)
            {
               //Debug.Log("Steps from target: " + searchedCell.stepsFromTarget.ToString());
               newCells.Add(searchedCell);
            }
        }

        return newCells;
    }

    private bool IsTargetWithinStep(MazeCell cell, Vector3 searchLocation)
    {
        var targetVector = cell.position - activeMaze.startingPosition;
        var searchVector = cell.position - searchLocation;
        var targetFound = targetVector.magnitude <= searchVector.magnitude ? true : false;

        return targetFound;
    }

    private bool IsValidSearchIndex(Vector3Int searchIndex)
    {

        if (
            searchIndex.x < 0 || searchIndex.x > maximumIndexVector.x - 1 ||
            searchIndex.y < 0 || searchIndex.y > maximumIndexVector.y - 1 || 
            searchIndex.z < 0 || searchIndex.z > maximumIndexVector.z - 1)
        {
            return false;
        }

        return true;
    }

    private void LogGridToConsole()
    {
        foreach (var gridCell in activeMaze.mazeGrid)
        {
            Debug.Log("Cell Searched: " + gridCell.index.ToString() + " at location : " + gridCell.position.ToString() 
                + ", Steps from target: " + gridCell.stepsFromTarget.ToString() + ", with collision: " + gridCell.hasCollision.ToString());
        }
    }    
}
