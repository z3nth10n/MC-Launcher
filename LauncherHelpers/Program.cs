using LauncherAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace LauncherHelpers
{
    internal class Program
    {
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
                string fversion = Path.Combine(API.AssemblyFolderPATH, "fversion.json");

                //Prepare objects
                //Aunque esto deberia salir del metodo 1
                JObject jobj = JObject.Parse(File.Exists(fversion) ? File.ReadAllText(fversion) : Convert.ToString(Properties.Resources.fversion));
                IEnumerable<string> rvers = jobj["recognizedVersions"].Cast<JValue>().Select(x => x.ToString());

                switch (opt)
                {
                    case 1:
                        Console.Clear();
                        API.GenerateWeights(fversion);
                        break;

                    case 2:
                        if (!File.Exists(fversion))
                        {
                            API.WriteLineStop("Please, you must generate fversion file, to do this you have to select option 1.");
                            App();
                            return;
                        }

                        //Start parsing...
                        //Select valid jars
                        IEnumerable<FileInfo> files = API.GetValidJars();

                        Console.WriteLine("There are {0} valid versions: {1}", files.Count(), string.Join(", ", files.Select(x => x.Name)));
                        Console.WriteLine();

                        string retobj = null;
                        JArray jArray = new JArray();

                        foreach (FileInfo file in files)
                        {
                            //Console.WriteLine("Parsing file: {0}; Ext: {1}", file.Name, file.Extension);
                            JObject deeper = null;
                            string version = API.GetVersionFromMinecraftJar(file, rvers, jobj, out deeper);
                            jArray.Add(deeper != null ? deeper : API.AddObject(file.Name, version));
                        }

                        if (jArray != null)
                        {
                            retobj = jArray.ToString();
                            File.WriteAllText(API.Base64PATH, retobj);
                        }
                        break;

                    case 3:
                        //First, we have to select the wanted version, in my case, I will do silly things to select the desired version...

                        KeyValuePair<string, string> selVersion = default(KeyValuePair<string, string>);
                        if (File.Exists(API.Base64PATH))
                        {
                            JArray jArr = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(API.Base64PATH));

                            if (jArr.Count == 1)
                                selVersion = new KeyValuePair<string, string>(jArr[0]["filename"].ToString(), jArr[0]["version"].ToString());
                            else
                            {
                                Console.WriteLine("There are several files in this folder, please select one of them:");
                                Console.WriteLine();

                                int i = 1;
                                Dictionary<string, string> filver = jArr.Cast<JToken>().ToDictionary(x => x["filename"].ToString(), x => x["version"].ToString());
                                foreach (var entry in filver)
                                {
                                    Console.WriteLine("{0}.- {1} ({2})", i, entry.Key, entry.Value);
                                    ++i;
                                }

                                Console.WriteLine();
                                Console.Write("Select one of them: ");
                                string opt1 = Console.ReadLine();

                                int num = 0;
                                if (int.TryParse(opt1, out num))
                                    selVersion = filver.ElementAt(num - 1);
                                else
                                {
                                    API.WriteLineStop("Please specify a numeric value.");
                                    App();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            //Introducir version manualmente
                            Console.Write("There isn't any reference, write a recognized version: ");
                            string version = Console.ReadLine();

                            if (rvers.Contains(version))
                                selVersion = new KeyValuePair<string, string>(version, version); //WIP ... Esto no deberia ser asi
                            else
                            {
                                API.WriteLineStop("Unrecognized versions, please restart...");
                                App();
                                return;
                            }
                        }

                        //Then, start downloading...

                        string nativesDir = Path.Combine(API.AssemblyFolderPATH, "natives");
                        API.PreviousChk(nativesDir);

                        Console.WriteLine();
                        //Generate natives
                        switch (API.GetSO())
                        {
                            case OS.Windows:
                                API.DownloadFile(Path.Combine(nativesDir, "lwjgl.dll"), "https://build.lwjgl.org/release/latest/windows/x86/lwjgl32.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "lwjgl64.dll"), "https://build.lwjgl.org/release/latest/windows/x64/lwjgl.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "OpenAL32.dll"), "https://build.lwjgl.org/release/latest/windows/x86/OpenAL32.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "OpenAL64.dll"), "https://build.lwjgl.org/release/latest/windows/x64/OpenAL.dll");

                                //JInput
                                API.DownloadFile(Path.Combine(nativesDir, "jinput-dx8.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-dx8.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "jinput-dx8_64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-dx8_64.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "jinput-raw.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-raw.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "jinput-raw_64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-raw_64.dll");

                                //WinTab case

                                if (Environment.Is64BitOperatingSystem)
                                    API.DownloadFile(Path.Combine(nativesDir, "jinput-wintab.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-wintab.dll");
                                else
                                    API.DownloadFile(Path.Combine(nativesDir, "jinput-wintab.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-wintab.dll");

                                //SAPIWrapper only if version is 1.12.2 or newer... (By the moment only Windows)
                                API.DownloadFile(Path.Combine(nativesDir, "SAPIWrapper_x64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/SAPIWrapper/windows/SAPIWrapper_x64.dll");
                                API.DownloadFile(Path.Combine(nativesDir, "SAPIWrapper_x86.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/SAPIWrapper/windows/SAPIWrapper_x86.dll");
                                break;

                            case OS.Linux:
                                //WIP
                                break;

                            case OS.OSx:
                                //WIP
                                break;
                        }

                        //Download common jars...

                        API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, "jinput.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/jinput.jar");
                        API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, "lwjgl.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/lwjgl.jar");
                        API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, "lwjgl_util.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/lwjgl_util.jar");

                        Console.WriteLine();

                        //Generate libraries

                        //First we have to download the desired json...
                        string jsonPath = API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, Path.GetFileNameWithoutExtension(selVersion.Key) + ".json"), string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.json", selVersion.Value));
                        JObject jObject = JObject.Parse(File.ReadAllText(jsonPath));

                        Console.WriteLine();

                        //First we have to identify if we are on bin or in versions folder, to get the root
                        string lPath = API.GetLibPath();

                        string ff = Path.Combine(API.AssemblyFolderPATH, selVersion.Key);
                        if (!API.IsValidJAR(ff))
                        {
                            //If, ie, this is a forge jar, we need to download the original minecraft version
                            API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, selVersion.Value + ".jar"), jObject["downloads"]["client"]["url"].ToString());

                            //Check if this a forge version
                            JObject forgeObj = API.GetForgeVersion(ff);

                            //Aqui tenemos que descargar las librerias del forge

                            foreach (var lib in forgeObj["libraries"].OfType<JObject>())
                            {
                                string name = lib["name"].ToString();
                                //Aqui hay que comprobar de varios sitios:

                                //http://central.maven.org/maven2/org/scala-lang/modules/scala-xml_2.11/1.0.2/
                                //http://files.minecraftforge.net/maven/
                                //y...
                                //http://store.ttyh.ru/ o ... github: https://github.com/ZZona-Dummies/jinput/raw/master/libraries/commons-codec/commons-codec/1.10/commons-codec-1.10.jar

                                string urlRepo = API.GetUrlFromLibName(name),
                                       libPath = Path.Combine(lPath, API.GetPathFromLibName(name, true));

                                if (!string.IsNullOrEmpty(urlRepo))
                                    API.DownloadFile(libPath, urlRepo);
                                else
                                    Console.WriteLine("Lib ({0}) hasn't valid url repo!! (Path: {1})", name, libPath); //Este salta solo para descargar el forge cosa que no hace falta porque ya está descargado asi que wala... A no ser que sea el instalador
                            }

                            Console.WriteLine();
                        }

                        //Then, with the JSON we will start to download libraries...
                        //Libraries are divided into artifacts and classifiers...

                        Console.WriteLine("LibPath: {0}", lPath);
                        Console.WriteLine();

                        if (!string.IsNullOrEmpty(lPath))
                        {
                            foreach (var lib in jObject["libraries"])
                            {
                                JToken dl = lib["downloads"],
                                       clssf = dl["classifiers"],
                                       artf = dl["artifact"];

                                if (clssf != null)
                                {
                                    foreach (var child in clssf.Children())
                                    {
                                        string name = child.Path.Substring(child.Path.LastIndexOf('.') + 1),
                                               soid = API.GetSO().ToString().ToLower();

                                        if (name.Contains(soid))
                                        { //With this, we ensure that we select "natives-windows" (in my case)
                                            var nats = child.OfType<JObject>();
                                            foreach (var tok in nats)
                                            {
                                                //Here we have every object...
                                                try
                                                {
                                                    API.DownloadFile(Path.Combine(lPath, API.CleverBackslashes(tok["path"].ToString())), tok["url"].ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine("Couldn't download classifier!! (DL-Path: {0})", dl.Path);
                                                    Console.WriteLine(ex);
                                                    Console.WriteLine();
                                                }
                                            }
                                        }
                                    }
                                }

                                //Download artifact...
                                try
                                {
                                    API.DownloadFile(Path.Combine(lPath, artf["path"].ToString().Replace('/', '\\')), artf["url"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Couldn't download artifact!! (DL-Path: {0})", dl.Path);
                                    Console.WriteLine(ex);
                                    Console.WriteLine();
                                }
                            }

                            Console.WriteLine();
                        }
                        else
                        {
                            API.WriteLineStop("Invalid instalation path, please move this executable next to a valid JAR file (minecraft.jar, forge.jar, etc...)");
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

                        Console.WriteLine("Execution: {0}", "java or java_path");
                        Console.WriteLine(@"Parameters: -Xmx{0}M -Xms{1}M -Xmn{1}M -Djava.library.path=""{2}"" -cp ""{3}"" -Dfml.ignoreInvalidMinecraftCertificates = true -Dfml.ignorePatchDiscrepancies = true -XX:+UseConcMarkSweepGC -XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy net.minecraft.client.main.Main --accessToken FML --userProperties { } --version {4} --username {5}",
                                            (API.GetTotalMemoryInBytes() / (Math.Pow(1024, 2) * 2)).ToString("F0"),
                                            (API.GetTotalMemoryInBytes() / (Math.Pow(1024, 2) * 16)).ToString("F0"),
                                            Path.Combine(API.AssemblyFolderPATH, "natives"),
                                            API.GetAllLibs(),
                                            API.GetVersionFromMinecraftJar(API.GetValidJars().ElementAt(0).FullName));
                        break;

                    case 5:
                        Environment.Exit(0);
                        break;

                    default:
                        API.WriteLineStop("Invalid option!");
                        App();
                        return;
                }
            }
            else
            {
                API.WriteLineStop("Please specify a numeric value.");
                App();
                return;
            }
        }
    }
}