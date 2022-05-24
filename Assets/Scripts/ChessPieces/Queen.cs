using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> array = new List<Vector2Int>();

        //quân hậu là hợp thể của xe và tượng nên ^^
        
        //TH1: Quân xe chạy theo chiều y từ 7 => 0
        for (int i = currentY - 1; i >= 0; i--)
        {
            //kiểm tra từ vị trí y=7 => y=0 nếu trống thì xe dc chạy
            if (board[currentX, i] == null)
            {
                array.Add(new Vector2Int(currentX, i));
            }
            //kiểm tra từ vị trí y=7 => y=0 nếu ko trống thì xét là cùng phe hay khác phe
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                {
                    array.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }

        //TH2: Quân xe chạy theo chiều y từ 0 => 7
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            //kiểm tra từ vị trí y=0 => y=7 nếu trống thì xe dc chạy
            if (board[currentX, i] == null)
            {
                array.Add(new Vector2Int(currentX, i));
            }
            //kiểm tra từ vị trí y=0 => y=7 nếu ko trống thì xét là cùng phe hay khác phe
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                {
                    array.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }

        //TH3: Quân xe chạy theo chiều x từ 0 => 7
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            //kiểm tra từ vị trí y=0 => y=7 nếu trống thì xe dc chạy
            if (board[i, currentY] == null)
            {
                array.Add(new Vector2Int(i, currentY));
            }
            //kiểm tra từ vị trí y=0 => y=7 nếu ko trống thì xét là cùng phe hay khác phe
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                {
                    array.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }

        //TH4: Quân xe chạy theo chiều x từ 7 => 0
        for (int i = currentX - 1; i >= 0; i--)
        {
            //kiểm tra từ vị trí y=7 => y=0 nếu trống thì xe dc chạy
            if (board[i, currentY] == null)
            {
                array.Add(new Vector2Int(i, currentY));
            }
            //kiểm tra từ vị trí y=7 => y=0 nếu ko trống thì xét là cùng phe hay khác phe
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                {
                    array.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }


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
