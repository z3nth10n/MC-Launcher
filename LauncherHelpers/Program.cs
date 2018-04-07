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
            Console.WriteLine("4.- Exit");
            Console.WriteLine();
            Console.Write("Select what do you want to do: ");
            string c = Console.ReadLine();
            Console.WriteLine();

            int opt = 0;
            if (int.TryParse(c, out opt))
            {
                string fversion = Path.Combine(API.AssemblyFolderPATH, "fversion.json");

                //Prepare objects
                JObject jobj = JObject.Parse(File.ReadAllText(fversion));
                IEnumerable<string> rvers = jobj["recognizedVersions"].Cast<JValue>().Select(x => x.ToString());

                switch (opt)
                {
                    case 1:
                        //Get estimated version from weight from jsons
                        //Esto se ejecutará si por ejemplo, el minecraft.jar no se encuentra, pero hay un jar de >1MB, con esto podremos estimar la version
                        //Para luego sacar las librerias y todo el embrollo ese
                        //Hacer esto cada semana, para que no se quede obsoleto el asunto
                        Console.Clear();

                        //Define folder of download
                        string fold = Path.Combine(API.AssemblyFolderPATH, "Versions");

                        if (!Directory.Exists(fold))
                            Directory.CreateDirectory(fold);

                        using (WebClient wc = new WebClient())
                        {
                            try
                            {
                                string json = wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json"), json2;

                                JObject jparse = JObject.Parse(json);

                                JArray arr2 = new JArray(), arr3 = new JArray();

                                //Para saber si debemos regenerar este archivo deberemos comprobar el ultimo archivo generado (su latest) con el de este...
                                //Simplemente lo que se debe de hacer es que en el OnLoad del Launcher dumpear el archivo incustrado en los recursos y ya comprobamos lo que estamos hablando

                                foreach (var j in jparse["versions"])
                                {
                                    string url = j["url"].ToString(), fname = Path.Combine(fold, Path.GetFileName(url));

                                    if (!File.Exists(fname))
                                        using (WebClient wc1 = new WebClient())
                                        {
                                            try
                                            {
                                                Console.WriteLine("Downloading {0}...", url);
                                                json2 = wc1.DownloadString(url);
                                                File.WriteAllText(fname, json2);
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Wrong url!!");
                                                return;
                                            }
                                        }
                                    else
                                        json2 = File.ReadAllText(fname);

                                    JObject json2obj = JObject.Parse(json2);

                                    //Now, we have to parse individual files
                                    arr2.Add(new JObject(new JProperty("id", j["id"]),
                                                         new JProperty("size", json2obj["downloads"]["client"]["size"])));

                                    //Add recognized names...
                                    arr3.Add(j["id"]);
                                }

                                File.WriteAllText(fversion, new JObject(new JProperty("creationTime", DateTime.Now), new JProperty("latest", jparse["latest"]), new JProperty("versions", arr2), new JProperty("recognizedVersions", arr3)).ToString());

                                Console.WriteLine("File generated succesfully!!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("No internet conn!!");
                                Console.WriteLine(ex);
                                return;
                            }
                        }
                        break;

                    case 2:
                        if (!File.Exists(fversion))
                        {
                            API.WriteLineStop("Please, you must generate fversion file, to do this you have to select option 1.");
                            App();
                            return;
                        }

                        Dictionary<string, int> weights = jobj["versions"].Cast<JObject>().ToDictionary(x => x["id"].ToString(), x => int.Parse(x["size"].ToString()));

                        //Start parsing...
                        DirectoryInfo dir = new DirectoryInfo(API.AssemblyFolderPATH);

                        //Select valid jars
                        IEnumerable<FileInfo> files = dir.GetFiles().Where(file => file.Extension == ".jar" && file.Length > 1024 * 1024);

                        Console.WriteLine("There are {0} valid versions: {1}", files.Count(), string.Join(", ", files.Select(x => x.Name)));
                        Console.WriteLine();

                        string retobj = null;
                        JArray jArray = new JArray();

                        foreach (FileInfo file in files)
                        {
                            //Console.WriteLine("Parsing file: {0}; Ext: {1}", file.Name, file.Extension);

                            string woutext = file.Name.Replace(file.Extension, "");
                            if (rvers.Contains(woutext))
                            {
                                Console.WriteLine("Found recognized version: {0}", woutext);

                                jArray.Add(AddObject(file.Name, woutext));
                                continue;
                            }

                            try
                            {
                                bool validJAR = false;

                                validJAR = (bool)API.ReadJAR(file.FullName, (zipfile, item, valid) =>
                                {
                                    //DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    //string ss = entry.Info.Substring(entry.Info.IndexOf("Timeblob"));
                                    //Console.WriteLine(epoch.AddSeconds(int.Parse(ss.Substring(0, ss.IndexOf('\n')).Replace("Timeblob: 0x", ""), NumberStyles.HexNumber)).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

                                    if (valid)
                                        return true;

                                    return false;
                                });

                                if (validJAR)
                                {
                                    Console.WriteLine("Analyzing valid JAR called {0}", file.Name);
                                    Console.WriteLine();
                                    int dkb = 1; //Estimated balanced weight
                                    weights = weights.Where(x => x.Value.Between(file.Length - (dkb * 1024), file.Length + (dkb * 1024))).ToDictionary(x => x.Key, x => x.Value);

                                    if (weights.Count == 1)
                                    { //Aqui devolvemos la key del elemento 0
                                        Console.WriteLine(weights.ElementAt(0).Value != file.Length ? "There is a posible version: {0}" : "The desired version is {0}", weights.ElementAt(0).Key);

                                        jArray.Add(AddObject(file.Name, weights.ElementAt(0).Key));
                                    }
                                    else if (weights.Count > 1)
                                    {
                                        Console.WriteLine("There are diferent versions that differs {0}KB from the local JAR file", dkb);
                                        weights.ForEach((x) =>
                                        {
                                            Console.WriteLine("{0}: {1} bytes (diff: {2} bytes)", x.Key, x.Value, Math.Abs(file.Length - x.Value));

                                            //We have to discard, because we want only one result from DeeperSearch...
                                            //If filename is not empty that means we want to dump into a file
                                            //if (!string.IsNullOrEmpty(file.Name))
                                            //    jArray.Add(new JObject(new JProperty("filename", file.Name), new JProperty("version", x.Key)));
                                        });

                                        Console.WriteLine();
                                        Console.WriteLine("Searching for a maching version...");
                                        Console.WriteLine();

                                        jArray.Add(DeeperSearch(file, weights.Keys.AsEnumerable(), true));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Your Minecraft JAR has changed a lot to be recognized as any established version, by this reason, we will make a deeper search... (Weight: {0})", file.Length);
                                        Console.WriteLine();
                                        //Raro que el minecraft-.jar esté aqui

                                        jArray.Add(DeeperSearch(file, rvers, true));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid version found ({0}), maybe this is a forge version ;-) ;-)", file.Name);
                                    Console.WriteLine();

                                    string validKey = (string)API.ReadJAR(file.FullName, (zipfile, item, valid) =>
                                    {
                                        if (!item.IsDirectory && item.Name == "version.json")
                                        {
                                            using (StreamReader s = new StreamReader(zipfile.GetInputStream(item)))
                                            {
                                                // stream with the file
                                                string contents = s.ReadToEnd();

                                                JObject obj = JObject.Parse(contents);
                                                return obj["jar"].ToString();
                                            }
                                        }

                                        return null;
                                    }, (item) => true);

                                    MsgValidKey(validKey);

                                    jArray.Add(AddObject(file.Name, validKey));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Not real JAR file!!");
                                Console.WriteLine(ex);
                            }
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

                        if (!API.IsValidJAR(Path.Combine(API.AssemblyFolderPATH, selVersion.Key)))
                        {
                            //If, ie, this is a forge jar, we need to download the original minecraft version
                            API.DownloadFile(Path.Combine(API.AssemblyFolderPATH, selVersion.Value + ".jar"), jObject["downloads"]["client"]["url"].ToString());
                            Console.WriteLine();
                        }

                        //Then, with the JSON we will start to download libraries...
                        //Libraries are divided into artifacts and classifiers...

                        //First we have to identify if we are on bin or in versions folder, to get the root
                        string libpath = "";

                        if (API.AssemblyFolderPATH.Contains("bin"))
                            libpath = Path.Combine(API.AssemblyFolderPATH.GetUpperFolders(), "libraries");
                        else if (API.AssemblyFolderPATH.Contains("versions"))
                            libpath = Path.Combine(API.AssemblyFolderPATH.GetUpperFolders(2), "libraries");

                        Console.WriteLine("LibPath: {0}", libpath);
                        Console.WriteLine();

                        if (!string.IsNullOrEmpty(libpath))
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
                                                    API.DownloadFile(Path.Combine(libpath, tok["path"].ToString().Replace('/', '\\')), tok["url"].ToString());
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
                                    API.DownloadFile(Path.Combine(libpath, artf["path"].ToString().Replace('/', '\\')), artf["url"].ToString());
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

        private static JObject DeeperSearch(FileInfo file, IEnumerable<string> col, bool dump = false)
        {
            string validKey = (string)API.ReadJAR(file.FullName, (zipfile, item, valid) =>
            {
                using (StreamReader s = new StreamReader(zipfile.GetInputStream(item)))
                {
                    // stream with the file
                    string contents = s.ReadToEnd();

                    foreach (string x in col)
                        if (item.Name.Contains(".class") && contents.Contains(x))
                        {
                            Console.WriteLine("Found valid version in entry {0} (Key: {1})", item.Name, x);
                            Console.WriteLine();
                            return x;
                        }
                }

                return false;
            });

            MsgValidKey(validKey, col);

            if (dump)
                return AddObject(file.Name, validKey);
            else
                return null;
        }

        private static void MsgValidKey(string validKey, IEnumerable<string> col = null)
        {
            if (!string.IsNullOrEmpty(validKey))
                Console.WriteLine("Found valid version: {0}", validKey);
            else
                Console.WriteLine("No version found in any of the {0} files!!", col == null ? 1 : col.Count()); //Aqui dariamos a elegir al usuario

            Console.WriteLine();
        }

        private static JObject AddObject(string filename, string version)
        {
            return new JObject(new JProperty("filename", filename), new JProperty("version", version));
        }
    }
}