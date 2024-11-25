using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Cysharp.Threading.Tasks;

public class MapCreator
{
    [SerializeField] private Node nodePrefab;
    private List<List<Node>> nodeMap = new List<List<Node>>();

    public List<List<Node>> CreateMap(int rows, int columns, Vector3 startPosition, float nodeSize)
    {
        nodeMap.Clear();

        for (int row = 0; row < rows; row++)
        {
            List<Node> currentRow = new List<Node>();

            for (int col = 0; col < columns; col++)
            {
                Vector3 position = startPosition + new Vector3(col * nodeSize, 0, row * nodeSize);
                Node node = Object.Instantiate(nodePrefab, position, Quaternion.identity);
                currentRow.Add(node);
            }

            nodeMap.Add(currentRow);
        }

        return nodeMap;
    }

    public Node GetNode(int row, int column)
    {
        if (row >= 0 && row < nodeMap.Count && column >= 0 && column < nodeMap[row].Count)
        {
            return nodeMap[row][column];
        }
        return null;
    }

    public async UniTask<List<List<Node>>> CreateMap(MapData mapData)
    {
        List<List<Node>> nodeMap = new List<List<Node>>();

        for (int row = 0; row < mapData.rows; row++)
        {
            List<Node> nodeRow = new List<Node>();
            for (int col = 0; col < mapData.columns; col++)
            {
                var nodeData = mapData.nodes[row][col];
                var node = await CreateNode(nodeData);
                nodeRow.Add(node);
            }
            nodeMap.Add(nodeRow);
        }

        return nodeMap;
    }

    private async UniTask<Node> CreateNode(NodeData nodeData)
    {
        var node = new Node();
        // Initialize node from nodeData
        return node;
    }
}