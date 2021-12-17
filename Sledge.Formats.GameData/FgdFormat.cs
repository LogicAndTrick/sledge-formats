using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Sledge.Formats.FileSystem;
using Sledge.Formats.GameData.Objects;
using Sledge.Formats.Tokens;
using static Sledge.Formats.Tokens.TokenParsing;

namespace Sledge.Formats.GameData
{
    public class FgdFormatter
    {
        public IFileResolver FileResolver { get; set; }
        
        private static readonly char[] ValidSymbols = {
            Symbols.At,             // @
            Symbols.Equal,          // =
            Symbols.Colon,          // :
            Symbols.OpenBracket,    // [
            Symbols.CloseBracket,   // ]
            Symbols.OpenParen,      // (
            Symbols.CloseParen,     // )
            Symbols.Plus,           // +
            Symbols.Comma,          // ,
            Symbols.Dot,            // .
            Symbols.Slash,          // /
            
            // For TrenchBroom's extended syntax
            Symbols.OpenBrace,
            Symbols.CloseBrace,
            Symbols.Ampersand,
            Symbols.Minus,
            Symbols.Greater,
        };

        private static readonly Tokeniser Tokeniser = new Tokeniser(ValidSymbols);

        /// <summary>
        /// Set to true to allow strings to contain newlines. In a valid FGD, this option is always safe.
        /// Leave this false to handle invalid FGDs with unclosed string quotes at the end of lines.
        /// </summary>
        public bool AllowNewlinesInStrings
        {
            get => Tokeniser.AllowNewlinesInStrings;
            set => Tokeniser.AllowNewlinesInStrings = value;
        }

        public FgdFormatter()
        {
            // No file resolver, includes will be ignored
        }

        public FgdFormatter(IFileResolver resolver)
        {
            FileResolver = resolver;
        }

        public static GameDefinition ReadFile(string path)
        {
            var folder = Path.GetDirectoryName(path);
            var formatter = new FgdFormatter(new DiskFileResolver(folder));
            using (var stream = File.OpenRead(path))
            {
                return formatter.Read(new StreamReader(stream));
            }
        }

        public GameDefinition Read(string text)
        {
            using (var reader = new StringReader(text))
            {
                return Read(reader);
            }
        }

        public GameDefinition Read(TextReader reader)
        {
            var def = new GameDefinition();
            
            var tokens = Tokeniser.Tokenise(reader);
            using (var it = tokens.GetEnumerator())
            {
                it.MoveNext();
                while (true)
                {
                    var currentToken = it.Current;
                    var t = it.Current;
                    switch (t?.Type)
                    {
                        case TokenType.Symbol when t.Symbol == Symbols.At:
                            if (!it.MoveNext() || it.Current == null || it.Current.Type != TokenType.Name || it.Current.Value == null)
                            {
                                throw new Exception($"Parsing error (line {t.Line}, column {t.Column}): Expected name after @ symbol");
                            }

                            switch (it.Current.Value.ToLower())
                            {
                                case "include":
                                    ParseInclude(def, it);
                                    break;
                                case "mapsize":
                                    ParseMapSize(def, it);
                                    break;
                                case "materialexclusion":
                                    ParseMaterialExclusion(def, it);
                                    break;
                                case "autovisgroup":
                                    ParseAutoVisgroup(def, it);
                                    break;
                                case "gridnav":
                                    ParseGridNav(def, it);
                                    break;
                                case "exclude":
                                    ParseExclude(def, it);
                                    break;
                                case "entitygroup":
                                    ParseEntityGroup(def, it);
                                    break;
                                case "baseclass":
                                case "pointclass":
                                case "solidclass":
                                case "keyframeclass":
                                case "moveclass":
                                case "npcclass":
                                case "filterclass":
                                case "pathclass":
                                case "cableclass":
                                case "overrideclass":
                                case "modelgamedata":
                                case "modelanimevent":
                                case "modelbreakcommand":
                                case "struct":
                                    ParseClass(def, it);
                                    break;
                                default:
                                    throw new Exception($"Parsing error (line {t.Line}, column {t.Column}): Not a known command: @{it.Current.Value}");
                            }
                            break;
                        case TokenType.End:
                            return def;
                        default:
                            // Skip until we get an @
                            t?.Warnings.Add($"Unknown token type: expected Symbol(@), got {t.Type}({t.Value})");
                            while (it.Current != null && !it.Current.Is(TokenType.Symbol, Symbols.At)) it.MoveNext();
                            break;
                    }

                    if (currentToken == it.Current)
                    {
                        throw new Exception("Parsing error: token did not advance");
                    }
                }
            }
        }

