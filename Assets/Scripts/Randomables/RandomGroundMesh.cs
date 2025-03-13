using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class RandomGroundMesh : RandomObject
    {
        public Material Material;

        [Header("Size")] public int vertical;
        public int horizontal;

        public float randomDelta;

        public MeshCollider collider;

        private MeshFilter filter;
        private MeshRenderer renderer;
        private Mesh mesh; // 재사용할 Mesh 객체

        private void Awake()
        {
            filter = gameObject.AddComponent<MeshFilter>();
            renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = Material;

            mesh = new Mesh();
            filter.mesh = mesh;
        }



        public void GenerateShape()
        {
            if (mesh != null)
            {
                mesh.Clear();
            }

            int vertLen = (vertical + 1) * (horizontal + 1);
            Vector3[] vertices = new Vector3[vertLen];
            Vector2[] uvs = new Vector2[vertLen];

            for (int i = 0; i < vertical + 1; i++)
            {
                for (int j = 0; j < horizontal + 1; j++)
                {
                    Vector3 vertex;
                    vertex.x = i - (float)vertical / 2;
                    vertex.y = Random.Range(0, randomDelta);
                    vertex.z = j - (float)horizontal / 2;
                    vertices[i * (horizontal + 1) + j] = vertex;

                    Vector2 uv;
                    uv.x = i % 2;
                    uv.y = j % 2;
                    uvs[i * (horizontal + 1) + j] = uv;
                }
            }

            List<int> triangles = new();
            for (int i = 0; i < vertical; i++)
            {
                int curLine = i * (horizontal + 1);
                for (int j = 0; j < horizontal; j++)
                {
                    triangles.Add(curLine + j);
                    triangles.Add(curLine + j + 1);
                    triangles.Add(curLine + horizontal + 1 + j + 1);

                    triangles.Add(curLine + horizontal + 1 + j + 1);
                    triangles.Add(curLine + horizontal + 1 + j);
                    triangles.Add(curLine + j);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            filter.mesh = mesh;
            if (collider != null)
            {
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
        }

        public override void Rand()
            => GenerateShape();
    }
}
