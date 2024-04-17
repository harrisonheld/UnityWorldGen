using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using WorldGenerator;
using UnityEditor;
using System.Threading;
using System.IO;

namespace UnitTests
{
    [TestFixture]

    public class MeshExportTests
    {
        [Test]
        public void ExportObjTest()
        {
            // create a simple mesh
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0)
            };
            mesh.normals = new Vector3[]
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            mesh.uv2 = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 0),
                new Vector2(3, 0)
            };
            mesh.triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

            string path = "Assets/mesh.obj";

            // export it
            MeshExporter exporter = new MeshExporter();
            exporter.ExportMesh(mesh, path);

            // check if the file exists
            Assert.IsTrue(System.IO.File.Exists(path));

            // read all text
            string fileContents = System.IO.File.ReadAllText(path);
            Assert.IsTrue(fileContents.Contains("v 0 0 0"));
            Assert.IsTrue(fileContents.Contains("v 1 0 0"));
            Assert.IsTrue(fileContents.Contains("v 1 1 0"));
            Assert.IsTrue(fileContents.Contains("v 0 1 0"));
            Assert.IsTrue(fileContents.Contains("vn 0 0 1"));
            Assert.IsTrue(fileContents.Contains("vn 0 0 1"));
            Assert.IsTrue(fileContents.Contains("vn 0 0 1"));
            Assert.IsTrue(fileContents.Contains("vn 0 0 1"));
            Assert.IsTrue(fileContents.Contains("vt 0 0"));
            Assert.IsTrue(fileContents.Contains("vt 1 0"));
            Assert.IsTrue(fileContents.Contains("vt 1 1"));
            Assert.IsTrue(fileContents.Contains("vt 0 1"));
            Assert.IsTrue(fileContents.Contains("vt2 0 0"));
            Assert.IsTrue(fileContents.Contains("vt2 1 0"));
            Assert.IsTrue(fileContents.Contains("vt2 2 0"));
            Assert.IsTrue(fileContents.Contains("vt2 3 0"));
            Assert.IsTrue(fileContents.Contains("f 1/1/1 2/2/2 3/3/3"));
            Assert.IsTrue(fileContents.Contains("f 1/1/1 3/3/3 4/4/4"));

            // delete the file
            System.IO.File.Delete("Assets/mesh.obj");
        }

        [Test]
        public void ObjExportErrors()
        {
            Mesh mesh = new Mesh();
            MeshExporter exporter = new MeshExporter();

            exporter.ExportMesh(mesh, "Assets/mesh.png");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Invalid file format. File path must end with .obj extension.");

            exporter.ExportMesh(mesh, "C:/Windows/System32/mesh.obj");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Failed to export mesh: Access to the path \"C:\\Windows\\System32\\mesh.obj\" is denied.");
        }
    }
}