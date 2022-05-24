using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> array = new List<Vector2Int>();

        //int direction = (team == 0) ? 1 : -1;
        //if ((board[currentX + direction, currentY * direction * 2] != null || board[currentX + direction, currentY * direction * 2] == null) && board[currentX,currentY].team!=null)
        //{
        //    array.Add(new Vector2Int(currentX + direction, currentY * direction * 2));
        //}

        //Th1: quân mã di chuyển hướng trên bên phải
        int x = currentX + 1;
        int y = currentY + 2;

        if (x < tileCountX && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 2;
        y = currentY + 1;

        if (x < tileCountX && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        //Th2: quân mã di chuyển hướng trên bên trái
        x = currentX - 1;
        y = currentY + 2;

        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 2;
        y = currentY + 1;

        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        //Th3: quân mã di chuyển hướng xuống bên phải
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        //Th4: quân mã di chuyển hướng xuống bên trái
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                array.Add(new Vector2Int(x, y));
            }
        }

        return array;
    }
}
