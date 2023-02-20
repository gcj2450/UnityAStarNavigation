//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using UnityEngine;
//using UnityEngine.AI;
//using static UnityEngine.UI.GridLayoutGroup;
//using UnityEngine.UIElements;

//public class TestSaveObject : MonoBehaviour
//{
//    public GameObject cube;
//    // Start is called before the first frame update
//    void Start()
//    {
//        //NavMeshData navMeshData = NavMeshData.FromNavMeshTriangulation(UnityEngine.AI.NavMesh.CalculateTriangulation());
//        //Debug.Log(navMeshData.vertices.Length + "___" + navMeshData.indices.Length);
//        //PersistObject(Application.dataPath + "/NavMeshTri.data", navMeshData);

//        //for (int i = 0,cnt= cube.GetComponent<MeshFilter>().mesh.vertices.Length; i < cnt; i++)
//        //{
//        //    Vector3 vertex = cube.GetComponent<MeshFilter>().mesh.vertices[i];
//        //  Debug.Log(  cube.transform.localToWorldMatrix.MultiplyPoint3x4(vertex));
//        //}
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyUp(KeyCode.A))
//        {

//            GetComponent<GridManager>().BuildGrid();
//        }
//        if (Input.GetKeyUp(KeyCode.B))
//        {
//            NavMeshData navMeshData = (NavMeshData)LoadObject(Application.dataPath + "/NavMeshTri.data");
//            Debug.Log(navMeshData.indices.Length);
//            NavMeshTriangulation navMeshTriangulation = NavMeshData.ToNavMeshTriangulation(navMeshData);
//            NavMeshExtensions.CalculateEdges(navMeshTriangulation);

//        }
//    }

//    /// <summary>
//    /// Persists the object on HD
//    /// </summary>
//    public void PersistObject(string _filePath,object objectToSave)
//    {
//        Debug.Log("PersistObject: Started");
//        FileStream stream = File.OpenWrite(_filePath);
//        BinaryFormatter formatter = new BinaryFormatter();
//        formatter.Serialize(stream, objectToSave);
//        stream.Close();

//        Debug.Log("PersistObject: Ended");
//    }

//    /// <summary>
//    /// Loads the object.
//    /// </summary>
//    public object LoadObject(string _filePath)
//    {
//        try
//        {
//            Debug.Log("LoadObject: Started");
//            //Open file to read saved DailyUsers object
//            if (File.Exists(_filePath))
//            {
//                FileStream stream = File.OpenRead(_filePath);
//                BinaryFormatter formatter = new BinaryFormatter();

//                object deserializedObject = formatter.Deserialize(stream);
//                stream.Close();
//                Debug.Log("LoadObject: Ended: "+deserializedObject==null);
//                return deserializedObject;
//            }
//            else
//            {
//                Debug.Log("not find");
//                return null;
//            }
            
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.Message);
//            return null;
//        }
//    }

//}

//[System.Serializable]
//public class Vector3Ser
//{
//    public float x, y, z;

//    public static Vector3Ser fromVector3(Vector3 vector3)
//    {
//        Vector3Ser vector3Ser = new Vector3Ser();
//        vector3Ser.x = vector3.x;
//        vector3Ser.y = vector3.y;
//        vector3Ser.z = vector3.z;
//        return vector3Ser;
//    }

//    public static Vector3 ToVector3(Vector3Ser vector3Ser)
//    {
//        Vector3 vector3 = Vector3.zero;
//        vector3.x = vector3Ser.x;
//        vector3.y = vector3Ser.y;
//        vector3.z = vector3Ser.z;
//        return vector3;
//    }
//}

//[Serializable]
//public struct NavMeshData
//{
//    [SerializeField]
//    public Vector3Ser[] vertices;
    
//    [SerializeField]
//    public int[] indices;
    
//    [SerializeField]
//    public int[] areas;

//    public static Vector3Ser[] ToVector3Sers(Vector3[] _vertices)
//    {
//        Vector3Ser[] vectors= new Vector3Ser[_vertices.Length];
//        for (int i = 0,cnt=_vertices.Length; i < cnt; i++)
//        {
//            vectors[i] = Vector3Ser.fromVector3( _vertices[i]);
//        }
//        return vectors;
//    }

//    public static Vector3[] FromVector3Sers(Vector3Ser[] _vector3s)
//    {
//        Vector3[] vectors = new Vector3[_vector3s.Length];
//        for (int i = 0, cnt = _vector3s.Length; i < cnt; i++)
//        {
//            vectors[i] = Vector3Ser.ToVector3(_vector3s[i]);
//        }
//        return vectors;
//    }

//    public static NavMeshData FromNavMeshTriangulation(NavMeshTriangulation navTri)
//    {
//        NavMeshData navMeshData = new NavMeshData();
//        navMeshData.vertices = ToVector3Sers (navTri.vertices);
//        navMeshData.indices = navTri.indices;
//        navMeshData.areas = navTri.areas;
//        return navMeshData;
//    }

//    public static NavMeshTriangulation ToNavMeshTriangulation(NavMeshData navMeshData)
//    {
//        NavMeshTriangulation navTri = new NavMeshTriangulation();
//        navTri.vertices = FromVector3Sers( navMeshData.vertices);
//        navTri.indices = navMeshData.indices;
//        navTri.areas = navMeshData.areas;
//        return navTri;
//    }
//}
