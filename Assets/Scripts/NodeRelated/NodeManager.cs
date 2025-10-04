using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

public class NodeManager : MonoBehaviour {

    public int minX;
    public int maxX;
    public int minY;
    public int maxY;
    public int minZ;
    public int maxZ;
    public LayerMask floorAndObstacles;
    public LayerMask nodeLayer;
    public PathNode nodePrefab;
    int _obstaclesLayer;
    int _floorLayer;
    List<PathNode> nodeList = new List<PathNode>();

	void Awake ()
    {
        _obstaclesLayer = LayerMask.NameToLayer("Obstacles");
        _floorLayer = LayerMask.NameToLayer("Floor");
        GenerateNodes();
    }

    public void GenerateNodes()
    {
        nodeList = new List<PathNode>();
        for (int i = minX; i <= maxX; i+=2)
        {
            for (float j = minY; j <= maxY; j+=0.5f)
            {
                for (int k = minZ; k <= maxZ; k+=2)
                {
                    var temp = Physics.OverlapBox(new Vector3(i, j, k), new Vector3(0.5f, 0.3f, 0.5f),Quaternion.identity,floorAndObstacles);
                    if (temp.Length <= 0)
                        continue;
                    bool exists = false;
                    bool blocked = false;            
                    foreach (var item in temp)
                    {
                        if (item.gameObject.layer == _floorLayer)
                            exists = true;
                        else if (item.gameObject.layer == _obstaclesLayer)
                            blocked = true;
                    }
                    if (!exists)
                        continue;
                    var existingNodes = Physics.OverlapBox(new Vector3(i, j-0.5f, k), new Vector3(0.3f, 0.3f, 0.3f), Quaternion.identity, nodeLayer);
                    if (existingNodes.Length > 0)
                    {
                        foreach (var item in existingNodes)
                            Destroy(item.gameObject);
                    }
                    PathNode node = Instantiate(nodePrefab);
                    node.transform.parent = transform;
                    node.transform.position = new Vector3(i, j, k);
                    if (blocked)
                        node.isBlocked = true;
                   
                    nodeList.Add(node);
                }
            }
        }
    }

    private void Start()
    {
        StartCoroutine(StartSearch());
    }

    IEnumerator StartSearch()
    {
        yield return new WaitForSeconds(0.5f);
        SearchNeighbors();
    }

    public void SearchNeighbors()
    {
        foreach (var item in nodeList)
        {
            if (item == null || item.isBlocked)
                continue;
            var temp = Physics.OverlapSphere(item.transform.position, 2.5f, nodeLayer).
                       Where(x => Mathf.Abs(x.transform.position.y - item.transform.position.y) <= 1.5f).
                       Select(x => x.GetComponent<PathNode>()).Where(x => x != null).Where(x => !x.isBlocked).
                       Where(x => !x.Equals(item)).ToList();
            var distances = temp.Select(x => Vector3.Distance(item.transform.position, x.transform.position));
            item.neighbors = temp.Zip(distances, (x, y) => Tuple.Create(x, y)).ToList();

            foreach (var n in item.neighbors)
            {
                n.Item1.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
        nodeList = null;
    }
}
