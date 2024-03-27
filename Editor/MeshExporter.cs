using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace WorldGenerator
{
    public class MeshExporter
    {
        public MeshExporter()
        {

        }

        /// <summary>
        /// Manually export to .obj. This is extremely slow.
        /// </summary>
        public void ExportMesh(Mesh mesh, string path)
        {
            if (!path.ToLower().EndsWith(".obj"))
            {
                Debug.LogError("Invalid file format. File path must end with .obj extension.");
                return;
            }

            StringBuilder sb = new StringBuilder();

            // Vertices
            foreach (Vector3 vertex in mesh.vertices)
            {
                sb.AppendLine(string.Format("v {0} {1} {2}", vertex.x, vertex.y, vertex.z));
            }

            // Normals
            foreach (Vector3 normal in mesh.normals)
            {
                sb.AppendLine(string.Format("vn {0} {1} {2}", normal.x, normal.y, normal.z));
            }

            // UVs
            foreach (Vector2 uv in mesh.uv)
            {
                sb.AppendLine(string.Format("vt {0} {1}", uv.x, uv.y));
            }

            // UV2s
            if (mesh.uv2 != null && mesh.uv2.Length > 0)
            {
                foreach (Vector2 uv2 in mesh.uv2)
                {
                    sb.AppendLine(string.Format("vt2 {0} {1}", uv2.x, uv2.y));
                }
            }

            // Triangles
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                    mesh.triangles[i] + 1, mesh.triangles[i + 1] + 1, mesh.triangles[i + 2] + 1));
            }

            // Write to file
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(sb.ToString());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to export mesh: " + e.Message);
            }
        }
    }
}
