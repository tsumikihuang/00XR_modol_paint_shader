
using UnityEngine;
using System.Collections.Generic;

namespace UChart
{
    public class Grid3D : Grid
    {
        public Color mainColor = new Color(0,0,0,0);

        public Color matchColor = Color.gray;

        public override void Draw()
        {
            Vector3 start = new Vector3(-gridSize / 2.0f,0,-gridSize / 2.0f);
            float cellSize = gridSize / division;
            float childSize = cellSize / division;

            var meshFilter = myGameobject.AddComponent<MeshFilter>();
            var meshRenderer = myGameobject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = "__GRID3D__";

            var verticesCount = (division + 1) * 4 + division * (division -1) * 4;
            Vector3[] vertices = new Vector3[ verticesCount];
            int[] indices = new int[verticesCount];
            Color[] colors = new Color[verticesCount];
            
            Vector2 level1 = new Vector2(0,0);
            Vector2 level2 = new Vector2(1,0);

            int vertexIndex = 0;
            for( int i = 0 ; i <= division ;i++ )
            {
                colors[vertexIndex] = mainColor;
                vertices[vertexIndex++] = start + new Vector3(cellSize * i ,0,0);
                colors[vertexIndex] = mainColor;
                vertices[vertexIndex++] = start + new Vector3(cellSize * i ,0,gridSize);

                if( i < division )
                {
                    for( int childIndex = 1 ; childIndex < division ; childIndex++ )
                    {
                        colors[vertexIndex] = matchColor;
                        vertices[vertexIndex++] = start + new Vector3(cellSize * i + childSize * childIndex,0,0);
                        colors[vertexIndex] = matchColor;
                        vertices[vertexIndex++] = start + new Vector3(cellSize * i + childSize * childIndex,0,gridSize);
                    }
                }
            }
            for( int j = 0 ; j <= division;j++ )
            {
                colors[vertexIndex] = mainColor;
                vertices[vertexIndex++] = start + new Vector3(0 ,0,cellSize * j);
                colors[vertexIndex] = mainColor;
                vertices[vertexIndex++] = start + new Vector3(gridSize ,0,cellSize * j);
                if( j < division )
                {
                    for( int childIndex = 1 ; childIndex < division ; childIndex++ )
                    {
                        colors[vertexIndex] = matchColor;
                        vertices[vertexIndex++] = start + new Vector3(0 ,0,cellSize * j + childSize * childIndex);
                        colors[vertexIndex] = matchColor;
                        vertices[vertexIndex++] = start + new Vector3(gridSize ,0,cellSize * j  + childSize * childIndex);
                    }
                }
            }

            // vertices
            mesh.vertices = vertices;
            // indices
            for( int i = 0 ; i < indices.Length ; i++ )
            {
                indices[i] = i;
            }
            mesh.colors = colors;
            mesh.SetIndices(indices,MeshTopology.Lines,0);
            meshFilter.mesh = mesh;

            switch(gridType)
            {
                case GridType.Basic:
                    meshRenderer.material = new Material(Shader.Find("UChart/Grid/Grid(Basic)")); 
                break;
                case GridType.AutoAlpha:
                    meshRenderer.material = new Material(Shader.Find("UChart/Grid/Grid"));
                break;
                default:
                    throw new UChartException("can't find this kind of grid shader.");
            }            
            meshRenderer.material.SetColor("_MainColor",mainColor);
            meshRenderer.material.SetColor("_MatchColor",matchColor);
        }
    }
}