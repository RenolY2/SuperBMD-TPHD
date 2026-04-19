using System;
using System.Globalization;
using System.Threading;
using SuperBMDLib;
using Newtonsoft.Json;
using System.IO;

namespace SuperBMDLib
{
    class Program
    {
        static void Main(string[] args)
        {
            // Prevents issues with reading/writing floats on European systems.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("", false);

            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                DisplayHelp();
                return;
            }

            var currpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var configpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currpath), "config.json");

            Arguments cmd_args = new Arguments(args);

            JsonSerializer serializer = new JsonSerializer();

            serializer.Converters.Add(
                (new Newtonsoft.Json.Converters.StringEnumConverter())
            );
            Console.WriteLine(configpath);
            try
            {
                using (TextReader file = File.OpenText(configpath))
                {
                    Console.WriteLine("Config found, loading arguments.");
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        var configargs = serializer.Deserialize<Arguments>(reader);

                        if (configargs.tphd_compatibility)
                        {
                            cmd_args.tphd_compatibility = true;
                            Console.WriteLine("Config: TPHD Compatibility enabled");
                        }

                        cmd_args.vertextype = configargs.vertextype;
                        cmd_args.fraction = configargs.fraction;
                        Console.WriteLine("Config: Adjusted vertex type and fraction");
                    }
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine("No config found, continuing.");
            }

            Model mod = Model.Load(cmd_args);

            if (cmd_args.input_path.EndsWith(".bmd") || cmd_args.input_path.EndsWith(".bdl"))
                mod.ExportAssImp(cmd_args.output_path, "dae", new ExportSettings());
            else
                mod.ExportBMD(cmd_args.output_path, cmd_args.output_bdl);
        }

        /// <summary>
        /// Prints credits and argument descriptions to the console.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine();
            Console.WriteLine("SuperBMD: A tool to import and export various 3D model formats into the Binary Model (BMD or BDL) format.");
            Console.WriteLine("Written by Sage_of_Mirrors/Gamma (@SageOfMirrors) and Yoshi2/RenolY2.");
            Console.WriteLine("Made possible with help from arookas, LordNed, xDaniel, and many others.");
            Console.WriteLine("Visit https://github.com/Sage-of-Mirrors/SuperBMD/wiki for more information.");
            Console.WriteLine();
            Console.WriteLine("Usage: SuperBMD.exe -i/--input filePath [-o/--output filePath] [-m/materialPresets filePath]\n" +
                              "       [-x/--texHeaders filePath] [-t/--tristrip mode] [-r/--rotate] [-b/--bdl]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("\t-i/--input              filePath\tPath to the input file, either a BMD/BDL file or a DAE model.");
            Console.WriteLine("\t-o/--output             filePath\tPath to the output file.");
            Console.WriteLine("\t-m/--materialPresets    filePath\tPath to the material presets JSON for DAE to BMD conversion.");
            Console.WriteLine("\t-x/--textureHeaders     filePath\tPath to the texture headers JSON for DAE to BMD conversion.");
            Console.WriteLine("\t-t/--tristrip           mode\t\tMode for tristrip generation.");
            Console.WriteLine("\t\tstatic: Only generate tristrips for static (unrigged) meshes.");
            Console.WriteLine("\t\tall:    Generate tristrips for all meshes.");
            Console.WriteLine("\t\tnone:   Do not generate tristrips.");
            Console.WriteLine();
            Console.WriteLine("\t-r/--rotate\t\t\t\tRotate the model from Z-up to Y-up orientation.");
            Console.WriteLine("\t-b/--bdl\t\t\t\tGenerate a BDL instead of a BMD.");
        }
    }
}
