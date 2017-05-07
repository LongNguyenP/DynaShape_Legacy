//using System;
//using System.Collections.Generic;
//using System.Windows.Media.Animation;
//using Autodesk.DesignScript.Runtime;


//namespace DynaShape.Goals
//{
//   [IsVisibleInDynamoLibrary(false)]
//   public class CyclicQuadGoal : Goal
//   {
//      public CyclicQuadGoal(List<Triple> nodeStartingPositions, float weight = 1f)
//      {
//         if (nodeStartingPositions.Count != 4) throw new Exception("Cyclic-Quad Goal: Node count must be 4");
//         Weight = weight;
//         StartingPositions = nodeStartingPositions.ToArray();
//         Moves = new Triple[StartingPositions.Length];
//      }


//      public override void Compute(List<Node> allNodes)
//      {
//         Triple A = allNodes[NodeIndices[0]].Position;
//         Triple B = allNodes[NodeIndices[1]].Position;
//         Triple C = allNodes[NodeIndices[2]].Position;
//         Triple D = allNodes[NodeIndices[3]].Position;

//         Triple c1, c2, c3, c4, n;
//         float rds;
//         Util.ComputeBestFitCircle(new List<Triple> { A, B, C }, out c1, out n, out rds);
//         Util.ComputeBestFitCircle(new List<Triple> { B, C, D }, out c2, out n, out rds);
//         Util.ComputeBestFitCircle(new List<Triple> { C, D, A }, out c3, out n, out rds);
//         Util.ComputeBestFitCircle(new List<Triple> { D, A, B }, out c4, out n, out rds);

//         Triple center = 0.25f * (c1 + c2 + c3 + c4);

//         float rA = center.DistanceTo(A);
//         float rB = center.DistanceTo(B);
//         float rC = center.DistanceTo(C);
//         float rD = center.DistanceTo(D);
//         float r = 0.25f * (rA + rB + rC + rD);

//         Moves[0] = (1f - r / rA) * (center - A);
//         Moves[1] = (1f - r / rB) * (center - B);
//         Moves[2] = (1f - r / rC) * (center - C);
//         Moves[3] = (1f - r / rD) * (center - D);
//      }


//      internal static Triple ComputeCircleCenter(Triple A, Triple B, Triple C)
//      {
//         Triple t = B - A;
//         Triple u = C - A;
//         Triple v = C - B;

//         Triple n = t.Cross(u);
//         float n2 = n.LengthSquared;

//         return A + (u * t.LengthSquared * u.Dot(v) - t * u.LengthSquared * t.Dot(u)) / (2f * n2);
//      }
//   }
//}
