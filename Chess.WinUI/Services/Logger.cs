using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.Services
{
    public static class Logger
    {
        public static void LogInfo(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string fullMethodName = GetMethodName(memberName, filePath);
            string className = GetClassName(filePath);

            EngineAPI.LogInfoWithCaller(message, fullMethodName, className, lineNumber);
        }


        public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string fullMethodName = GetMethodName(memberName, filePath);
            string className = GetClassName(filePath);

            EngineAPI.LogErrorWithCaller(message, fullMethodName, className, lineNumber);
        }


        public static void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string fullMethodName = GetMethodName(memberName, filePath);
            string className = GetClassName(filePath);

            EngineAPI.LogWarningWithCaller(message, fullMethodName, className, lineNumber);
        }


        public static void LogDebug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string fullMethodName = GetMethodName(memberName, filePath);
            string className = GetClassName(filePath);

            EngineAPI.LogDebugWithCaller(message, fullMethodName, className, lineNumber);
        }


        private static string GetClassName(string filePath)
        {
            string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
            return className;
        }


        private static string GetMethodName(string memberName, string filePath)
        {
            string className = GetClassName(filePath);
            string methodName = memberName == ".ctor" ? className : memberName;     // .ctor is the constructor in C#

            string fullMethodName = $"{className}.{methodName}";
            return fullMethodName;
        }
    }
}
