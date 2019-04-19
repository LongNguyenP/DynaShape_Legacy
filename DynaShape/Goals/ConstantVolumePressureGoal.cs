using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace DynaShape.Goals
{
    [IsVisibleInDynamoLibrary(false)]
    public class ConstantVolumePressureGoal : Goal
    {
        public float VolumePressureConstant;
        public Mesh Mesh;

        private List<int> faces;
        private float currentVolumeInversed;
            
        public ConstantVolumePressureGoal(Mesh mesh, float volumePressureConstant, float weight = 1f)
        {
            try
            {
                currentVolumeInversed = 1f / (float)mesh.Volume;
            }
            catch (Exception)
            {
                throw new Exception("The input mesh is not valid (It must be a closed mesh so that its volume is computable)");
            }

            Mesh = mesh;
            faces = Mesh.VertexIndicesByTri();
            Weight = weight;
            VolumePressureConstant = volumePressureConstant;
            List<Point> vertices = mesh.Vertices();
                StartingPositions = new Triple[mesh.VertexCount];
            for (int i = 0; i < mesh.VertexCount; i++)
                StartingPositions[i] = vertices[i].ToTriple();

            Moves = new Triple[StartingPositions.Length];
            Weights = new float[StartingPositions.Length];
        }


        internal override void Compute(List<Node> allNodes)
        {
            List<Point> vertices = new List<Point>();
            foreach (int i in NodeIndices)
                vertices.Add(allNodes[i].Position.ToPoint());

            Mesh = Mesh.ByVerticesAndIndices(vertices, faces);

            for (int i = 0; i < NodeCount; i++)
                Moves[i] = Triple.Zero;
            
            int faceCount = faces.Count / 3;


            try
            {
                float currentVolume = (float)Mesh.Volume;
                currentVolumeInversed = 1f / currentVolume;
            }
            catch (Exception) 
            {
            }
            

            for (int i = 0; i < faceCount; i++)
            {
                int iA = faces[i * 3 + 0];
                int iB = faces[i * 3 + 1];
                int iC = faces[i * 3 + 2];

                Triple A = allNodes[NodeIndices[iA]].Position;
                Triple B = allNodes[NodeIndices[iB]].Position;
                Triple C = allNodes[NodeIndices[iC]].Position;

                Triple n = (B - A).Cross(C - A);

                Triple force = VolumePressureConstant * currentVolumeInversed * n * 0.16666666666666f;

                Moves[iA] += force;
                Moves[iB] += force;
                Moves[iC] += force;
            }

            Weights.FillArray(Weight);
        }
    }
}
