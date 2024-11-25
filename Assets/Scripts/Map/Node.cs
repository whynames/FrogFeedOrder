using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class Node : MonoBehaviour
{
    private Stack<Cell> cells;

    private void Awake()
    {
        cells = new Stack<Cell>();
    }

    public Stack<Cell> GetCells()
    {
        return new Stack<Cell>(cells.ToArray());
    }

    public void AddCell(Cell cell)
    {
        if (cells == null)
        {
            cells = new Stack<Cell>();
        }
        cells.Push(cell);
        cell.transform.SetParent(transform);
    }

    public void RemoveCell(Cell cell)
    {
        Stack<Cell> tempStack = new Stack<Cell>();
        while (cells.Count > 0)
        {
            Cell currentCell = cells.Pop();
            if (currentCell != cell)
            {
                tempStack.Push(currentCell);
            }
        }
        while (tempStack.Count > 0)
        {
            cells.Push(tempStack.Pop());
        }
    }

    public Cell GetTopCell()
    {
        return cells.Count > 0 ? cells.Peek() : null;
    }

    public async UniTask<Cell> RemoveTopCell()
    {
        var cell = cells.Count > 0 ? cells.Pop() : null;
        await cell.Close();
        var newCell = GetTopCell();
        newCell?.Open();
        return cell;
    }
}