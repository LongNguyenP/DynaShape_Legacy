// Decompiled with JetBrains decompiler
// Type: FooNodeModel.FooTimer
// Assembly: FooNodeModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9722B888-60DA-41F8-8E46-5257FFA4686B
// Assembly location: D:\Cloud Storage\Mega\FooNodeModel\bin\FooNodeModel.dll

using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using System.Collections.Generic;
using System.Timers;
using Autodesk.DesignScript.Runtime;

namespace FooNodeModel
{
    [NodeName("FooTimer")]
    [NodeDescription("FooTimer")]
    [NodeCategory("FooTimer")]
    [InPortNames("Interval")]
    [InPortTypes("double")]
    [InPortDescriptions("Time interval, in miliseconds")]
    [OutPortNames("Iteration")]
    [OutPortTypes("double")]
    [OutPortDescriptions(("The current iteration count"))]
  
    [IsDesignScriptCompatible]
    public class NmFooTimer : NodeModel
    {
        private int i;
        private double interval;

        public NmFooTimer()
        {
            this.RegisterAllPorts();
        }

        protected override void OnBuilt()
        {
            Timer timer = new Timer(1);
            timer.Elapsed -= TimerOnElapsed;
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }


        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            (sender as Timer).Dispose();
            OnNodeModified(true);
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0), 
                    AstFactory.BuildDoubleNode(i++))
            };
        }
    }
}
