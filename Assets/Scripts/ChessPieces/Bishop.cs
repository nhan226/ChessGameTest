using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> array = new List<Vector2Int>();

        //Th1: quân tượng di chuyển dg chéo hướng trên bên phải
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
            {
                array.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    array.Add(new Vector2Int(x, y));
                }
                break;
            }
        }

        //Th2: quân tượng di chuyển dg chéo hướng trên bên trái
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
            {
                array.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    array.Add(new Vector2Int(x, y));
                }
                break;
            }
        }

        //Th3: quân tượng di chuyển dg chéo hướng dưới bên phải
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
            {
                array.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    array.Add(new Vector2Int(x, y));
                }
                break;
            }
        }

        //Th4: quân tượng di chuyển dg chéo hướng dưới bên trái
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
            {
                array.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    array.Add(new Vector2Int(x, y));
                }
                break;
            }
        }

        return array;
    }
}
