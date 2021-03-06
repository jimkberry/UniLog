﻿using System.Linq;
using System.Collections.Generic;
using System;

namespace UniLog
{
    //
    // UniLogger
    //

    public class UniLogger
    {
        public enum Level
        {
            Debug = 10,
            Verbose = 20,
            Info = 30,
            Warn = 40,
            Error = 50,
            Off = 1000,
        }

        //
        // Statics
        //

        // Public API
        // ReSharper disable MemberCanBePrivate.Global,UnusedMember.Global,FieldCanBeMadeReadOnly.Global
        public static Dictionary<Level, string> LevelNames {get;} = new Dictionary<Level, string>()
        {
            {Level.Debug, "Debug"},
            {Level.Verbose, "Verbose"},
            {Level.Info, "Info"},
            {Level.Warn, "Warn"},
            {Level.Error, "Error"},
            {Level.Off, "Off"},
        };

        // 0 = name
        // 1 = level
        // 2 = message

        public static Level DefaultLevel = Level.Warn;
        public static bool DefaultThrowOnError = false;

        public static UniLogger GetLogger(string name)
        {
            // level and format only get applied if the logger is new
            return  UniLoggerCollection.GetLogger(name);
        }

        public static Level LevelFromName(string name)
        {
            Level l = LevelNames.FirstOrDefault(x => x.Value == name).Key;
            return l==0 ? DefaultLevel : l;
        }

        public static void SetupLevels(Dictionary<string,string> levels)
        {
            foreach (string lName in levels.Keys)
            {
                GetLogger(lName).LogLevel = LevelNames.FirstOrDefault(x => x.Value == levels[lName]).Key;
            }
        }

#if UNITY_2019_1_OR_NEWER

        public string DefaultFormat = "{1}: {2}";

        //
        // Unity Implementation
        //

        protected UnityEngine.Logger unityLogger;

        public UniLogger(string name)
        {
            Name = name;
            LogLevel = DefaultLevel;
            LogFormat = DefaultFormat;
            ThrowOnError = DefaultThrowOnError;
            unityLogger  = new UnityEngine.Logger(UnityEngine.Debug.unityLogger.logHandler);
        }

        private void _Write(string name, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string outMsg = string.Format(LogFormat, name, LevelNames[lvl], msg);
                //string outMsg = string.Format(LogFormat, LevelNames[lvl], msg);
                switch (lvl)
                {
                case Level.Debug:
                case Level.Verbose:
                case Level.Info:
                    unityLogger.Log($"{name}: {outMsg}");
                    break;
                case Level.Warn:
                    unityLogger.LogWarning(name, outMsg);
                    break;
                case Level.Error:
                    if (ThrowOnError)
                        throw new Exception($"{name}: {outMsg}");
                    else
                        unityLogger.LogError(name, outMsg);
                    break;
                }
            }
        }

#else
        //
        // Non-unity
        //

        public string DefaultFormat = "[{0}] {1}: {2}";

        public UniLogger(string name)
        {
            Name = name;
            LogLevel = DefaultLevel;
            LogFormat = DefaultFormat;
            ThrowOnError = DefaultThrowOnError;
        }

        private void _Write(string name, Level lvl, string msg)
        {
            if (lvl >= LogLevel)
            {
                string outMsg = string.Format(LogFormat, name, LevelNames[lvl], msg);

                if (lvl >= Level.Error && ThrowOnError)
                    throw new Exception(outMsg);
                else
                    Console.WriteLine(outMsg);
            }
        }

#endif

        // Instance
        public string Name {get;}
        public Level LogLevel;
        public string LogFormat;
        public bool ThrowOnError;

        public void Info(string msg) => _Write(Name, Level.Info, msg);
        public void Verbose(string msg) => _Write(Name, Level.Verbose, msg);
        public void Debug(string msg) => _Write(Name, Level.Debug, msg);
        public void Warn(string msg) => _Write(Name, Level.Warn, msg);
        public void Error(string msg) => _Write(Name, Level.Error, msg);

        // End  API
        // ReSharper enable MemberCanBePrivate.Global,UnusedMember.Global,FieldCanBeMadeReadOnly.Global

    }

    public class UniLoggerCollection
    {
        private readonly Dictionary<string, UniLogger> _loggers;

        //
        // Yeah, it's a singleton
        //
        private static UniLoggerCollection _instance;

        public static UniLoggerCollection GetInstance()
        {
            _instance = _instance ?? new UniLoggerCollection();
            return _instance;
        }
        public static UniLogger GetLogger(string name)
        {
            UniLoggerCollection inst = GetInstance();
            return inst._loggers.ContainsKey(name) ? inst._loggers[name] : inst.AddLogger(name);
        }

        private UniLoggerCollection()
        {
            _loggers = new Dictionary<string, UniLogger>();
        }

        private  UniLogger AddLogger(string name)
        {
            return _loggers[name] = new UniLogger(name);
        }

    }
}
