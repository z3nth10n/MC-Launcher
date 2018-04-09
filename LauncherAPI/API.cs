﻿using ICSharpCode.SharpZipLib.Zip;
using MimeTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LauncherAPI
{
    public enum OS { Windows, Linux, OSx, Other }

    public static class API
    {
        private static bool _off, chk;

        public static void ForEach<T>(
this IEnumerable<T> source,
Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static void ForEachStop<T>(
this IEnumerable<T> source,
Func<T, bool> action)
        {
            foreach (T element in source)
                if (action(element))
                    break;
        }

        public static bool Between(this int val, int min, int max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this int val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static bool Between(this long val, long min, long max, bool exclusive = false)
        {
            if (!exclusive)
                return val >= min && val <= max;
            else
                return val > min && val < max;
        }

        public static OS GetSO()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OS.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OS.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OS.OSx;
            else
                return OS.Other;
        }

        public static object ReadJAR(string path, Func<ZipFile, ZipEntry, bool, object> jarAction, Func<ZipEntry, bool> func = null)
        {
            return ReadJAR<object>(path, jarAction, func);
        }

        public static T ReadJAR<T>(string path, Func<ZipFile, ZipEntry, bool, T> jarAction, Func<ZipEntry, bool> func = null)
        {
            T v = default(T);
            using (var zip = new ZipInputStream(File.OpenRead(path)))
            {
                using (ZipFile zipfile = new ZipFile(path))
                {
                    ZipEntry item;
                    while ((item = zip.GetNextEntry()) != null)
                    {
                        if (func == null)
                            func = (i) => !i.IsDirectory && i.Name == "net/minecraft/client/main/Main.class";

                        v = jarAction(zipfile, item, func(item));

                        if (v == null)
                            continue;

                        switch (v.GetType().Name.ToLower())
                        {
                            case "boolean":
                                if ((bool)(object)v)
                                    return (T)(object)true;
                                break;

                            case "string":
                                if (!string.IsNullOrEmpty((string)(object)v))
                                    return v;
                                else
                                {
                                    Console.WriteLine("String null reading jar!");
                                    return default(T);
                                }

                            default:
                                Console.WriteLine("Unrecognized type: {0} (returning... nervermind)", v.GetType().Name.ToLower());
                                return v; //Break before it continues and returns null value
                        }
                    }
                }
            }

            return v;
        }

        public static bool IsValidJAR(string file)
        {
            return ReadJAR(file, (zipfile, entry, valid) =>
            {
                //DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                //string ss = entry.Info.Substring(entry.Info.IndexOf("Timeblob"));
                //Console.WriteLine(epoch.AddSeconds(int.Parse(ss.Substring(0, ss.IndexOf('\n')).Replace("Timeblob: 0x", ""), NumberStyles.HexNumber)).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

                return valid;
            });
        }

        public static string Base64PATH
        {
            get
            {
                byte[] arr = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(LocalPATH, "Base64");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, Convert.ToBase64String(arr)) + ".json";

                return path;
            }
        }

        public static string LocalPATH
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "z3nth10n", "Launcher");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string AssemblyFolderPATH
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static bool OfflineMode
        {
            get
            {
                if (!chk)
                {
                    _off = !CheckForInternetConnection();
                    chk = true;
                }
                return _off;
            }
        }

        public static void ChkConn(object objState)
        {
            if (chk) chk = false;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool PreviousChk(string path)
        {
            bool isf = Directory.Exists(path) && IsDirectory(path);
            string fold = isf ? path : Path.GetDirectoryName(path);
            if (!Directory.Exists(fold))
                Directory.CreateDirectory(fold);

            if (!isf && File.Exists(path))
                return false;

            return true;
        }

        public static void UrlToLocalFile(string url, string path)
        {
            if (!PreviousChk(path))
                return;

            using (WebClient wc = new WebClient())
                wc.DownloadFile(url, path);
        }

        public static string URLToLocalFile(string url)
        {
            string fil = "";

            using (WebClient wc = new WebClient())
            {
                byte[] by = wc.DownloadData(url);
                string[] arr = Directory.GetFiles(LocalPATH);

                fil = Path.Combine(LocalPATH, string.Format("file{0}{1}",
                                            arr.Length,
                                            MimeTypeMap.GetExtension(GetContentType(url))));

                if (arr.Any(x => File.ReadAllBytes(x) != by))
                    File.WriteAllBytes(fil, by);
            }

            if (string.IsNullOrEmpty(fil))
                throw new Exception("Coudn't retrieve file name.");

            return fil;
        }

        public static string GetContentType(string url)
        {
            string contentType = "";

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (response != null)
                    contentType = response.ContentType;
            }

            return contentType;
        }

        public static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        public static void Shake(Form form)
        {
            var original = form.Location;
            var rnd = new Random();
            const int shake_amplitude = 10;
            for (int i = 0; i < 10; i++)
            {
                form.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
                System.Threading.Thread.Sleep(20);
            }
            form.Location = original;
        }

        public static void SoundBytes(byte[] arr)
        {
            using (MemoryStream ms = new MemoryStream(arr))
            {
                SoundPlayer simpleSound = new SoundPlayer(ms);
                simpleSound.Play();
            }
        }

        public static string DownloadFile(string path, string url, bool overwrite = false)
        {
            if (PreviousChk(path) || overwrite)
            {
                Console.WriteLine("Downloading '{0}' from '{1}', please wait...", Path.GetFileName(path), url.CleverSubstring());
                using (WebClient wc = new WebClient())
                    File.WriteAllBytes(path, wc.DownloadData(url));
            }
            else
                Console.WriteLine("File '{0}' already exists! Skipping...", Path.GetFileName(path));

            return path;
        }

        public static void WriteLineStop(string val = "")
        {
            WriteLineStop(val, null);
        }

        public static void WriteLineStop(string val, params object[] objs)
        {
            Console.WriteLine(val, objs);
            Console.Read();
        }

        public static void WriteStop(string val = "")
        {
            WriteStop(val, null);
        }

        public static void WriteStop(string val, params object[] objs)
        {
            Console.Write(val, objs);
            Console.Read();
        }

        public static string GetUpperFolders(this string cpath, int levels = 1)
        {
            if (!IsDirectory(cpath)) cpath = Path.GetDirectoryName(cpath);

            for (int i = 0; i < levels; ++i)
                cpath = Path.GetDirectoryName(cpath);

            return cpath;
        }

        public static bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static string CleverSubstring(this string str, int limit = 50)
        {
            return (str.Length >= limit ? (str.Substring(0, limit / 2) + "..." + str.Substring(str.Length - (limit / 2 - 1))) : str);
        }

        public static JObject GetForgeVersion(string path)
        {
            return ReadJAR(path, (zipfile, item, valid) =>
            {
                if (!item.IsDirectory && item.Name == "version.json")
                    using (StreamReader s = new StreamReader(zipfile.GetInputStream(item)))
                    {
                        string contents = s.ReadToEnd();
                        //Console.WriteLine(contents);

                        return (JObject)JsonConvert.DeserializeObject(contents);
                    }

                return null;
            }, (item) => true);
        }

        public static string GetUrlFromLibName(string name)
        {
            string namePath = GetPathFromLibName(name),
                   mavenUrl = string.Format("http://central.maven.org/maven2/{0}", namePath),
                   githubUrl = string.Format("https://github.com/ZZona-Dummies/MC-Dependencies/raw/master/libraries/{0}", namePath);

            if (RemoteFileExists(mavenUrl))
                return mavenUrl;
            else if (RemoteFileExists(githubUrl))
                return githubUrl;

            return "";
        }

        public static string GetPathFromLibName(string name, bool clever = false)
        {
            string relurl = name.Replace(name.Substring(0, name.IndexOf(':')), name.Substring(0, name.IndexOf(':')).Replace('.', '/')),
                   file = name.Substring(name.IndexOf(':') + 1).Replace(':', '-') + ".jar";

            relurl = relurl.Replace(':', '/');

            string ret = string.Format("{0}/{1}", relurl, file);
            return clever ? CleverBackslashes(ret) : ret;
        }

        public static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    //Returns TRUE if the Status code == 200
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public static string CleverBackslashes(string path)
        {
            return GetSO() != OS.Windows ? path : path.Replace('/', '\\');
        }

        public static string GetLibPath()
        {
            string libpath = "";

            if (AssemblyFolderPATH.Contains("bin"))
                libpath = Path.Combine(AssemblyFolderPATH.GetUpperFolders(), "libraries");
            else if (AssemblyFolderPATH.Contains("versions"))
                libpath = Path.Combine(AssemblyFolderPATH.GetUpperFolders(2), "libraries");

            return libpath;
        }

        public static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        public static string GetAllLibs(bool outputDebug = false)
        {
            string libFolder = GetLibPath(),
                   ret = "";

            foreach (string file in DirSearch(libFolder))
                ret += file + ";";

            if (!string.IsNullOrEmpty(ret))
            {
                string rr = ret.Substring(0, ret.Length - 1);
                if (outputDebug) Console.WriteLine(rr);
                return rr;
            }
            else
            {
                Console.WriteLine("Null lib folder!");
                return "";
            }
        }

        public static List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                        files.Add(f);
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return files;
        }

        public static string GetVersionFromMinecraftJar(string path)
        {
            JObject deeper = null;

            JObject jobj = JObject.Parse(GenerateWeights());
            IEnumerable<string> rvers = jobj["recognizedVersions"].Cast<JValue>().Select(x => x.ToString());

            return GetVersionFromMinecraftJar(new FileInfo(path), rvers, jobj, out deeper);
        }

        public static string GetVersionFromMinecraftJar(FileInfo file, IEnumerable<string> rvers, JObject jobj, out JObject deeper)
        {
            Dictionary<string, int> weights = jobj["versions"].Cast<JObject>().ToDictionary(x => x["id"].ToString(), x => int.Parse(x["size"].ToString()));

            string version = "",
                   woutext = file.Name.Replace(file.Extension, "");

            if (rvers.Contains(woutext))
            {
                Console.WriteLine("Found recognized version: {0}", woutext);

                deeper = null;
                return version;
            }

            try
            {
                if (IsValidJAR(file.FullName))
                {
                    Console.WriteLine("Analyzing valid JAR called {0}", file.Name);
                    Console.WriteLine();
                    int dkb = 1; //Estimated balanced weight
                    weights = weights.Where(x => x.Value.Between(file.Length - (dkb * 1024), file.Length + (dkb * 1024))).ToDictionary(x => x.Key, x => x.Value);

                    if (weights.Count == 1)
                    {
                        Console.WriteLine(weights.ElementAt(0).Value != file.Length ? "There is a posible version: {0}" : "The desired version is {0}", weights.ElementAt(0).Key);

                        deeper = null;
                        return weights.ElementAt(0).Key;
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

                        deeper = DeeperSearch(file, weights.Keys.AsEnumerable(), out version, true);
                        return version;
                    }
                    else
                    {
                        Console.WriteLine("Your Minecraft JAR has changed a lot to be recognized as any established version, by this reason, we will make a deeper search... (Weight: {0})", file.Length);
                        Console.WriteLine();
                        //Raro que el minecraft-.jar esté aqui

                        deeper = DeeperSearch(file, rvers, out version, true);
                        return version;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid version found ({0}), maybe this is a forge version ;-) ;-)", file.Name);
                    Console.WriteLine();

                    version = GetForgeVersion(file.FullName)["jar"].ToString();

                    MsgValidKey(version);

                    deeper = AddObject(file.Name, version);
                    return version;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Not real JAR file!!");
                Console.WriteLine(ex);
            }

            deeper = null;
            return version;
        }

        public static JObject DeeperSearch(FileInfo file, IEnumerable<string> col, out string validKey, bool dump = false)
        {
            validKey = (string)ReadJAR(file.FullName, (zipfile, item, valid) =>
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

        public static JObject AddObject(string filename, string version)
        {
            return new JObject(new JProperty("filename", filename), new JProperty("version", version));
        }

        private static void MsgValidKey(string validKey, IEnumerable<string> col = null)
        {
            if (!string.IsNullOrEmpty(validKey))
                Console.WriteLine("Found valid version: {0}", validKey);
            else
                Console.WriteLine("No version found in any of the {0} files!!", col == null ? 1 : col.Count()); //Aqui dariamos a elegir al usuario

            Console.WriteLine();
        }

        public static string GenerateWeights(string fverPath = "")
        {
            //Get estimated version from weight from jsons
            //Hacer esto cada semana, para que no se quede obsoleto el asunto, WIP ... esto tengo que implementando con lo que he dicho del latest ... si el latests es igual al local entonces devolvemos el local

            //Define folder of download
            string fold = Path.Combine(AssemblyFolderPATH, "Versions");

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
                                    return "";
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

                    string ret = new JObject(new JProperty("creationTime", DateTime.Now), new JProperty("latest", jparse["latest"]), new JProperty("versions", arr2), new JProperty("recognizedVersions", arr3)).ToString();

                    if (!string.IsNullOrEmpty(fverPath))
                        File.WriteAllText(fverPath, ret);

                    Console.WriteLine("File generated succesfully!!");

                    return ret;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No internet conn!!");
                    Console.WriteLine(ex);
                    return "";
                }
            }
        }

        public static IEnumerable<FileInfo> GetValidJars()
        {
            DirectoryInfo dir = new DirectoryInfo(AssemblyFolderPATH);

            //Select valid jars
            return dir.GetFiles().Where(file => file.Extension == ".jar" && file.Length > 1024 * 1024);
        }

        public static string DownloadLibraries(IEnumerable<string> rvers)
        {
            //First, we have to select the wanted version, in my case, I will do silly things to select the desired version...

            KeyValuePair<string, string> selVersion = default(KeyValuePair<string, string>);
            if (File.Exists(Base64PATH))
            {
                JArray jArr = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(Base64PATH));

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
                        return "Please specify a numeric value.";
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
                    return "Unrecognized versions, please restart...";
            }

            //Then, start downloading...

            string nativesDir = Path.Combine(AssemblyFolderPATH, "natives");
            PreviousChk(nativesDir);

            Console.WriteLine();
            //Generate natives
            switch (GetSO())
            {
                case OS.Windows:
                    DownloadFile(Path.Combine(nativesDir, "lwjgl.dll"), "https://build.lwjgl.org/release/latest/windows/x86/lwjgl32.dll");
                    DownloadFile(Path.Combine(nativesDir, "lwjgl64.dll"), "https://build.lwjgl.org/release/latest/windows/x64/lwjgl.dll");
                    DownloadFile(Path.Combine(nativesDir, "OpenAL32.dll"), "https://build.lwjgl.org/release/latest/windows/x86/OpenAL32.dll");
                    DownloadFile(Path.Combine(nativesDir, "OpenAL64.dll"), "https://build.lwjgl.org/release/latest/windows/x64/OpenAL.dll");

                    //JInput
                    DownloadFile(Path.Combine(nativesDir, "jinput-dx8.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-dx8.dll");
                    DownloadFile(Path.Combine(nativesDir, "jinput-dx8_64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-dx8_64.dll");
                    DownloadFile(Path.Combine(nativesDir, "jinput-raw.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-raw.dll");
                    DownloadFile(Path.Combine(nativesDir, "jinput-raw_64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-raw_64.dll");

                    //WinTab case

                    if (Environment.Is64BitOperatingSystem)
                        DownloadFile(Path.Combine(nativesDir, "jinput-wintab.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86_64/jinput-wintab.dll");
                    else
                        DownloadFile(Path.Combine(nativesDir, "jinput-wintab.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/native/windows/x86/jinput-wintab.dll");

                    //SAPIWrapper only if version is 1.12.2 or newer... (By the moment only Windows)
                    DownloadFile(Path.Combine(nativesDir, "SAPIWrapper_x64.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/SAPIWrapper/windows/SAPIWrapper_x64.dll");
                    DownloadFile(Path.Combine(nativesDir, "SAPIWrapper_x86.dll"), "https://github.com/ZZona-Dummies/jinput/raw/master/SAPIWrapper/windows/SAPIWrapper_x86.dll");
                    break;

                case OS.Linux:
                    //WIP
                    break;

                case OS.OSx:
                    //WIP
                    break;
            }

            //Download common jars...

            DownloadFile(Path.Combine(AssemblyFolderPATH, "jinput.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/jinput.jar");
            DownloadFile(Path.Combine(AssemblyFolderPATH, "lwjgl.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/lwjgl.jar");
            DownloadFile(Path.Combine(AssemblyFolderPATH, "lwjgl_util.jar"), "https://github.com/ZZona-Dummies/jinput/raw/master/JarNatives/lwjgl_util.jar");

            Console.WriteLine();

            //Generate libraries

            //First we have to download the desired json...
            string jsonPath = DownloadFile(Path.Combine(AssemblyFolderPATH, Path.GetFileNameWithoutExtension(selVersion.Key) + ".json"), string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.json", selVersion.Value));
            JObject jObject = JObject.Parse(File.ReadAllText(jsonPath));

            Console.WriteLine();

            //First we have to identify if we are on bin or in versions folder, to get the root
            string lPath = GetLibPath();

            string ff = Path.Combine(AssemblyFolderPATH, selVersion.Key);
            if (!IsValidJAR(ff))
            {
                //If, ie, this is a forge jar, we need to download the original minecraft version
                DownloadFile(Path.Combine(AssemblyFolderPATH, selVersion.Value + ".jar"), jObject["downloads"]["client"]["url"].ToString());

                //Check if this a forge version
                JObject forgeObj = GetForgeVersion(ff);

                //Aqui tenemos que descargar las librerias del forge

                foreach (var lib in forgeObj["libraries"].OfType<JObject>())
                {
                    string name = lib["name"].ToString();
                    //Aqui hay que comprobar de varios sitios:

                    //http://central.maven.org/maven2/org/scala-lang/modules/scala-xml_2.11/1.0.2/
                    //http://files.minecraftforge.net/maven/
                    //y...
                    //http://store.ttyh.ru/ o ... github: https://github.com/ZZona-Dummies/jinput/raw/master/libraries/commons-codec/commons-codec/1.10/commons-codec-1.10.jar

                    string urlRepo = GetUrlFromLibName(name),
                           libPath = Path.Combine(lPath, GetPathFromLibName(name, true));

                    if (!string.IsNullOrEmpty(urlRepo))
                        DownloadFile(libPath, urlRepo);
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
                                   soid = GetSO().ToString().ToLower();

                            if (name.Contains(soid))
                            { //With this, we ensure that we select "natives-windows" (in my case)
                                var nats = child.OfType<JObject>();
                                foreach (var tok in nats)
                                {
                                    //Here we have every object...
                                    try
                                    {
                                        DownloadFile(Path.Combine(lPath, CleverBackslashes(tok["path"].ToString())), tok["url"].ToString());
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
                        DownloadFile(Path.Combine(lPath, artf["path"].ToString().Replace('/', '\\')), artf["url"].ToString());
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
                return "Invalid instalation path, please move this executable next to a valid JAR file (minecraft.jar, forge.jar, etc...)";
            return "";
        }
    }
}