using UnityEngine;

[CreateAssetMenu(fileName = "PrefabFactory", menuName = "ScriptableObjects/PrefabFactory", order = 1)]
public class PrefabFactory : ScriptableObject
{
    public Node nodePrefab;
    public Cell cellPrefab;
    public Frog frogPrefab;
    public Arrow arrowPrefab;
    public Berry berryPrefab;


    // Add more prefabs as needed
}