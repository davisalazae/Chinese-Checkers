using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AI {
    private Tilemap tilemap;
    private GridScript gridScript;
    public Vector3Int strongestMove;
    public Dictionary<string, List<Vector3Int>> lockedInCoords;
    private string[] colors = { "blue", "yellow", "green", "white", "magenta", "red" };
    public AI(Tilemap tilemap, GridScript gridScript) {
        this.tilemap = tilemap;
        this.gridScript = gridScript;
        lockedInCoords = new Dictionary<string, List<Vector3Int>>();
        foreach (string color in colors) {
            lockedInCoords.Add(color, new List<Vector3Int>());
        }
    }

    //this function finds the highest priority piece, and tries to move it forward efficiently
    //if hint mode is true, it doesn't actually place the piece, it just updates the selectionChoice and placementChoice variables
    public void TakeTurn(bool hintMode, Dictionary<string, string> OppositeColors, Dictionary<string, List<Vector3Int>> WinPositions) {
        //find all the selectable pieces 
        List<Vector3Int> selectables = new List<Vector3Int>();
        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++) {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++) {
                //check if this tile position has a tile in it (playable piece or empty board space)
                if (tilemap.HasTile(new Vector3Int(x, y, 0))) {
                    //find out what color the piece is, add it to selectables if it's selectable
                    if (tilemap.GetTile(new Vector3Int(x, y, 0)).name.Contains(gridScript.currentTurnColor) &&
                        !lockedInCoords[gridScript.currentTurnColor].Contains(new Vector3Int(x, y, 0)))
                        selectables.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        //prioritize pieces
        List<Vector3Int> prioritizedSelectables = prioritizePieces(selectables, gridScript.currentTurnColor, WinPositions);
        //get highest priority goal
        Vector3Int highestPriorityGoal = getHighestPriorityGoal(gridScript.currentTurnColor, OppositeColors, WinPositions);
        //in case a piece only has bad moves, keep track of it on the offchance it's still our best shot
        bool pieceDropped = false; Vector3Int droppedPiece = new Vector3Int(0, 0, 0);
        //temp: shuffle prioritized selectables
        List<Vector3Int> shuffledSelectables = prioritizedSelectables.OrderBy(x => Random.value).ToList();
        foreach (Vector3Int prioritizedSelectable in shuffledSelectables) {
            //make sure we're in selection mode
            if (gridScript.mode == GridScript.Mode.Selection) {
                Vector3 psW = tilemap.CellToWorld(prioritizedSelectable);
                gridScript.attemptPieceSelection(tilemap.CellToWorld(prioritizedSelectable));
            }
        }
        //if we're still in selection mode after the above, then a good piece couldn't be found, so just pick up the last one
        if (gridScript.mode == GridScript.Mode.Selection) {
            //forcefully make a bad move if there's no other option so that something happens at least
            if (pieceDropped) gridScript.attemptPieceSelection(droppedPiece);
            return;
        }
        //find a strong move to make by seeing which move gets us the closest to the highest priority goal
        float shortestDistance = float.MaxValue; strongestMove = gridScript.validTiles[0];
        //for each valid tile,
        foreach (Vector3Int validTile in gridScript.validTiles) {
            //find the distance between this valid move and the highest priority goal
            Vector3 validTileWorldPosition = tilemap.CellToWorld(validTile);
            Vector3 highestPriorityGoalWorldPosition = tilemap.CellToWorld(highestPriorityGoal);
            float distance = Vector3.Distance(validTileWorldPosition, highestPriorityGoalWorldPosition);
            //if this tile has a shorter distance, make it the new strongest move
            if (distance < shortestDistance) {
                shortestDistance = distance;
                strongestMove = validTile;
            }
        }
        //if we're not in hint mode, actually place the piece down
        if (!hintMode) {
            gridScript.attemptPiecePlacement(tilemap.CellToWorld(strongestMove));
            //check if the strongest move is equal to the highest priority goal, and lock the piece in if it is, 
            //so that the AI doesn't just move pieces back and forth in the win zone
            if (strongestMove == highestPriorityGoal) {
                //tile image asset to key conversion
                string key = gridScript.currentTurnColor;
                lockedInCoords[key].Add(highestPriorityGoal);
            }
        }
    }

    //this method just loops through the selectable pieces and prioritizas them based on average distance from the goals.
    //it calculates average distance, sticks them in a dictionary, then sorts the dictionary
    public List<Vector3Int> prioritizePieces(List<Vector3Int> selectables, string currentMoveColor, Dictionary<string, List<Vector3Int>> WinPositions) {
        //get the list of goal positions
        List<Vector3Int> goalPositions = WinPositions[currentMoveColor];
        //store their distance values in a dict
        Dictionary<Vector3Int, float> distanceDictionary = new Dictionary<Vector3Int, float>();
        //for each of selectables, find its average distance from the goals, and add it to the dictionary
        foreach (Vector3Int selectable in selectables) {
            //get the world position
            Vector3 selectableWorldPosition = tilemap.CellToWorld(selectable);
            //initialice total distance variable for average calculation
            float totalDistance = 0;
            //for each goal position, add its distance from the selectable to the total distance
            foreach (Vector3Int goalPosition in goalPositions) {
                Vector3 goalWorldPosition = tilemap.CellToWorld(goalPosition);
                float distance = Vector3.Distance(selectableWorldPosition, goalWorldPosition);
                totalDistance += distance;
            }
            //find the average distance of this piece from the goal and add it to the dict
            float averageDistance = totalDistance / 10;
            distanceDictionary.Add(selectable, averageDistance);
        }
        //sort the dict
        //while the dictionary has values in it, take the largest value distance one and put it in the return list
        List<Vector3Int> prioritizedSelectables = new List<Vector3Int>();
        while (distanceDictionary.Count > 0) {
            Vector3Int farthest = new Vector3Int(0, 0, 0);
            float biggestDistance = 0;
            foreach (Vector3Int key in distanceDictionary.Keys) {
                if (distanceDictionary[key] > biggestDistance) {
                    farthest = key;
                    biggestDistance = distanceDictionary[key];
                }
            }
            prioritizedSelectables.Add(farthest);
            distanceDictionary.Remove(farthest);
        }
        return prioritizedSelectables;
    }

    //this method finds the highest priority goal by seeing which unfilled goal is the farthest away from the opposite side
    //(ie, we want to fill the goals from back to front so we don't block pieces)
    public Vector3Int getHighestPriorityGoal(string currentMoveColor, Dictionary<string, string> OppositeColors, Dictionary<string, List<Vector3Int>> WinPositions) {
        //get the starting positions for the current move by getting the winning positions from the opposite color
        string oppositeColor = OppositeColors[currentMoveColor];
        List<Vector3Int> startingPositions = WinPositions[oppositeColor];
        //for each of goal positions, find which one has the farthest average distance from the starting positions
        float longestDistance = float.MinValue; Vector3Int priorityGoal = new Vector3Int(0, 0, 0);
        foreach (Vector3Int goalPosition in WinPositions[currentMoveColor]) {
            float totalDistance = 0;
            Vector3 goalPositionWorld = tilemap.CellToWorld(goalPosition);
            foreach (Vector3Int startPosition in startingPositions) {
                Vector3 startPositionWorld = tilemap.CellToWorld(startPosition);
                float distance = Vector3.Distance(startPositionWorld, goalPositionWorld);
                totalDistance += distance;
            }
            //if this goal's average distance is greater than the others so far, set it as the priority goal, unless it's already filled
            float averageDistance = totalDistance / 10;
            if (averageDistance > longestDistance && !tilemap.GetTile(goalPosition).name.Contains(currentMoveColor)) {
                longestDistance = averageDistance;
                priorityGoal = goalPosition;
            }
        }
        return priorityGoal;
    }

    //this method checks to see if the piece has any moves that actually move towards the goal. used to make sure it doesn't get stuck 
    //in a loop making bad moves on a high priority piece instead of okay moves on medium priority piece
    public bool canMoveCloser(Vector3Int selectedPiece, Vector3Int highestPriorityGoal) {
        //set to true if the pieces can move closer
        bool passed = false;
        //for each valid move location,
        foreach (Vector3Int validTile in gridScript.validTiles) {
            //get the world positions of the valid moves, the goal, and the selected piece
            Vector3 validTileWorldPosition = tilemap.CellToWorld(validTile);
            Vector3 winWorldPosition = tilemap.CellToWorld(highestPriorityGoal);
            Vector3 selectedPieceWorldPosition = tilemap.CellToWorld(selectedPiece);
            //check and see if making this move would make the piece closer to goal or farther away
            float validTileDistance = Vector3.Distance(validTileWorldPosition, winWorldPosition);
            float selectedPieceDistance = Vector3.Distance(selectedPieceWorldPosition, winWorldPosition);
            if (!passed && (validTileDistance + gridScript.cellSizeInWorldUnits / 2) < selectedPieceDistance) {
                passed = true;
            }
        }
        return passed;
    }
}
