using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Valve;

namespace Sledge.Formats.Tests.Valve
{
    [TestClass]
    public class TestLiblist
    {
        private readonly Stream _libList = new MemoryStream(Encoding.ASCII.GetBytes(@"// Valve Game Info file
//  These are key/value pairs.  Certain mods will use different settings.
//
game ""Half-Life""
startmap ""c0a0""
trainmap ""t0a0""
mpentity ""info_player_deathmatch""
gamedll ""dlls\hl.dll""
gamedll_linux ""dlls/hl.so""
gamedll_osx ""dlls/hl.dylib""
secure ""1""
type ""singleplayer_only""

"));
        [TestMethod]
        public void TestLoading()
        {
            _libList.Position = 0;
            var lib = new Liblist(_libList);
            Assert.AreEqual("Half-Life", lib.Game);
            Assert.AreEqual("c0a0", lib.StartingMap);
            Assert.AreEqual("t0a0", lib.TrainingMap);
            Assert.AreEqual("info_player_deathmatch", lib.MultiplayerEntity);
            Assert.AreEqual("dlls\\hl.dll", lib.GameDll);
            Assert.AreEqual("dlls/hl.so", lib.GameDllLinux);
            Assert.AreEqual("dlls/hl.dylib", lib.GameDllOsx);
            Assert.AreEqual(true, lib.Secure);
            Assert.AreEqual("singleplayer_only", lib.Type);
        }

        [TestMethod]
        public void TestSaving()
        {
            var lib = new Liblist(_libList)
            {
                Game = "Half-Life",
                StartingMap = "c0a0",
                TrainingMap = "t0a0",
                MultiplayerEntity = "info_player_deathmatch",
                GameDll = "dlls\\hl.dll",
                GameDllLinux = "dlls/hl.so",
                GameDllOsx = "dlls/hl.dylib",
                Secure = true,
                Type = "singleplayer_only"
            };

            string output;
            using (var ms = new MemoryStream())
            {
                lib.Write(ms);
                ms.Position = 0;
                output = Encoding.ASCII.GetString(ms.ToArray());
            }

            Assert.AreEqual(@"game ""Half-Life""
startmap ""c0a0""
trainmap ""t0a0""
mpentity ""info_player_deathmatch""
gamedll ""dlls\hl.dll""
gamedll_linux ""dlls/hl.so""
gamedll_osx ""dlls/hl.dylib""
secure ""1""
type ""singleplayer_only""
", output);
        }
    }
}