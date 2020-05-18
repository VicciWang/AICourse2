using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI : MonoBehaviour
{
    public Text whosTurn;
    public Text end;
    void Update()
    {
        //remind player the current turn
        if (Board.Instance.turn == ChessType.Black)
            whosTurn.text = "Black Turn";
        if (Board.Instance.turn == ChessType.White)
            whosTurn.text = "White Turn";

        //game ending text
        if (Board.Instance.isEnding == true)
        {
            if(Board.Instance.BlackCount>Board.Instance.WhiteCount)
                end.text = "Black Win";
            else if (Board.Instance.BlackCount < Board.Instance.WhiteCount)
                end.text = "White Win";
            else
                end.text = "Even";
        }

        //quit app
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();            
    }
}
