using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace UnityEngine {
    public enum LogCategories {
        Round,
        Game,
        GameManager,
    }

    public static class Console {
        static Dictionary<LogCategories, Color> colors =
                new Dictionary<LogCategories, Color>() {
                {LogCategories.Round, new Color(0.2f, 0.650f, 1)},
                {LogCategories.Game, new Color(0.2f, 0.376f, 1)},
                {LogCategories.GameManager, new Color(0.498f, 0.2f, 1)}
        };

        #region Log.
        [Conditional("DEBUG")]
        public static void Log(object message) {
            Debug.Log(FormatMessage(Color.white, message));
        }

        [Conditional("DEBUG")]
        public static void Log(LogCategories category, object message) {
            if (colors.ContainsKey(category)) {
                Debug.Log(FormatMessageWithCategory(colors[category], category.ToString(), message));
            } else {
                Debug.Log(FormatMessageWithCategory(Color.white, category.ToString(), message));
            }
        }

        [Conditional("DEBUG")]
        public static void LogFormat(string format, params object[] args) {
            Debug.Log(FormatMessage(Color.white, string.Format(format, args)));
        }

        [Conditional("DEBUG")]
        public static void LogFormat(LogCategories category, string format, params object[] args) {
            if (colors.ContainsKey(category)) {
                Debug.Log(FormatMessageWithCategory(colors[category], category.ToString(), string.Format(format, args)));
            } else {
                Debug.Log(FormatMessageWithCategory(Color.white, category.ToString(), string.Format(format, args)));
            }
        }
        #endregion

        #region Warning.
        [Conditional("DEBUG")]
        public static void LogWarning(object message) {
            Debug.LogWarning(FormatMessage(Color.yellow, message));
        }

        [Conditional("DEBUG")]
        public static void LogWarning(LogCategories category, object message) {
            if (colors.ContainsKey(category)) {
                Debug.LogWarning(FormatMessageWithCategory(colors[category], category.ToString(), message));
            } else {
                Debug.LogWarning(FormatMessageWithCategory(Color.yellow, category.ToString(), message));
            } 
        }

        [Conditional("DEBUG")]
        public static void LogWarningFormat(string format, params object[] args) {
            Debug.LogWarningFormat(FormatMessage(Color.yellow, string.Format(format, args)));
        }

        [Conditional("DEBUG")]
        public static void LogWarningFormat(LogCategories category, string format, params object[] args) {
            if (colors.ContainsKey(category)) {
                Debug.LogWarningFormat(FormatMessageWithCategory(colors[category], category.ToString(), string.Format(format, args)));
            } else {
                Debug.LogWarningFormat(FormatMessageWithCategory(Color.yellow, category.ToString(), string.Format(format, args)));
            }
        }
        #endregion

        #region Error.
        [Conditional("DEBUG")]
        public static void LogError(object message) {
            Debug.LogError(FormatMessage(Color.red, message));
        }

        [Conditional("DEBUG")]
        public static void LogError(LogCategories category, object message) {
            if (colors.ContainsKey(category)) {
                Debug.LogError(FormatMessageWithCategory(colors[category], category.ToString(), message));
            } else {
                Debug.LogError(FormatMessageWithCategory(Color.red, category.ToString(), message));
            }
        }

        [Conditional("DEBUG")]
        public static void LogErrorFormat(string format, params object[] args) {
            Debug.LogErrorFormat(FormatMessage(Color.red, string.Format(format, args)));
        }

        [Conditional("DEBUG")]
        public static void LogErrorFormat(LogCategories category, string format, params object[] args) {
            if (colors.ContainsKey(category)) {
                Debug.LogErrorFormat(FormatMessageWithCategory(colors[category], category.ToString(), string.Format(format, args)));
            } else {
                Debug.LogErrorFormat(FormatMessageWithCategory(Color.red, category.ToString(), string.Format(format, args)));
            }
        }
        #endregion
        #region Exception.
        [Conditional("DEBUG")]
        public static void LogException(Exception exception) {
            Debug.LogError(FormatMessage(Color.red, exception.Message));
        }

        [Conditional("DEBUG")]
        public static void LogException(LogCategories category, Exception exception) {
            if (colors.ContainsKey(category)) {
                Debug.LogError(FormatMessageWithCategory(colors[category], category.ToString(), exception.Message));
            } else {
                Debug.LogError(FormatMessageWithCategory(Color.red, category.ToString(), exception.Message));
            }
        }
        #endregion


        private static string FormatMessage(Color color, object message) {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{message}</color>";
        }

        private static string FormatMessageWithCategory(Color color, string category, object message) {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}><b>[{category}]</b> {message}</color>";
        }
    }
}