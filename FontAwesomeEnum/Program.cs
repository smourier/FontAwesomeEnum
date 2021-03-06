﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FontAwesomeEnum
{
    class Program
    {
        private static readonly Dictionary<string, List<string>> _prefixes = new Dictionary<string, List<string>>();
        private static bool _optionHelp;
        private static bool _optionEnums;
        private static bool _optionResources;
        private static string _optionInputPath;
        private static string _optionOutputPath;
        private static string _optionPrefixAttributeName;
        private static string _optionSvgsPath;
        private static bool _optionDuo;

        private static void Main()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                SafeMain();
            }
            else
            {
                try
                {
                    SafeMain();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void SafeMain()
        {
            Console.WriteLine("FontAwesomeEnum - Version 1.4.1 Copyright (C) Simon Mourier 2013-" + DateTime.Now.Year + ". All rights reserved.");
            Console.WriteLine("");

            _optionHelp = CommandLineUtilities.GetArgument("?", false);
            _optionInputPath = CommandLineUtilities.GetArgument<string>(0, null);
            _optionOutputPath = CommandLineUtilities.GetArgument(1, "FontAwesomeEnum.cs");
            _optionPrefixAttributeName = CommandLineUtilities.GetArgument<string>(2, null);
            _optionSvgsPath = CommandLineUtilities.GetArgument<string>(3, null);
            _optionEnums = CommandLineUtilities.GetArgument("enums", true);
            _optionResources = CommandLineUtilities.GetArgument("resources", true);
            _optionDuo = CommandLineUtilities.GetArgument("duo", false);

            if (_optionHelp || Environment.GetCommandLineArgs().Length == 1)
            {
                Console.WriteLine("Format is FontAwesomeEnum.exe <variables.less input file path> [.cs output file] [prefix] [svgs path] [options]");
                Console.WriteLine();
                Console.WriteLine("    .cs output file      The output C# file.");
                Console.WriteLine("                         Default value is FontAwesomeEnum.cs");
                Console.WriteLine();
                Console.WriteLine("    prefix               A prefix attribute name.");
                Console.WriteLine("                         If specified, attributes are added corresponding to Font Awesome prefixed (brands, solid, etc.)");
                Console.WriteLine();
                Console.WriteLine("    svgs path            A directory path that contains Font Awesome svgs.");
                Console.WriteLine("                         Relative to variable.less path.");
                Console.WriteLine("                         Default value is ..\\..\\svgs");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine();
                Console.WriteLine("    /duo                 Adds duotone enums (Font Awesome 5.10+). Implicitely needs a valid svgs implicit or explicit directory path.");
                Console.WriteLine("    /enums:false         Prevents enums creation.");
                Console.WriteLine("    /resources:false     Prevents resources (char) creation.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Input file path: " + _optionInputPath);
            Console.WriteLine("Output file path: " + _optionOutputPath);
            Console.WriteLine("Generate enums: " + _optionEnums);
            Console.WriteLine("Generate resources: " + _optionResources);
            Console.WriteLine("Generate duotones secondaries: " + _optionDuo);
            if (_optionPrefixAttributeName != null || _optionDuo)
            {
                Console.WriteLine("Prefix attribute name: " + _optionPrefixAttributeName);
                string path = _optionSvgsPath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Path.GetFullPath(Path.Combine(_optionInputPath, @"..\..\svgs"));
                }

                Console.WriteLine("SVGs prefixes path: " + path);
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Error: there is no svgs directory path at '" + path + "'.");
                    return;
                }

                foreach (var prefixDir in Directory.GetDirectories(path))
                {
                    string prefixName = Path.GetFileName(prefixDir);
                    foreach (var fa in Directory.GetFiles(prefixDir, "*.svg"))
                    {
                        string faName = Path.GetFileNameWithoutExtension(fa);
                        if (!_prefixes.TryGetValue(faName, out var list))
                        {
                            list = new List<string>();
                            _prefixes.Add(faName, list);
                        }
                        list.Add(prefixName);
                    }
                }
            }

            Console.WriteLine();

            string version = null;
            var enums = new List<Tuple<string, string, string>>();
            using (var reader = new StreamReader(_optionInputPath, Encoding.Default))
            {
                do
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    const string versionToken = "@fa-version:";
                    const string varToken = "@fa-var-";
                    line = line.Trim();

                    if (line.StartsWith(versionToken))
                    {
                        version = line.Substring(versionToken.Length + 1).Trim();
                        if (version.Length > 2 && version[0] == '"' && version[version.Length - 2] == '"')
                        {
                            version = version.Substring(1, version.Length - 3);
                        }

                        Console.WriteLine("Font Awesome detected version: " + version);
                        continue;
                    }

                    if (!line.StartsWith(varToken))
                        continue;

                    int valuePos = line.IndexOf(':');
                    if (valuePos < 0)
                        continue;

                    string varName = line.Substring(varToken.Length, valuePos - varToken.Length);
                    string value = line.Substring(valuePos + 1).Trim();
                    if (!value.StartsWith("\"\\") || !value.EndsWith("\";"))
                        continue;

                    value = value.Substring(2, value.Length - 2 - 2);
                    string name = Camel(line.Substring(varToken.Length, valuePos - varToken.Length));
                    enums.Add(new Tuple<string, string, string>(name, value, varName));
                }
                while (true);
            }

            if (enums.Count == 0)
            {
                Console.WriteLine("No variable was found.");
                return;
            }

            Console.WriteLine("Variables detected: " + enums.Count);
            using (var writer = new StreamWriter(_optionOutputPath, false))
            {
                writer.WriteLine("//------------------------------------------------------------------------------");
                writer.WriteLine("// <auto-generated>");
                writer.WriteLine("//     This code was generated by a tool.");
                writer.WriteLine("//     Runtime Version:" + Environment.Version);
                writer.WriteLine("//");
                writer.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
                writer.WriteLine("//     the code is regenerated.");
                writer.WriteLine("// </auto-generated>");
                writer.WriteLine("//------------------------------------------------------------------------------");
                writer.WriteLine();

                writer.WriteLine("namespace FontAwesome");
                writer.WriteLine("{");
                if (_optionEnums)
                {
                    writer.WriteLine("\t/// <summary>");
                    if (version != null)
                    {
                        writer.WriteLine("\t/// Font Awesome Resources V" + version);
                    }
                    else
                    {
                        writer.WriteLine("\t/// Font Awesome Resources.");
                    }
                    writer.WriteLine("\t/// </summary>");
                    writer.WriteLine("\tpublic enum FontAwesomeEnum");
                    writer.WriteLine("\t{");
                    for (int i = 0; i < enums.Count; i++)
                    {
                        var kv = enums[i];
                        writer.WriteLine("\t\t/// <summary>");
                        writer.WriteLine("\t\t/// fa-" + kv.Item3 + " glyph (" + kv.Item2 + ").");
                        writer.WriteLine("\t\t/// </summary>");

                        bool duo = false;
                        if (_prefixes.TryGetValue(kv.Item3, out var list))
                        {
                            if (!string.IsNullOrWhiteSpace(_optionPrefixAttributeName))
                            {
                                foreach (var prefix in list)
                                {
                                    writer.Write('\t');
                                    writer.Write('\t');
                                    writer.WriteLine("[" + _optionPrefixAttributeName + "(\"" + prefix + "\")]");
                                }
                            }

                            if (_optionDuo && list.Contains("solid"))
                            {
                                duo = true;
                            }
                        }
                        writer.Write('\t');
                        writer.Write('\t');
                        writer.Write(GetValidIdentifier(kv.Item1));
                        writer.Write(" = 0x");
                        writer.Write(kv.Item2);
                        if (duo)
                        {
                            writer.WriteLine(',');
                            writer.WriteLine();
                            writer.WriteLine("\t\t/// <summary>");
                            writer.WriteLine("\t\t/// fa-" + kv.Item3 + " secondary glyph (10" + kv.Item2 + ").");
                            writer.WriteLine("\t\t/// </summary>");
                            writer.Write('\t');
                            writer.Write('\t');
                            writer.Write(GetValidIdentifier(kv.Item1) + "Secondary");
                            writer.Write(" = 0x10");
                            writer.Write(kv.Item2);
                        }

                        if (i < (enums.Count - 1))
                        {
                            writer.WriteLine(',');
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine("\t}");
                }
                writer.WriteLine("");

                if (_optionResources)
                {
                    writer.WriteLine("\t/// <summary>");
                    writer.WriteLine("\t/// Font Awesome Resources.");
                    writer.WriteLine("\t/// </summary>");
                    writer.WriteLine("\tpublic static partial class FontAwesomeResource");
                    writer.WriteLine("\t{");
                    for (int i = 0; i < enums.Count; i++)
                    {
                        var kv = enums[i];
                        writer.WriteLine("\t\t/// <summary>");
                        writer.WriteLine("\t\t/// fa-" + kv.Item3 + " glyph (" + kv.Item2 + ").");
                        writer.WriteLine("\t\t/// </summary>");
                        if (_prefixes.TryGetValue(kv.Item3, out var list))
                        {
                            if (!string.IsNullOrWhiteSpace(_optionPrefixAttributeName))
                            {
                                foreach (var prefix in list)
                                {
                                    writer.Write('\t');
                                    writer.Write('\t');
                                    writer.WriteLine("[" + _optionPrefixAttributeName + "(\"" + prefix + "\")]");
                                }
                            }
                        }

                        writer.WriteLine("\t\tpublic const char " + GetValidIdentifier(kv.Item1) + " = '\\u" + kv.Item2 + "';");
                        if (i < (enums.Count - 1))
                        {
                            writer.WriteLine();
                        }
                    }
                    writer.WriteLine("\t}");
                }

                writer.WriteLine("}");
            }
            Console.WriteLine("Output file was successfully written.");
        }

        private static string Camel(string s)
        {
            if (s == null)
                return s;

            var sb = new StringBuilder(s.Length);
            bool next = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(char.ToUpper(s[i]));
                    continue;
                }

                if (s[i] == '-')
                {
                    next = true;
                    continue;
                }

                if (next)
                {
                    sb.Append(char.ToUpper(s[i]));
                    next = false;
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        private static string GetValidIdentifier(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            int start = 0;
            var sb = new StringBuilder(text.Length);
            if (IsValidIdentifierStart(text[0]))
            {
                sb.Append(text[0]);
                start = 1;
            }
            else
            {
                sb.Append('_');
            }

            bool nextUpper = false;
            for (int i = start; i < text.Length; i++)
            {
                if (IsValidIdentifierPart(text[i]))
                {
                    if (nextUpper)
                    {
                        sb.Append(char.ToUpper(text[i], CultureInfo.CurrentCulture));
                        nextUpper = false;
                    }
                    else
                    {
                        sb.Append(text[i]);
                    }
                }
                else
                {
                    if (text[i] == ' ')
                    {
                        nextUpper = true;
                    }
                    else
                    {
                        sb.Append('_');
                    }
                }
            }
            return sb.ToString();
        }

        private static bool IsValidIdentifierStart(char character)
        {
            if (character == '_')
                return true;

            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            switch (category)
            {
                case UnicodeCategory.UppercaseLetter://Lu
                case UnicodeCategory.LowercaseLetter://Ll
                case UnicodeCategory.TitlecaseLetter://Lt
                case UnicodeCategory.ModifierLetter://Lm
                case UnicodeCategory.OtherLetter://Lo
                case UnicodeCategory.LetterNumber://Nl
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsValidIdentifierPart(char character)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            switch (category)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.Format:
                    return true;

                default:
                    return false;
            }
        }
    }
}
