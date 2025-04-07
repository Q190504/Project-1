using Unity.Entities;
using UnityEngine;

public class CostFieldHelper : MonoBehaviour
{
    public static CostFieldHelper instance;

    [SerializeField] private float nodeSizeScale;
    private EntityCommandBufferSystem ecbSystem;
    private int terrainMask;

    private void Awake()
    {
        instance = this;
        terrainMask = LayerMask.GetMask("Impassable");
    }

    public byte EvaluateCost(Vector3 worldPos, float nodeSize)
    {
        byte newCost = 1;
        Vector2 boxSize = Vector2.one * nodeSize * nodeSizeScale;
        Collider2D[] obstacles = Physics2D.OverlapBoxAll(worldPos, boxSize, 0f, terrainMask);
        ////used to increase the cell that collides with 2+ obstacles
        //bool hasIncreasedCost = false;

        foreach (Collider2D col in obstacles)
        {
            if(col.gameObject.layer == 9)
            {
                newCost = byte.MaxValue;
                break;
            }
            ////Add more layer in here if necessary
            ////EX:
            //else if(!hasIncreasedCost && col.gameObject.layer == 10)
            //{
            //    newCost = 3;
            //    hasIncreasedCost = true;
            //}
        }

        return newCost;
    }
}
