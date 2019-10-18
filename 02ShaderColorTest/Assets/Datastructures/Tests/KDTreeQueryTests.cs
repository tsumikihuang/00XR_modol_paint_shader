using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructures.ViliWonka.Tests {

    using KDTree;
    
    public class KDTreeQueryTests : MonoBehaviour {

        public int K = 13;

        [Range(0f, 100f)]
        public float Radius = 0.1f;

        public bool DrawQueryNodes = true;

        Vector3[] vertices;
        KDTree tree;

        KDQuery query;

        public GameObject model;

        void Awake() {
            MeshCollider meshCollider = model.GetComponent<MeshCollider>();
            if (meshCollider == null || meshCollider.sharedMesh == null)
            {
                Debug.LogError("there is no meshCollider or sharedMesh");
                return;
            }
            vertices = meshCollider.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = model.GetComponent<Collider>().transform.TransformPoint(vertices[i]);
            }

            query = new KDQuery();
            
            tree = new KDTree(vertices, 32);
        }


        void Update() {
            //tree.Rebuild();
        }

        private void OnDrawGizmos() {

            if(query == null) {
                return;
            }

            Vector3 size = 1.0f * Vector3.one;

            for(int i = 0; i < vertices.Length; i++) {

                Gizmos.DrawCube(vertices[i], size);
            }

            var resultIndices = new List<int>();

            Color markColor = Color.red;
            markColor.a = 0.5f;
            Gizmos.color = markColor;

            /////////////////////////////////////////////////////////////////////////////////////Radius
            query.Radius(tree, transform.position, Radius, resultIndices);
            Gizmos.DrawWireSphere(transform.position, Radius);

            /////////////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < resultIndices.Count; i++) {

                Gizmos.DrawCube(vertices[resultIndices[i]], 2f * size);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, 4f * size);

            if(DrawQueryNodes) {
                query.DrawLastQuery();
            }
        }
    }
}