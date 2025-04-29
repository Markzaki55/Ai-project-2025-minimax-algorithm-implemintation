using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    [Header("Component References")]
    public GameStateController gameController;                       // Reference to the gamecontroller
    public Button interactiveButton;                                 // Reference to this button
    public Text internalText;                                        // Reference to this Text

    public void UpdateTile()
    {
        internalText.text = gameController.GetPlayersTurn();
        interactiveButton.image.sprite = gameController.GetPlayerSprite();
        interactiveButton.interactable = false;
        gameController.EndTurn();
    }

    public void ResetTile()
    {
        internalText.text = "";
        interactiveButton.image.sprite = gameController.tileEmpty;
    }
}
