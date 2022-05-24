using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        //return base.GetAvailableMoves(ref board, tileCountX, tileCountY); ko bao h dc trả về cái return này nó sẽ trùng giá trị

        List<Vector2Int> array = new List<Vector2Int>();

        //kiểm tra nếu là chốt bên trắng thì đi lên theo hướng forward, ngược lại nếu bên đen thì đi lên theo hướng back
        int direction = (team == 0) ? 1 : -1;

        //kiểm tra xem vị trí bàn cờ phía trước 1 ô có đang trống để chốt có thể di chuyển 1 ô về phía trước
        if (board[currentX, currentY + direction] == null)
        {
            //nếu ô đó trống thì con chốt ng chơi đang cầm sẽ có thể đi vào 
            array.Add(new Vector2Int(currentX, currentY + direction));
        }

        if (board[currentX, currentY + direction] == null)
        {
            //kiểm tra xem vị trí bàn cờ phía trước 2 ô có đang trống để chốt có thể di chuyển 2 ô về phía trước (phe trắng)
            //kiểm tra vị trí con chốt đó ở đầu game nằm ở vị trí y =1 thì sẽ dc di chuyển 2 ô (khi và chỉ khi ở vị trí y=1, phe trắng)
            if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
            {
                array.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }

            //if (board[currentX, currentY + (direction * 2)] == null && team == 0 && currentY == 1)
            //{
            //}

            ////kiểm tra xem vị trí bàn cờ phía trước 2 ô có đang trống để chốt có thể di chuyển 2 ô về phía trước (phe đen)
            //kiểm tra vị trí con chốt đó ở đầu game nằm ở vị trí y =6 thì sẽ dc di chuyển 2 ô (khi và chỉ khi ở vị trí y=6, phe đen)
            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
            {
                array.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
            //if (board[currentX, currentY + (direction * 2)] == null && team == 1 && currentY == 6)
            //{
            //}
        }

        //kiểm tra nếu currentX != 7 thì chuẩn bị múc nó
        if (currentX != tileCountX - 1)
        {
            //nếu trên bàn cờ vị trí bên phải và phía trên 1 đơn vị có quân nào ko?
            //kiểm tra quân cờ đó có phải team mình ko. Nếu ko phải thì xúc nó
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                array.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
        if (currentX != 0)
        {
            //nếu trên bàn cờ vị trí bên phải và phía trên 1 đơn vị có quân nào ko?
            //kiểm tra quân cờ đó có phải team mình ko. Nếu ko phải thì xúc nó
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                array.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return array;

    }

    public override SpecialMove GetSpecialMove(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMove)
    {
        int direction = (team == 0) ? 1 : -1;
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
            return SpecialMove.Promotion;

        //EnPassant
        if (moveList.Count > 0)
        {
            //tạo biến lastMove và gán giá trị bằng giá trị cuối cùng trg list moveList
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)//kiểm tra nc đi cuối cùng có phải là nc đi của 1 con chốt
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)//lấy vị trí y của previousPosition - vị trí y của bước di chuyển ng chơi ms đi có = 2 khum.
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team)//kiểm tra 1 quân cờ tại vị trí lastMove[1] khác team
                    {
                        if (lastMove[1].y == currentY)//nếu quân chốt của mình và chốt đối thủ cùng vị trí y
                        {
                            if (lastMove[1].x == currentX - 1)//kiểm tra vị trí x của quân cờ đối thủ ở vị trí bên trái quân chốt của tui
                            {
                                availableMove.Add(new Vector2Int(currentX - 1, currentY + direction));//quân chốt của tôi sẽ di chuyển tới vị trí [x-1,y+1](nếu bên trắng), [x-1,y-1](nếu bên đen)
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1)//kiểm tra vị trí x của quân cờ đối thủ ở vị trí bên phải quân chốt của tui
                            {
                                availableMove.Add(new Vector2Int(currentX + 1, currentY + direction));//quân chốt của tôi sẽ di chuyển tới vị trí [x+1,y+1](nếu bên trắng), [x+1,y-1](nếu bên đen)
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;//lun trả về none nếu con chốt đó ko vào vị trí nc đi đặc biệt
    }
}
