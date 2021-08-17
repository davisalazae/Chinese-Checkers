using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tests : MonoBehaviour
{

    
    Tilemap map;
    GridScript gridScript;
    bool testingAnimatedActions = false; //this is set to true when the animations are playing, and we're waiting for them to play out
    float elapsed = 0; //this is used to time our waits so we can let the animations play out
    int testIndex = -1; //this is set to 0 or 1 depending on which test we're doing
    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.FindGameObjectWithTag("tilemap").GetComponent<Tilemap>();
        gridScript = GameObject.FindGameObjectWithTag("grid").GetComponent<GridScript>();
        executeTests();
    }

    private void Update() {
        
        //if we're testing an animated action
        if (testingAnimatedActions) {
            //track the elapsed time
            elapsed += Time.deltaTime;
            //if two seconds have passed
            if (elapsed > 2) {
                //if we're doing the basic movement test, check the result, then initiate the jump test
                if (testIndex == 0) {
                    basicMovementTestValidation();
                    InitiateJumpMovementTest();
                }
                //else if we're doing the jump movement test, just check the result
                else if (testIndex == 1) {
                    jumpTestValidation();
                }
            }
        }
    }

    public void executeTests() {
        TestSelection();
        TestTurnOrder();
        InitiateBasicMovementTest();
    }


    /**
     * Tests for the selection and placement are here.
     */

    private void TestSelection() {
        //try selecting all colors except blue
        //loop through the entire grid, and try selecting any non-blue colors. if the state changes, the test fails
        bool testFailed = false;
        for (int x = map.cellBounds.min.x; x < map.cellBounds.max.x; x++) {
            for (int y = map.cellBounds.min.y; y < map.cellBounds.max.y; y++) {
                Vector3Int tileCoords = new Vector3Int(x, y, 0);
                if (map.HasTile(tileCoords) && !map.GetTile(tileCoords).name.Contains("blue")) {
                    gridScript.attemptPieceSelection(map.CellToWorld(tileCoords));
                    if (gridScript.mode == GridScript.Mode.Placement) {
                        testFailed = true;
                    }
                }
            }
        }
        if (testFailed) {
            Debug.LogError("Selection test failed case 1!");
        } else {
            Debug.Log("Selection test passed case 1.");
        }
        //loop through until a blue piece is found, then attempt to select it. the state should change to placement mode
        testFailed = false;
        int selectableCount = 0;
        for (int x = map.cellBounds.min.x; x < map.cellBounds.max.x; x++) {
            for (int y = map.cellBounds.min.y; y < map.cellBounds.max.y; y++) {
                Vector3Int tileCoords = new Vector3Int(x, y, 0);
                if (map.HasTile(tileCoords) && map.GetTile(tileCoords).name.Contains("blue")) {
                    gridScript.attemptPieceSelection(map.CellToWorld(tileCoords));
                    if (gridScript.mode != GridScript.Mode.Placement) {
                        //testFailed = true;
                    } else {
                        selectableCount++;
                        gridScript.unselect();
                    }
                }
            }
        }
        if (selectableCount != 7) {
            Debug.LogError("Selection test failed case 2!");
        } else {
            Debug.Log("Selection test passed case 2.");
        }
    }

    private void TestTurnOrder() {
        //test moving from blue to the next turn
        Debug.Log("Testing turn order advancement.");
        gridScript.currentTurnColor = "blue";
        gridScript.advanceTurn();
        if (!gridScript.currentTurnColor.Equals(GridScript.Colors[1])) {
            Debug.LogError("Turn order test failed test case 1!");
        } else {
            Debug.Log("Turn order test passed test case 1.");
        }
        //test moving from last turn to blue turn
        gridScript.currentTurnColor = GridScript.Colors[GridScript.Colors.Length - 1];
        gridScript.advanceTurn();
        if (!gridScript.currentTurnColor.Equals("blue")) {
            Debug.LogError("Turn order test failed test case 2!");
        } else {
            Debug.Log("Turn order test passed test case 2.");
        }
    }

    private void InitiateBasicMovementTest() {
        //attempt basic movement
        Vector3 testPieceStartingLocation = map.CellToWorld(new Vector3Int(0, 5, 0)); //bottom right blue piece
        Vector3 testPieceMovementLocation = map.CellToWorld(new Vector3Int(0, 4, 0)); //move down and left
        gridScript.attemptPieceSelection(testPieceStartingLocation);
        gridScript.attemptPiecePlacement(testPieceMovementLocation);
        testingAnimatedActions = true;
        testIndex = 0;
        elapsed = 0;
    }

    private void basicMovementTestValidation() {
        if (!map.GetTile(new Vector3Int(0, 4, 0)).name.Contains("blue")) {
            Debug.LogError("Placement test case 1 failed!");
        } else {
            Debug.Log("Placement test case 1 passed.");
        }
        testingAnimatedActions = false;
    }
    private void InitiateJumpMovementTest() {
        //attempt jump
        gridScript.currentTurnColor = "blue";
        Vector3 testPieceStartingLocation = map.CellToWorld(new Vector3Int(-1, 5, 0)); //bottom right blue piece
        Vector3 testPieceMovementLocation = map.CellToWorld(new Vector3Int(0, 3, 0)); //move down and left
        gridScript.attemptPieceSelection(testPieceStartingLocation);
        gridScript.attemptPiecePlacement(testPieceMovementLocation);
        testingAnimatedActions = true;
        testIndex = 1;
        elapsed = 0;
    }

    private void jumpTestValidation() {
        if (!map.GetTile(new Vector3Int(0, 3, 0)).name.Contains("blue")) {
            Debug.LogError("Placement test case 2 failed!");
        } else {
            Debug.Log("Placement test case 2 passed.");
        }
        testingAnimatedActions = false;
    }
}
