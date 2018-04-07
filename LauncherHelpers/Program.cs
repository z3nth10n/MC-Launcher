using LauncherAPI;
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
            App();
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static void App()
        {
            Console.WriteLine("1.- Generate weights from versions");
            Console.WriteLine("2.- Try to identify a version");
            Console.WriteLine("3.- Exit");
            Console.WriteLine();
            Console.Write("Select what do you want to do: ");
            string c = Console.ReadLine();
            Console.WriteLine();

            int opt = 0;
            if (int.TryParse(c, out opt))
            {
                string fversion = Path.Combine(API.AssemblyPATH, "fversion.json");
                switch (opt)
                {
                    case 1:
                        //Get estimated version from weight from jsons
                        //Esto se ejecutará si por ejemplo, el minecraft.jar no se encuentra, pero hay un jar de >1MB, con esto podremos estimar la version
                        //Para luego sacar las librerias y todo el embrollo ese
                        //Hacer esto cada semana, para que no se quede obsoleto el asunto
                        Console.Clear();

                        //Define folder of download
                        string fold = Path.Combine(API.AssemblyPATH, "Versions");

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
                            Console.WriteLine("Please, you must generate fversion file, to do this you have to select option 1.");
                            App();
                            return;
                        }

                        //Prepare objects
                        JObject jobj = JObject.Parse(File.ReadAllText(fversion));
                        IEnumerable<string> rvers = jobj["recognizedVersions"].Cast<JValue>().Select(x => x.ToString());

                        Dictionary<string, int> weights = jobj["versions"].Cast<JObject>().ToDictionary(x => x["id"].ToString(), x => int.Parse(x["size"].ToString()));

                        //Start parsing...
                        DirectoryInfo dir = new DirectoryInfo(API.AssemblyPATH);

                        //Select valid jars
                        IEnumerable<FileInfo> files = dir.GetFiles().Where(file => file.Extension == ".jar" && file.Length > 1024 * 1024);

                        Console.WriteLine("There are {0} valid versions: {1}", files.Count(), string.Join(", ", files.Select(x => x.Name)));
                        Console.WriteLine();

                        foreach (FileInfo file in files)
                        {
                            //Console.WriteLine("Parsing file: {0}; Ext: {1}", file.Name, file.Extension);

                            string woutext = file.Name.Replace(file.Extension, "");
                            if (rvers.Contains(woutext))
                            {
                                Console.WriteLine("Found recognized version: {0}", woutext);
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
                                    weights = weights.Where(x => x.Value >= (file.Length - dkb * 1024) && x.Value <= (file.Length + dkb * 1024) || x.Value == file.Length).ToDictionary(x => x.Key, x => x.Value);

                                    if (weights.Count == 1)
                                    { //Aqui devolvemos la key del elemento 0
                                        Console.WriteLine(weights.ElementAt(0).Value != file.Length ? "There is a posible version: {0}" : "The desired version is {0}", weights.ElementAt(0).Key);
                                    }
                                    else if (weights.Count > 1)
                                    {
                                        Console.WriteLine("There are diferent versions that differs {0}KB from the local JAR file", dkb);
                                        weights.ForEach((x) =>
                                        {
                                            Console.WriteLine("{0}: {1} bytes (diff: {2} bytes)", x.Key, x.Value, Math.Abs(file.Length - x.Value));
                                        });

                                        Console.WriteLine();
                                        Console.WriteLine("Searching for a maching version...");
                                        Console.WriteLine();

                                        DeeperSearch(file, weights.Keys.AsEnumerable());
                                    }
                                    else
                                    {
                                        Console.WriteLine("Your Minecraft JAR has changed a lot to be recognized as any established version, by this reason, we will make a deeper search... (Weight: {0})", file.Length);
                                        Console.WriteLine();
                                        //Aqui lo que podemos es leer el JAR entero y ver si localizamos una string en concreto
                                        //Aunque es raro que el minecraft-.jar esté aqui

                                        DeeperSearch(file, rvers);
                                    }

                                    //Aqui ya seria cuestion de devolver el weights tal cual para hacer lo que se necesite con la identificación, o incluso darle a elegir al usuario si hubiese mas de una opcion
                                }
                                else
                                {
                                    Console.WriteLine("Invalid version found ({0}), maybe this is a forge version ;-) ;-)", file.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Not real JAR file!!");
                                Console.WriteLine(ex);
                            }
                        }
                        break;

                    case 3:
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid option!");
                        App();
                        return;
                }
            }
            else
            {
                Console.WriteLine("Please specify a numeric value.");
                App();
                return;
            }
        }

        private static void DeeperSearch(FileInfo file, IEnumerable<string> col)
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

            if (!string.IsNullOrEmpty(validKey))
                Console.WriteLine("Found valid version: {0}", validKey);
            else
                Console.WriteLine("No version found in any of the {0} files!!", col.Count()); //Aqui dariamos a elegir al usuario

            Console.WriteLine();
        }
    }
}