using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.Fields)]
public class MapData
{
    [SerializeField]
    public List<List<NodeData>> nodes = new List<List<NodeData>>();
    public int rows;
    public int columns;

    public MapData(List<List<NodeData>> nodeMap)
    {
        rows = nodeMap.Count;
        columns = nodeMap[0].Count;

        foreach (var row in nodeMap)
        {
            List<NodeData> nodeDataRow = new List<NodeData>();
            foreach (var node in row)
            {
                nodeDataRow.Add(node);
            }
            nodes.Add(nodeDataRow);
        }
    }
}

[Serializable]
public class NodeData
{
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public List<CellData> cells = new List<CellData>();

    public NodeData() { }

    public NodeData(Node node)
    {
        position = node.transform.position;
        foreach (var cell in node.GetCells())
        {
            cells.Add(new CellData(cell));
        }
    }

    public Stack<CellData> GetCellsStack()
    {
        return new Stack<CellData>(cells);
    }
}

[Serializable]
public class CellData
{
    public CellType type;
    public CellColor color;
    public Direction facing;

    public CellData() { }

    public CellData(Cell cell)
    {
        type = cell.Type;
        color = cell.Color;
        facing = cell.Facing;
    }
}