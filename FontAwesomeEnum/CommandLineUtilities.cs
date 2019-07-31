﻿using System;
using System.Collections.Generic;

namespace FontAwesomeEnum
{
    internal static class CommandLineUtilities
    {
        private static readonly Dictionary<string, string> _namedArguments;
        private static readonly Dictionary<int, string> _positionArguments;

        static CommandLineUtilities()
        {
            _namedArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _positionArguments = new Dictionary<int, string>();

            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    continue;

                string arg = Nullify(args[i]);
                if (arg == null)
                    continue;

                string upper = arg.ToUpperInvariant();
                if (string.Equals(arg, "/?", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-?", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(upper, "/HELP", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(upper, "-HELP", StringComparison.OrdinalIgnoreCase))
                {
                    HelpRequested = true;
                }

                bool named = false;
                if (arg[0] == '-' || arg[0] == '/')
                {
                    arg = arg.Substring(1);
                    named = true;
                }
                string name;
                string value;
                int pos = arg.IndexOf(':');
                if (pos < 0)
                {
                    name = arg;
                    value = null;
                }
                else
                {
                    name = arg.Substring(0, pos).Trim();
                    value = arg.Substring(pos + 1).Trim();
                }
                _positionArguments[i - 1] = arg;
                
                if (named)
                {
                    _namedArguments[name] = value;
                }
            }
        }

        public static IDictionary<string, string> NamedArguments => _namedArguments;
        public static IDictionary<int, string> PositionArguments => _positionArguments;
        public static bool HelpRequested { get; }

        public static string CommandLineWithoutExe
        {
            get
            {
                string line = Environment.CommandLine;
                bool inParens = false;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == ' ' && !inParens)
                        return line.Substring(i + 1).TrimStart();

                    if (line[i] == '"')
                    {
                        inParens = !inParens;
                    }
                }
                return line;
            }
        }

        public static T GetArgument<T>(IEnumerable<string> arguments, string name, T defaultValue, IFormatProvider provider = null)
        {
            if (arguments == null)
                return defaultValue;

            foreach (string arg in arguments)
            {
                if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    int pos = arg.IndexOfAny(new[] { '=', ':' }, 1);
                    string argName = pos < 0 ? arg.Substring(1) : arg.Substring(1, pos - 1);
                    if (string.Compare(name, argName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        string value = pos < 0 ? string.Empty : arg.Substring(pos + 1).Trim();
                        if (value.Length == 0)
                        {
                            if (typeof(T) == typeof(bool)) // special case for bool args: if it's there, return true
                                return (T)(object)true;

                            return defaultValue;
                        }
                        return ChangeType(value, defaultValue, provider);
                    }
                }
            }
            return defaultValue;
        }

        public static T GetArgument<T>(int index, T defaultValue, IFormatProvider provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out string s))
                return defaultValue;

            return ChangeType(s, defaultValue, provider);
        }

        public static object GetArgument(int index, object defaultValue, Type conversionType, IFormatProvider provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out string s))
                return defaultValue;

            return ChangeType(s, conversionType, defaultValue, provider);
        }

        public static T GetArgument<T>(string name, T defaultValue, IFormatProvider provider = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!_namedArguments.TryGetValue(name, out string s))
                return defaultValue;

            if (typeof(T) == typeof(bool) && string.IsNullOrEmpty(s))
                return (T)(object)true;

            return ChangeType(s, defaultValue, provider);
        }

        public static bool HasArgument(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return _namedArguments.TryGetValue(name, out _);
        }

        public static object GetArgument(string name, object defaultValue, Type conversionType, IFormatProvider provider = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));

            if (!_namedArguments.TryGetValue(name, out string s))
                return defaultValue;

            if (conversionType == typeof(bool) && string.IsNullOrEmpty(s))
                return true;

            return ChangeType(s, conversionType, defaultValue, provider);
        }

        private static string Nullify(string value)
        {
            if (value == null)
                return value;

            value = value.Trim();
            return value.Length == 0 ? null : value;
        }

        private static T ChangeType<T>(object value, T defaultValue, IFormatProvider provider) => (T)ChangeType(value, typeof(T), defaultValue, provider);
        private static object ChangeType(object value, Type conversionType, object defaultValue, IFormatProvider provider)
        {
            if (value == null)
            {
                if (conversionType.IsValueType)
                    return Activator.CreateInstance(conversionType);

                return null;
            }

            if (conversionType.IsAssignableFrom(value.GetType()))
                return value;

            try
            {
                return Convert.ChangeType(value, conversionType, provider);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
