using LauncherAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LauncherHelpers
{
    internal class Program
    {
        protected Program()
        {
        }

        private static void Main(string[] args)
        {
            App(false, true);
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static void App(bool clear = true, bool main = false)
        {
            if (clear)
                Console.Clear();
            else if (!clear && !main)
                Console.WriteLine();

            Console.WriteLine("1.- Generate weights from versions");
            Console.WriteLine("2.- Try to identify a version");
            Console.WriteLine("3.- Download JSONs, libraries and natives");
            Console.WriteLine("4.- Get execution parameters");
            Console.WriteLine("5.- Exit");
            Console.WriteLine();
            Console.Write("Select what do you want to do: ");
            string c = Console.ReadLine();
            Console.WriteLine();

            int opt = 0;
            if (int.TryParse(c, out opt))
            {
                string fversion = Path.Combine(APIBasics.AssemblyFolderPATH, "fversion.json");

                //Prepare objects
                //Aunque esto deberia salir del metodo 1
                JObject jobj = JObject.Parse(File.Exists(fversion) ? File.ReadAllText(fversion) : Convert.ToString(Properties.Resources.fversion));
                IEnumerable<string> rvers = jobj["recognizedVersions"].Cast<JValue>().Select(x => x.ToString());

                switch (opt)
                {
                    case 1:
                        Console.Clear();
                        APILauncher.GenerateWeights(fversion);
                        break;

                    case 2:
                        if (!File.Exists(fversion))
                        {
                            APIBasics.WriteLineStop("Please, you must generate fversion file, to do this you have to select option 1.");
                            App();
                            return;
                        }

                        //Start parsing...
                        //Select valid jars
                        IEnumerable<FileInfo> files = APILauncher.GetValidJars();

                        Console.WriteLine("There are {0} valid versions: {1}", files.Count(), string.Join(", ", files.Select(x => x.Name)));
                        Console.WriteLine();

                        string retobj = null;
                        JArray jArray = new JArray();

                        foreach (FileInfo file in files)
                        {
                            JObject deeper = null;
                            string version = APILauncher.GetVersionFromMinecraftJar(file, rvers, jobj, out deeper);
                            jArray.Add(deeper != null ? deeper : APILauncher.AddObject(file.Name, version));
                        }

                        if (jArray.Equals(null))
                        {
                            retobj = jArray.ToString();
                            File.WriteAllText(APIBasics.Base64PATH, retobj);
                        }
                        break;

                    case 3:
                        string errorStr = APILauncher.DownloadLibraries(rvers);

                        if (!string.IsNullOrEmpty(errorStr))
                        {
                            APIBasics.WriteLineStop(errorStr);
                            App();
                            return;
                        }
                        break;

                    case 4:

                        //{0} = RAM / 2
                        //{1} = RAM / 16
                        //{2} = D:\JUEGOS\Minecraft\Minecraft Pirata\versions\1.12.2\natives
                        //{3} = D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\mojang\patchy\1.1\patchy - 1.1.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\oshi - project\oshi - core\1.1\oshi - core - 1.1.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\net\java\dev\jna\jna\4.4.0\jna - 4.4.0.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\net\java\dev\jna\platform\3.4.0\platform - 3.4.0.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\ibm\icu\icu4j - core - mojang\51.2\icu4j - core - mojang - 51.2.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\net\sf\jopt - simple\jopt - simple\5.0.3\jopt - simple - 5.0.3.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\paulscode\codecjorbis\20101023\codecjorbis - 20101023.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\paulscode\codecwav\20101023\codecwav - 20101023.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\paulscode\libraryjavasound\20101123\libraryjavasound - 20101123.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\paulscode\librarylwjglopenal\20100824\librarylwjglopenal - 20100824.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\paulscode\soundsystem\20120107\soundsystem - 20120107.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\io\netty\netty - all\4.1.9.Final\netty - all - 4.1.9.Final.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\google\guava\guava\21.0\guava - 21.0.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\commons\commons - lang3\3.5\commons - lang3 - 3.5.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\commons - io\commons - io\2.5\commons - io - 2.5.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\commons - codec\commons - codec\1.10\commons - codec - 1.10.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\net\java\jinput\jinput\2.0.5\jinput - 2.0.5.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\net\java\jutils\jutils\1.0.0\jutils - 1.0.0.jar; D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\google\code\gson\gson\2.8.0\gson-2.8.0.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\mojang\authlib\1.5.25\authlib-1.5.25.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\mojang\realms\1.10.19\realms-1.10.19.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\commons\commons-compress\1.8.1\commons-compress-1.8.1.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\httpcomponents\httpclient\4.3.3\httpclient-4.3.3.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\commons-logging\commons-logging\1.1.3\commons-logging-1.1.3.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\httpcomponents\httpcore\4.3.2\httpcore-4.3.2.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\it\unimi\dsi\fastutil\7.1.0\fastutil-7.1.0.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\logging\log4j\log4j-api\2.8.1\log4j-api-2.8.1.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\apache\logging\log4j\log4j-core\2.8.1\log4j-core-2.8.1.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\lwjgl\lwjgl\lwjgl\2.9.4-nightly-20150209\lwjgl-2.9.4-nightly-20150209.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\org\lwjgl\lwjgl\lwjgl_util\2.9.4-nightly-20150209\lwjgl_util-2.9.4-nightly-20150209.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\libraries\com\mojang\text2speech\1.10.3\text2speech-1.10.3.jar;D:\JUEGOS\Minecraft\Minecraft Pirata\versions\1.12.2\1.12.2.jar
                        //{4} = Version
                        //{5} = Username
                        //{6} = {{ }}

                        Console.WriteLine(APILauncher.GenerateLaunchProccess().Arguments);
                        break;

                    case 5:
                        Environment.Exit(0);
                        break;

                    default:
                        APIBasics.WriteLineStop("Invalid option!");
                        App();
                        return;
                }
            }
            else
            {
                APIBasics.WriteLineStop("Please specify a numeric value.");
                App();
            }
        }
    }
}