using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.GameData.Objects;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Tests.Fgd;

[TestClass]
public class TestFgdBasic
{
    [TestMethod]
    public void TestIncludeWithNoFileResolver()
    {
        const string fgd = @"
@include ""test.fgd""
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        CollectionAssert.AreEqual(new [] { "test.fgd" }, def.Includes);
    }

    [TestMethod]
    public void TestMapSize()
    {
        const string fgd = @"
@mapsize(-16384, 16384)
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(-16384, def.MapSizeLow);
        Assert.AreEqual(16384, def.MapSizeHigh);
    }

    [TestMethod]
    public void TestMaterialExclusion()
    {
        const string fgd = @"
@MaterialExclusion
[
    ""one""
    ""two""
    ""three""
]
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        CollectionAssert.AreEquivalent(new [] { "one", "two", "three" }, def.MaterialExclusions);
    }

    [TestMethod]
    public void TestAutoVisGroup()
    {
        const string fgd = @"
@AutoVisGroup = ""Auto""
[
    ""VisOne""
    [
        ""test_ent_one""
        ""test_ent_two""
    ]

    ""VisTwo""
    [
        ""test_ent_two""
    ]
]
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.AutoVisgroups.Count);
        var vg = def.AutoVisgroups[0];
        Assert.AreEqual("Auto", vg.Name);
        Assert.AreEqual(2, vg.Groups.Count);
        var v1 = vg.Groups.Find(x => x.Name == "VisOne");
        var v2 = vg.Groups.Find(x => x.Name == "VisTwo");
        Assert.IsNotNull(v1);
        Assert.IsNotNull(v2);
        CollectionAssert.AreEquivalent(new [] { "test_ent_one", "test_ent_two" }, v1.EntityNames);
        CollectionAssert.AreEquivalent(new [] { "test_ent_two" }, v2.EntityNames);
    }

    [TestMethod]
    public void TestEmptyClass()
    {
        const string fgd = @"
@BaseClass = Test []
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual("Test", cls.Name);
        Assert.AreEqual(ClassType.BaseClass, cls.ClassType);
        Assert.AreEqual("", cls.AdditionalInformation);
        Assert.AreEqual("", cls.Description);
        Assert.IsFalse(cls.BaseClasses.Any());
        Assert.IsFalse(cls.Behaviours.Any());
        Assert.IsFalse(cls.InOuts.Any());
        Assert.IsFalse(cls.Properties.Any());
    }

    [TestMethod]
    public void TestBaseAndBehaviours()
    {
        const string fgd = @"
@BaseClass base(Appearflags, Angles) size(-16 -16 -36, 16 16 36) color(0 255 0) halfgridsnap = PlayerClass []
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual("PlayerClass", cls.Name);
        Assert.AreEqual(ClassType.BaseClass, cls.ClassType);
        Assert.AreEqual("", cls.AdditionalInformation);
        Assert.AreEqual("", cls.Description);

        Assert.AreEqual(2, cls.BaseClasses.Count);
        CollectionAssert.AreEqual(new [] { "Appearflags", "Angles" }, cls.BaseClasses);

        Assert.AreEqual(3, cls.Behaviours.Count);

        Assert.AreEqual("size", cls.Behaviours[0].Name);
        CollectionAssert.AreEqual(new[] { "-16", "-16", "-36", "16", "16", "36" }, cls.Behaviours[0].Values);

        Assert.AreEqual("color", cls.Behaviours[1].Name);
        CollectionAssert.AreEqual(new[] { "0", "255", "0" }, cls.Behaviours[1].Values);

        Assert.AreEqual("halfgridsnap", cls.Behaviours[2].Name);
        Assert.AreEqual(0, cls.Behaviours[2].Values.Count);

        Assert.IsFalse(cls.InOuts.Any());
        Assert.IsFalse(cls.Properties.Any());
    }

    [TestMethod]
    public void TestDescription()
    {
        const string fgd = @"
@BaseClass = Test : ""Descr"" + ""iption""  []
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual("Test", cls.Name);
        Assert.AreEqual(ClassType.BaseClass, cls.ClassType);
        Assert.AreEqual("Description", cls.Description);
        Assert.AreEqual("", cls.AdditionalInformation);
        Assert.IsFalse(cls.BaseClasses.Any());
        Assert.IsFalse(cls.Behaviours.Any());
        Assert.IsFalse(cls.InOuts.Any());
        Assert.IsFalse(cls.Properties.Any());
    }

    [TestMethod]
    public void TestDescriptionAndInformation()
    {
        const string fgd = @"
@BaseClass = Test : ""Descr"" + ""iption"" : ""Additional information""  []
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual("Test", cls.Name);
        Assert.AreEqual(ClassType.BaseClass, cls.ClassType);
        Assert.AreEqual("Description", cls.Description);
        Assert.AreEqual("Additional information", cls.AdditionalInformation);
        Assert.IsFalse(cls.BaseClasses.Any());
        Assert.IsFalse(cls.Behaviours.Any());
        Assert.IsFalse(cls.InOuts.Any());
        Assert.IsFalse(cls.Properties.Any());
    }

    [TestMethod]
    public void TestNoClassBody()
    {
        const string fgd = @"
@BaseClass = Test1 : ""A""+""B""+""C""
@BaseClass = Test2
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(2, def.Classes.Count);
        var cls1 = def.Classes[0];
        Assert.AreEqual("Test1", cls1.Name);
        var cls2 = def.Classes[1];
        Assert.AreEqual("Test2", cls2.Name);
    }

    [TestMethod]
    public void TestProperties()
    {
        const string fgd = @"
@BaseClass = Test [
    one(integer)
    two(string) : ""desc"" + ""ription""
    three(string) : ""desc"" : ""def"" + ""ault""
    four(color255) readonly
    five(string) report : ""desc"" : ""def""
    six(string) : ""d"" : -123 : ""details""
    seven(string) : ""d"" :  : ""details""
    eight(choices) : ""eight"" : 0 =
    [
        0: ""c"" + ""1""
        1: ""c2"" : ""det"" + ""ails""
    ]
    spawnflags(flags) = 
    [
        1 : ""flag1"" : 0
        2 : ""flag2"" : 1
    ]
    nine(string) report readonly
    ten(string) readonly report
]
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual("Test", cls.Name);

        Assert.AreEqual(11, cls.Properties.Count);

        AssertProperty(cls.Properties[0], "one", VariableType.Integer, "", "", "", false, false);
        AssertProperty(cls.Properties[1], "two", VariableType.String, "description", "", "", false, false);
        AssertProperty(cls.Properties[2], "three", VariableType.String, "desc", "default", "", false, false);
        AssertProperty(cls.Properties[3], "four", VariableType.Color255, "", "", "", true, false);
        AssertProperty(cls.Properties[4], "five", VariableType.String, "desc", "def", "", false, true);
        AssertProperty(cls.Properties[5], "six", VariableType.String, "d", "-123", "details", false, false);
        AssertProperty(cls.Properties[6], "seven", VariableType.String, "d", "", "details", false, false);
        AssertProperty(cls.Properties[7], "eight", VariableType.Choices, "eight", "0", "", false, false);
        AssertProperty(cls.Properties[8], "spawnflags", VariableType.Flags, "", "", "", false, false);
        AssertProperty(cls.Properties[9], "nine", VariableType.String, "", "", "", true, true);
        AssertProperty(cls.Properties[10], "ten", VariableType.String, "", "", "", true, true);

        AssertOption(cls.Properties[7].Options[0], "0", "c1", false, "");
        AssertOption(cls.Properties[7].Options[1], "1", "c2", false, "details");

        AssertOption(cls.Properties[8].Options[0], "1", "flag1", false, "");
        AssertOption(cls.Properties[8].Options[1], "2", "flag2", true, "");

        void AssertProperty(Property actual, string name, VariableType type, string desc, string dfault, string details, bool ro, bool report)
        {
            Assert.AreEqual(name, actual.Name);
            Assert.AreEqual(type, actual.VariableType);
            Assert.AreEqual(desc, actual.Description);
            Assert.AreEqual(dfault, actual.DefaultValue);
            Assert.AreEqual(details, actual.Details);
            Assert.AreEqual(ro, actual.Metadata.ContainsKey("readonly"));
            Assert.AreEqual(report, actual.Metadata.ContainsKey("report"));
        }

        void AssertOption(Option actual, string key, string desc, bool on, string details)
        {
            Assert.AreEqual(key, actual.Key);
            Assert.AreEqual(desc, actual.Description);
            Assert.AreEqual(on, actual.On);
            Assert.AreEqual(details, actual.Details);
        }
    }

    [TestMethod]
    public void TestDecimalDefault()
    {
        const string fgd = @"
@PointClass = test
[
	test(integer) : ""Description"" : 1.5
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
    }

    [TestMethod]
    public void TestHyphenatedProperties()
    {
        const string fgd = @"
@PointClass = test
[
	test0-test1(integer) : ""Test""
]";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual("test0-test1", def.Classes[0].Properties[0].Name);
    }

    [TestMethod]
    public void TestAllowNewlinesInStrings()
    {
        const string fgd = @"
@PointClass = test
[
	test(integer) : ""New
Line""
]";
        var format = new FgdFormatter { AllowNewlinesInStrings = true };
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual("New\nLine", def.Classes[0].Properties[0].Description);

        Assert.ThrowsException<TokenParsingException>(() =>
        {
            var format2 = new FgdFormatter { AllowNewlinesInStrings = false };
            var def2 = format2.Read(fgd);
        });
    }

    [TestMethod]
    public void TestPreamble()
    {
        const string fgd = @"
// @MESS REWRITE:
// ""classname"": ""macro_insert""
// ""template_map"": ""{dir()}\monster_warp.rmf""
// @MESS;
@PointClass = test
[
	test(integer) : ""Test""
]";
        const string expectedPreamble = @"
@MESS REWRITE:
""classname"": ""macro_insert""
""template_map"": ""{dir()}\monster_warp.rmf""
@MESS;
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(expectedPreamble.Trim().Replace("\r", ""), string.Join("", def.Classes[0].Preamble.Select(x => x.Value.Trim(' '))).Trim());
    }

    [TestMethod]
    public void TestRealWorld()
    {
        const string fgd = @"
//
// worldspawn
//

@SolidClass = worldspawn : ""World entity""
[
	message(string) : ""Map Description / Title""
	skyname(string) : ""environment map (cl_skyname)""
	sounds(integer) : ""CD track to play"" : 1
	light(integer) : ""Default light level""
	WaveHeight(string) : ""Default Wave Height""
	MaxRange(string) : ""Max viewable distance"" : ""4096""
	chaptertitle(string) : ""Chapter Title Message""
	startdark(choices) : ""Level Fade In"" : 0 =
	[	
		0 : ""No""
		1 : ""Yes""
	]
	gametitle(choices) : ""Display game title"" : 0 = 
	[	
		0 : ""No""
		1 : ""Yes""
	]
	newunit(choices) : ""New Level Unit"" : 0 = 
	[
		0 : ""No, keep current""
		1 : ""Yes, clear previous levels""
	]
	mapteams(string) : ""Map Team List""
	defaultteam(choices) : ""Default Team"" : 0 = 
	[
		0 : ""Fewest Players""
		1 : ""First Team""
	]
]

//
// BaseClasses
//

@BaseClass = ZHLT
[
	zhlt_lightflags(choices) : ""ZHLT Lightflags"" : 0 =
	[
		0 : ""Default""
		1 : ""Embedded Fix""
		2 : ""Opaque (blocks light)""
		3 : ""Opaque + Embedded fix""
		6 : ""Opaque + Concave Fix""
	]
	light_origin(string) : ""Light Origin Target""
]

@BaseClass = ZHLT_point
[
	_fade(string) : ""ZHLT Fade"" : ""1.0""
	_falloff(choices) : ""ZHLT Falloff"" : 0 =
	[
		0 : ""Default""
		1 : ""Inverse Linear""
		2 : ""Inverse Square""
	]
]
";
        var format = new FgdFormatter();
        var def = format.Read(fgd);
        Assert.AreEqual(3, def.Classes.Count);
    }
}