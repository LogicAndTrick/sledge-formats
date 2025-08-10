using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.GameData.Objects;

namespace Sledge.Formats.GameData.Tests.Fgd;

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
        var format = new FgdFormat();
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
        var format = new FgdFormat();
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
        var format = new FgdFormat();
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
        var format = new FgdFormat();
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
        var format = new FgdFormat();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        var meta = cls.Dictionaries[0];
        Assert.AreEqual("metadata", meta.Name);
        Assert.AreEqual("Math Counter", meta["entity_tool_name"].Value);
        Assert.AreEqual("Logic", meta["entity_tool_group"].Value);
        Assert.AreEqual("Store a numeric value and perform arithmetic operations on it", meta["entity_tool_tip"].Value);
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
        var format = new FgdFormat();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);

        var p1 = def.Classes[0].Properties[0];
        var p2 = def.Classes[0].Properties[1];
        var p3 = def.Classes[0].Properties[2];

        Assert.IsTrue(p1.Metadata.ContainsKey("report"));

        Assert.AreEqual("0.0", p2.Metadata["min"].Value);
        Assert.AreEqual("180.0", p2.Metadata["max"].Value);
        Assert.AreEqual("Test Group", p3.Metadata["group"].Value);
    }

    [TestMethod]
    public void TestModelAnimEvent()
    {
        const string fgd = @"
@ModelAnimEvent duration_info { LengthSeconds=""Length"" Speed=""Speed"" Value=""Value"" } = AE_SCRIPT_EVENT_FIRE_INPUT 
[
	input(string) : ""Input""
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        Assert.AreEqual(ClassType.ModelAnimEvent, def.Classes[0].ClassType);

        var durInfo = def.Classes[0].Dictionaries[0];
        Assert.AreEqual("duration_info", durInfo.Name);
        Assert.AreEqual("Length", durInfo["LengthSeconds"].Value);
        Assert.AreEqual("Speed", durInfo["Speed"].Value);
        Assert.AreEqual("Value", durInfo["Value"].Value);

        var p1 = def.Classes[0].Properties[0];

        Assert.AreEqual("input", p1.Name);
        Assert.AreEqual(VariableType.String, p1.VariableType);
        Assert.AreEqual("Input", p1.Description);
    }

    [TestMethod]
    public void TestGenericVariableTypes()
    {
        const string fgd = @"
@PointClass = test
[
    resourceTexture(resource:texture) : ""Resource:Texture""
    arrayStructMapExtension(array:struct:map_extension) : ""Array:Struct:MapExtension""
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);
        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];
        Assert.AreEqual(VariableType.Resource, cls.Properties[0].VariableType);
        Assert.AreEqual("texture", cls.Properties[0].SubType);
        Assert.AreEqual(VariableType.Array, cls.Properties[1].VariableType);
        Assert.AreEqual("struct:map_extension", cls.Properties[1].SubType);
    }

    [TestMethod]
    public void TestNestedDictionaries()
    {
        const string fgd = @"
@ModelBreakCommand
	locator_axis
	{
		transform =
		{
			origin_key = ""anchor_position""
			angles_key = ""anchor_angles""
		}
	}
	physicsjoint_hinge
	{
		transform =
		{
			origin_key = ""anchor_position""
			angles_key = ""anchor_angles""
		}
		enable_limit = ""enable_limit""
		min_angle = ""min_angle""
		max_angle = ""max_angle""
        some_boolean = true
	}
= break_create_joint_revolute : ""Creates a revolute (hinge) joint between two spawned breakpieces""
[]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        var locatorAxis = def.Classes[0].Dictionaries[0];
        var locatorAxisTransform = (GameDataDictionary) locatorAxis["transform"].Value;
        Assert.AreEqual("locator_axis", locatorAxis.Name);
        Assert.AreEqual("transform", locatorAxisTransform.Name);
        Assert.AreEqual("anchor_position", locatorAxisTransform["origin_key"].Value);
        Assert.AreEqual("anchor_angles", locatorAxisTransform["angles_key"].Value);

        var physicsjointHinge = def.Classes[0].Dictionaries[1];
        var physicsjointHingeTransform = (GameDataDictionary) physicsjointHinge["transform"].Value;
        Assert.AreEqual("physicsjoint_hinge", physicsjointHinge.Name);
        Assert.AreEqual("transform", physicsjointHingeTransform.Name);
        Assert.AreEqual("enable_limit", physicsjointHinge["enable_limit"].Value);
        Assert.AreEqual("min_angle", physicsjointHinge["min_angle"].Value);
        Assert.AreEqual("max_angle", physicsjointHinge["max_angle"].Value);
        Assert.AreEqual(true, physicsjointHinge["some_boolean"].Value);
        Assert.AreEqual("anchor_position", physicsjointHingeTransform["origin_key"].Value);
        Assert.AreEqual("anchor_angles", physicsjointHingeTransform["angles_key"].Value);
    }

    [TestMethod]
    public void TestDictionaryInProperties()
    {
        const string fgd = @"@PointClass = light_barn : ""A cinematic barn door style light""
[
	color(color255) { enabled={ variable=""colormode"" value=""0"" } } : ""Color"" : ""255 255 255""
	luminaire_size(float) { group=""Luminaire"" min=""0.0"" max=""90.0"" } : ""Area Light Size"" : ""4"" : ""Area light size, in degrees subtended by a point 100 units away from the near plane.""
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);

        var ent = def.Classes[0];
        Assert.AreEqual(2, ent.Properties.Count);

        var color = ent.Properties[0];
        Assert.AreEqual(1, color.Metadata.Count);
        Assert.IsInstanceOfType(color.Metadata["enabled"].Value, typeof(GameDataDictionary));
        var colorEnabled = (GameDataDictionary)color.Metadata["enabled"].Value;
        Assert.AreEqual(2, colorEnabled.Count);
        Assert.AreEqual("colormode", colorEnabled["variable"].Value);
        Assert.AreEqual("0", colorEnabled["value"].Value);

        var luminaireSize = ent.Properties[1];
        Assert.AreEqual(3, luminaireSize.Metadata.Count);
        Assert.AreEqual("Luminaire", luminaireSize.Metadata["group"].Value);
        Assert.AreEqual("0.0", luminaireSize.Metadata["min"].Value);
        Assert.AreEqual("90.0", luminaireSize.Metadata["max"].Value);
    }

    [TestMethod]
    public void TestArraysInDictionaries()
    {
        const string fgd = @"@PointClass = light_barn : ""A cinematic barn door style light""
[
	fade_size_start(float) { group=""Render"" min=""0.0"" max=""1.0"" enabled={ variable=""directlight"" values=[""2"", ""3""] } } : ""Fade Out Start Size"" : "".05"" : ""Screen size where the light will begin fading out"" 
	fade_size_end(float) { group=""Render"" min=""0.0"" max=""1.0"" enabled={ variable=""directlight"" values=[""2"", ""3""] } } : ""Fade Out End Size"" : "".025"" : ""Screen size where the light will be fully faded out"" 
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);

        var ent = def.Classes[0];
        Assert.AreEqual(2, ent.Properties.Count);

        var fadeSizeStart = ent.Properties[0];
        Assert.AreEqual(4, fadeSizeStart.Metadata.Count);
        Assert.IsInstanceOfType(fadeSizeStart.Metadata["enabled"].Value, typeof(GameDataDictionary));
        var fadeSizeStartEnabled = (GameDataDictionary)fadeSizeStart.Metadata["enabled"].Value;
        Assert.AreEqual(2, fadeSizeStartEnabled.Count);
        Assert.AreEqual("directlight", fadeSizeStartEnabled["variable"].Value);
        Assert.AreEqual(GameDataDictionaryValueType.Array, fadeSizeStartEnabled["values"].Type);
        Assert.AreEqual("2", ((List<GameDataDictionaryValue>)fadeSizeStartEnabled["values"].Value)[0].Value);
        Assert.AreEqual("3", ((List<GameDataDictionaryValue>)fadeSizeStartEnabled["values"].Value)[1].Value);

        var fadeSizeEnd = ent.Properties[1];
        Assert.AreEqual(4, fadeSizeEnd.Metadata.Count);
        Assert.IsInstanceOfType(fadeSizeEnd.Metadata["enabled"].Value, typeof(GameDataDictionary));
        var fadeSizeEndEnabled = (GameDataDictionary)fadeSizeEnd.Metadata["enabled"].Value;
        Assert.AreEqual(2, fadeSizeEndEnabled.Count);
        Assert.AreEqual("directlight", fadeSizeEndEnabled["variable"].Value);
        Assert.AreEqual(GameDataDictionaryValueType.Array, fadeSizeEndEnabled["values"].Type);
        Assert.AreEqual("2", ((List<GameDataDictionaryValue>)fadeSizeEndEnabled["values"].Value)[0].Value);
        Assert.AreEqual("3", ((List<GameDataDictionaryValue>)fadeSizeEndEnabled["values"].Value)[1].Value);
    }

    [TestMethod]
    public void TestSlashesAndAmpersandsInSubTypes()
    {
        const string fgd = @"@PointClass = light_barn_and_ai_attached_item_manager : ""Combined from two entities in the dota fgds""
[
	light_style( vdata_choice:scripts/light_styles.vdata ) { group=""Style"" } : ""Style"" : """" : ""Light Style""
	light_style_output_event0(vdata_choice:scripts/light_style_event_types.vdata) { group=""Style"" } : ""Style Output Event 0"" : """" : ""Name of the event that triggers the OnStyleEvent0 output.""
	item_1( vdata_choice:scripts/grenades.vdata&scripts/misc.vdata&scripts/npc_abilities.vdata ) : ""Item Or Ability Subclass 1"" : """" : ""Subclass of item or ability to attach in slot 1.""
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);

        var ent = def.Classes[0];
        Assert.AreEqual(3, ent.Properties.Count);

        Assert.AreEqual(VariableType.VDataChoice, ent.Properties[0].VariableType);
        Assert.AreEqual("scripts/light_styles.vdata", ent.Properties[0].SubType);

        Assert.AreEqual(VariableType.VDataChoice, ent.Properties[1].VariableType);
        Assert.AreEqual("scripts/light_style_event_types.vdata", ent.Properties[1].SubType);

        Assert.AreEqual(VariableType.VDataChoice, ent.Properties[2].VariableType);
        Assert.AreEqual("scripts/grenades.vdata&scripts/misc.vdata&scripts/npc_abilities.vdata", ent.Properties[2].SubType);
    }

    [TestMethod]
    public void TestVDataTypes()
    {
        const string fgd = @"@VData = Blessing_t
[
	editorGroupID(string) : ""Editor Group""
]

@VDataDerived base( blessing_editor_guide ) = blessing_editor_guide_circle
[
	editorGroupID(string) : ""Editor Group""
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(2, def.Classes.Count);
        Assert.AreEqual(ClassType.VData, def.Classes[0].ClassType);
        Assert.AreEqual(ClassType.VDataDerived, def.Classes[1].ClassType);
    }

    [TestMethod]
    public void TestHelpInfoCommand()
    {
        // Found in Dota2Test repo, not sure if used
        const string fgd = @"
@helpinfo( ""ai_basenpc"", ""tools/help/fgd/ai_basenpc.txt"" )
";
        var format = new FgdFormat();
        var def = format.Read(fgd);
    }

    [TestMethod]
    public void TestVisgroupFilterCommand()
    {
        const string fgd = @"
@VisGroupFilter { filter_type = ""toolsMaterial""		material = ""toolsclip.vmat""				group =	""Clip""				parent_group = ""Tool Brushes"" }

@VisGroupFilter { filter_type = ""entityTag""		tag = ""Lighting""		group = ""Lighting""			parent_group = ""Entities"" }
";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(2, def.VisgroupFilters.Count);

        Assert.AreEqual("toolsMaterial", def.VisgroupFilters[0].FilterType);
        Assert.AreEqual("toolsclip.vmat", def.VisgroupFilters[0].Material);
        Assert.AreEqual("Clip", def.VisgroupFilters[0].Group);
        Assert.AreEqual("Tool Brushes", def.VisgroupFilters[0].ParentGroup);

        Assert.AreEqual("entityTag", def.VisgroupFilters[1].FilterType);
        Assert.AreEqual("Lighting", def.VisgroupFilters[1].Tag);
        Assert.AreEqual("Lighting", def.VisgroupFilters[1].Group);
        Assert.AreEqual("Entities", def.VisgroupFilters[1].ParentGroup);
    }

    [TestMethod]
    public void TestDictionaryInMetadataArray()
    {
        const string fgd = @"
@OverrideClass
	metadata
	{
		class_game_keys =
		[
			{ key = ""clientSideEntity"" value = 1 }
		]
	} = light_environment [ ]
";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);
        var cls = def.Classes[0];

        Assert.AreEqual(1, cls.Dictionaries.Count);
        var meta = cls.Dictionaries[0];

        Assert.AreEqual("metadata", meta.Name);
        Assert.AreEqual(1, meta.Count);
        Assert.AreEqual("class_game_keys", meta.Keys.First());
        var cgk = meta.Values.First();

        Assert.AreEqual(GameDataDictionaryValueType.Array, cgk.Type);
        Assert.IsInstanceOfType(cgk.Value, typeof(List<GameDataDictionaryValue>));
        var arr = (List<GameDataDictionaryValue>)cgk.Value;

        Assert.AreEqual(1, arr.Count);
        var dict = arr[0];

        Assert.AreEqual(GameDataDictionaryValueType.Dictionary, dict.Type);
        Assert.IsInstanceOfType(dict.Value, typeof(GameDataDictionary));
        var dv = (GameDataDictionary)dict.Value;
        
        Assert.AreEqual(2, dv.Count);
        Assert.AreEqual("clientSideEntity", dv["key"].Value);
        Assert.AreEqual(1m, dv["value"].Value);
    }

    [TestMethod]
    public void TestMetadataInInputOutput()
    {
        const string fgd = @"
@BaseClass = Example
[
	output ExampleOutput(void) { is_some_test = true } : ""This is a test""
    input ExampleInput(void) { another_test = 1 }
]";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);

        var ent = def.Classes[0];
        Assert.AreEqual(2, ent.InOuts.Count);

        var out1 = ent.InOuts.Single(x => x.IOType == IOType.Output);
        var in1 = ent.InOuts.Single(x => x.IOType == IOType.Input);

        Assert.AreEqual(1, out1.Metadata.Count);
        Assert.AreEqual(true, out1.Metadata["is_some_test"].Value);

        Assert.AreEqual(1, in1.Metadata.Count);
        Assert.AreEqual(1m, in1.Metadata["another_test"].Value);
    }

    [TestMethod]
    public void TestMetadataDictionaryWithNegativeValue()
    {
        const string fgd = @"@PointClass metadata { view_attach_offset = [ -10.0, 0.0, 0.0 ] } = test : ""test"" []";
        var format = new FgdFormat();
        var def = format.Read(fgd);

        Assert.AreEqual(1, def.Classes.Count);

        var ent = def.Classes[0];
        Assert.AreEqual("test", ent.Name);
        Assert.AreEqual(1, ent.Dictionaries.Count);

        var meta = ent.Dictionaries[0];
        var vao = meta["view_attach_offset"];
        Assert.AreEqual(GameDataDictionaryValueType.Array, vao.Type);
        Assert.IsInstanceOfType<List<GameDataDictionaryValue>>(vao.Value);

        var values = (List<GameDataDictionaryValue>)vao.Value;
        Assert.AreEqual(3, values.Count);
        Assert.AreEqual(-10m, values[0].Value);
        Assert.AreEqual(0m, values[1].Value);
        Assert.AreEqual(0m, values[2].Value);
    }
}