using UnityEngine;

public class ScaleExistingTrees : MonoBehaviour
{
    public Terrain terrain;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [ContextMenu("Scale Existing Trees")]
    void ScaleTreesOnTerrain()
    {
        if (!terrain)
        {
            terrain = Terrain.activeTerrain;
        }

        if (maxScale < minScale)
        {
            maxScale = minScale;
        }

        TreeInstance[] treeInstances = terrain.terrainData.treeInstances;

        for (int i = 0; i < treeInstances.Length; i++)
        {
            float randomScale = Random.Range(minScale, maxScale);
            treeInstances[i].widthScale = randomScale;
            treeInstances[i].heightScale = randomScale;
        }

        terrain.terrainData.treeInstances = treeInstances;
        Debug.Log("Tree scaling complete");
    }
}