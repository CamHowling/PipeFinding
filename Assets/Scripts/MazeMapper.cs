using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeMapper : MonoBehaviour
{
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

        public override bool Equals(object obj)
        {
            var otherMaze = obj as MazeCell;
            var isEqual = this.index.Equals(otherMaze.index);
            return isEqual;
        }
    }

    public class MazeStep
    {
        public Vector3 deltaPosition;
        public Vector3Int deltaIndex;
    }

    //UI and Scene input
    public GameObject input;
    public GameObject sprinkler1;
    public GameObject sprinkler2;
    public float stepValue;

    public List<Maze> mazes = new List<Maze>();

    //Private fields
    private bool activeMapping = false;
    private Maze activeMaze;
    private List<MazeStep> axisMovementSteps;
    private Vector3 collisionBoxScale;

    // Start is called before the first frame update
    void Start()
    {
        updateAxisMovementVectors();
        var sprinkler1ToSprinkler2 = new Maze
        {
            startingPosition = sprinkler1.gameObject.transform.position,
            targetPosition = sprinkler2.gameObject.transform.position
        };

        var originCell = new MazeCell
        {
            position = sprinkler1ToSprinkler2.startingPosition,
            stepsFromTarget = 0,
            index = new Vector3Int(0, 0, 0)
        };

        sprinkler1ToSprinkler2.searchCells.Add(originCell);

        var inputToSprinkler1 = new Maze
        {
            startingPosition = input.gameObject.transform.position,
            targetPosition = sprinkler2.gameObject.transform.position
        };

        var originCell2 = new MazeCell
        {
            position = sprinkler1ToSprinkler2.startingPosition,
            stepsFromTarget = 0,
            index = new Vector3Int(0, 0, 0)
        };

        inputToSprinkler1.searchCells.Add(originCell2);

        mazes.Add(sprinkler1ToSprinkler2);
        mazes.Add(inputToSprinkler1);
    }

    // Update is called once per frame
    void Update()
    {
        if(activeMapping)
        {
            var cellsToSearch = new List<MazeCell>();

            foreach (var cell in activeMaze.searchCells)
            {
                var newCells = searchNearbyCells(cell);
                cellsToSearch.AddRange(newCells);
            }

            activeMaze.searchCells = cellsToSearch.Distinct().ToList();
        }
    }

    //May need to remove annotation
    [HideInInspector]
    public void beginMapping(Maze maze)
    {
        activeMaze = mazes[0];
        activeMapping = true;
    }

    [HideInInspector]
    public void updateAxisMovementVectors()
    {
        collisionBoxScale = new Vector3(stepValue/2, stepValue/2, stepValue/2);

        axisMovementSteps.Clear();
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

    private List<MazeCell> searchNearbyCells(MazeCell cell)
    {
        var newCells = new List<MazeCell>();

        foreach (var searchStep in axisMovementSteps)
        {
            //check if the search index has already been searched
            var searchIndex = cell.index + searchStep.deltaIndex;
            if (activeMaze.mazeGrid[searchIndex.x,searchIndex.y,searchIndex.z] != null)
            {
                continue;
            }

            var searchLocation = cell.position + searchStep.deltaPosition;
            var colliders = Physics.OverlapBox(searchLocation, collisionBoxScale);
            var searchedCell = new MazeCell
            {
                position = searchLocation,
                index = searchIndex,
                stepsFromTarget = cell.stepsFromTarget + 1,
                hasCollision = colliders.Length > 0 ? true : false
            };

            activeMaze.mazeGrid[searchIndex.x, searchIndex.y, searchIndex.z] = searchedCell;
            newCells.Add(searchedCell);
        }

        return newCells;
    }
}
