using UnityEngine;
using VContainer;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIView uiViewPrefab;
    private UIView currentView;


    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    public void Initialize()
    {
        InstantiateUIView();
    }

    private void InstantiateUIView()
    {
        if (currentView == null)
        {
            currentView = Instantiate(uiViewPrefab, transform);
        }
    }

    private void HandleGameStateChanged(GameState state)
    {

    }
}