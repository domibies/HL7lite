using System;
using System.IO;
using System.Reflection;

namespace HL7Lite.Test
{
    public static class SamplesPath
    {
        public static string Value => Path.GetDirectoryName(typeof(SamplesPath).GetTypeInfo().Assembly.Location) + "/";
    }
}
