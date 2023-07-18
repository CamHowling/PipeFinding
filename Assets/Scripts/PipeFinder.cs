using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PipeFinder : MonoBehaviour
{
    //UI and Scene input
    public MazeMapper mazeMapper;
    public List<Maze> mazes = new List<Maze>();
    public GameObject basePipe;
    public GameObject anglePipe;
    public GameObject tPipe;

    private Button generatePipesButton;

    //Private fields
    private float stepValue;
    private Vector3Int currentMazeIndex;
    private Vector3[] pipeline;
    private bool calculatePipeline = false;
    private bool renderPipes = false;
    private Maze mazeToRender;
    private List<Vector3Int> stepVectors;

    // Start is called before the first frame update
    void Start()
    {
        generatePipesButton = GameObject.Find("Pathfinding Button").GetComponent<Button>();
        stepValue = mazeMapper.stepValue;
        basePipe.transform.localScale = new Vector3(stepValue, stepValue/2, stepValue);
        UpdateIndexStepVectors();
    }

    // Update is called once per frame
    void Update()
    {
        if (calculatePipeline)
        {
            if (currentMazeIndex != null) 
            {
                var currentCell = mazeToRender.mazeGrid[currentMazeIndex.x, currentMazeIndex.y, currentMazeIndex.z];
                Instantiate(basePipe, currentCell.position, Quaternion.identity);
            }

            //check here to see if a pipe in this position has already been rendered
            var adjacentCells = GetNextCells(currentMazeIndex);
            var nextCell = adjacentCells.FirstOrDefault();
            //var nextNextCell = GetNextCell(nextCell.index);
            //var firstStepDirection = (nextCell.position - currentCell.position).normalized;
            //var secondStep = (nextNextCell.position - nextCell.position).normalized;
            if (nextCell == null)
            {
                calculatePipeline = false;
                return;
            }

            currentMazeIndex = nextCell.index;
        }
    }

    public void GeneratePipes()
    {
        calculatePipeline = false;
        generatePipesButton.interactable = false;
        mazes = mazeMapper.mazes;
        mazeToRender = mazes.FirstOrDefault(x => x.isSearchComplete);
        if (mazeToRender != null)
        {
            currentMazeIndex = mazeToRender.pipeStartIndex;
            calculatePipeline = true;
            return;
        }
    }

    private List<MazeCell> GetNextCells(Vector3Int index)
    {
        List<MazeCell> cells = new List<MazeCell>();
        foreach (var stepVector in stepVectors) 
        {
            var stepCell = mazeToRender.mazeGrid[index.x + stepVector.x, index.y + stepVector.y, index.z + stepVector.z];
            var isTarget = stepCell != null && stepCell.position == mazeToRender.targetPosition;
            if (isTarget)
            {
                cells.Clear();
                Instantiate(basePipe, stepCell.position, Quaternion.identity);
                return cells;
            }

            if (stepCell != null && !stepCell.hasCollision)
            {
                cells.Add(stepCell);
            }
        }

        var orderedCells = cells.OrderBy(x => x.stepsFromTarget).ToList();
        var fewestSteps = orderedCells[0].stepsFromTarget;
        var nextCells = cells.Where(x => x.stepsFromTarget == fewestSteps).ToList();
        return nextCells;
    }

    private void UpdateIndexStepVectors()
    {
        stepVectors = new List<Vector3Int>();
        stepVectors.Add(new Vector3Int(1, 0, 0));
        stepVectors.Add(new Vector3Int(-1, 0, 0));
        stepVectors.Add(new Vector3Int(0, 1, 0));
        stepVectors.Add(new Vector3Int(0, -1, 0));
        stepVectors.Add(new Vector3Int(0, 0, 1));
        stepVectors.Add(new Vector3Int(0, 0, -1));
    }
}
