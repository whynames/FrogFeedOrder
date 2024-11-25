using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;


public class MapLoader
{

    private readonly PrefabFactory prefabFactory;
    private const float CELL_Y_OFFSET = 0.1f;
    private bool isMapLoaded = false;
    public bool IsMapLoaded => isMapLoaded;

    [Inject]
    public MapLoader(PrefabFactory prefabFactory)
    {

        this.prefabFactory = prefabFactory;
    }

    public async UniTask<List<List<Node>>> LoadMap(string mapName = "MapEasy")
    {
        isMapLoaded = false;
        var handle = Addressables.LoadAssetAsync<TextAsset>($"{mapName}");
        var mapJson = await handle.Task;

        MapData mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(mapJson.text);
        Debug.Log($"Loaded map: {mapName}");
        Addressables.Release(handle);

        var map = await CreateMapFromData(mapData);
        GameEvents.MapLoaded(map);
        isMapLoaded = true;
        return map;
    }

    private async UniTask<List<List<Node>>> CreateMapFromData(MapData mapData)
    {
        List<List<Node>> nodeMap = new List<List<Node>>();

        for (int row = 0; row < mapData.rows; row++)
        {
            List<Node> nodeRow = new List<Node>();
            for (int col = 0; col < mapData.columns; col++)
            {
                var nodeData = mapData.nodes[row][col];
                nodeData.position = new Vector3(col, 0, row); // Assign position based on grid logic
                var node = await CreateNode(nodeData);
                nodeRow.Add(node);
            }
            nodeMap.Add(nodeRow);
        }

        return nodeMap;
    }

    private async UniTask<Node> CreateNode(NodeData nodeData)
    {
        var frogGame = LifetimeScope.Find<FrogGame>();
        var container = frogGame.Container;
        var node = Object.Instantiate(prefabFactory.nodePrefab, frogGame.transform);
        node.transform.position = nodeData.position;
        var spriteProvider = container.Resolve<SpriteProvider>();

        for (int i = 0; i < nodeData.cells.Count; i++)
        {
            var cellData = nodeData.cells[i];
            var cell = Object.Instantiate(prefabFactory.cellPrefab);

            // Set cell position with y-offset for stacking visual
            cell.transform.position = node.transform.position + new Vector3(0, i * CELL_Y_OFFSET, 0);
            cell.transform.SetParent(node.transform);

            // Only instantiate owner prefab for top cell
            cell.OnOpened += () =>
              {
                  var owner = InstantiateCellOwner(cellData, container);
                  var cellOwner = owner.GetComponent<ICellOwner>();
                  cellOwner.SetCellData(cellData.color, cellData.facing, cellData.type);
                  if (owner != null)
                  {
                      owner.GetComponent<ICellOwner>().Initialize(spriteProvider);
                      owner.transform.SetParent(cell.transform);
                      owner.transform.localPosition = Vector3.zero;
                      cell.SetOwner(cellOwner);
                      cell.Initialize(cellData.color, cellData.facing, spriteProvider);

                  }
              };



            if (i == nodeData.cells.Count - 1)
                cell.Open();
            else
                cell.Deactivate();
            node.AddCell(cell);
        }

        return node;
    }

    private GameObject InstantiateCellOwner(CellData cellData, IObjectResolver container)
    {
        GameObject owner = cellData.type switch
        {
            CellType.Frog => Object.Instantiate(prefabFactory.frogPrefab).gameObject,
            CellType.Arrow => Object.Instantiate(prefabFactory.arrowPrefab).gameObject,
            CellType.Berry => Object.Instantiate(prefabFactory.berryPrefab).gameObject,
            _ => null
        };

        container.InjectGameObject(owner);

        return owner;
    }
}