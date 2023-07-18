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
    private MazeCell currentCell;
    private float stepValue;
    private Vector3Int currentMazeIndex;
    private List<MazePipe> pipeline;
    private bool calculatePipeline = false;
    private Maze mazeToRender;
    private List<Vector3Int> stepVectors;

    // Start is called before the first frame update
    void Start()
    {
        pipeline = new List<MazePipe>();
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
            if (currentMazeIndex == null) 
            {
                return;
            }

            currentCell = mazeToRender.mazeGrid[currentMazeIndex.x, currentMazeIndex.y, currentMazeIndex.z];
            AddPipe(currentCell);

            //check here to see if a pipe in this position has already been rendered
            var nextCell = GetNextCell(currentMazeIndex);
            if (nextCell == null)
            {
                calculatePipeline = false;
                return;
            }

            currentMazeIndex = nextCell.index;
        }

        RenderPipeline();
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

    private void RenderPipeline()
    {
        foreach (var pipe in pipeline)
        {
            if (!pipe.IsRendered)
            {
                Instantiate(basePipe, pipe.position, Quaternion.identity);
                pipe.IsRendered = true;
            }
        }
    }

    private MazeCell GetNextCell(Vector3Int index)
    {
        List<MazeCell> cells = new List<MazeCell>();
        foreach (var stepVector in stepVectors) 
        {
            var stepCell = mazeToRender.mazeGrid[index.x + stepVector.x, index.y + stepVector.y, index.z + stepVector.z];
            var isTarget = stepCell != null && stepCell.position == mazeToRender.targetPosition;
            if (isTarget)
            {
                AddPipe(stepCell);
                return null;
            }

            if (stepCell == null || stepCell.hasCollision)
            {
                continue;
            }

            cells.Add(stepCell);

        }

        var orderedCells = cells.OrderBy(x => x.stepsFromTarget).ToList();
        var fewestSteps = orderedCells[0].stepsFromTarget;
        var nextCells = cells.Where(x => x.stepsFromTarget == fewestSteps).ToList();
        return nextCells.FirstOrDefault();
    }

    private void AddPipe(MazeCell cell)
    {
        var previousCell = currentCell;
        currentCell = cell;

        var pipe = new MazePipe()
        {
            position = currentCell.position,
        };

        pipeline.Add(pipe);
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


    private Dictionary<PipeType, GameObject> GetPipePrefabDictionary()
    {
        var dictionary = new Dictionary<PipeType, GameObject>
        {
            { PipeType.Base, basePipe },
            { PipeType.TSection, tPipe },
            { PipeType.RightAngle, anglePipe }
        };

        return dictionary;
    }
}