        private void ParseInclude(GameDefinition def, IEnumerator<Token> it)
        {
            /* halflife2.fgd:
            @include "base.fgd"
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            Expect(it, TokenType.Name, x => x.ToLower() == "include");
            var fileName = Expect(it, TokenType.String).Value;
            if (def.Includes.Contains(fileName))
            {
                // Duplicate include, ignore it
            }
            else if (FileResolver == null)
            {
                def.Includes.Add(fileName);
                // No file resolver, unable to include it
            }
            else
            {
                try
                {
                    def.Includes.Add(fileName);
                    using (var inc = FileResolver.OpenFile(fileName))
                    {
                        var incDef = Read(new StreamReader(inc));

                        // Merge the included gamedata into the current one
                        if (incDef.MapSizeHigh.HasValue) def.MapSizeHigh = incDef.MapSizeHigh;
                        if (incDef.MapSizeLow.HasValue) def.MapSizeLow = incDef.MapSizeLow;
                        def.Includes.AddRange(incDef.Includes.Where(x => !def.Includes.Contains(x)));
                        foreach (var cls in incDef.Classes) def.MergeClass(cls);
                        foreach (var vis in incDef.AutoVisgroups) def.MergeAutoVisGroupSection(vis);
                        def.MaterialExclusions.AddRange(incDef.MaterialExclusions.Where(x => !def.MaterialExclusions.Any(y => String.Equals(x, y, StringComparison.InvariantCultureIgnoreCase))));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Parsing error (line {t.Line}, column {t.Column}): Error while parsing included file {fileName}", ex);
                }
            }
        }

        private void ParseMapSize(GameDefinition def, IEnumerator<Token> it)
        {
            /* base.fgd:
            @mapsize(-16384, 16384)
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");
            
            Expect(it, TokenType.Name, x => x.ToLower() == "mapsize");
            Expect(it, TokenType.Symbol, Symbols.OpenParen);
            var low = ParseInteger(it);
            Expect(it, TokenType.Symbol, Symbols.Comma);
            var high = ParseInteger(it);
            Expect(it, TokenType.Symbol, Symbols.CloseParen);

            def.MapSizeLow = low;
            def.MapSizeHigh = high;
        }

        private void ParseMaterialExclusion(GameDefinition def, IEnumerator<Token> it)
        {
            /* tf2.fgd:
            @MaterialExclusion
            [
	            // Names of the sub-directories we don't want to load materials from
	            "ambulance"
                // snipped a bunch
	            "voice"
            ]
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            Expect(it, TokenType.Name, x => x.ToLower() == "materialexclusion");
            Expect(it, TokenType.Symbol, Symbols.OpenBracket);
            while (it.Current?.Type == TokenType.String)
            {
                var ex = it.Current.Value;
                if (!def.MaterialExclusions.Contains(ex)) def.MaterialExclusions.Add(ex);
                it.MoveNext();
            }
            Expect(it, TokenType.Symbol, Symbols.CloseBracket);
        }

        private void ParseAutoVisgroup(GameDefinition def, IEnumerator<Token> it)
        {
            /* tf2.fgd:
            @AutoVisGroup = "Custom"
            [
	            "AI, Choreo"
	            [
		            "ai_speechfilter"
                    // snip
		            "point_template"
	            ]

	            "Cameras"
	            [
		            "info_observer_point"
                    // snip
		            "sky_camera"
	            ]
                
                // ...etc...
            ]
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            Expect(it, TokenType.Name, x => x.ToLower() == "autovisgroup");
            Expect(it, TokenType.Symbol, Symbols.Equal);
            var sectionName = Expect(it, TokenType.String);
            Expect(it, TokenType.Symbol, Symbols.OpenBracket);
            
            var section = new AutoVisgroupSection
            {
                Name = sectionName.Value
            };

            while (it.Current?.Type == TokenType.String)
            {
                var vg = new AutoVisgroup
                {
                    Name = it.Current.Value
                };
                it.MoveNext();

                Expect(it, TokenType.Symbol, Symbols.OpenBracket);
                while (it.Current?.Type == TokenType.String)
                {
                    var ent = it.Current.Value;
                    if (!vg.EntityNames.Contains(ent)) vg.EntityNames.Add(ent);
                    it.MoveNext();
                }
                Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                section.Groups.Add(vg);
            }

            Expect(it, TokenType.Symbol, Symbols.CloseBracket);

            def.AutoVisgroups.Add(section);
        }

        private void ParseGridNav(GameDefinition def, IEnumerator<Token> it)
        {
            /* dota.fgd:
            @gridnav(64, 32, 32, 16384)
            */
            // I don't really know what these values mean. Something to do with dota nav mesh I think...

            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            def.GridNavValues = new List<int>();

            Expect(it, TokenType.Name, x => x.ToLower() == "gridnav");
            Expect(it, TokenType.Symbol, Symbols.OpenParen);
            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseParen) == false)
            {
                def.GridNavValues.Add(ParseInteger(it));
                if (it.Current?.Is(TokenType.Symbol, Symbols.Comma) == true) it.MoveNext();
            }
            Expect(it, TokenType.Symbol, Symbols.CloseParen);
        }
        
        private void ParseExclude(GameDefinition def, IEnumerator<Token> it)
        {
            /* hlvr.fgd:
            @exclude color_correction_volume
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            Expect(it, TokenType.Name, x => x.ToLower() == "exclude");
            var excluded = Expect(it, TokenType.Name).Value;

            var cls = def.GetClass(excluded);
            if (cls != null) def.Classes.Remove(cls);
        }

        private void ParseEntityGroup(GameDefinition def, IEnumerator<Token> it)
        {
            /* hlvr.fgd:
            @EntityGroup "Player" {	start_expanded = true }
            @EntityGroup "Items"
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            Expect(it, TokenType.Name, x => x.ToLower() == "entitygroup");

            var name = ParseAppendedString(it);
            var group = new EntityGroup(name);
            def.EntityGroups.Add(group);

            if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
            {
                Expect(it, TokenType.Symbol, Symbols.OpenBrace);
                while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBrace) == false)
                {
                    var key = Expect(it, TokenType.Name).Value;
                    Expect(it, TokenType.Symbol, Symbols.Equal);
                    var value = Expect(it, TokenType.Name).Value;
                    if (key == "start_expanded") group.StartExpanded = value == "true";
                    else throw new Exception($"Parsing error (line {t.Line}, column {t.Column}): Unknown entity group metadata key: {key}");
                }
                Expect(it, TokenType.Symbol, Symbols.CloseBrace);
            }
        }

        private void ParseClass(GameDefinition def, IEnumerator<Token> it)
        {
            /*
            half-life.fgd:
            @BaseClass = Sequence
            [
	            sequence(integer) : "Animation Sequence (editor)"
            ]
            @PointClass base(Targetname, Angles) size(-16 -16 -16, 16 16 16) color(255 0 0) = env_blood : "Blood Effects" 
            [
	            color(choices) : "Blood Color" : 0 =
	            [
		            0 : "Red (Human)"
		            1 : "Yellow (Alien)"
	            ]
	            amount(string) : "Amount of blood (damage to simulate)" : "100"
	            spawnflags(flags) =
	            [
		            1: "Random Direction" : 0
		            2: "Blood Stream" : 0
		            4: "On Player" : 0
		            8: "Spray decals" : 0
	            ]
            ]
            */
            /* quake2.fgd (TrenchBroom):
            @PointClass base(Appearflags) color(255 128 0) size(-16 -16 0, 16 16 16)	model({{
              spawnflags & 32 -> { "path": ":models/deadbods/dude/tris.md2", "frame": 5 },
              spawnflags & 16 -> { "path": ":models/deadbods/dude/tris.md2", "frame": 4 },
              spawnflags &  8 -> { "path": ":models/deadbods/dude/tris.md2", "frame": 3 },
              spawnflags &  4 -> { "path": ":models/deadbods/dude/tris.md2", "frame": 2 },
              spawnflags &  2 -> { "path": ":models/deadbods/dude/tris.md2", "frame": 1 },
                             	            ":models/deadbods/dude/tris.md2"
              }}) = misc_deadsoldier : "Dead guys! 6 of em!"
            [
	            spawnflags(Flags) =
	            [
		            1 : "On Back" : 0
		            2 : "On Stomach" : 0
		            4 : "Back, Decap" : 0
		            8 : "Fetal Position" : 0
		            16 : "Sitting, Decap" : 0
		            32 : "Impaled" : 0
	            ]
            ]
            @PointClass base(Weapons) model({ "path": ":models/weapons/g_shotg/tris.md2" }) = weapon_shotgun : "Shotgun" []
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            var typeName = Expect(it, TokenType.Name).Value;
            if (!Enum.TryParse(typeName, true, out ClassType classType))
            {
                throw new Exception($"Parsing error (line {t.Line}, column {t.Column}): Unknown class type {t.Value}");
            }

            //var classType = ClassTypes[typeName.Value];

            var cls = new GameDataClass("", "", classType);

            // Read behaviours
            while (it.Current?.Is(TokenType.Name) == true)
            {
                var bname = Expect(it, TokenType.Name).Value;
                var bvalues = new List<string>();

                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenParen) == true)
                {
                    var bparams = BalanceBrackets(it, Symbols.OpenParen, Symbols.CloseParen);
                    if (bparams.Count > 0)
                    {
                        // remove the brackets at the start/end
                        bparams.RemoveAt(0);
                        bparams.RemoveAt(bparams.Count - 1);
                        bparams.Add(new Token(TokenType.End));

                        using (IEnumerator<Token> paramIt = bparams.GetEnumerator()) // don't use `var` here, we need this boxed into an object
                        {
                            paramIt.MoveNext();
                            while (paramIt.Current != null && !paramIt.Current.Is(TokenType.End))
                            {
                                var pc = paramIt.Current;

                                if (pc.Is(TokenType.Symbol, Symbols.Comma))
                                {
                                    paramIt.MoveNext();
                                    continue;
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.OpenParen))
                                {
                                    bvalues.Add(String.Join("", BalanceBrackets(paramIt, Symbols.OpenParen, Symbols.CloseParen).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.OpenBrace))
                                {
                                    bvalues.Add(String.Join("", BalanceBrackets(paramIt, Symbols.OpenBrace, Symbols.CloseBrace).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.OpenBracket))
                                {
                                    bvalues.Add(String.Join("", BalanceBrackets(paramIt, Symbols.OpenBracket, Symbols.CloseBracket).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.Minus) || pc.Is(TokenType.Symbol, Symbols.Plus) || pc.Is(TokenType.Number))
                                {
                                    bvalues.Add(ParseDecimal(paramIt).ToString(CultureInfo.InvariantCulture));
                                    continue;
                                }
                                else
                                {
                                    bvalues.Add(pc.Value);
                                }

                                paramIt.MoveNext();
                            }
                        }
                    }
                }
                // New metadata syntax from source 2
                else if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
                {
                    var dict = new GameDataDictionary(bname);
                    cls.Dictionaries.Add(dict);
                    ParseGameDataDictionary(it, dict);
                }

                if (bname.ToLower() == "base")
                {
                    cls.BaseClasses.AddRange(bvalues);
                }
                else
                {
                    cls.Behaviours.Add(new Behaviour(bname, bvalues.ToArray()));
                }
            }

            // Read class name
            Expect(it, TokenType.Symbol, Symbols.Equal);
            cls.Name = Expect(it, TokenType.Name).Value;

            // Read description / details
            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                it.MoveNext();
                cls.Description = ParseAppendedString(it);
            }

            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                it.MoveNext();
                cls.AdditionalInformation = ParseAppendedString(it);
            }

            // If the next token isn't an open bracket, we assume that the class definition is finished
            if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBracket) != true)
            {
                def.MergeClass(cls);
                return;
            }

            // Otherwise parse the class properties and so on

            Expect(it, TokenType.Symbol, Symbols.OpenBracket);

            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
            {
                ParseClassMember(cls, it);
            }

            Expect(it, TokenType.Symbol, Symbols.CloseBracket);

            def.MergeClass(cls);
        }

        private static void ParseGameDataDictionary(IEnumerator<Token> it, GameDataDictionary dict)
        {
            /*
	        metadata
	        {
		        key1 = "Value1"
		        key2 = "Value2"
                child =
                {
                    key3 = "Value3"
                }
	        }
            */
            Expect(it, TokenType.Symbol, Symbols.OpenBrace);
            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBrace) == false)
            {
                var metaKey = Expect(it, TokenType.Name).Value;
                Expect(it, TokenType.Symbol, Symbols.Equal);
                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
                {
                    var metaValue = new GameDataDictionary(metaKey);
                    ParseGameDataDictionary(it, metaValue);
                    dict[metaKey] = new GameDataDictionaryValue(metaValue);
                }
                else if (it.Current?.Is(TokenType.Name) == true)
                {
                    if (it.Current.Value == "true") dict[metaKey] = new GameDataDictionaryValue(true);
                    else if (it.Current.Value == "false") dict[metaKey] = new GameDataDictionaryValue(false);
                    else throw new Exception($"Parsing error (line {it.Current.Line}, column {it.Current.Column}): Unknown dictionary value {it.Current.Value}");
                    it.MoveNext();
                }
                else if (it.Current?.Is(TokenType.Number) == true)
                {
                    var metaValue = ParseDecimal(it);
                    dict[metaKey] = new GameDataDictionaryValue(metaValue);
                }
                else
                {
                    var metaValue = ParseAppendedString(it);
                    dict[metaKey] = new GameDataDictionaryValue(metaValue);
                }
            }

            Expect(it, TokenType.Symbol, Symbols.CloseBrace);
        }

        private void ParseClassMember(GameDataClass cls, IEnumerator<Token> it)
        {
            var first = Expect(it, TokenType.Name);
            if (it.Current == null) throw new Exception($"Parsing error (line {first.Line}, column {first.Column}): Unexpected end of token stream");

            var second = it.Current;
            var isModelData = cls.ClassType == ClassType.ModelAnimEvent || cls.ClassType == ClassType.ModelGameData || cls.ClassType == ClassType.ModelBreakCommand;

            if (!isModelData && (first.Value == "input" || first.Value == "output"))
            {
                // input name(type) [ : "description" ]
                // output name(type) [ : "description" ]
                var iotype = first.Value == "input" ? IOType.Input : IOType.Output;
                var name = Expect(it, TokenType.Name).Value;
                Expect(it, TokenType.Symbol, Symbols.OpenParen);
                var type = ParseVariableType(it);
                Expect(it, TokenType.Symbol, Symbols.CloseParen);

                var io = new IO(iotype, type.type, type.subType, name);
                cls.InOuts.Add(io);
                if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                {
                    // description
                    it.MoveNext();
                    io.Description = ParseAppendedString(it);
                }
            }
            else if (second.Is(TokenType.Symbol, Symbols.OpenParen))
            {
                // name(type) [ : "description" [ : [ default_value ] [ : "details" ] ] ] [ = options_decl ]
                var name = first.Value;
                Expect(it, TokenType.Symbol, Symbols.OpenParen);
                var type = ParseVariableType(it);
                Expect(it, TokenType.Symbol, Symbols.CloseParen);

                var prop = new Property(name, type.type, type.subType);
                cls.Properties.Add(prop);

                var next = it.Current;
                if (next == null) throw new Exception($"Parsing error (line {first.Line}, column {first.Column}): Unexpected end of token stream");
                var nv = next.Value.ToLower();

                /* Source 1 metadata for properties
                a(string) report
                b(string) readonly
                // Below not ever seen in the wild
                c(string) readonly report
                d(string) report readonly
                // This all falls apart if a property with no description/default value is followed
                // by a property with a name of "readonly" or "report"...hopefully this doesn't happen.
                 */
                while (next.Type == TokenType.Name && (nv == "readonly" || nv == "report"))
                {
                    prop.Metadata.Add(next.Value, "");
                    it.MoveNext();
                    next = it.Current;
                    if (next == null) throw new Exception($"Parsing error (line {first.Line}, column {first.Column}): Unexpected end of token stream");
                    nv = next.Value.ToLower();
                }

                /* Source 2 metadata for properties:
                test1(studio) [report] : ""Test1""
                test2(float) [ min=""0.0"", max=""180.0"" ] : ""Test2"" : ""0"" : ""Test#2""
                test3(boolean) [ group=""Test Group"" ] : ""Test3"" : 0 : ""Test#3""
                 */
                if (next.Is(TokenType.Symbol, Symbols.OpenBracket))
                {
                    Expect(it, TokenType.Symbol, Symbols.OpenBracket);

                    while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
                    {
                        var metaName = Expect(it, TokenType.Name).Value;
                        var metaValue = "";
                        if (it.Current?.Is(TokenType.Symbol, Symbols.Equal) == true)
                        {
                            it.MoveNext();
                            metaValue = ParseAppendedString(it);
                        }
                        prop.Metadata[metaName] = metaValue;

                        // Skip comma if it exists
                        if (it.Current?.Is(TokenType.Symbol, Symbols.Comma) == true) it.MoveNext();
                    }

                    Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                }

                // Read description/default/details
                if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                {
                    // description
                    it.MoveNext();

                    // Check if the next token is `:`, which means there is no description
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == false)
                    {
                        prop.Description = ParseAppendedString(it);
                    }

                    // default value
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                    {
                        // Check if the next token is `:`, which means there is no default value
                        it.MoveNext();
                        if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == false)
                        {
                            if (it.Current.Type == TokenType.Name)
                            {
                                prop.DefaultValue = Expect(it, TokenType.Name).Value;
                            }
                            else if (it.Current.Type == TokenType.String)
                            {
                                prop.DefaultValue = ParseAppendedString(it);
                            }
                            else if (it.Current.Is(TokenType.Symbol, Symbols.Minus) || it.Current.Is(TokenType.Symbol, Symbols.Plus) || it.Current.Type == TokenType.Number)
                            {
                                prop.DefaultValue = ParseDecimal(it).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                throw new Exception($"Parsing error (line {it.Current.Line}, column {it.Current.Column}): Expected string or value, got {it.Current.Type}({it.Current.Value})");
                            }
                        }
                    }

                    // details
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                    {
                        it.MoveNext();
                        prop.Details = ParseAppendedString(it);
                    }
                }

                // Read options
                if (it.Current?.Is(TokenType.Symbol, Symbols.Equal) == true)
                {
                    Expect(it, TokenType.Symbol, Symbols.Equal);
                    Expect(it, TokenType.Symbol, Symbols.OpenBracket);
                    while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
                    {
                        ParsePropertyOption(prop, it);
                    }
                    Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                }
            }
            else
            {
                throw new Exception($"Parsing error (line {second.Line}, column {second.Column}): Expected input, output, or property declaration");
            }
        }

        private void ParsePropertyOption(Property prop, IEnumerator<Token> it)
        {
            // value : description
            // value : description : 0
            // value : description : 0 : "details"
            // value : description : "details"
            var opt = new Option();

            Debug.Assert(it.Current != null);
            if (it.Current.Is(TokenType.Symbol, Symbols.Minus) || it.Current.Is(TokenType.Symbol, Symbols.Plus) || it.Current.Type == TokenType.Number)
            {
                opt.Key = ParseDecimal(it).ToString(CultureInfo.InvariantCulture);
            }
            else if (it.Current.Type == TokenType.Name || it.Current.Type == TokenType.String)
            {
                opt.Key = it.Current.Value;
                it.MoveNext();
            }
            else
            {
                throw new Exception($"Parsing error (line {it.Current.Line}, column {it.Current.Column}): Expected choice value, instead got {it.Current.Type}({it.Current.Value})");
            }

            Expect(it, TokenType.Symbol, Symbols.Colon);

            Debug.Assert(it.Current != null);
            if (it.Current.Type == TokenType.Name)
            {
                opt.Description = it.Current.Value;
                it.MoveNext();
            }
            else if (it.Current.Type == TokenType.String)
            {
                opt.Description = ParseAppendedString(it);
            }
            else
            {
                throw new Exception($"Parsing error (line {it.Current.Line}, column {it.Current.Column}): Expected choice description, instead got {it.Current.Type}({it.Current.Value})");
            }
            
            prop.Options.Add(opt);

            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) != true) return;
            
            Expect(it, TokenType.Symbol, Symbols.Colon);

            if (it.Current?.Is(TokenType.Number) == true)
            {
                // If we have a value, it's the flag default
                opt.On = it.Current.Value == "1";
                it.MoveNext();

                // If we find another colon, there's a long description present as well
                // Otherwise we're finished here
                if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) != true) return;
                it.MoveNext();
            }

            opt.Details = ParseAppendedString(it);
        }

        private (VariableType type, string subType) ParseVariableType(IEnumerator<Token> it)
        {
            var token = it.Current;
            Debug.Assert(token != null, nameof(token) + " != null");

            var type = Expect(it, TokenType.Name).Value;

            var subType = "";
            // support the source2 `resource:model` syntax... (also `array:struct:map_extension`)
            while (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                if (subType.Length > 0) subType += ":";
                it.MoveNext();
                subType += Expect(it, TokenType.Name).Value;
            }
            if (subType.Length > 0) Console.WriteLine(type + ":" +subType);

            type = type.Replace("_", "").ToLower();
            if (Enum.TryParse(type, true, out VariableType vt))
            {
                return (vt, subType);
            }

            throw new Exception($"Parsing error (line {token.Line}, column {token.Column}): Unknown variable type {type}");
        }
    }
}
