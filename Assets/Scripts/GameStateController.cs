using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum AIDifficulty
{
    Easy,
    Medium,
    Hard
}

public class GameStateController : MonoBehaviour
{
    [Header("TitleBar References")]
    public Image playerXIcon;
    public Image playerOIcon;
    public InputField player1InputField;
    public InputField player2InputField;
    public Text winnerText;
    public Text player2StatusText; // Added to show if Player 2 is AI

    [Header("Misc References")]
    public GameObject endGameState;

    [Header("Asset References")]
    public Sprite tilePlayerO;
    public Sprite tilePlayerX;
    public Sprite tileEmpty;
    public Text[] tileList;

    [Header("GameState Settings")]
    public Color inactivePlayerColor;
    public Color activePlayerColor;
    public string whoPlaysFirst;
    public AIDifficulty aiDifficulty = AIDifficulty.Easy;
    public bool isPlayer2AI = true;

    private string playerTurn;
    private string player1Name;
    private string player2Name;
    private int moveCount;

    private void Start()
    {
        playerTurn = whoPlaysFirst;
        if (playerTurn == "X") playerOIcon.color = inactivePlayerColor;
        else playerXIcon.color = inactivePlayerColor;

        player1InputField.onValueChanged.AddListener(delegate { OnPlayer1NameChanged(); });
        player2InputField.onValueChanged.AddListener(delegate { OnPlayer2NameChanged(); });

        player1Name = player1InputField.text;
        player2Name = player2InputField.text;

        // Update Player 2's status based on whether they are controlled by AI or not
        if (isPlayer2AI)
            player2StatusText.text = "AI Playing";
        else
            player2StatusText.text = "Player 2";

        if (playerTurn == "O" && isPlayer2AI)
            Invoke(nameof(PerformAIMove), 0.5f);
    }

    public void EndTurn()
    {
        moveCount++;
        if (CheckWinCondition())
        {
            GameOver(playerTurn);
        }
        else if (moveCount >= 9)
        {
            GameOver("D");
        }
        else
        {
            ChangeTurn();
            if (playerTurn == "O" && isPlayer2AI)
                Invoke(nameof(PerformAIMove), 0.5f);
        }
    }

    private bool CheckWinCondition()
    {
        string p = playerTurn;
        return (tileList[0].text == p && tileList[1].text == p && tileList[2].text == p) ||
               (tileList[3].text == p && tileList[4].text == p && tileList[5].text == p) ||
               (tileList[6].text == p && tileList[7].text == p && tileList[8].text == p) ||
               (tileList[0].text == p && tileList[3].text == p && tileList[6].text == p) ||
               (tileList[1].text == p && tileList[4].text == p && tileList[7].text == p) ||
               (tileList[2].text == p && tileList[5].text == p && tileList[8].text == p) ||
               (tileList[0].text == p && tileList[4].text == p && tileList[8].text == p) ||
               (tileList[2].text == p && tileList[4].text == p && tileList[6].text == p);
    }

    private void PerformAIMove()
    {
        if (aiDifficulty == AIDifficulty.Easy)
        {
            EasyAIMove();
        }
        else if (aiDifficulty == AIDifficulty.Medium)
        {
            MediumAIMove();
        }
        else if (aiDifficulty == AIDifficulty.Hard)
        {
            HardAIMove();
        }
    }

    private void EasyAIMove()
    {
        List<int> availableTiles = new List<int>();
        for (int i = 0; i < tileList.Length; i++)
        {
            if (string.IsNullOrEmpty(tileList[i].text))
                availableTiles.Add(i);
        }

        if (availableTiles.Count > 0)
        {
            int randomIndex = availableTiles[Random.Range(0, availableTiles.Count)];
            tileList[randomIndex].GetComponentInParent<TileController>().UpdateTile();
        }
    }

