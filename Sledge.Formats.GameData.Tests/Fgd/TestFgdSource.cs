using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.GameData.Objects;

namespace Sledge.Formats.GameData.Tests.Fgd
{
    [TestClass]
    public class TestFgdSource
    {
        [TestMethod]
        public void TestInputOutput()
        {
            const string fgd = @"
@BaseClass = Test
[
    input input_thing(integer)
	output output_thing(void) : ""Output thing""
]
";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(1, def.Classes.Count);
            var cls = def.Classes[0];
            Assert.AreEqual(2, cls.InOuts.Count);

            var io1 = cls.InOuts[0];
            Assert.AreEqual(IOType.Input, io1.IOType);
            Assert.AreEqual("input_thing", io1.Name);
            Assert.AreEqual(VariableType.Integer, io1.VariableType);
            Assert.AreEqual("", io1.Description);

            var io2 = cls.InOuts[1];
            Assert.AreEqual(IOType.Output, io2.IOType);
            Assert.AreEqual("output_thing", io2.Name);
            Assert.AreEqual(VariableType.Void, io2.VariableType);
            Assert.AreEqual("Output thing", io2.Description);
        }

        [TestMethod]
        public void TestGridNav()
        {
            // not sure what these values actually are
            const string fgd = @"
@gridnav(64, 32, 32, 16384)
";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            CollectionAssert.AreEqual(new[] { 64, 32, 32, 16384 }, def.GridNavValues);
        }

        [TestMethod]
        public void TestExclude()
        {
            const string fgd = @"
@PointClass = test
@exclude test
";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(0, def.Classes.Count);
        }

        [TestMethod]
        public void TestEntityGroup()
        {
            const string fgd = @"
@EntityGroup ""NPCs"" { start_expanded = true }
@EntityGroup ""Items""
";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(2, def.EntityGroups.Count);
            Assert.AreEqual("NPCs", def.EntityGroups[0].Name);
            Assert.AreEqual(true, def.EntityGroups[0].StartExpanded);
            Assert.AreEqual("Items", def.EntityGroups[1].Name);
            Assert.AreEqual(false, def.EntityGroups[1].StartExpanded);
        }

        [TestMethod]
        public void TestClassMetadata()
        {
            const string fgd = @"
@PointClass base(Targetname, EnableDisable) iconsprite(""editor/math_counter.vmat"") tags( Logic )
	metadata
	{
		entity_tool_name = ""Math Counter""
		entity_tool_group = ""Logic""
		entity_tool_tip = ""Store a numeric value and perform arithmetic operations on it""
	}
= math_counter :
	""Holds a numeric value and performs arithmetic operations upon it. If either the minimum or maximum "" +
	""legal value is nonzero, OutValue will be clamped to the legal range, and the OnHitMin/OnHitMax "" +
	""outputs will be fired at the appropriate times. If both min and max are set to zero, no clamping is "" +
	""performed and only the OutValue output will be fired.""
[
]";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(1, def.Classes.Count);
            var cls = def.Classes[0];
            Assert.AreEqual("Math Counter", cls.Metadata["entity_tool_name"]);
            Assert.AreEqual("Logic", cls.Metadata["entity_tool_group"]);
            Assert.AreEqual("Store a numeric value and perform arithmetic operations on it", cls.Metadata["entity_tool_tip"]);
        }

        [TestMethod]
        public void TestPropertyMetadata()
        {
            const string fgd = @"
@BaseClass = Test
[
    test1(studio) [report] : ""Test1""
    test2(float) [ min=""0.0"", max=""180.0"" ] : ""Test2"" : ""0"" : ""Test#2""
    test3(boolean) [ group=""Test Group"" ] : ""Test3"" : 0 : ""Test#3""
]";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(1, def.Classes.Count);

            var p1 = def.Classes[0].Properties[0];
            var p2 = def.Classes[0].Properties[1];
            var p3 = def.Classes[0].Properties[2];

            Assert.IsTrue(p1.Metadata.ContainsKey("report"));

            Assert.AreEqual("0.0", p2.Metadata["min"]);
            Assert.AreEqual("180.0", p2.Metadata["max"]);
            Assert.AreEqual("Test Group", p3.Metadata["group"]);
        }

        [TestMethod]
        public void TestModelAnimEvent()
        {
            const string fgd = @"
@ModelAnimEvent duration_info { LengthSeconds=""Length"" Speed=""Speed"" Value=""Value"" } = AE_SCRIPT_EVENT_FIRE_INPUT 
[
	input(string) : ""Input""
]";
            var format = new FgdFormatter();
            var def = format.Read(fgd);
            Assert.AreEqual(1, def.ModelDataClasses.Count);
            Assert.AreEqual(ClassType.ModelAnimEvent, def.ModelDataClasses[0].ClassType);
            Assert.AreEqual("Length", def.ModelDataClasses[0].ModelDurationInfo["LengthSeconds"]);
            Assert.AreEqual("Length", def.ModelDataClasses[0].ModelDurationInfo["LengthSeconds"]);
            Assert.AreEqual("Speed", def.ModelDataClasses[0].ModelDurationInfo["Speed"]);
            Assert.AreEqual("Value", def.ModelDataClasses[0].ModelDurationInfo["Value"]);

            var p1 = def.ModelDataClasses[0].Properties[0];

            Assert.AreEqual("input", p1.Name);
            Assert.AreEqual(VariableType.String, p1.VariableType);
            Assert.AreEqual("Input", p1.Description);
        }
    }
}
