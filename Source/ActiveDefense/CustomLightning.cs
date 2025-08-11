using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace ActiveDefense
{
    public static class LightningBoltMeshMakerCustom
    {
        private static List<Vector2> verts2D;
        private static Vector2 lightningStart;

        private const float VertexInterval = 0.25f;
        private const float MeshWidth = 2f;
        private const float UVIntervalY = 0.04f;
        private const float PerturbAmp = 12f;
        private const float PerturbFreq = 0.007f;

        public static Mesh NewBoltMesh(Vector3 start, Vector3 end)
        {
            lightningStart = new Vector2(start.x,start.y);
            // 1. Вектор и сегменты
            Vector3 dir = end - start;
            float length = dir.magnitude;
            dir.Normalize();

            int segments = (int)Math.Ceiling(length / VertexInterval);
            Vector3 step = dir * VertexInterval;

            // 2. Базовые точки
            List<Vector3> vertsBase = new List<Vector3>();
            Vector3 current = start;
            for (int i = 0; i <= segments; i++)
            {
                vertsBase.Add(current);
                current += step;
            }

            // 3. Пертурбация
            Perlin perlin = new Perlin(PerturbFreq, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);

            List<Vector3> vertsPerturbed = new List<Vector3>();
            // Вычисляем перпендикуляр (направление колебаний)
            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;

            for (int i = 0; i < vertsBase.Count; i++)
            {
                float offset = PerturbAmp * (float)perlin.GetValue(i, 0.0, 0.0);
                vertsPerturbed.Add(vertsBase[i] + perp * offset);
            }

            // 4. Двойные вершины для ширины
            List<Vector3> finalVerts = new List<Vector3>();
            for (int i = 0; i < vertsPerturbed.Count; i++)
            {
                Vector3 sideDir = Vector3.Cross(Vector3.up, dir).normalized; // поперечник для толщины
                Vector3 v1 = vertsPerturbed[i] - sideDir * (MeshWidth / 2f);
                Vector3 v2 = vertsPerturbed[i] + sideDir * (MeshWidth / 2f);
                finalVerts.Add(v1);
                finalVerts.Add(v2);
            }

            // 5. UV
            Vector2[] uvs = new Vector2[finalVerts.Count];
            float uvY = 0f;
            for (int i = 0; i < finalVerts.Count; i += 2)
            {
                uvs[i] = new Vector2(0f, uvY);
                uvs[i + 1] = new Vector2(1f, uvY);
                uvY += UVIntervalY;
            }

            // 6. Треугольники
            int[] tris = new int[(finalVerts.Count - 2) * 3];
            for (int i = 0; i < finalVerts.Count - 2; i += 2)
            {
                int triIndex = i * 3;
                tris[triIndex] = i;
                tris[triIndex + 1] = i + 1;
                tris[triIndex + 2] = i + 2;

                tris[triIndex + 3] = i + 2;
                tris[triIndex + 4] = i + 1;
                tris[triIndex + 5] = i + 3;
            }

            // 7. Mesh
            Mesh mesh = new Mesh
            {
                vertices = finalVerts.ToArray(),
                uv = uvs,
                triangles = tris,
                name = "CustomLightning"
            };
            return mesh;
        }
    }
    public class Thing_LightningBoltTemp : Thing
    {
        private Vector3 start;
        private Vector3 end;
        private int TicksBase = 20;
        private int ticksLeft = 20;
        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

        public void Setup(Vector3 startPos, Vector3 endPos)
        {
            start = startPos;
            end = endPos;
            ticksLeft = 20;
            Log.Message("Ending setup lightning bolt");
        }
        
        protected override void Tick()
        {
            base.Tick();
            float fade = 1 - (1 / TicksBase * (TicksBase - ticksLeft));
            Graphics.DrawMesh(CustomLightningBoltMeshPool.GetRandomBoltMesh(start,end), Vector3.zero, Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, fade), 0);
            Log.Message("Drawed lightning bolt");
            ticksLeft--;
            if (ticksLeft <= 0)
                Destroy(DestroyMode.Vanish);
        }
    }
    public static class CustomLightningBoltMeshPool
    {
        private static List<Mesh> boltMeshes = new List<Mesh>();

        private const int NumBoltMeshesMax = 20;

        public static Mesh GetRandomBoltMesh(Vector3 Start,Vector3 End)
        {
                if (boltMeshes.Count < NumBoltMeshesMax)
                {
                    Mesh mesh = LightningBoltMeshMakerCustom.NewBoltMesh(Start,End);
                    boltMeshes.Add(mesh);
                    return mesh;
                }
                return boltMeshes.RandomElement();
        }
    }
}
