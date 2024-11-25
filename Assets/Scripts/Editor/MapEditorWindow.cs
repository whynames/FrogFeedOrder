using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapEditorWindow : EditorWindow
{
    private int rows = 5;
    private int columns = 5;
    private Vector2 scrollPosition;
    private CellType selectedCellType = CellType.Berry;
    private CellColor selectedColor = CellColor.Red;
    private Direction selectedDirection = Direction.Right;
    private float cellSize = 50f;
    private string mapName = "NewMap";
    private int berryCount = 1;

    // Editor-only data structures
    private class EditorCell
    {
        public CellType Type;
        public CellColor Color;
        public Direction Direction;
    }

    private class EditorNode
    {
        public Stack<EditorCell> Cells = new Stack<EditorCell>();
    }

    private List<List<EditorNode>> editorMap = new List<List<EditorNode>>();
    private EditorNode selectedNode;
    private EditorCell selectedCell;

    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void CreateMap()
    {
        editorMap.Clear();
        for (int row = 0; row < rows; row++)
        {
            var newRow = new List<EditorNode>();
            for (int col = 0; col < columns; col++)
            {
                newRow.Add(new EditorNode());
            }
            editorMap.Add(newRow);
        }
    }

    private void DrawNodeEditor()
    {
        if (editorMap.Count == 0) CreateMap();

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;
        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        // Draw background
        EditorGUI.DrawRect(gridRect, Color.black);

        // Draw cells
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Rect cellRect = new Rect(gridRect.x + col * cellSize, gridRect.y + row * cellSize,
                                       cellSize - 2, cellSize - 2);
                EditorNode node = editorMap[row][col];
                DrawNodePreview(node, cellRect);
            }
        }
    }

    private void DrawNodePreview(EditorNode node, Rect rect)
    {
        bool isSelected = selectedNode == node;
        EditorGUI.DrawRect(rect, isSelected ? Color.cyan : Color.gray);

        // Draw stack preview
        if (node.Cells.Count > 0)
        {
            string stackPreview = "";
            foreach (var cell in node.Cells)
            {
                Color cellColor = GetColorFromCellType(cell.Color);
                stackPreview = $"<color=#{ColorUtility.ToHtmlStringRGB(cellColor)}>{GetInitialFromCellType(cell.Type)}</color>" + stackPreview;
            }
            EditorGUI.LabelField(rect, stackPreview, new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                richText = true,
                normal = { textColor = Color.white }
            });
        }

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            selectedNode = node;
            selectedCell = null;
            Repaint();
        }
    }

    private string GetInitialFromCellType(CellType type)
    {
        return type switch
        {
            CellType.Berry => "B",
            CellType.Frog => "F",
            CellType.Arrow => "A",
            _ => ""
        };
    }

    private Color GetColorFromCellType(CellColor color)
    {
        return color switch
        {
            CellColor.Red => Color.red,
            CellColor.Green => Color.green,
            CellColor.Blue => Color.blue,
            // Add other colors as needed
            _ => Color.white
        };
    }

    private bool[] IsValidDirection(int row, int col)
    {
        bool isLeftEdge = col == 0;
        bool isRightEdge = col == columns - 1;
        bool isTopEdge = row == 0;
        bool isBottomEdge = row == rows - 1;

        return new bool[]
        {
            !isTopEdge,   // Up
            !isRightEdge, // Right
            !isBottomEdge,  // Down
            !isLeftEdge,  // Left
        };
    }

    private void GetNodePosition(EditorNode node, out int row, out int col)
    {
        row = -1;
        col = -1;
        for (int r = 0; r < editorMap.Count; r++)
        {
            for (int c = 0; c < editorMap[r].Count; c++)
            {
                if (editorMap[r][c] == node)
                {
                    row = r;
                    col = c;
                    return;
                }
            }
        }
    }

    private void DrawNodeInspector()
    {
        if (selectedNode == null) return;

        GetNodePosition(selectedNode, out int row, out int col);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Node Stack", EditorStyles.boldLabel);

        // Add cell buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Berry")) AddCell(selectedNode, CellType.Berry);

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Add Frog")) AddCell(selectedNode, CellType.Frog);
        if (selectedCellType == CellType.Frog)
            selectedDirection = DrawDirectionPopup(selectedDirection, row, col);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Add Arrow")) AddCell(selectedNode, CellType.Arrow);
        if (selectedCellType == CellType.Arrow)
            selectedDirection = DrawDirectionPopup(selectedDirection, row, col);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        // Show stack
        var cells = selectedNode.Cells.ToList();
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"{cells[i].Type}");

            cells[i].Type = (CellType)EditorGUILayout.EnumPopup(cells[i].Type);
            cells[i].Color = (CellColor)EditorGUILayout.EnumPopup(cells[i].Color);

            if (cells[i].Type != CellType.Berry)
            {
                cells[i].Direction = DrawDirectionPopup(cells[i].Direction, row, col);
            }

            GUI.enabled = i > 0;
            if (GUILayout.Button("↓", GUILayout.Width(25))) SwapCells(selectedNode, i, i - 1);
            GUI.enabled = i < cells.Count - 1;
            if (GUILayout.Button("↑", GUILayout.Width(25))) SwapCells(selectedNode, i, i + 1);
            GUI.enabled = true;

            if (GUILayout.Button("X", GUILayout.Width(25))) RemoveCell(selectedNode, cells[i]);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SwapCells(EditorNode node, int index1, int index2)
    {
        var cells = node.Cells.ToList();
        if (index1 >= 0 && index2 >= 0 && index1 < cells.Count && index2 < cells.Count)
        {
            var temp = cells[index1];
            cells[index1] = cells[index2];
            cells[index2] = temp;

            node.Cells.Clear();
            for (int i = cells.Count - 1; i >= 0; i--)
            {
                node.Cells.Push(cells[i]);
            }
        }
    }

    private void AddCell(EditorNode node, CellType type)
    {
        var newCell = new EditorCell
        {
            Type = type,
            Color = selectedColor,
            Direction = selectedDirection
        };
        node.Cells.Push(newCell);
        Repaint();
    }

    private void RemoveCell(EditorNode node, EditorCell cell)
    {
        var tempStack = new Stack<EditorCell>();
        while (node.Cells.Count > 0)
        {
            var current = node.Cells.Pop();
            if (current != cell)
            {
                tempStack.Push(current);
            }
        }
        while (tempStack.Count > 0)
        {
            node.Cells.Push(tempStack.Pop());
        }
    }

    private void SaveMap()
    {
        var mapData = new MapData(editorMap.Select(row =>
            row.Select(editorNode =>
            {
                var nodeData = new NodeData
                {
                    position = Vector3.zero, // Set a default position or calculate as needed
                    cells = editorNode.Cells.Select(cell => new CellData
                    {
                        type = cell.Type,
                        color = cell.Color,
                        facing = cell.Direction
                    }).ToList()
                };
                return nodeData;
            }).ToList()
        ).ToList());

        // Log the count of cells for debugging

        string json = Unity.Plastic.Newtonsoft.Json.JsonConvert.SerializeObject(mapData, new Unity.Plastic.Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Unity.Plastic.Newtonsoft.Json.Formatting.Indented,
            ReferenceLoopHandling = Unity.Plastic.Newtonsoft.Json.ReferenceLoopHandling.Ignore
        });

        Debug.Log(mapName);
        string path = $"Assets/Levels/{mapName}.json";

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    private async void LoadMap(string mapName = "mapEasy")
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>($"{mapName}");
        var mapJson = await handle.Task;

        MapData mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(mapJson.text);

        Addressables.Release(handle);

        CreateMapFromData(mapData);
        this.mapName = mapName;
    }

    private void CreateMapFromData(MapData mapData)
    {
        // Clear existing map
        editorMap.Clear();
        for (int row = 0; row < mapData.rows; row++)
        {
            var newRow = new List<EditorNode>();
            for (int col = 0; col < mapData.columns; col++)
            {
                var nodeData = mapData.nodes[row][col];
                var editorNode = new EditorNode();
                editorNode.Cells = new Stack<EditorCell>();

                // Create cells and stack them
                foreach (var cellData in nodeData.cells)
                {
                    var newCell = new EditorCell
                    {
                        Type = cellData.type,
                        Color = cellData.color,
                        Direction = cellData.facing
                    };
                    editorNode.Cells.Push(newCell);
                }

                newRow.Add(editorNode);
            }
            editorMap.Add(newRow);
        }

        // Activate the top cell in each node
        foreach (var row in editorMap)
        {
            foreach (var node in row)
            {
                if (node.Cells.Count > 0)
                {
                    // Activate the top cell (if you have a method to handle activation)
                    ActivateTopCell(node);
                }
            }
        }

        Repaint(); // Refresh the editor window
    }

    private void ActivateTopCell(EditorNode node)
    {
        // Logic to activate the top cell in the node
        // This could involve enabling a GameObject or changing a property
        if (node.Cells.Count > 0)
        {
            var topCell = node.Cells.Peek();
            // Implement your activation logic here
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        // Draw grid settings
        EditorGUILayout.BeginHorizontal();
        rows = EditorGUILayout.IntField("Rows:", rows);
        columns = EditorGUILayout.IntField("Columns:", columns);
        EditorGUILayout.EndHorizontal();

        // Draw cell type selector
        selectedCellType = (CellType)EditorGUILayout.EnumPopup("Cell Type:", selectedCellType);
        selectedColor = (CellColor)EditorGUILayout.EnumPopup("Cell Color:", selectedColor);

        EditorGUILayout.Space();

        // Draw grid
        DrawNodeEditor();

        // Draw node inspector
        DrawNodeInspector();

        // Save button
        mapName = EditorGUILayout.TextField("Map Name:", mapName);
        if (GUILayout.Button("Save Level"))
        {
            SaveMap();
        }

        EditorGUILayout.BeginHorizontal();
        berryCount = EditorGUILayout.IntField("Berry Count:", berryCount);
        if (GUILayout.Button("Add Berries to All"))
        {
            foreach (var row in editorMap)
            {
                foreach (var node in row)
                {
                    for (int i = 0; i < berryCount; i++)
                    {
                        AddCell(node, CellType.Berry);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // Load saved levels
        DrawLoadLevelButtons();

        EditorGUILayout.EndVertical();
    }

    private void DrawLoadLevelButtons()
    {
        string[] files = Directory.GetFiles("Assets/Levels", "*.json");
        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (GUILayout.Button(fileName))
            {
                LoadMap(fileName);
            }
        }
    }

    private Direction DrawDirectionPopup(Direction currentDirection, int row, int col)
    {
        var displayedOptions = new List<GUIContent>();
        var currentIndex = 0;

        bool[] validDirections = IsValidDirection(row, col);




        for (int i = 0; i < 4; i++)
        {
            Direction dir = (Direction)i;
            var content = new GUIContent(dir.ToString());
            if (!validDirections[i])
            {
                content.text += " (Invalid)";
            }
            displayedOptions.Add(content);

            if (dir == currentDirection)
            {
                currentIndex = i;
            }
        }

        int selectedIndex = EditorGUILayout.Popup(
            new GUIContent("Direction"),
            currentIndex,
            displayedOptions.ToArray()
        );

        Direction selectedDirection = (Direction)selectedIndex;
        if (!validDirections[selectedIndex] && selectedDirection != currentDirection)
        {
            return currentDirection;
        }
        return selectedDirection;
    }
}