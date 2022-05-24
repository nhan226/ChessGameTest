using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> array = new List<Vector2Int>();

        //vua di chuyển hướng phải
        if (currentX + 1 < tileCountX)
        {
            //Th1: vua di chuyển qua bên phải
            if (board[currentX + 1, currentY] == null)
            {
                array.Add(new Vector2Int(currentX + 1, currentY));
            }
            else if (board[currentX + 1, currentY].team != team)
            {
                array.Add(new Vector2Int(currentX + 1, currentY));
            }

            //Th2: vua di chuyển qua phía trên bên phải
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY + 1] == null)
                {
                    array.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
                else if (board[currentX + 1, currentY + 1].team != team)
                {
                    array.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
            }

            //Th3: vua di chuyển qua phía dưới bên phải
            if (currentY - 1 >= 0)
            {
                if (board[currentX + 1, currentY - 1] == null)
                {
                    array.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
                else if (board[currentX + 1, currentY - 1].team != team)
                {
                    array.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
            }
        }

        //vua di chuyển hướng trái
        if (currentX - 1 >= 0)
        {
            //Th4: vua di chuyển qua bên trái
            if (board[currentX - 1, currentY] == null)
            {
                array.Add(new Vector2Int(currentX - 1, currentY));
            }
            else if (board[currentX - 1, currentY].team != team)
            {
                array.Add(new Vector2Int(currentX - 1, currentY));
            }

            //Th5: vua di chuyển qua phía trên bên trái
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX - 1, currentY + 1] == null)
                {
                    array.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
                else if (board[currentX - 1, currentY + 1].team != team)
                {
                    array.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
            }

            //Th6: vua di chuyển qua phía dưới bên trái
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null)
                {
                    array.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
                else if (board[currentX - 1, currentY - 1].team != team)
                {
                    array.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
            }
        }

        //vua di chuyển hướng lên
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
            {
                array.Add(new Vector2Int(currentX, currentY + 1));
            }
        }

        //vua di chuyển hướng xuống
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
            {
                array.Add(new Vector2Int(currentX, currentY - 1));
            }
        }

        return array;
    }

    public override SpecialMove GetSpecialMove(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMove)
    {
        SpecialMove r = SpecialMove.None;

        //List.Find: tìm kiếm 1 phần tử đầu tiên phù hợp vs giá trị và dc trả về
        Vector2Int[] kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));//tìm vị trí của vua: vua trắng (x=4,y=0), vua đen (x=4,y=7)
        Vector2Int[] leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));//tìm vị trí của xe: xe trái trắng (x=0,y=0), xe trái đen (x=0,y=7)
        Vector2Int[] rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));//tìm vị trí của xe: xe phải trắng (x=7,y=0), xe phải đen (x=7,y=7)

        //kiểm tra xem vua có đang ở vị trí ban đầu và chưa lần nào di chuyển từ lúc bắt đầu trận
        if (kingMove == null && currentX == 4)
        {
            //White team
            if (team == 0)
            {
                //kiểm tra xem xe trái bên trắng chưa lần nào di chuyển từ lúc bắt đầu trận
                if (leftRook == null)
                {
                    //kiểm tra vị trí [x,y]==[0,0] là quân xe 
                    if (board[0, 0].type == ChessPieceType.Rook)
                    {
                        //và quân xe này có thuộc team trắng ko
                        if (board[0, 0].team == 0)
                        {
                            //kiểm tra ô [x,y] == [3,0] có trống ko?
                            if (board[3, 0] == null)
                            {
                                //kiểm tra ô [x,y] == [2,0] có trống ko?
                                if (board[2, 0] == null)
                                {
                                    //kiểm tra ô [x,y] == [1,0] có trống ko?
                                    if (board[1, 0] == null)
                                    {
                                        availableMove.Add(new Vector2Int(2, 0));//nếu các ô trống hết thì vua sẽ dc di chuyển wa vị trí [x,y]=[2,0]
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                //kiểm tra xem xe phải bên trắng chưa lần nào di chuyển từ lúc bắt đầu trận
                if (rightRook == null)
                {
                    //kiểm tra vị trí [x,y]==[7,0] là quân xe 
                    if (board[7, 0].type == ChessPieceType.Rook)
                    {
                        //và quân xe này có thuộc team trắng ko
                        if (board[7, 0].team == 0)
                        {
                            //kiểm tra ô [x,y] == [5,0] có trống ko?
                            if (board[5, 0] == null)
                            {
                                //kiểm tra ô [x,y] == [6,0] có trống ko?
                                if (board[6, 0] == null)
                                {
                                    availableMove.Add(new Vector2Int(6, 0));//nếu các ô trống hết thì vua sẽ dc di chuyển wa vị trí [x,y]=[6,0]
                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
            else //black team
            {
                //kiểm tra xem xe trái bên đen chưa lần nào di chuyển từ lúc bắt đầu trận
                if (leftRook == null)
                {
                    //kiểm tra vị trí [x,y]==[0,7] là quân xe 
                    if (board[0, 7].type == ChessPieceType.Rook)
                    {
                        //và quân xe này có thuộc team đen ko
                        if (board[0, 7].team == 1)
                        {
                            //kiểm tra ô [x,y] == [3,7] có trống ko?
                            if (board[3, 7] == null)
                            {
                                //kiểm tra ô [x,y] == [2,7] có trống ko?
                                if (board[2, 7] == null)
                                {
                                    //kiểm tra ô [x,y] == [1,7] có trống ko?
                                    if (board[1, 7] == null)
                                    {
                                        availableMove.Add(new Vector2Int(2, 7));//nếu các ô trống hết thì vua sẽ dc di chuyển wa vị trí [x,y]=[2,7]
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                //kiểm tra xem xe phải bên đen chưa lần nào di chuyển từ lúc bắt đầu trận
                if (rightRook == null)
                {
                    //kiểm tra vị trí [x,y]==[7,7] là quân xe 
                    if (board[7, 7].type == ChessPieceType.Rook)
                    {
                        //và quân xe này có thuộc team đen ko
                        if (board[7, 7].team == 1)
                        {
                            //kiểm tra ô [x,y] == [5,7] có trống ko?
                            if (board[5, 7] == null)
                            {
                                //kiểm tra ô [x,y] == [6,7] có trống ko?
                                if (board[6, 7] == null)
                                {
                                    availableMove.Add(new Vector2Int(6, 7));//nếu các ô trống hết thì vua sẽ dc di chuyển wa vị trí [x,y]=[6,7]
                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }

        }

        return r;
    }
}
