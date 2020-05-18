using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ChessType chessColor = ChessType.Black;
    void Update()
    {
        if (chessColor == Board.Instance.turn)
            PlayChess();
    }
    public virtual void PlayChess()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //play chess from mouse inputs
        if (Input.GetMouseButtonDown(0) && pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8)
        {
            Board.isSimulating = false;

            Board.Instance.CheckAllowedMoves();
            Board.Instance.StartPlay(new int[2]{ (int)pos.x,(int)pos.y});
        }
    }
}
