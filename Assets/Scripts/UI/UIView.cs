using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI remainingMovesText;
    [SerializeField] private TextMeshProUGUI levelIndexText;


    private void Start()
    {
        GameEvents.OnMovesChanged += UpdateMovesText;

    }

    private void OnDestroy()
    {
        GameEvents.OnMovesChanged -= UpdateMovesText;

    }

    private void UpdateMovesText(int moves)
    {
        remainingMovesText.text = $"Moves: {moves}";
    }

    public void SetLevelIndex(int index)
    {
        levelIndexText.text = $"Level {index}";
    }


}