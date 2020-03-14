
using System;
using System.IO;

namespace selfiebot
{
    public class Config
    {
        public static String PhotoTempPath =
            Path.Combine(System.Environment.CurrentDirectory, "TEMP");
        public static String PhotoDeletePath =
            Path.Combine(System.Environment.CurrentDirectory, "DEL");

        public static String PhotoPornPath =
            Path.Combine(System.Environment.CurrentDirectory, "PORN");
        public static String PhotoRTPath =
            Path.Combine(System.Environment.CurrentDirectory, "RT");

        public static String PhotoSEXYPath =
            Path.Combine(System.Environment.CurrentDirectory, "SEXY");


    }
}