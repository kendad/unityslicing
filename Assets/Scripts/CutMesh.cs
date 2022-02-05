using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutMesh : MonoBehaviour
{
    //PUBLIC VARIABLES
    public GameObject planeObject;
    //PRIVATE VARIABLES
    private Plane planeConstruct;

    //game_object_data
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] UVs;
    private int[] triangles;
    private Material[] materials;

    //List
    private List<Vector3> verticesList=new List<Vector3>();
    private List<int> trianglesList=new List<int>();
    private List<Vector3> normalsList=new List<Vector3>();

    //intersection Points
    private List<Vector3> intersectionPoints = new List<Vector3>();

    //triangles on/off side of plane
    private List<int> trianglesOnPositiveSide = new List<int>();
    private List<int> trianglesOnNegativeSide = new List<int>();
    private List<int> trianglesToIgnore = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        PlaneConstructData();
        GetObjectMeshData();
        V_L_Converter();
        //CreateGameObject();
        GenerateIntersectionPoints();
        SeperatePointsByPlaneSide();
    }

    // Update is called once per frame
    void Update()
    {
        /*PlaneConstructData();
        GenerateIntersectionPoints();
        SeperatePointsByPlaneSide();*/

        DrawIntersectPoints();
        DrawPointsOnSidesOfPlane();

        //clear data
        /*intersectionPoints.Clear();
        trianglesOnPositiveSide.Clear();
        trianglesOnNegativeSide.Clear();
        trianglesToIgnore.Clear();*/
    }

    void GetObjectMeshData()
    {
        Mesh meshData = this.gameObject.GetComponent<MeshFilter>().mesh;
        MeshRenderer meshRendererData = this.gameObject.GetComponent<MeshRenderer>();
        vertices = meshData.vertices;
        normals = meshData.normals;
        triangles = meshData.triangles;
        UVs = meshData.uv;
        materials = meshRendererData.materials;
    }

    void V_L_Converter()
    {
        for(int i = 0; i < vertices.Length; i++)
        {
            verticesList.Add(vertices[i]);
        }
        for (int i = 0; i < triangles.Length; i++)
        {
            trianglesList.Add(triangles[i]);
        }
        for (int i = 0; i < normals.Length; i++)
        {
            normalsList.Add(normals[i]);
        }
    }

    void CreateGameObject()
    {
        GameObject go1=new GameObject();
        go1.name = "go1";
        MeshRenderer meshRendererData=go1.AddComponent<MeshRenderer>();
        MeshFilter meshFilterData=go1.AddComponent<MeshFilter>();
        meshFilterData.mesh.vertices = verticesList.ToArray();
        meshFilterData.mesh.normals = normalsList.ToArray();
        meshFilterData.mesh.triangles = trianglesList.ToArray();
        meshFilterData.mesh.uv = UVs;
        meshRendererData.materials = materials;
    }

    void GenerateIntersectionPoints()
    {
        RaycastHit hitObject;
        bool isHit = false;
        for(int i = 0; i < triangles.Length; i+=3)
        {
            isHit = false;
            if(Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i]]), this.transform.TransformPoint(vertices[triangles[i + 1]]) - this.transform.TransformPoint(vertices[triangles[i]]),out hitObject,Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i + 1]]), this.transform.TransformPoint(vertices[triangles[i]]))))
            {
                if(hitObject.transform.name=="Plane") intersectionPoints.Add(hitObject.point);
                isHit = true;
            }
            if (Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i + 1]]), this.transform.TransformPoint(vertices[triangles[i + 2]]) - this.transform.TransformPoint(vertices[triangles[i + 1]]), out hitObject, Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i + 2]]), this.transform.TransformPoint(vertices[triangles[i+1]]))))
            {
                if (hitObject.transform.name == "Plane") intersectionPoints.Add(hitObject.point);
                isHit=true;
            }
            if (Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i + 2]]), this.transform.TransformPoint(vertices[triangles[i]]) - this.transform.TransformPoint(vertices[triangles[i + 2]]), out hitObject,Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i]]),this.transform.TransformPoint(vertices[triangles[i+2]]))))
            {
                if (hitObject.transform.name == "Plane") intersectionPoints.Add(hitObject.point);
                isHit=true;
            }
            if (isHit) trianglesToIgnore.Add(i);
            //Debug.DrawRay(this.transform.TransformPoint(vertices[triangles[i]]),(this.transform.TransformPoint(vertices[triangles[i+1]])- this.transform.TransformPoint(vertices[triangles[i]])).normalized,Color.blue);
            //Debug.DrawRay(this.transform.TransformPoint(vertices[triangles[i+1]]),(this.transform.TransformPoint(vertices[triangles[i+2]])-this.transform.TransformPoint(vertices[triangles[i+1]])).normalized,Color.blue);
            //Debug.DrawRay(this.transform.TransformPoint(vertices[triangles[i+2]]),(this.transform.TransformPoint(vertices[triangles[i]])-this.transform.TransformPoint(vertices[triangles[i+2]])).normalized,Color.blue);
        }
    }

    void DrawIntersectPoints()
    {
        foreach (Vector3 point in intersectionPoints)
        {
            Debug.DrawLine(this.transform.TransformPoint(point), this.transform.TransformPoint(point) + new Vector3(0.03f, 0.03f,0.03f),Color.red);
        }
    }

    void PlaneConstructData()
    {
        planeConstruct = new Plane(-planeObject.transform.forward, planeObject.transform.position);
    }

    void SeperatePointsByPlaneSide()
    {
        for(int i = 0; i < triangles.Length; i+=3)
        {
            if (trianglesToIgnore.Contains(i)) continue;//ignore the triangles that have an intersection point
            
            if (planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i]]))&& planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i+1]]))&& planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i+2]])))
            {
                trianglesOnPositiveSide.Add(triangles[i]);
                trianglesOnPositiveSide.Add(triangles[i+1]);
                trianglesOnPositiveSide.Add(triangles[i+2]);
            }
            else
            {
                trianglesOnNegativeSide.Add(triangles[i]);
                trianglesOnNegativeSide.Add(triangles[i + 1]);
                trianglesOnNegativeSide.Add(triangles[i + 2]);
            }
        }
    }

    void DrawPointsOnSidesOfPlane()
    {
        for(int i = 0; i < trianglesOnPositiveSide.Count; i += 3)
        {
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i+1]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i+1]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i+2]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i+2]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
        }
        for (int i = 0; i < trianglesOnNegativeSide.Count; i += 3)
        {
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 1]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 1]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 2]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 2]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
        }
    }
}
