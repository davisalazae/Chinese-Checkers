using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * This script checks for user input and modifies the tilemap based on 
 **/
public class GridScript : MonoBehaviour{
    //this enumeration represents the interaction state. 
    //in selection mode, the user can click a piece to select it. In placement mode, the user can place the selected piece.
    [HideInInspector] public enum Mode{Selection, Placement, Animating}
    //mode variable, defaults to selection mode
    [HideInInspector] public Mode mode = Mode.Selection;
    //Reference to the tilemap object 
    Tilemap map;
    //stores the position of the currently selected piece the user wants to move
    [HideInInspector] public Vector3Int activeCell;
    //stores the distance from the center of one cell to another, used for easily finding adjacent cells by just adding this distance to another cell's position
    [HideInInspector] public float cellSizeInWorldUnits;
    //the normalized vectors for the six possible travel directions, used when determining valid piece movement
    Vector2[] directions = { new Vector2(1, 0), new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized, new Vector2(-1, 0), new Vector2(-1, 1).normalized, new Vector2(1, 1).normalized };
    //this array holds the tiles that the selected piece can legally be moved to
    [HideInInspector] public List<Vector3Int> validTiles = new List<Vector3Int>();
    //callback to the checkwin function in GameControllerScript.cs. This should be called when a piece is moved. 
    [HideInInspector] public Action checkWin;
    [HideInInspector] public Action<bool> promptAI;
    //reference to the TestModule prefab so the tests can be executed
    public GameObject AutomatedTestingModulePrefab;
    public bool TestsEnabled = true;
    //this string array represents the colors, used to determine whose turn it is
    private static string[] Colors6Players = { "blue", "yellow", "green", "white", "magenta", "red" };
    private static string[] Colors4Players = { "blue", "green", "white", "red" };
    private static string[] Colors2Players = { "blue", "white" };
    [HideInInspector] public static string[] Colors = Colors6Players;
    //this string's value corresponds to whose turn it is
    [HideInInspector] public string currentTurnColor = Colors[0];
    //references to tiles used to display move suggestions
    public Tile suggestedPlacement;
    //references to tiles used to display empty cells
    public Tile emptyCell;
    //reference to the tile used for debugging
    public Tile errorTile;
    //reference to the sprite used for animating piece movement
    public GameObject animationPiece;
    Vector3 animationBegin, animationEnd;
    TileBase sourceTile;
    Vector3Int targetPos;
    public Sprite[] sprites = new Sprite[6];

