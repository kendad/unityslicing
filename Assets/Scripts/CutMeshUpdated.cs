using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutMeshUpdated : MonoBehaviour
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
    private List<Vector3> verticesList = new List<Vector3>();
    private List<int> trianglesList = new List<int>();
    private List<Vector3> normalsList = new List<Vector3>();

    //intersection Points
    private List<Vector3> intersectionPoints = new List<Vector3>();

    //triangles on/off side of plane
    private List<int> trianglesOnPositiveSide = new List<int>();
    private List<int> trianglesOnNegativeSide = new List<int>();
    private List<int> trianglesToIgnore = new List<int>();
    private List<Vector3> newPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        PlaneConstructData();
        GetObjectMeshData();
        V_L_Converter();
        //GenerateIntersectionPoints();
        SeperatePointsByPlaneSide();
        CreateNewFaceTriangles();
        CreateGameObject();
        this.gameObject.SetActive(false);
        planeObject.SetActive(false);
        /*foreach (KeyValuePair<int, List<Vector3>> value in triInterPairs){
            Debug.Log("Triangle: "+value.Key);
            foreach(Vector3 point in value.Value)
            {
                Debug.Log(point);
            }
            Debug.Log("####");
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        /*PlaneConstructData();
        GenerateIntersectionPoints();
        SeperatePointsByPlaneSide();*/

        //DrawIntersectPoints();
        //DrawPointsOnSidesOfPlane();

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

    void V_L_Converter()//ARRAY TO LIST CONVERTER
    {
        for (int i = 0; i < vertices.Length; i++)
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
        GameObject go1 = new GameObject();
        MeshRenderer meshRendererData = go1.AddComponent<MeshRenderer>();
        MeshFilter meshFilterData = go1.AddComponent<MeshFilter>();
        meshFilterData.mesh.vertices = verticesList.ToArray();
        meshFilterData.mesh.normals = normalsList.ToArray();
        //meshFilterData.mesh.RecalculateNormals();
        meshFilterData.mesh.triangles = trianglesOnPositiveSide.ToArray();
        //meshFilterData.mesh.uv = UVs;
        meshRendererData.materials = materials;
        go1.AddComponent<Rigidbody>();
        go1.AddComponent<BoxCollider>();

        GameObject go2 = new GameObject();
        meshRendererData = go2.AddComponent<MeshRenderer>();
        meshFilterData = go2.AddComponent<MeshFilter>();
        meshFilterData.mesh.vertices = verticesList.ToArray();
        meshFilterData.mesh.normals = normalsList.ToArray();
        //meshFilterData.mesh.RecalculateNormals();
        meshFilterData.mesh.triangles = trianglesOnNegativeSide.ToArray();
        //meshFilterData.mesh.uv = UVs;
        meshRendererData.materials = materials;
        go2.AddComponent<Rigidbody>();
        go2.AddComponent<BoxCollider>();
    }

    void GenerateIntersectionPoints()
    {
        RaycastHit hitObject;
        List<Vector3> tmpPoints = new List<Vector3>();
        bool isHit = false;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            isHit = false;
            hitObject = new RaycastHit();
            tmpPoints = new List<Vector3>();
            if (Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i]]), this.transform.TransformPoint(vertices[triangles[i + 1]]) - this.transform.TransformPoint(vertices[triangles[i]]), out hitObject, Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i + 1]]), this.transform.TransformPoint(vertices[triangles[i]]))))
            {
                if (hitObject.transform.name == "Plane") intersectionPoints.Add(hitObject.point);
                tmpPoints.Add(hitObject.point);
                isHit = true;
            }
            if (Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i + 1]]), this.transform.TransformPoint(vertices[triangles[i + 2]]) - this.transform.TransformPoint(vertices[triangles[i + 1]]), out hitObject, Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i + 2]]), this.transform.TransformPoint(vertices[triangles[i + 1]]))))
            {
                if (hitObject.transform.name == "Plane") intersectionPoints.Add(hitObject.point);
                tmpPoints.Add(hitObject.point);
                isHit = true;
            }
            if (Physics.Raycast(this.transform.TransformPoint(vertices[triangles[i + 2]]), this.transform.TransformPoint(vertices[triangles[i]]) - this.transform.TransformPoint(vertices[triangles[i + 2]]), out hitObject, Vector3.Distance(this.transform.TransformPoint(vertices[triangles[i]]), this.transform.TransformPoint(vertices[triangles[i + 2]]))))
            {
                if (hitObject.transform.name == "Plane") intersectionPoints.Add(hitObject.point);
                tmpPoints.Add(hitObject.point);
                isHit = true;
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
            Debug.DrawLine(this.transform.TransformPoint(point), this.transform.TransformPoint(point) + new Vector3(0.03f, 0.03f, 0.03f), Color.red);
        }
    }

    void PlaneConstructData()
    {
        planeConstruct = new Plane(-planeObject.transform.forward, planeObject.transform.position);
    }

    void SeperatePointsByPlaneSide()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            //if (trianglesToIgnore.Contains(i)) continue;//ignore the triangles that have an intersection point

            if (planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i]])) && planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i + 1]])) && planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i + 2]])))
            {
                trianglesOnPositiveSide.Add(triangles[i]);
                trianglesOnPositiveSide.Add(triangles[i + 1]);
                trianglesOnPositiveSide.Add(triangles[i + 2]);
            }
            else if(planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i]]))==false && planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i + 1]]))==false && planeConstruct.GetSide(this.transform.TransformPoint(vertices[triangles[i + 2]]))==false)
            {
                trianglesOnNegativeSide.Add(triangles[i]);
                trianglesOnNegativeSide.Add(triangles[i + 1]);
                trianglesOnNegativeSide.Add(triangles[i + 2]);
            }
            else
            {
                Vector3[] pointsOnTriangle = {
                    vertices[triangles[i]], 
                    vertices[triangles[i+1]], 
                    vertices[triangles[i+2]]
                };
                CreateNewTriangles(pointsOnTriangle, i);
            }
        }
    }

    void CreateNewTriangles(Vector3[] pointsOnTriangle,int triangleIndex)
    {
        Vector3 newVertex1 = new Vector3();
        Vector3 newVertex2 = new Vector3();

        List<Vector3> posSide = new List<Vector3>();
        List<Vector3> negSide = new List<Vector3>();

        foreach(Vector3 point in pointsOnTriangle)
        {
            if (planeConstruct.GetSide(point))
            {
                posSide.Add(point);
            }
            else { negSide.Add(point); }
        }

        if (posSide.Count == 1)
        {
            for(int i = 0; i < negSide.Count; i++)
            {
                float distance;
                Vector3 direction = (negSide[i] - posSide[0]).normalized;
                Ray ray = new Ray(posSide[0],direction);
                planeConstruct.Raycast(ray,out distance);
                if (i == 0) newVertex1 = posSide[0] + (direction * distance);
                if (i == 1) newVertex2 = posSide[0] + (direction * distance);
            }
            verticesList.Add(newVertex1);
            verticesList.Add(newVertex2);
            newPoints.Add(newVertex1);
            newPoints.Add(newVertex2);
            normalsList.Add(normals[triangles[triangleIndex]]);
            normalsList.Add(normals[triangles[triangleIndex]]);
            
            if(pointsOnTriangle.ToList().IndexOf(posSide[0])==0 || pointsOnTriangle.ToList().IndexOf(posSide[0]) == 2)
            {   //one triangle on positive side
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[0]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex2));
                //two triangles on the negative side
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[0]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[1]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex1));
                //
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[1]));
            }
            else
            {
                //one triangle on positive side
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[0]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex1));
                //two triangles on the negative side
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[1]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[0]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex2));
                //
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[0]));

            }
        }

        if(negSide.Count==1)
        {
            for(int i = 0; i < posSide.Count; i++)
            {
                float distance;
                Vector3 direction = (posSide[i] - negSide[0]).normalized;
                Ray ray = new Ray(negSide[0], direction);
                planeConstruct.Raycast(ray, out distance);
                if (i == 0) newVertex1 = negSide[0] + (direction * distance);
                if (i == 1) newVertex2 = negSide[0] + (direction * distance);
            }
            verticesList.Add(newVertex1);
            verticesList.Add(newVertex2);
            newPoints.Add(newVertex1);
            newPoints.Add(newVertex2);
            normalsList.Add(normals[triangles[triangleIndex]]);
            normalsList.Add(normals[triangles[triangleIndex]]);

            if (pointsOnTriangle.ToList().IndexOf(negSide[0])==0 || pointsOnTriangle.ToList().IndexOf(negSide[0]) == 2)
            {
                //one triangle on negative side
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[0]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex2));
                //two triangles on the positive side
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[0]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[1]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex1));
                //
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[1]));
            }
            else
            {
                //one triangle on negative side
                trianglesOnNegativeSide.Add(verticesList.IndexOf(negSide[0]));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnNegativeSide.Add(verticesList.IndexOf(newVertex1));
                //two triangles on the positive side
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[1]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[0]));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex2));
                //
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex1));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(newVertex2));
                trianglesOnPositiveSide.Add(verticesList.IndexOf(posSide[0]));
            }
        }
    }

    void CreateNewFaceTriangles()
    {
        for(int i = 1; i < newPoints.Count-1; i++)
        {
            trianglesOnPositiveSide.Add(verticesList.IndexOf(newPoints[0]));
            trianglesOnPositiveSide.Add(verticesList.IndexOf(newPoints[i+1]));
            trianglesOnPositiveSide.Add(verticesList.IndexOf(newPoints[i]));
            //
            trianglesOnNegativeSide.Add(verticesList.IndexOf(newPoints[0]));
            trianglesOnNegativeSide.Add(verticesList.IndexOf(newPoints[i]));
            trianglesOnNegativeSide.Add(verticesList.IndexOf(newPoints[i+1]));
        }
    }

    void DrawPointsOnSidesOfPlane()
    {
        for (int i = 0; i < trianglesOnPositiveSide.Count; i += 3)
        {
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i + 1]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i + 1]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i + 2]]), this.transform.TransformPoint(vertices[trianglesOnPositiveSide[i + 2]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.green);
        }
        for (int i = 0; i < trianglesOnNegativeSide.Count; i += 3)
        {
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 1]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 1]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
            Debug.DrawLine(this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 2]]), this.transform.TransformPoint(vertices[trianglesOnNegativeSide[i + 2]]) + new Vector3(0.03f, 0.03f, 0.03f), Color.yellow);
        }
    }
}