    private void MediumAIMove()
    {
        // Check if AI can win or block a player
        int bestMove = GetBestMove();
        if (bestMove != -1)
        {
            tileList[bestMove].GetComponentInParent<TileController>().UpdateTile();
        }
        else
        {
            EasyAIMove(); // Fallback to random if no winning/blocking move
        }
    }
    private void HardAIMove()
    {
        int bestMove = -1;
        int bestScore = int.MinValue;

        // Check all possible moves
        for (int i = 0; i < tileList.Length; i++)
        {
            if (string.IsNullOrEmpty(tileList[i].text))
            {
                // Try the move
                tileList[i].text = "O";
                int score = MinimaxAlgorithm(0, false); // Changed method name to avoid ambiguity
                tileList[i].text = ""; // Undo move

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }

        if (bestMove != -1)
        {
            tileList[bestMove].GetComponentInParent<TileController>().UpdateTile();
        }
    }

    // Renamed from Minimax to MinimaxAlgorithm to avoid ambiguity
    private int MinimaxAlgorithm(int depth, bool isMaximizing)
    {
        // Check terminal states
        if (CheckWinForPlayer("O")) return 10 - depth;
        if (CheckWinForPlayer("X")) return depth - 10;
        if (IsBoardFull()) return 0;

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < tileList.Length; i++)
            {
                if (string.IsNullOrEmpty(tileList[i].text))
                {
                    tileList[i].text = "O";
                    int score = MinimaxAlgorithm(depth + 1, false);
                    tileList[i].text = "";
                    bestScore = Mathf.Max(score, bestScore);
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < tileList.Length; i++)
            {
                if (string.IsNullOrEmpty(tileList[i].text))
                {
                    tileList[i].text = "X";
                    int score = MinimaxAlgorithm(depth + 1, true);
                    tileList[i].text = "";
                    bestScore = Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }
    }

    private bool CheckWinForPlayer(string player)
    {
        return (tileList[0].text == player && tileList[1].text == player && tileList[2].text == player) ||
               (tileList[3].text == player && tileList[4].text == player && tileList[5].text == player) ||
               (tileList[6].text == player && tileList[7].text == player && tileList[8].text == player) ||
               (tileList[0].text == player && tileList[3].text == player && tileList[6].text == player) ||
               (tileList[1].text == player && tileList[4].text == player && tileList[7].text == player) ||
               (tileList[2].text == player && tileList[5].text == player && tileList[8].text == player) ||
               (tileList[0].text == player && tileList[4].text == player && tileList[8].text == player) ||
               (tileList[2].text == player && tileList[4].text == player && tileList[6].text == player);
    }

    private bool IsBoardFull()
    {
        for (int i = 0; i < tileList.Length; i++)
        {
            if (string.IsNullOrEmpty(tileList[i].text))
                return false;
        }
        return true;
    }





    private int GetBestMove()
    {
        // First, check for winning or blocking moves
        for (int i = 0; i < tileList.Length; i++)
        {
            if (string.IsNullOrEmpty(tileList[i].text))
            {
                tileList[i].text = "O"; // Try AI's move
                if (CheckWinCondition())
                {
                    tileList[i].text = "";
                    return i;
                }
                tileList[i].text = ""; // Reset if no win

                tileList[i].text = "X"; // Try player's move
                if (CheckWinCondition())
                {
                    tileList[i].text = "";
                    return i;
                }
                tileList[i].text = ""; // Reset if no block
            }
        }
        return -1; // No immediate win or block found
    }

  

    private int MinimaxEvaluate()
    {
        if (CheckWinCondition()) return 1; // AI wins
        else if (CheckWinCondition()) return -1; // Player wins
        return 0; // Draw
    }

    public void ChangeTurn()
    {
        playerTurn = (playerTurn == "X") ? "O" : "X";
        if (playerTurn == "X")
        {
            playerXIcon.color = activePlayerColor;
            playerOIcon.color = inactivePlayerColor;
        }
        else
        {
            playerXIcon.color = inactivePlayerColor;
            playerOIcon.color = activePlayerColor;
        }
    }

    private void GameOver(string winningPlayer)
    {
        switch (winningPlayer)
        {
            case "D":
                winnerText.text = "DRAW";
                break;
            case "X":
                winnerText.text = player1Name;
                break;
            case "O":
                winnerText.text = isPlayer2AI ? "AI" : player2Name;
                break;
        }

        endGameState.SetActive(true);
        ToggleButtonState(false);
    }

    public void RestartGame()
    {
        moveCount = 0;
        playerTurn = whoPlaysFirst;
        ToggleButtonState(true);
        endGameState.SetActive(false);

        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<TileController>().ResetTile();
        }

        if (playerTurn == "O" && isPlayer2AI)
            Invoke(nameof(PerformAIMove), 0.5f);
    }

    private void ToggleButtonState(bool state)
    {
        for (int i = 0; i < tileList.Length; i++)
        {
            tileList[i].GetComponentInParent<Button>().interactable = state;
        }
    }

    public string GetPlayersTurn()
    {
        return playerTurn;
    }

    public Sprite GetPlayerSprite()
    {
        return playerTurn == "X" ? tilePlayerX : tilePlayerO;
    }

    public void OnPlayer1NameChanged()
    {
        player1Name = player1InputField.text;
    }

    public void OnPlayer2NameChanged()
    {
        player2Name = player2InputField.text;
    }
}