    // Start is called before the first frame update
    void Start(){
        switch (Globals.NumberOfPlayers)
        {
            case 2:
                Colors = Colors2Players;
                break;
            case 4:
                Colors = Colors4Players;
                break;
            case 6:
                Colors = Colors6Players;
                break;
        }
        //get reference to tilemap
        map = GetComponentInChildren<Tilemap>();
        //calculate cell size for things that use it to traverse the map using world coords
        cellSizeInWorldUnits = Vector3.Distance(map.CellToWorld(new Vector3Int(0, 0, 0)), map.CellToWorld(new Vector3Int(0, 1, 0)));
        //start in selection mode
        mode = 0;
        //run tests if tests are on 
        if (TestsEnabled) {
            GameObject automatedTests = GameObject.Instantiate(AutomatedTestingModulePrefab);
        }
        animationPiece = GameObject.FindGameObjectWithTag("AnimationPiece");
        animationPiece.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
        //if the current color player has finished, just move to the next turn
        if (Globals.won.Contains(currentTurnColor)){
            advanceTurn();
        }
        //if a piece is in the middle of animating, continue the animation and then return
        else if (mode == Mode.Animating) {
            animate();
            return;
        }
        //if the AI is making a turn, ignore user input
        else if (!TestsEnabled && mode == Mode.Selection && Array.Exists(Globals.AIColors, el => el.Equals(currentTurnColor))){
            if (promptAI != null) promptAI(false);
            return;
        }
        
        //if user clicks the left mouse button, call the appropriate method based on the current mode
        else if (Input.GetMouseButtonDown(0)){
            switch (mode){
                case (Mode.Selection):
                    attemptPieceSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    break;
                case (Mode.Placement):
                    attemptPiecePlacement(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    break;
            }
        }
        //if user clicks the right mouse button, cancel piece placement by going back into piece selection mode
        else if (Input.GetMouseButtonDown(1)) {
            if (mode == Mode.Placement){
                foreach (Vector3Int validTile in validTiles){
                    map.SetTile(validTile, emptyCell);
                }
                mode = Mode.Selection;
            }
        }
    }

    /**
     * This method checks to see if the user clicked a moveable piece.
     * If the piece is moveable, it calculates the valid moves for that piece (including jumps)
     * and switches to Placement mode
     */
    public void attemptPieceSelection(Vector3 worldPosition){
        //convert the mouse position into a grid position
        Vector3Int cellpos = map.WorldToCell(worldPosition);
        //check if the user clicked a moveable piece
        if (map.HasTile(cellpos) && map.GetTile(cellpos).name.Contains(currentTurnColor)){
            //else reset the valid tiles array so we can populate it
            validTiles.Clear();
            //loop through each of the 6 directions to find out which ones are valid to move to
            for (int i = 0; i < 6; i++){
                //find the adjacent tile in the given direction by adding an offset to the clicked cell
                Vector2 cellWorldPosition = map.CellToWorld(cellpos); //convert grid pos to world pos
                Vector2 offset = directions[i] * cellSizeInWorldUnits; //calculate offset vector
                Vector2 adjacentTileWorldPosition = cellWorldPosition + offset; //add offset to clicked cell position
                TileBase adjacentTile = map.GetTile(map.WorldToCell(adjacentTileWorldPosition)); //convert world position to grid position and get the tile
                //if this adjacent tile is a valid move, add it to the tiles array and move on to the next direction
                if (adjacentTile != null && adjacentTile.name.Equals("circle_black_tile"))
                {
                    validTiles.Add(map.WorldToCell(adjacentTileWorldPosition));
                    map.SetTile(map.WorldToCell(adjacentTileWorldPosition), suggestedPlacement);
                    continue;
                }
                //if the adjacent tile is not valid, check if a jump is possible
                adjacentTileWorldPosition = adjacentTileWorldPosition + offset; //offset in the same direction as earlier, once more
                adjacentTile = map.GetTile(map.WorldToCell(adjacentTileWorldPosition)); //convert world positon to grid position and get the tile
                //if the tile is valid, add it to the valid tiles array
                if (adjacentTile != null && adjacentTile.name.Equals("circle_black_tile"))
                {
                    validTiles.Add(map.WorldToCell(adjacentTileWorldPosition));
                    map.SetTile(map.WorldToCell(adjacentTileWorldPosition), suggestedPlacement);
                    //TODO check for sequential jumps
                }
            }
            //if there's valid moves, switch to placement mode, mark the clicked cell as active
            if (validTiles.Count > 0)
            {
                mode = Mode.Placement;
                activeCell = cellpos;
            }
        } 
    }

    /**
     * This method checks to see if the cell the user clicks on is a valid move for the selected piece.
     * If so, we swap the tiles and then proceeed to animation mode. 
     * */
    public void attemptPiecePlacement(Vector3 worldPosition){
        //convert the mouse position into a grid position, and then check and see if it's on the gameboard
        Vector3Int cellpos = map.WorldToCell(worldPosition);
        bool pieceMoved = false;
        //check and see if the clicked position is any of the valid positions
        foreach (Vector3Int validTile in validTiles){
            //if the user clicked a valid placement tile
            if (cellpos == validTile){
                //remove the indicators from the validtiles
                foreach (Vector3Int validTile2 in validTiles){
                    map.SetTile(validTile2, emptyCell);
                }
                //get source and target tile
                sourceTile = map.GetTile(activeCell);
                TileBase targetTile = map.GetTile(cellpos);
                targetPos = cellpos;
                //setup animation
                mode = Mode.Animating;
                animationBegin = map.CellToWorld(activeCell);
                animationEnd = map.CellToWorld(cellpos);
                //set the active cell to black by getting the black image from the target cell
                map.SetTile(activeCell, targetTile);
                //this was for debugging but we can probably move the checkwin call back up here again
                pieceMoved = true;
            }
        }
        //if we moved a piece, notify the game controller to check for a win
        if (pieceMoved){
            //callback to GameControllerScript
            if (checkWin != null)
                checkWin();
        }
    }

    public void advanceTurn() {
        int newTurnIndex = Array.IndexOf(Colors, currentTurnColor) + 1;
        //reset index to 0 (blue player) if it goes out of the array bounds
        if (newTurnIndex > Colors.Length - 1) newTurnIndex = 0;
        currentTurnColor = Colors[newTurnIndex];
    }

    bool animationBegun = false;
    //float animationDuration = .01f; //this is the divisor for the lerp duration, so don't set it to zero
    float animationDuration = .7f; //this is the divisor for the lerp duration, so don't set it to zero
    float animationElapsed = 0;
    Vector3 startingScale = new Vector3(1, 1, 1); Vector3 endingScale = new Vector3(2, 2, 2);
    public void animate() {
        //accumulate deltatime
        animationElapsed += Time.deltaTime;
        //if this is the first frame of the animation, initialize the animation vars
        if (!animationBegun) {
            //set sprite
            for (int i = 0; i < 6; i++) {
                if (sourceTile.name.Contains(Colors6Players[i])) {
                    animationPiece.GetComponent<SpriteRenderer>().sprite = sprites[i];
                }
            }
            animationElapsed = 0;
            animationPiece.transform.position = animationBegin;
            animationBegun = true;
            animationPiece.SetActive(true);
        }
        //lerp
        animationPiece.transform.position = Vector3.Lerp(animationBegin, animationEnd, animationElapsed/animationDuration);
        if (animationElapsed < animationDuration / 2) {
            animationPiece.transform.localScale = Vector3.Lerp(startingScale, endingScale, animationElapsed / (animationDuration /2));
        } else {
            animationPiece.transform.localScale = Vector3.Lerp(endingScale, startingScale, (animationElapsed-(animationDuration/2)) / (animationDuration / 2));
        }
        Vector3 newPosition = animationPiece.transform.position;
        newPosition.z = -1;
        animationPiece.transform.position = newPosition;
        //if we're done animating, end the animation and go to the next turn
        if (animationElapsed > animationDuration) {
            //turn off the animation piece
            animationPiece.SetActive(false);
            //put tile sprite back down
            map.SetTile(targetPos, sourceTile);
            //reset the animation flag
            animationBegun = false;
            //advance to next player's turn
            mode = Mode.Selection;
            advanceTurn();
        }
    }

    //simple method that removes the indicators from the valid tiles, and sets the mode back to selection mode
    public void unselect(){
        foreach (Vector3Int validTile in validTiles){
            map.SetTile(validTile, emptyCell);
        }
        mode = Mode.Selection;
    }

    //this is for debug only. just prints debug info and instructions to the screen
    private void OnGUI() {
        string modestring = (mode == Mode.Selection) ? "Selection" : "Placement";
        GUI.Box(new Rect(0, 0, 200, 25), "Debug: " + modestring + " mode.");
        GUI.Box(new Rect(0, 50, 200, 25), "Debug: " + currentTurnColor + "'s turn.");
        GUI.Box(new Rect(0, 200, 300, 50), "Left click to select/move piece.\nRight click to drop piece without moving.\nPress H for hint.");
    }
}
