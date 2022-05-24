using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}

public class ChessBoard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float yOffSet = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = .3f;
    [SerializeField] private float deathSpacing = .3f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen;

    [Header("Prefab & Material")]//tạo các con cờ với từng màu theo team
    [SerializeField] private GameObject[] prefabs;//6 loại cờ xe, mã, chốt,...
    [SerializeField] private Material[] teamMaterials;// 2 loại màu cho 2 team trắng và đen

    //LOGIC
    private ChessPiece[,] chessPieces;//vị trí [x,y] của từng quân cờ
    private ChessPiece currentlyDragging;//quân cờ đang cầm
    private List<Vector2Int> availableMove = new List<Vector2Int>();//những ô mà quân cờ có thể đi dc và giá trị này sẽ dc lưu lại đâu đó trong class
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;//chiều rộng bàn cờ vua dc gán cứng 8 ô
    private const int TILE_COUNT_Y = 8;//chiều dài bàn cờ vua dc gán cứng 8 ô
    private GameObject[,] tiles;//chứa vị trí của tất cả các ô dc tạo spawn ra theo vị trí tileCountX và tileCountY
    private Camera currentCamera;
    private Vector2Int currentHover;//Ô hiện tại ng chơi đang trỏ con chuột vào
    private Vector3 bounds;
    private bool isWhiteTurn;

    private SpecialMove specialMove;//biến để lấy tên các nc di chuyển đặc biệt
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private void Awake()
    {
        isWhiteTurn = true;

        GenerateTile(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "HighLight")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);//lấy các chỉ mục của ô mà tôi đã đánh

            //nếu chưa có ô nào dc chọn thì currentHover sẽ chọn ô đó để highlight
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //nếu đã có ô dc chọn, sau đó click con trỏ chọn ô khác thì currentHover sẽ highlight ô sau đó, còn ô trước đó sẽ tắt highlight
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMove, currentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //khi ng chơi click chuột trái vào quân cờ
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //kiểm tra xem đến lượt ng chơi nào
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        //quân cờ ng chơi đang cầm sẽ có vị trí hitPosition.x và hitPosition.y
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        //lấy danh sách các ô mà quân cờ có thể đến và highlight các ô đó
                        availableMove = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        //Lấy dnah sách các bước di chuyển đặc biệt
                        specialMove = currentlyDragging.GetSpecialMove(ref chessPieces, ref moveList, ref availableMove);

                        PreventCheck();//Kiểm tra ngăn chặn nguy hiểm cho quân vua
                        HighLightTiles();
                    }
                }
            }
            //khi ng chơi giải phóng quân cờ
            if (Input.GetMouseButtonUp(0) && currentlyDragging != null)
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }
                //sau khi ng chơi hoàn thành bước đi cho quân cờ thì tắt highlight và trên tay ng chơi currentlyDragging = null
                currentlyDragging = null;
                RemoveHighLightTiles();
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMove, currentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighLightTiles();
            }
        }

        if (currentlyDragging)//khi ng chơi đang cầm quân cờ
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffSet);
            float distance = 0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);//ray.getpoint: Trả về một điểm theo đơn vị khoảng cách dọc theo tia.
            }
        }
    }

    //Generate the board
    private void GenerateTile(float tileSize, int tileCountX, int tileCountY)
    {
        yOffSet += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0} , Y:{1}", x, y));
        tileObject.transform.parent = transform;

        //tạo lưới cho tileObject
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;//Hiển thị các lưới được chèn bởi MeshFilter hoặc TextMesh.

        Vector3[] vertices = new Vector3[4];//tạo 4 đỉnh góc bàn cờ
        vertices[0] = new Vector3(x * tileSize, yOffSet, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffSet, (y + 1) * tileSize) - bounds;//y chỉ chạy tới 7 nên phải công thêm 1 để set vị trí ô này ở dỉnh trên cùng bên trái
        vertices[2] = new Vector3((x + 1) * tileSize, yOffSet, y * tileSize) - bounds;//x chỉ chạy tới 7 nên phải cộng thêm 1 để set vị trí ô này ở đỉnh dưới cùng bên phải
        vertices[3] = new Vector3((x + 1) * tileSize, yOffSet, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];//vị trí [x,y] khi quân cờ dc spawn ra

        int whiteTeam = 0;
        int blackTeam = 1;

        //White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        //Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)//spawn theo từng loại cờ với màu tương ứng theo team
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    //Position All Pieces
    private void PositionAllPieces()//set vị trí ban đầu của tất cả các quân cờ
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force = false)//vị trí ban đầu của từng quân cờ
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffSet, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //HighLight Tiles
    private void HighLightTiles()
    {
        for (int i = 0; i < availableMove.Count; i++)
        {
            tiles[availableMove[i].x, availableMove[i].y].layer = LayerMask.NameToLayer("HighLight");
        }
    }
    private void RemoveHighLightTiles()
    {
        for (int i = 0; i < availableMove.Count; i++)
        {
            tiles[availableMove[i].x, availableMove[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMove.Clear();
    }

    //CheckMate && UIWinning: kiểm tra điều kiện win
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnResetBtn()
    {
        //UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        //fields reset
        currentlyDragging = null;
        availableMove.Clear();
        moveList.Clear();

        //clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }

                chessPieces[x, y] = null;
            }
        }
        for (int i = 0; i < deadWhites.Count; i++)
        {
            Destroy(deadWhites[i].gameObject);
        }
        for (int i = 0; i < deadBlacks.Count; i++)
        {
            Destroy(deadBlacks[i].gameObject);
        }
        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnExitBtn()
    {
        Application.Quit();
    }

    //Special Move
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            Vector2Int[] newMove = moveList[moveList.Count - 1];//nc đi mà tôi thực hiện
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];//vị trí quân chốt của tui
            Vector2Int[] targetPawnPosition = moveList[moveList.Count - 2];//nc đi mà đối thủ thực hiện trước đó
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];//vị trí quân chốt của đối thủ

            if (myPawn.currentX == enemyPawn.currentX)//chốt của tôi và chốt của đối thủ có cùng vị trí x (khi chốt của tôi đã đi dg chéo)
            {
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)//xét xem quân chốt đang ăn từ dưới lên (y=y-1) hay trên xuống (y=y+1)
                {
                    if (enemyPawn.team == 0)//nếu quân chốt đối thủ là trằng thì tức là quân đen đang ăn trên xuống
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffSet, -1.25f * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else//nếu quân chốt đối thủ là đen thì tức là quân trắng đang ăn dưới lên
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(-1f * tileSize, yOffSet, 8f * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }

                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;//câu lệnh này đảm bảo chúng ta sẽ xóa tham chiếu của quân cờ tại vị trí cũ
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                Debug.Log("run");
                //nếu quân chốt trắng và có vị trí y=7 thì sẽ là cái gì đó
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);//tạo 1 con hậu ms quân trắng
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);//xóa con chốt cũ
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;//gán vị trí của con chốt cũ đó cho con hậu ms tạo. Vị trí đó sẽ là vị trí ban đầu của con hậu ms
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                //nếu quân chốt đen và có vị trí y=0 thì sẽ là cái gì đó
                if (targetPawn.team == 0 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);//tạo 1 con hậu ms quân đen
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);//xóa con chốt cũ
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;//gán vị trí của con chốt cũ đó cho con hậu ms tạo. Vị trí đó sẽ là vị trí ban đầu của con hậu ms
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //kiểm tra nếu vua đã di chuyển đến ô x=2. Hoán đổi vị trí vua vs xe bên trái
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0)//hoán đổi vị trí vua vs xe trắng bên trái
                {
                    ChessPiece rook = chessPieces[0, 0];//vị trí ban đầu của xe trắng trái lúc chưa di chuyển
                    chessPieces[3, 0] = rook;//thay đổi vị trí xe trắng trái về ô x=3, y=0
                    PositionSinglePiece(3, 0);//vị trí ban đầu của quân xe trắng thay đổi
                    chessPieces[0, 0] = null;//xóa tham chiếu đến vị trí ban đầu cũ của xe trắng
                }
                else if (lastMove[1].y == 7)//hoán đổi vị trí vs xe đen bên trái
                {
                    ChessPiece rook = chessPieces[0, 7];//vị trí ban đầu của xe đen trái lúc chưa di chuyển
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }

            //kiểm tra nếu vua đã di chuyển đến ô có x=6. Hoán đổi vị trí vua vs xe bên phải
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0)//hoán đổi vị trí vs xe trắng bên phải
                {
                    ChessPiece rook = chessPieces[7, 0];//vị trí ban đầu của xe trắng trái lúc chưa di chuyển
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)//hoán đổi vị trí vs xe đen bên phải
                {
                    ChessPiece rook = chessPieces[7, 7];//vị trí ban đầu của xe đen trái lúc chưa di chuyển
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck()
    {
        //lấy thông tin vị trí quân vua đang ở đâu trên bàn
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyDragging.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }

        //gửi giá trị của availableMove của quân vua và xóa các nc đi có thể đưa quân vua vào tầm kiểm soát
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMove, targetKing);
    }
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        //lưu các giá trị hiện tại, để đặt lại sau khi gọi hàm
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();//danh sách chứa các nc đi có hại cho vua nên cần phải xóa

        //lặp lại các bước di chuyển trong anh sách availableMove
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;//biến mô phỏng lại các nc đi của 1 quân cờ, vị trí chính xác quân cờ đi dc và biến này dc khai báo trg phạm vi vòng for
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);//khai báo vị trí của vua
            //mô phỏng bước di chuyển của vua
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);//vì chúng ta đã thay đổi vị trí của quân vua nên kingPositionThisSim ko còn đúng nữa vì thế gán lại vị trí ms cho vua

            //copy [,] lại bàn cờ nhưng ko phải là reference
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPiece = new List<ChessPiece>();//khai báo danh sách các quân cờ tấn công mô phỏng, xem xét đội khác có thể giết vua tôi ko
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    simulation[x, y] = chessPieces[x, y];//gán giá trị vị trí của từng quân vào biến simulation
                    if (simulation[x, y].team != cp.team)
                        simAttackingPiece.Add(simulation[x, y]);//thêm tất cả các mảnh đối phương vào bên trong mảng
                }
            }

            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            //did one of piece got taken down during our simulation
            ChessPiece deadPiece = simAttackingPiece.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
            {
                simAttackingPiece.Remove(deadPiece);
            }

            //nhận biết tất cả quân tất công thuộc vị trí nào trg mô phỏng
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPiece.Count; a++)
            {
                List<Vector2Int> pieceMove = simAttackingPiece[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMove.Count; b++)
                {
                    simMoves.Add(pieceMove[b]);
                }
            }
            //vua sẽ bị bế đi nên chúng ta remove nc đi đó
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            //khôi phục lại các dữ liệu nc đi
            cp.currentX = actualX;
            cp.currentY = actualY;
        }

        //loại bỏ khỏi danh sách availableMove hiện tại
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }

    //Operations
    /// <summary>
    /// Kiểm tra xem nước đi đó có hợp lệ ko??
    /// </summary>
    /// <param name="moves"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        //khi ko có nc đi nào hợp lệ cho quân cờ đang cầm trả về false
        if (!ContainsValidMove(ref availableMove, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //có quân cờ nào trên vị trí quân cờ khác đang tiến lại
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];//ocp: other chess piece

            if (cp.team == ocp.team)//tránh trường hợp ng chơi đặt 2 quân cờ cùng màu vào 1 vị trí
            {
                return false;
            }

            //nếu quân cờ đó là quân địch thì chết và dc đưa ra ngoài bàn cờ và thu nhỏ lại
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(1);

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffSet, -1.25f * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(0);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(-1f * tileSize, yOffSet, 8f * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }

        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;//câu lệnh này đảm bảo chúng ta sẽ xóa tham chiếu của quân cờ tại vị trí cũ tránh tình trạng khi thực hiện nc đi xong, sau đó ng chơi vô tình bấm lại vào ô cũ của quân cờ thì vẫn có thể di chuyển quân cờ mặc dù ô đó đang rỗng.

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;

        //moveList sẽ chỉ tích lũy các nc đi mà ng chơi thực hiện, và cũng có thể là nc đi trg tương lai của ng chơi
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });//previousPosition: vị trí trước đó của quân cờ. new Vector2Int(x,y): vị trí sau đó khi mà ng chơi đã di chuyển quân cờ đó.

        ProcessSpecialMove();//hàm để kiểm tra xem quân cờ có làm bước đi đặc biệt chưa. Nếu rồi thì chúng ta cần xử lý 1 cái gì đó

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one;//(-1,-1)
    }
}
