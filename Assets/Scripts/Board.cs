using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    static private Board _instance;
    public ChessType turn = ChessType.Black;
    public int[,] grid;//lines on board 
    public int[,] simulateGrid;//grid to simulate

    public GameObject[] ChessPrefab;//black , white chess prefab
    public Transform parent;

    public List<Vector2> availableMoves = new List<Vector2>();//store current available positions

    public int WhiteCount = 0;
    public int BlackCount = 0;

    private float offsetChess = 0.5f;
    private float eplision = -0.001f;

    public bool whiteCan = true;
    public bool blackCan = true;

    public bool possibleMove = false;

    private int checkIsValidCount;

    private float timer = 5;

    static public bool isSimulating = false;//control monte carlo AI to simulate
    public bool isEnding = false;//game state

    //singleton pattern
    public static Board Instance
    {
        get { return _instance; }
    }
    private void Awake()
    {
        if (Instance == null)
            _instance = this;
    }
    //initialize grid and simulateGrid, four chesses in the beginning
    void Start()
    {
        grid = new int[8, 8];//lines on board
        //four chesses at beginning
        grid[3, 3] = (int)ChessType.Black;
        grid[4, 4] = (int)ChessType.Black;
        grid[3, 4] = (int)ChessType.White;
        grid[4, 3] = (int)ChessType.White;
        //for simulate
        simulateGrid = new int[8, 8];
        simulateGrid[3, 3] = (int)ChessType.Black;
        simulateGrid[4, 4] = (int)ChessType.Black;
        simulateGrid[3, 4] = (int)ChessType.White;
        simulateGrid[4, 3] = (int)ChessType.White;
    }
    void Update()
    {
        //if no more moves for current player, let component play
        if (turn == ChessType.Black)
        {
            CheckAllowedMoves();
            if (availableMoves.Count == 0)
                turn = ChessType.White;
        }
        if (turn == ChessType.White)
        {
            CheckAllowedMoves();
            if (availableMoves.Count == 0)
                turn = ChessType.Black;
        }

        if (isEnding)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                //restart game
                _instance = null;
                this.Start();
                SceneManager.LoadScene(0);
            }
        }
    }
    //use player/AI input position to initialize chess
    public bool StartPlay(int[] pos)
    {
        pos[0] = Mathf.Clamp(pos[0], 0, 8);
        pos[1] = Mathf.Clamp(pos[1], 0, 8);
        //if there is chess on certain position , return
        if (grid[pos[0], pos[1]] != 0)
            return false;
        //initialize valid position count
        checkIsValidCount = 0;

        if (isSimulating == true)
        {
            if (turn == ChessType.Black)
            {
                simulateGrid[pos[0], pos[1]] = 1;
                //check if winning state
                CheckWinning(pos);
                turn = ChessType.White;
            }
            else if (turn == ChessType.White)
            {
                simulateGrid[pos[0], pos[1]] = 2;
                //check if winning state
                CheckWinning(pos);
                turn = ChessType.Black;
            }
        }
        else
        {
            if (turn == ChessType.Black)
            {
                grid[pos[0], pos[1]] = 1;
                simulateGrid[pos[0], pos[1]] = 1;
                //check if winning state
                CheckWinning(pos);
                //if invalid position, initialize new chess
                if (blackCan)
                {
                    GameObject move = Instantiate(ChessPrefab[0], new Vector3(pos[0] + offsetChess, pos[1] + offsetChess), Quaternion.identity);
                    move.transform.SetParent(parent);
                }
                else
                {
                    grid[pos[0], pos[1]] = 0;
                    simulateGrid[pos[0], pos[1]] = 0;
                    return false;
                }
                turn = ChessType.White;
            }
            else if (turn == ChessType.White)
            {
                grid[pos[0], pos[1]] = 2;
                simulateGrid[pos[0], pos[1]] = 2;
                //check if winning state
                CheckWinning(pos);
                //if invalid position, initialize new chess
                if (whiteCan)
                {
                    GameObject move = Instantiate(ChessPrefab[1], new Vector3(pos[0] + offsetChess, pos[1] + offsetChess), Quaternion.identity);
                    move.transform.SetParent(parent);
                }
                else
                {
                    grid[pos[0], pos[1]] = 0;
                    simulateGrid[pos[0], pos[1]] = 0;
                    return false;
                }
                turn = ChessType.Black;
            }
        }
        return true;
    }
    //check current potential positions for current player
    public void CheckAllowedMoves()
    {
        possibleMove = false;
        availableMoves.Clear();

        if (isSimulating == false)
        {
            //checking real grid
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (grid[i, j] == 0)
                    {
                        CheckAvailablePos(new int[2] { i, j });
                    }
                    else
                        continue;
                }
            }
        }
        else
        {
            //checking simulation grid
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (simulateGrid[i, j] == 0)
                    {
                        CheckAvailablePos(new int[2] { i, j });
                    }
                    else
                        continue;
                }
            }
        }
        if (availableMoves.Count != 0 && isEnding==false)
        {
            possibleMove = true;
        }
    }
    //check valid position, not including convert chess
    public void CheckAvailablePos(int[] pos)
    {
        CheckLine(pos, new int[2] { 1, 1 }, false);
        CheckLine(pos, new int[2] { 1, 0 }, false);
        CheckLine(pos, new int[2] { 1, -1 }, false);
        CheckLine(pos, new int[2] { 0, 1 }, false);
    }
    //check valid position, including convert chess
    public void CheckWinning(int[] pos)
    {
        CheckLine(pos, new int[2] { 1, 1 }, true);
        CheckLine(pos, new int[2] { 1, 0 }, true);
        CheckLine(pos, new int[2] { 1, -1 }, true);
        CheckLine(pos, new int[2] { 0, 1 }, true);

        //check if the input position is valid, if not, limit player
        if (checkIsValidCount == 0 && turn == ChessType.Black)
        {
            blackCan = false;
            Debug.Log("Invalid");
        }
        else if (checkIsValidCount == 0 && turn == ChessType.White)
        {
            whiteCan = false;
            Debug.Log("Invalid");
        }
        else if (checkIsValidCount != 0 && turn == ChessType.Black)
        {
            blackCan = true;
        }
        else if (checkIsValidCount != 0 && turn == ChessType.White)
        {
            whiteCan = true;
        }

        //initialize black and white chess count
        BlackCount = 0;
        WhiteCount = 0;

        //calculate black and white chess
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (isSimulating == false)
                {
                    if (grid[i, j] == 1)
                        BlackCount++;
                    if (grid[i, j] == 2)
                        WhiteCount++;
                }
                else
                {
                    if (simulateGrid[i, j] == 1)
                        BlackCount++;
                    if (simulateGrid[i, j] == 2)
                        WhiteCount++;
                }
            }
        }
        //check game state
        if (BlackCount + WhiteCount == 64)
        {
            if (BlackCount > WhiteCount)
                Debug.Log("black win");
            else if (BlackCount < WhiteCount)
                Debug.Log("white win");
            else
                Debug.Log("even");

            if(isSimulating==false)
                isEnding = true;
        }
        else if (BlackCount == 0)
        {
            if (isSimulating == false)
                isEnding = true;
            Debug.Log("white win");
        }
        else if (WhiteCount == 0)
        {
            if (isSimulating == false)
                isEnding = true;
            Debug.Log("black win");
        }
    }
    //reset simualte board to current board everytime after AI simulation
    public void ResetBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                simulateGrid[i, j] = grid[i, j];
            }
        }
        turn = ChessType.White;
    }
    //check available moves, convertion, simulation
    public int CheckLine(int[] pos, int[] offset, bool isForChessConvert)
    {
        bool matching = false;
        bool continueChecking = false;
        Stack<Vector3> tempTransChess = new Stack<Vector3>();

        //check left
        for (int i = offset[0], j = offset[1]; (pos[0] + i >= 0 && pos[0] + i < 8) && 
            (pos[1] + j >= 0 && pos[1] + j < 8); i += offset[0], j += offset[1])
        {
            //check for AI simulation
            if (isSimulating == true)
            {
                //if the same color's chess is neighbor, break, 
                //if the same color's chess is not neighbor and linked, add the position to available moves
                if (simulateGrid[pos[0] + i, pos[1] + j] == (int)turn)
                {
                    if (!continueChecking)
                        break;
                    else
                    {
                        matching = true;
                        if (isForChessConvert)
                            checkIsValidCount++;
                        if (!isForChessConvert)
                            availableMoves.Add(new Vector2(pos[0], pos[1]));
                        break;
                    }
                }
                else if (simulateGrid[pos[0] + i, pos[1] + j] == 0)//if neighbor is empty, break
                    break;
                else
                {
                    tempTransChess.Push(new Vector3(pos[0] + i, pos[1] + j));
                    continueChecking = true;
                }
            }
            else
            {
                if (grid[pos[0] + i, pos[1] + j] == (int)turn)
                {
                    if (!continueChecking)
                        break;
                    else
                    {
                        matching = true;
                        if (isForChessConvert)
                            checkIsValidCount++;
                        if (!isForChessConvert)
                            availableMoves.Add(new Vector2(pos[0], pos[1]));
                        break;
                    }
                }
                else if (grid[pos[0] + i, pos[1] + j] == 0)
                    break;
                else
                {
                    tempTransChess.Push(new Vector3(pos[0] + i, pos[1] + j));
                    continueChecking = true;
                }
            }
        }
        //if convert chesses is greater than zero and not for AI simulate, initialize new convert chesses
        while (tempTransChess.Count != 0 && isForChessConvert && isSimulating==false)
        {
            Vector3 v = tempTransChess.Pop();
            v += new Vector3(offsetChess, offsetChess, eplision);
            eplision += -0.001f;
            if (turn == ChessType.Black && matching)
            {
                grid[(int)v.x, (int)v.y] = (int)turn;
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
                GameObject trans = Instantiate(ChessPrefab[0], v, Quaternion.identity);
                trans.transform.SetParent(parent);
            }
            if (turn == ChessType.White && matching)
            {
                grid[(int)v.x, (int)v.y] = (int)turn;
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
                GameObject trans = Instantiate(ChessPrefab[1], v, Quaternion.identity);
                trans.transform.SetParent(parent);
            }
        }
        //if convert chesses is greater than zero and for AI simulate, store position to available moves
        while (tempTransChess.Count != 0 && isForChessConvert && isSimulating == true)
        {
            Vector3 v = tempTransChess.Pop();
            if (turn == ChessType.Black && matching)
            {
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
            }
            if (turn == ChessType.White && matching)
            {
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
            }
        }

        //check right
        matching = false;
        continueChecking = false;
        for (int i = -offset[0], j = -offset[1]; (pos[0] + i >= 0 && pos[0] + i < 8) &&
            (pos[1] + j >= 0 && pos[1] + j < 8); i -= offset[0], j -= offset[1])
        {
            if (isSimulating == true)
            {
                if (simulateGrid[pos[0] + i, pos[1] + j] == (int)turn)
                {
                    if (!continueChecking)
                        break;
                    else
                    {
                        matching = true;
                        if (isForChessConvert)
                            checkIsValidCount++;
                        if (!isForChessConvert)
                            availableMoves.Add(new Vector2(pos[0], pos[1]));
                        break;
                    }
                }
                else if (simulateGrid[pos[0] + i, pos[1] + j] == 0)
                    break;
                else
                {
                    tempTransChess.Push(new Vector3(pos[0] + i, pos[1] + j));
                    continueChecking = true;
                }
            }
            else
            {
                if (grid[pos[0] + i, pos[1] + j] == (int)turn)
                {
                    if (!continueChecking)
                        break;
                    else
                    {
                        matching = true;
                        if (isForChessConvert)
                            checkIsValidCount++;
                        if (!isForChessConvert)
                            availableMoves.Add(new Vector2(pos[0], pos[1]));
                        break;
                    }
                }
                else if (grid[pos[0] + i, pos[1] + j] == 0)
                    break;
                else
                {
                    tempTransChess.Push(new Vector3(pos[0] + i, pos[1] + j));
                    continueChecking = true;
                }
            }
        }
        while (tempTransChess.Count != 0 && isForChessConvert && isSimulating == false)
        {
            Vector3 v = tempTransChess.Pop();
            v += new Vector3(offsetChess, offsetChess, eplision);
            eplision += -0.001f;
            if (turn == ChessType.Black && matching)
            {
                grid[(int)v.x, (int)v.y] = (int)turn;
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
                GameObject trans = Instantiate(ChessPrefab[0], v, Quaternion.identity);
                trans.transform.SetParent(parent);
            }
            if (turn == ChessType.White && matching)
            {
                grid[(int)v.x, (int)v.y] = (int)turn;
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
                GameObject trans = Instantiate(ChessPrefab[1], v, Quaternion.identity);
                trans.transform.SetParent(parent);
            }
        }
        while (tempTransChess.Count != 0 && isForChessConvert && isSimulating == true)
        {
            Vector3 v = tempTransChess.Pop();
            if (turn == ChessType.Black && matching)
            {
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
            }
            if (turn == ChessType.White && matching)
            {
                simulateGrid[(int)v.x, (int)v.y] = (int)turn;
            }
        }
        return checkIsValidCount;
    }
}
//chess color
public enum ChessType
{
    None,
    Black,
    White
}