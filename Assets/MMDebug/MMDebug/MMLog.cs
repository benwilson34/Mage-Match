using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace MMDebug {
    public class MMLog {
        public enum LogLevel { None = 0, Standard, Detailed, Deep }

        private static LogLevel logLevel = LogLevel.Detailed;
        private static StringBuilder report;
        private static string c_dataStructure = "cyan", c_backendGroup = "blue", c_playerGroup = "green", c_userExperience = "magenta";

        // should have constructor?
        public static void Init(LogLevel level) {
            logLevel = level;
            report = new StringBuilder();
            Application.logMessageReceived += OnLogMessageReceived;
        }

        public static void Log(string script, string color, string msg, LogLevel level = LogLevel.Standard) {
            if ((int)level <= (int)logLevel) {
                report.AppendLine(script + ": " + msg);
                Debug.Log("<b><color=" + color + ">" + script + ": </color></b>" + msg);
            }
        }

        public static void LogWarning(string msg) {
            //report.AppendLine(msg);
            Debug.LogWarning(msg);
        }

        public static void LogError(string msg) {
            //report.AppendLine(msg);
            Debug.LogError(msg);
        }

        static void OnLogMessageReceived(string msg, string stackTrace, LogType type) {
            if (msg.ToCharArray()[0] != '<') // not great
                report.AppendLine(msg);
        }

        public static void SaveReportTXT() {
            DateTime dt = DateTime.Now;
            string filePath = "MageMatch_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
            filePath += dt.Hour + "-" + dt.Minute + "-" + dt.Second + "_DebugLog";
            filePath = @"/" + filePath + ".txt";

            File.WriteAllText(filePath, report.ToString());
        }

        // ---------------------------------------------------------------------------------

        public static void Log_AnimCont(string msg, LogLevel level = LogLevel.Standard) {
            Log("ANIM-CONT", c_userExperience, msg, level);
        }

        public static void Log_AudioCont(string msg, LogLevel level = LogLevel.Standard) {
            Log("AUDIO-CONT", c_userExperience, msg, level);
        }

        public static void Log_BoardCheck(string msg, LogLevel level = LogLevel.Deep) {
            Log("BOARDCHECK", c_dataStructure, msg, level);
        }

        public static void Log_Commish(string msg, LogLevel level = LogLevel.Standard) {
            Log("COMMISH", "orange", msg, level);
        }

        public static void Log_EffectCont(string msg, LogLevel level = LogLevel.Standard) {
            Log("EFFECT-CONT", c_backendGroup, msg, level);
        }

        public static void Log_EnchantFx(string msg, LogLevel level = LogLevel.Standard) {
            Log("ENCHANT-FX", "purple", msg, level);
        }

        public static void Log_EventCont(string msg, LogLevel level = LogLevel.Standard) {
            Log("EVENT-CONT", c_backendGroup, msg, level);
        }

        public static void Log_HexGrid(string msg, LogLevel level = LogLevel.Deep) {
            Log("HEX-GRID", c_dataStructure, msg, level);
        }

        public static void Log_InputCont(string msg, LogLevel level = LogLevel.Standard) {
            Log("INPUT-CONT", c_backendGroup, msg, level);
        }

        public static void Log_MageMatch(string msg, LogLevel level = LogLevel.Standard) {
            Log("MAGE MATCH", "teal", msg, level);
        }

        public static void Log_Player(string msg, LogLevel level = LogLevel.Standard) {
            Log("PLAYER", c_playerGroup, msg, level);
        }

        public static void Log_SyncMan(string msg, LogLevel level = LogLevel.Standard) {
            Log("SYNC-MAN", c_backendGroup, msg, level);
        }

        public static void Log_Targeting(string msg, LogLevel level = LogLevel.Standard) {
            Log("TARGETING", c_backendGroup, msg, level);
        }

        public static void Log_TileBehav(string msg, LogLevel level = LogLevel.Standard) {
            Log("TILE BEHAV", c_dataStructure, msg, level);
        }

        public static void Log_UICont(string msg, LogLevel level = LogLevel.Standard) {
            Log("UI-CONT", c_userExperience, msg, level);
        }

        // ---------------------------------------------------------------------------------

        public static void Log_Enfuego(string msg, LogLevel level = LogLevel.Standard) {
            Log("ENFUEGO", c_playerGroup, msg, level);
        }

        public static void Log_Gravekeeper(string msg, LogLevel level = LogLevel.Standard) {
            Log("GRAVEKEEPER", c_playerGroup, msg, level);
        }

    }
}
