using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.GameData.Objects;
using Sledge.Formats.Precision;

namespace Sledge.Formats.GameData.Tests.Def;

[TestClass]
public class TestDefBasic
{
    private static GameDefinition Parse(string def)
    {
        var df = new DefFormat();
        return df.Read(def);
    }

    [TestMethod]
    public void TestEmpty()
    {
        var def = Parse("");
        Assert.AreEqual(0, def.Classes.Count);

        def = Parse("\n\n");
        Assert.AreEqual(0, def.Classes.Count);
    }

    [TestMethod]
    public void TestComments()
    {
        const string text = @"//hello";
        var def = Parse(text);
        Assert.AreEqual(0, def.Classes.Count);
    }

    [TestMethod]
    public void TestBasic()
    {
        const string text = @"
/*QUAKED some_thing
*/
";
        var def = Parse(text);
        Assert.AreEqual(1, def.Classes.Count);

        var cls = def.Classes[0];
        Assert.AreEqual("some_thing", cls.Name);
    }

    [TestMethod]
    public void TestSolid()
    {
        const string text = @"
/*QUAKED func_solid (0 .1 .2) ? FLAG1 FLAG2
description
*/
";
        var def = Parse(text);
        Assert.AreEqual(1, def.Classes.Count);

        var cls = def.Classes[0];
        Assert.AreEqual("func_solid", cls.Name);
        Assert.AreEqual("description", cls.Description);
        Assert.AreEqual(1, cls.Behaviours.Count);
        
        var col = cls.Behaviours[0];
        Assert.AreEqual(Color.FromArgb(255, 0, Convert.ToInt32(0.1d * 255), Convert.ToInt32(0.2d * 255)), col.GetColour(0));

        Assert.AreEqual(1, cls.Properties.Count);
        var flg = cls.Properties[0];
        Assert.AreEqual("spawnflags", flg.Name);
        Assert.AreEqual(2, flg.Options.Count);
        Assert.AreEqual("1", flg.Options[0].Key);
        Assert.AreEqual("FLAG1", flg.Options[0].Description);
        Assert.AreEqual("2", flg.Options[1].Key);
        Assert.AreEqual("FLAG2", flg.Options[1].Description);
    }

    [TestMethod]
    public void TestPoint()
    {
        const string text = @"
/*QUAKED weapon_shotgun (0 .5 .8) (-16 -16 0) (16 16 32)
{ model("":progs/g_shotgn.mdl""); }
Shotgun.
*/
";
        var def = Parse(text);
        Assert.AreEqual(1, def.Classes.Count);

        var cls = def.Classes[0];
        Assert.AreEqual("weapon_shotgun", cls.Name);
        Assert.AreEqual("Shotgun.", cls.Description);
        Assert.AreEqual(2, cls.Behaviours.Count);
        
        var col = cls.Behaviours[0];
        Assert.AreEqual(Color.FromArgb(255, 0, (int)(0.5d * 255), (int)(0.8d * 255)), col.GetColour(0));

        var siz = cls.Behaviours[1];
        // todo more assertions
    }

    [TestMethod]
    public void TestMultiple()
    {
        const string text = @"
/*QUAKED weapon_shotgun (0 .5 .8) (-16 -16 0) (16 16 32)
{ model("":progs/g_shotgn.mdl""); }
Shotgun.
*/
/*QUAKED func_solid (0 .1 .2) ? FLAG1 FLAG2
description
*/
";
        var def = Parse(text);
        Assert.AreEqual(2, def.Classes.Count);

        var cls1 = def.Classes[0];
        Assert.AreEqual("weapon_shotgun", cls1.Name);

        var cls2 = def.Classes[0];
        Assert.AreEqual("func_solid", cls2.Name);
    }
}