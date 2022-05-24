using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,//chốt
    Rook = 2,//xe
    Knight = 3,//mã
    Bishop = 4,//tượng
    Queen = 5,
    King = 6
}
public class ChessPiece : MonoBehaviour
{
    public int team;//0 là ng chơi quân trắng, 1 là ng chơi quân đen
    public int currentX;//vị trí x hiện tại của con cờ
    public int currentY;//vị trí y hiện tại của cờ
    public ChessPieceType type;

    private Vector3 desiredPosition;//vị trí mong muốn
    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == 0) ? new Vector3(0, 180, 0) : Vector3.zero);
    }
    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    /// <summary>
    /// Quân cờ có thể di chuyển dc đến vị trí đó
    /// </summary>
    /// <param name="board"></param>
    /// <param name="tileCountX"></param>
    /// <param name="tileCountY"></param>
    /// <returns></returns>
    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> array = new List<Vector2Int>();

        array.Add(new Vector2Int(3, 3));
        array.Add(new Vector2Int(3, 4));
        array.Add(new Vector2Int(4, 3));
        array.Add(new Vector2Int(4, 4));

        return array;
    }

    public virtual SpecialMove GetSpecialMove(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMove)
    {
        return SpecialMove.None;//dc dùng để giới hạn mỗi quân cờ chỉ có thể sử dụng nc đi đặc biệt 1 lần
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}
