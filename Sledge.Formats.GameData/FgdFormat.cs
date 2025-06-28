using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Sledge.Formats.FileSystem;
using Sledge.Formats.GameData.Objects;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData
{
    public class FgdFormat
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

        public FgdFormat()
        {
            // No file resolver, includes will be ignored
        }

        public FgdFormat(IFileResolver resolver)
        {
            FileResolver = resolver;
        }

        public static GameDefinition ReadFile(string path)
        {
            var folder = Path.GetDirectoryName(path);
            var formatter = new FgdFormat(new DiskFileResolver(folder));
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
                                throw new TokenParsingException(t, $"Expected name after @ symbol");
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
                                case "visgroupfilter":
                                    ParseVisgroupFilter(def, it);
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
                                case "helpinfo":
                                    // helpinfo command only found in Dota2 Test repo, doesn't appear to be used in newer FGD files - ignore it for now
                                    // it looks like: `@helpinfo( "entity_name", "path/to/some/file.txt" )`
                                    it.Current.Warnings.Add("The `@helpinfo` command doesn't appear to be used, ignoring it for now.");
                                    TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "helpinfo");
                                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenParen);
                                    while (it.Current?.Is(TokenType.Symbol, Symbols.CloseParen) == false) it.MoveNext();
                                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseParen);
                                    break;
                                default:
                                    if (!Enum.TryParse(it.Current.Value, true, out ClassType _))
                                    {
                                        throw new TokenParsingException(t, $"Not a known command: @{it.Current.Value}");
                                    }
                                    ParseClass(def, it, t.Leaders);
                                    break;
                            }
                            break;
                        case TokenType.End:
                            return def;
                        default:
                            // Skip until we get an @
                            t?.Warnings.Add($"Unknown token type: expected Symbol(@), got {t.Type}({t.Value})");
                            while (it.Current != null && !it.Current.Is(TokenType.Symbol, Symbols.At) && !it.Current.Is(TokenType.End)) it.MoveNext();
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

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "include");
            var fileName = TokenParsing.Expect(it, TokenType.String).Value;
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
                    throw new TokenParsingException(t, $"Error while parsing included file {fileName}", ex);
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
            
            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "mapsize");
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenParen);
            var low = TokenParsing.ParseInteger(it);
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.Comma);
            var high = TokenParsing.ParseInteger(it);
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseParen);

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

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "materialexclusion");
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);
            while (it.Current?.Type == TokenType.String)
            {
                var ex = it.Current.Value;
                if (!def.MaterialExclusions.Contains(ex)) def.MaterialExclusions.Add(ex);
                it.MoveNext();
            }
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);
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

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "autovisgroup");
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.Equal);
            var sectionName = TokenParsing.Expect(it, TokenType.String);
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);
            
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

                TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);
                while (it.Current?.Type == TokenType.String)
                {
                    var ent = it.Current.Value;
                    if (!vg.EntityNames.Contains(ent)) vg.EntityNames.Add(ent);
                    it.MoveNext();
                }
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                section.Groups.Add(vg);
            }

            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);

            def.AutoVisgroups.Add(section);
        }

        private void ParseVisgroupFilter(GameDefinition def, IEnumerator<Token> it)
        {
            /* dota base.fgd:
            @VisGroupFilter { filter_type = "toolsMaterial"		material = "toolsclip.vmat"				group =	"Clip"				parent_group = "Tool Brushes" }
            ... etc ...

            @VisGroupFilter { filter_type = "entityTag"		tag = "Lighting"		group = "Lighting"			parent_group = "Entities" }
            ... etc ...
            */

            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "visgroupfilter");

            var dict = new GameDataDictionary(string.Empty);
            ParseGameDataDictionary(it, dict);

            def.VisgroupFilters.Add(new VisgroupFilter(dict));
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

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "gridnav");
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenParen);
            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseParen) == false)
            {
                def.GridNavValues.Add(TokenParsing.ParseInteger(it));
                if (it.Current?.Is(TokenType.Symbol, Symbols.Comma) == true) it.MoveNext();
            }
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseParen);
        }
        
        private void ParseExclude(GameDefinition def, IEnumerator<Token> it)
        {
            /* hlvr.fgd:
            @exclude color_correction_volume
            */
            var t = it.Current;
            Debug.Assert(t != null, nameof(t) + " != null");

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "exclude");
            var excluded = TokenParsing.Expect(it, TokenType.Name).Value;

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

            TokenParsing.Expect(it, TokenType.Name, x => x.ToLower() == "entitygroup");

            var name = TokenParsing.ParseAppendedString(it);
            var group = new EntityGroup(name);
            def.EntityGroups.Add(group);

            if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
            {
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBrace);
                while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBrace) == false)
                {
                    var key = TokenParsing.Expect(it, TokenType.Name).Value;
                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.Equal);
                    var value = TokenParsing.Expect(it, TokenType.Name).Value;
                    if (key == "start_expanded") group.StartExpanded = value == "true";
                    else throw new TokenParsingException(t, $"Unknown entity group metadata key: {key}");
                }
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBrace);
            }
        }

        private void ParseClass(GameDefinition def, IEnumerator<Token> it, List<Token> preamble)
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

            var typeName = TokenParsing.Expect(it, TokenType.Name).Value;
            if (!Enum.TryParse(typeName, true, out ClassType classType))
            {
                throw new TokenParsingException(t, $"Unknown class type {t.Value}");
            }

            //var classType = ClassTypes[typeName.Value];

            var cls = new GameDataClass("", "", classType); 
            cls.Preamble.AddRange(preamble.Where(x => x.Type != TokenType.Whitespace));

            // Read behaviours
            while (it.Current?.Is(TokenType.Name) == true)
            {
                var bname = TokenParsing.Expect(it, TokenType.Name).Value;
                var bvalues = new List<string>();

                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenParen) == true)
                {
                    var bparams = TokenParsing.BalanceBrackets(it, Symbols.OpenParen, Symbols.CloseParen);
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
                                    bvalues.Add(String.Join("", TokenParsing.BalanceBrackets(paramIt, Symbols.OpenParen, Symbols.CloseParen).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.OpenBrace))
                                {
                                    bvalues.Add(String.Join("", TokenParsing.BalanceBrackets(paramIt, Symbols.OpenBrace, Symbols.CloseBrace).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.OpenBracket))
                                {
                                    bvalues.Add(String.Join("", TokenParsing.BalanceBrackets(paramIt, Symbols.OpenBracket, Symbols.CloseBracket).Select(x => x.Value)));
                                }
                                else if (pc.Is(TokenType.Symbol, Symbols.Minus) || pc.Is(TokenType.Symbol, Symbols.Plus) || pc.Is(TokenType.Number))
                                {
                                    bvalues.Add(TokenParsing.ParseDecimal(paramIt).ToString(CultureInfo.InvariantCulture));
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
                    foreach (var bvalue in bvalues)
                    {
                        foreach (var clsBase in def.Classes)
                        {
                            if (clsBase.Name == bvalue)
                            {
                                cls.Inherit(new List<GameDataClass> { clsBase });
                            }
                        }
                    }

                    cls.BaseClasses.AddRange(bvalues);
                }
                else
                {
                    cls.Behaviours.Add(new Behaviour(bname, bvalues.ToArray()));
                }
            }

            // Read class name
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.Equal);
            cls.Name = TokenParsing.Expect(it, TokenType.Name).Value;

            // Read description / details
            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                it.MoveNext();
                cls.Description = TokenParsing.ParseAppendedString(it);
            }

            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                it.MoveNext();
                cls.AdditionalInformation = TokenParsing.ParseAppendedString(it);
            }

            // If the next token isn't an open bracket, we assume that the class definition is finished
            if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBracket) != true)
            {
                def.MergeClass(cls);
                return;
            }

            // Otherwise parse the class properties and so on

            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);

            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
            {
                ParseClassMember(cls, it);
            }

            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);

            def.MergeClass(cls);
        }

        private static GameDataDictionaryValue ParseGameDataDictionaryValue(IEnumerator<Token> it)
        {
            if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBracket) == true)
            {
                var vals = new List<GameDataDictionaryValue>();
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);
                while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
                {
                    vals.Add(ParseGameDataDictionaryValue(it));
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Comma) == true) it.MoveNext();
                    else break;
                }
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                return new GameDataDictionaryValue(vals);
            }
            else if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
            {
                var dict = new GameDataDictionary("");
                ParseGameDataDictionary(it, dict);
                return new GameDataDictionaryValue(dict);
            }
            else if (it.Current?.Is(TokenType.Name) == true)
            {
                var cur = it.Current;
                it.MoveNext();
                if (cur.Value == "true") return new GameDataDictionaryValue(true);
                else if (cur.Value == "false") return new GameDataDictionaryValue(false);
                else throw new TokenParsingException(cur, $"Unknown dictionary value {cur.Value}");
            }
            else if (it.Current?.Is(TokenType.Number) == true || it.Current?.Is(TokenType.Symbol, Symbols.Minus) == true)
            {
                var metaValue = TokenParsing.ParseDecimal(it);
                return new GameDataDictionaryValue(metaValue);
            }
            else
            {
                var metaValue = TokenParsing.ParseAppendedString(it);
                return new GameDataDictionaryValue(metaValue);
            }
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
                    key4 = ["2", "3"]
                }
	        }
            */
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBrace);
            while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBrace) == false)
            {
                var metaKey = TokenParsing.Expect(it, TokenType.Name).Value;
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.Equal);
                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
                {
                    var metaValue = new GameDataDictionary(metaKey);
                    ParseGameDataDictionary(it, metaValue);
                    dict[metaKey] = new GameDataDictionaryValue(metaValue);
                }
                else
                {
                    dict[metaKey] = ParseGameDataDictionaryValue(it);
                }
            }

            TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBrace);
        }

        private void ParseClassMember(GameDataClass cls, IEnumerator<Token> it)
        {
            var first = TokenParsing.Expect(it, TokenType.Name);
            if (it.Current == null) throw new TokenParsingException(first, $"Unexpected end of token stream");

            var second = it.Current;
            var isModelData = cls.ClassType == ClassType.ModelAnimEvent || cls.ClassType == ClassType.ModelGameData || cls.ClassType == ClassType.ModelBreakCommand;

            if (!isModelData && (first.Value == "input" || first.Value == "output"))
            {
                // input name(type) [ : "description" ]
                // output name(type) [ : "description" ]
                var iotype = first.Value == "input" ? IOType.Input : IOType.Output;
                var name = TokenParsing.Expect(it, TokenType.Name).Value;
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenParen);
                var type = ParseVariableType(it);
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseParen);

                var io = new IO(iotype, type.type, type.subType, name);
                cls.InOuts.Add(io);

                /* Source 2 dictionary-based metadata:
                output OnStartTouchAll(void) { is_activator_important = true } : "..."
                */
                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
                {
                    ParseGameDataDictionary(it, io.Metadata);
                }

                if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                {
                    // description
                    it.MoveNext();
                    io.Description = TokenParsing.ParseAppendedString(it);
                }
            }
            else if (second.Is(TokenType.Symbol, Symbols.OpenParen))
            {
                // name(type) [ : "description" [ : [ default_value ] [ : "details" ] ] ] [ = options_decl ]
                var name = first.Value;
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenParen);
                var type = ParseVariableType(it);
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseParen);

                var prop = new Property(name, type.type, type.subType);
                cls.Properties.Add(prop);

                var next = it.Current;
                if (next == null) throw new TokenParsingException(next, $"Unexpected end of token stream");
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
                    if (next == null) throw new TokenParsingException(first, $"Unexpected end of token stream");
                    nv = next.Value.ToLower();
                }

                /* Source 2 metadata for properties:
                test1(studio) [report] : ""Test1""
                test2(float) [ min=""0.0"", max=""180.0"" ] : ""Test2"" : ""0"" : ""Test#2""
                test3(boolean) [ group=""Test Group"" ] : ""Test3"" : 0 : ""Test#3""
                 */
                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBracket) == true)
                {
                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);

                    while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
                    {
                        var metaName = TokenParsing.Expect(it, TokenType.Name).Value;
                        var metaValue = "";
                        if (it.Current?.Is(TokenType.Symbol, Symbols.Equal) == true)
                        {
                            it.MoveNext();
                            metaValue = TokenParsing.ParseAppendedString(it);
                        }
                        prop.Metadata[metaName] = metaValue;

                        // Skip comma if it exists
                        if (it.Current?.Is(TokenType.Symbol, Symbols.Comma) == true) it.MoveNext();
                    }

                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                }

                /* Source 2 dictionary-based metadata:
                color(color255) { enabled={ variable="colormode" value="0" } } : "Color" : "255 255 255"
	            colortemperature(float) { min="1500" max="15000" enabled={ variable="colormode" value="1" } } : "Color Temperature (K)" : "6600"
	            brightness_lumens(float) { min="0.0" max="4000" enabled={ variable="brightness_units" value="1" } } : "Brightness (Lumens)" : "224" : "Brightness in lumens"
                 */
                if (it.Current?.Is(TokenType.Symbol, Symbols.OpenBrace) == true)
                {
                    ParseGameDataDictionary(it, prop.Metadata);
                }

                // Read description/default/details
                if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                {
                    // description
                    it.MoveNext();

                    // Check if the next token is `:`, which means there is no description
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == false)
                    {
                        prop.Description = TokenParsing.ParseAppendedString(it);
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
                                prop.DefaultValue = TokenParsing.Expect(it, TokenType.Name).Value;
                            }
                            else if (it.Current.Type == TokenType.String)
                            {
                                prop.DefaultValue = TokenParsing.ParseAppendedString(it);
                            }
                            else if (it.Current.Is(TokenType.Symbol, Symbols.Minus) || it.Current.Is(TokenType.Symbol, Symbols.Plus) || it.Current.Type == TokenType.Number)
                            {
                                prop.DefaultValue = TokenParsing.ParseDecimal(it).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                throw new TokenParsingException(it.Current, $"Expected string or value, got {it.Current.Type}({it.Current.Value})");
                            }
                        }
                    }

                    // details
                    if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
                    {
                        it.MoveNext();
                        prop.Details = TokenParsing.ParseAppendedString(it);
                    }
                }

                // Read options
                if (it.Current?.Is(TokenType.Symbol, Symbols.Equal) == true)
                {
                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.Equal);
                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.OpenBracket);
                    while (it.Current?.Is(TokenType.Symbol, Symbols.CloseBracket) == false)
                    {
                        ParsePropertyOption(prop, it);
                    }
                    TokenParsing.Expect(it, TokenType.Symbol, Symbols.CloseBracket);
                }
            }
            else
            {
                throw new TokenParsingException(second, $"Expected input, output, or property declaration");
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
                opt.Key = TokenParsing.ParseDecimal(it).ToString(CultureInfo.InvariantCulture);
            }
            else if (it.Current.Type == TokenType.Name || it.Current.Type == TokenType.String)
            {
                opt.Key = it.Current.Value;
                it.MoveNext();
            }
            else
            {
                throw new TokenParsingException(it.Current, $"Expected choice value, instead got {it.Current.Type}({it.Current.Value})");
            }

            TokenParsing.Expect(it, TokenType.Symbol, Symbols.Colon);

            Debug.Assert(it.Current != null);
            if (it.Current.Type == TokenType.Name)
            {
                opt.Description = it.Current.Value;
                it.MoveNext();
            }
            else if (it.Current.Type == TokenType.String)
            {
                opt.Description = TokenParsing.ParseAppendedString(it);
            }
            else
            {
                throw new TokenParsingException(it.Current, $"Expected choice description, instead got {it.Current.Type}({it.Current.Value})");
            }
            
            prop.Options.Add(opt);

            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) != true) return;
            
            TokenParsing.Expect(it, TokenType.Symbol, Symbols.Colon);

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

            opt.Details = TokenParsing.ParseAppendedString(it);
        }

        private (VariableType type, string subType) ParseVariableType(IEnumerator<Token> it)
        {
            var token = it.Current;
            Debug.Assert(token != null, nameof(token) + " != null");

            var type = TokenParsing.Expect(it, TokenType.Name).Value;

            var subType = "";
            // support the source2 `resource:model` syntax... (also `array:struct:map_extension`)
            // it also supports file paths, such as: `scripts/grenades.vdata`
            // or &'d together like: `scripts/grenades.vdata&scripts/misc.vdata&scripts/npc_abilities.vdata`
            // in these cases we just add them all into the subtype, the editor can work out what to do with it.
            if (it.Current?.Is(TokenType.Symbol, Symbols.Colon) == true)
            {
                TokenParsing.Expect(it, TokenType.Symbol, Symbols.Colon);
                while (it.Current?.Is(TokenType.Symbol, Symbols.CloseParen) == false)
                {
                    subType += it.Current.Value;
                    it.MoveNext();
                }
            }

            type = type.Replace("_", "").ToLower();
            if (Enum.TryParse(type, true, out VariableType vt))
            {
                return (vt, subType);
            }

            throw new TokenParsingException(token, $"Unknown variable type {type}");
        }
    }
}