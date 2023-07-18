using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public GameObject BasePipeText;
    public GameObject AnglePipeText;
    public GameObject TPipeText;
    public GameObject TotalPipeLengthText;
    public GameObject RightCamera;
    public GameObject PipeCamera;
    public GameObject AboveCamera;
    public GameObject PipeVerticalCamera;

    //Private
    private Button generatePipesButton;

    //Fields
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
        basePipe.transform.localScale = new Vector3(stepValue, stepValue, stepValue);
        anglePipe.transform.localScale = new Vector3(stepValue, stepValue, stepValue);
        tPipe.transform.localScale = new Vector3(stepValue, stepValue, stepValue);
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
            var nextCell = GetNextCell((Vector3Int)currentMazeIndex);
            if (nextCell == null)
            {
                calculatePipeline = false;
            }

            currentMazeIndex = nextCell != null ? nextCell.index : Vector3Int.zero;
        }

        RenderPipeline();
        UpdatePipeCounts();
    }

    public void GeneratePipes()
    {
        RightCamera.GetComponent<Camera>().enabled = false;
        PipeCamera.GetComponent<Camera>().enabled = true;
        AboveCamera.GetComponent<Camera>().enabled = false;
        PipeVerticalCamera.GetComponent<Camera>().enabled = true;

        calculatePipeline = false;
        generatePipesButton.interactable = false;
        if (mazes.Count == 0)
        {
            mazes = mazeMapper.mazes;
        }
        
        mazeToRender = mazes.FirstOrDefault(x => x.isSearchComplete && !x.isRenderComplete);
        if (mazeToRender != null)
        {
            currentMazeIndex = mazeToRender.pipeStartIndex;
            calculatePipeline = true;
            return;
        }
    }

    private void UpdatePipeCounts()
    {
        var baseCount = 0;
        var angleCount = 0;
        var tCount = 0;

        foreach (var pipe in pipeline)
        {
            switch (pipe.type)
            {
                case PipeType.Base: 
                    baseCount++; 
                    break;
                case PipeType.RightAngle: 
                    angleCount++; 
                    break;
                case PipeType.TSection: 
                    tCount++; 
                    break;
            }
        }

        BasePipeText.GetComponent<TextMeshProUGUI>().text = baseCount.ToString();
        AnglePipeText.GetComponent<TextMeshProUGUI>().text = angleCount.ToString();
        TPipeText.GetComponent <TextMeshProUGUI>().text = tCount.ToString();
        TotalPipeLengthText.GetComponent<TextMeshProUGUI>().text = (pipeline.Count * stepValue).ToString("0.00") + " M";
    }

    private void RenderPipeline()
    {
        foreach (var pipe in pipeline)
        {
            var pipeIndex = pipeline.IndexOf(pipe);
            if ((pipe.prevPipe == null && pipeIndex != 0) ||
                (pipe.nextPipe == null && pipeIndex != pipeline.Count()))
            {
                UpdatePipeTypeAndRotation(pipe, pipeIndex);
            }

            if (pipe.gameObject == null)
            {
                pipe.gameObject = Instantiate(basePipe, pipe.position, pipe.rotation);
            }
        }

        if (calculatePipeline == false && pipeline.Count() > 0 && pipeline[pipeline.Count() - 1].gameObject != null)
        {
            var mazeToUpdate = mazes.FirstOrDefault(x => x.isSearchComplete && !x.isRenderComplete);
            if (mazeToUpdate != null) 
            {
                mazeToUpdate.isRenderComplete = true;
            }
            
            GeneratePipes();
        }
    }

    private void UpdatePipeTypeAndRotation(MazePipe pipe, int pipeIndex)
    {
        pipe.prevPipe = pipeIndex != 0 ? pipe.position - pipeline[pipeIndex - 1].position : null;
        pipe.nextPipe = pipeIndex != pipeline.Count - 1 ? pipeline[pipeIndex + 1].position - pipe.position : null;
        pipe.rotation = Quaternion.identity;
        pipe.type = PipeType.Base;

        if (pipe.prevPipe == null && pipe.nextPipe == null)
        {
            return;
        }

        var nextUnitVector = pipe.nextPipe != null ? ((Vector3)pipe.nextPipe).normalized : Vector3.zero;
        if (pipe.prevPipe == null && pipe.nextPipe != null)
        {
            pipe.rotation = Quaternion.FromToRotation(Vector3.up, nextUnitVector);
            return;
        }

        var prevUnitVector = pipe.prevPipe != null ? ((Vector3)pipe.prevPipe).normalized : Vector3.zero;
        if (pipe.prevPipe != null && pipe.nextPipe == null)
        {
            pipe.rotation = Quaternion.FromToRotation(Vector3.up, prevUnitVector);
            return;
        }

        //Straight pipe
        var dotProduct = Vector3.Dot(prevUnitVector, nextUnitVector);
        if (dotProduct == 1)
        {

            pipe.rotation = Quaternion.FromToRotation(Vector3.up, nextUnitVector);
            return;
        }

        //Orthogonal, IE angle pipe
        if (dotProduct == 0)
        {
            
            pipe.rotation = Quaternion.LookRotation(nextUnitVector, prevUnitVector);
            pipe.type = PipeType.RightAngle;
            if (pipe.gameObject != null)
            {
                Destroy(pipe.gameObject);
            }

            pipe.gameObject = Instantiate(anglePipe, pipe.position, pipe.rotation);
            return;
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
