using System;
using System.Reflection;

var asm = Assembly.LoadFrom(@"C:\Users\Usuario\.nuget\packages\microsoft.agents.ai.workflows\1.0.0-rc4\lib\net8.0\Microsoft.Agents.AI.Workflows.dll");
foreach (var t in asm.GetTypes()) { Console.WriteLine(t.FullName + " : " + (t.BaseType?.FullName ?? "none")); }
