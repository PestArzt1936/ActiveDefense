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
        private static Vector2 lightningEnd;
        private static Vector2 lightningPerpendicular;

        private const float VertexInterval = 0.25f;
        private const float MeshWidth = 2f;
        private const float UVIntervalY = 0.04f;
        private const float PerturbAmp = 3f;
        private const float PerturbFreq = 0.007f;

        private static void CountPerpendicular()
        {
            Vector2 vec = lightningEnd - lightningStart;
            lightningPerpendicular = new Vector2(-vec.y,vec.x);
        }
        public static Mesh NewBoltMesh(Vector3 start, Vector3 end)
        {
            lightningStart = new Vector2(start.x,start.z);
            lightningEnd = new Vector2(end.x,end.z);
            CountPerpendicular();
            MakeVerticesBase();
            PeturbVerticesRandomly();
            DoubleVertices();
            return MeshFromVerts();
        }
        private static void MakeVerticesBase()
        {
            float magn = (lightningEnd - lightningStart).magnitude;
            int num = (int)Math.Ceiling(magn / VertexInterval);
            Vector2 vector = (lightningEnd - lightningStart) / num;
            verts2D = new List<Vector2>();
            Vector2 zero = Vector2.zero;
            for (int i = 0; i < num; i++)
            {
                verts2D.Add(zero);
                zero += vector;
            }
        }
        private static void PeturbVerticesRandomly()
        {
            Perlin perlin = new Perlin(PerturbFreq, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
            List<Vector2> list = verts2D.ListFullCopy();
            // Xa*Xb+Ya*Yb=0
            
            verts2D.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0 || i == list.Count - 1)
                {
                    verts2D.Add(list[i]);
                }
                else
                {
                    //Grade of lightning bolt. Stable "start" and "end" and unstable beautiful middle of bolt
                    float t = (float)i / (list.Count - 1);
                    float localAmp = PerturbAmp * Mathf.Sin(t * Mathf.PI);
                    float num = localAmp * (float)perlin.GetValue(i, 0.0, 0.0);
                    Vector2 item = list[i] + num * lightningPerpendicular.normalized;
                    verts2D.Add(item);
                }
            }
        }
        private static void DoubleVertices()
        {
            List<Vector2> list = verts2D.ListFullCopy();
            Vector2 perp = lightningPerpendicular.normalized * (MeshWidth / 2f);
            verts2D.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Vector2 left = list[i] - perp;
                Vector2 right = list[i] + perp;
                verts2D.Add(left);
                verts2D.Add(right);
            }
        }
        private static Mesh MeshFromVerts()
        {
            Vector3[] array = new Vector3[verts2D.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Vector3(verts2D[i].x, 0f, verts2D[i].y);
            }
            float num = 0f;
            Vector2[] array2 = new Vector2[verts2D.Count];
            for (int j = 0; j < verts2D.Count; j += 2)
            {
                array2[j] = new Vector2(0f, num);
                array2[j + 1] = new Vector2(1f, num);
                if (j < 10)
                    num += UVIntervalY;
                else
                    num -= UVIntervalY;
            }
            int quadCount = (verts2D.Count / 2) - 1;
            int[] tris = new int[quadCount * 6];
            for (int i = 0; i < quadCount; i++)
            {
                int vi = i * 2;
                int ti = i * 6;
                tris[ti] = vi;
                tris[ti + 1] = vi + 1;
                tris[ti + 2] = vi + 2;
                tris[ti + 3] = vi + 2;
                tris[ti + 4] = vi + 1;
                tris[ti + 5] = vi + 3;
            }
            return new Mesh
            {
                vertices = array,
                uv = array2,
                triangles = tris,
                name = "MeshFromVerts()"
            };
        }
    }
    public class Thing_LightningBoltTemp : Thing
    {
        private Vector3 start;
        private Vector3 end;
        private int TicksBase = 20;
        private int ticksLeft = 20;
        CustomLightningBoltMeshPool pool;
        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

        public void Setup(Vector3 startPos, Vector3 endPos)
        {
            pool = new CustomLightningBoltMeshPool();
            start = startPos;
            end = endPos;
            ticksLeft = 20;
        }
        
        protected override void Tick()
        {
            if (ticksLeft != 0)
            {
                base.Tick();
                float fade = 1 - (1 / TicksBase * (TicksBase - ticksLeft));

                //Angle of lightning bolt-_- (Tired of that shit)
                Vector3 dir = end - start;
                dir.y = 0f;
                float angle = Mathf.Atan2(dir.x, dir.z);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);


                Graphics.DrawMesh(pool.GetRandomBoltMesh(start, end), start, rotation, FadedMaterialPool.FadedVersionOf(LightningMat, fade), 0);


                ticksLeft--;
            }
            if (ticksLeft <= 0)
            {
                pool.RefreshList();
                //Destroy(DestroyMode.Vanish);
            }
        }
    }
    public class CustomLightningBoltMeshPool
    {
        private List<Mesh> boltMeshes = new List<Mesh>();

        private const int NumBoltMeshesMax = 1;

        public Mesh GetRandomBoltMesh(Vector3 Start,Vector3 End)
        {
                if (boltMeshes.Count < NumBoltMeshesMax)
                {
                    Mesh mesh = LightningBoltMeshMakerCustom.NewBoltMesh(Start,End);
                    boltMeshes.Add(mesh);
                    return mesh;
                }
            return boltMeshes.RandomElement();
        }
        public void RefreshList()
        {
            if (boltMeshes.Count == NumBoltMeshesMax)
            {
                boltMeshes.Clear();
            }
        }
    }
}
