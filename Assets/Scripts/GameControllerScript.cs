using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
/**
 * This script is used to initialize the game board and check for wins.
 **/
public class GameControllerScript : MonoBehaviour{
    //This dictionary contains colors and their opposing colors as key/value pairs. This way we can dynamically determine where the pieces need to move to satisfy the win condition.
    //For example, plugging in 'blue' returns the color opposite of blue ('white').
    Dictionary<string, string> OppositeColors = new Dictionary<string, string>();
    //This dictionary contains a color and its win positions as key/value pairs. 
    //For example, plugging in the 'blue' key returns the list of positions that must be filled by blue pieces for the blue player to win (the positions initially filled with white).
    Dictionary<string, List<Vector3Int>> WinPositions = new Dictionary<string, List<Vector3Int>>();
    //reference to the tilemap. this is used to set up the WinPositions dictionary.
    Tilemap tilemap;
    //reference to the script that controls piece movement. this is necessary to set the checkforwin() callback.
    GridScript gridScript;
    //references to the 3 possible board prefabs
    public GameObject boardPrefab2, boardPrefab4, boardPrefab6, hintPrefab;
    //hint marker
    private GameObject HintMarker;
    //reference to strongest move, used for hints and for AI moves
    Vector3Int strongestMove;
    bool hintMode = false; Vector3Int selectionChoice, placementChoice;
    //reference to the AI instance;
    AI ai;

    // Start is called before the first frame update
    void Start(){
        Globals.won = "";
        switch (Globals.NumberOfPlayers)
        {
            case 2:
                Instantiate(boardPrefab2);
                break;
            case 4:
                Instantiate(boardPrefab4);
                break;
            case 6:
                Instantiate(boardPrefab6);
                break;
        }
        //get references to the tilemap and grid script
        tilemap = GameObject.FindGameObjectWithTag("tilemap").GetComponent<Tilemap>();
        gridScript = GameObject.FindGameObjectWithTag("grid").GetComponent<GridScript>();
        //set up opposite color array
        OppositeColors.Add("blue", "white");
        OppositeColors.Add("white", "blue");
        OppositeColors.Add("yellow", "magenta");
        OppositeColors.Add("magenta", "yellow");
        OppositeColors.Add("red", "green");
        OppositeColors.Add("green", "red");
        //initialize empty list objects for win positions
        foreach (string key in OppositeColors.Keys){
            WinPositions.Add(key, new List<Vector3Int>());
        }
        //populate the winposition lists by looping through all the possible tile positions
        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++){
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++){
                //check if this possible tile position has a tile in it (playable piece or empty board space)
                if (tilemap.HasTile(new Vector3Int(x, y, 0))){
                    //find out what color the piece is, and then add its position to the winposition array for the opposite color
                    //eg, if the piece in this tile is white, add its position to the blue winpositions list
                    foreach (string key in OppositeColors.Keys){
                        if (tilemap.GetTile(new Vector3Int(x, y, 0)).name.Contains(OppositeColors[key])){
                            WinPositions[key].Add(new Vector3Int(x, y, 0));
                        }
                    }
                }
            }
        }
        //set up checkforwin callback
        gridScript.checkWin += checkForWin;
        gridScript.promptAI += AITurn;

        HintMarker = Instantiate(hintPrefab);
        HintMarker.SetActive(false);

        ai = new AI(tilemap, gridScript);
    }

    private void Update()
    {
        //if the user requests a hint
        if (!hintMode && Input.GetKey(KeyCode.H)){
            //make sure it's not an AI turn and the gridscript is in a valid mode
            if (!Array.Exists(Globals.AIColors, el => el.Equals(gridScript.currentTurnColor)) && 
                (gridScript.mode == GridScript.Mode.Placement || gridScript.mode == GridScript.Mode.Selection)){
                //enter hint mode, turn on the marker, and search for the best move
                hintMode = true;
                HintMarker.SetActive(true);
                showHint();
            }
        }
        //if hint mode is on, make sure the marker focuses on the proper tile depending on the current mode
        if (hintMode)
        {
            if (gridScript.mode == GridScript.Mode.Selection)
            {
                HintMarker.transform.position = tilemap.CellToWorld(selectionChoice);
            } else if (gridScript.mode == GridScript.Mode.Placement)
            {
                HintMarker.transform.position = tilemap.CellToWorld(placementChoice);
            }
        }
    }

    public void checkForWin(){
        hintMode = false; //turning this off here so that after the player makes a move, the hint will disappear
        HintMarker.SetActive(false);
        //loop through all the colors
        foreach (string color in WinPositions.Keys){
            if (Globals.won.Contains(color)) continue;
            //reset the score counter for this color
            int score = 0;
            //loop through the winpositions for this color
            foreach (Vector3Int position in WinPositions[color]){
                //find out what color piece is in the given winposition
                string tileName = tilemap.GetTile(position).name;
                //if the correct color piece is in the given position, increase the score for this color by 1
                if (tileName.Contains(color)){
                    score++;
                    //if this color player has 10 pieces that are all in win positions, they win
                    if (score == 10){
                        Debug.Log(color + " has met win condition");
                        //if this is the first win, append the color, else append a comma and then the color
                        if (Globals.won.Length == 0) Globals.won = color;
                        // When game is over, move to win / loss screen
                        SceneManager.LoadScene("EndingScene");
                    }
                }
            }
        }
    }

    public void showHint()
    {
        //make AI find a good move
        AITurn(true);
        strongestMove = ai.strongestMove;
        //save selection choice
        selectionChoice = gridScript.activeCell;
        placementChoice = strongestMove;
        gridScript.unselect();
    }

    public void AITurn(bool hintMode) {
        ai.TakeTurn(hintMode, OppositeColors, WinPositions);
    }

    

    

    

    
}
