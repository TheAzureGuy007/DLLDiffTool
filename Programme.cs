using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
//Refrence for using the "" Algorithm
using NReco.Text;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace DLLDiffTool
{
    class Program
    {        
        static void Main(string[] args)
        {
            // CReatig the Seperate folders for ILCODE 
            //Note: This Code may need auto cleanup in future
            //string ILCodePath = string.Concat(Environment.CurrentDirectory, @"\ILCodeTextFiles");
            string ILCodePath_1ES = string.Concat(Environment.CurrentDirectory, @"\1ES_ILCodeTextFiles");
            string ILCodePath_CDMBuild = string.Concat(Environment.CurrentDirectory, @"\CDMBuild_ILCodeTextFiles");
            // This gives out "E:\Test_apps\DLLDiffTool\DLLDiffTool\bin\Debug"
            string currentDirectory = Environment.CurrentDirectory;
            // Trim the above to "E:\Test_apps\DLLDiffTool\DLLDiffTool\Tools\ildasm.exe"
            string IldasmToolPath = currentDirectory.Replace(@"bin\Debug", "Tools");
            string driveLetter = Path.GetPathRoot(Environment.CurrentDirectory);
            try
            {
                // Code which takes up the 1 ES folder path
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(@"Please paste the complete binary path from your '1ES' folder [ hint: D:\1ES.SCOM\SystemCenter\.\SCOM\out\debug-amd64\~~~~~~\YOUR.dll ]");
                Console.ResetColor();
                string oneESDllPath = Console.ReadLine();
                Console.WriteLine();
                // Code which takes up the CDM Build folder path
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(@"Please paste the complete binary path from your 'CDMBuild' folder [ hint: \\cdmbuilds\builds\~~~~~~\debug\amd64\Modules\AI\YOUR.dll ]");
                Console.ResetColor();
                string cdmBuildDllPath = Console.ReadLine();
                List<string> dllsPaths1 = new List<string>();
                dllsPaths1.Add(oneESDllPath);
                dllsPaths1.Add(cdmBuildDllPath);
                Console.WriteLine("Analysis Started.... ");
                //IEnumerable<System.IO.FileInfo> list1 = dir.Getfile("*.dll", System.IO.SearchOption.AllDirectories);
                // List out dlls which are present not in CDMBuild folder                
                bool flag3 = dllsCheck(dllsPaths1);
                var entityMissDic1 = new Dictionary<string, int>() {
                                                                          {".class", 0},
                                                                          {".method", 0},
                                                                          {"interface", 0},
                                                                          {".property", 0},
                                                                          {".assembly", 0},
                                                                          {"length", 0},
                                                                        };
                var entityMissDic2 = new Dictionary<string, int>() {
                                                                          {".class", 0},
                                                                          {".method", 0},
                                                                          {"interface", 0},
                                                                          {".property", 0},
                                                                          {".assembly", 0},
                                                                          {"length", 0},
                                                                       };
                Process p = new Process();
                foreach (var dllpath in dllsPaths1)
                {
                    // Write code which can show up the list of files there in each 
                    Assembly assembly = Assembly.ReflectionOnlyLoadFrom(dllpath);
                    bool flag4;
                    if (dllsPaths1[0] == dllpath)
                    {
                        string dll_txt_Name = string.Concat(assembly.ManifestModule.Name, ".1ES", ".txt");
                        string filename = Path.GetFileName(dllpath);
                        // Getting the dll file size in KB
                        string length = GetFileSizeInKB(dllpath);
                        string temp_txt_Location = ILCodePath_1ES + @"\" + dll_txt_Name;
                        string strCmdText = @"ildasm /tok /byt " + dllpath + @" /out=" + temp_txt_Location;
                        Console.WriteLine();
                        Console.WriteLine("Comparing {0}", filename);
                        flag4 = true;
                        //Creating process which can Convert DLL to ILCODE(in Text format)
                        DllToILCode(p, strCmdText, IldasmToolPath, driveLetter);
                        StreamReader sr = new StreamReader(temp_txt_Location);
                        bool flag1 = EntitiesReader(temp_txt_Location, length, dllpath, entityMissDic1, entityMissDic2, flag4, sr);
                    }
                    else
                    {
                        string dll_txt_Name = string.Concat(assembly.ManifestModule.Name, ".CDMBUILD", ".txt");
                        string filename = Path.GetFileName(dllpath);
                        // Getting the dll file size in KB
                        string length = GetFileSizeInKB(dllpath);
                        string temp_txt_Location = ILCodePath_CDMBuild + @"\" + dll_txt_Name;
                        string strCmdText = @"ildasm /tok /byt " + dllpath + @" /out=" + temp_txt_Location;
                        Console.WriteLine();
                        Console.WriteLine("Comparing {0}", filename);
                        flag4 = true;
                        //Creating process which can Convert DLL to ILCODE(in Text format)
                        DllToILCode(p, strCmdText, IldasmToolPath, driveLetter);
                        flag4 = false;
                        StreamReader sr1 = new StreamReader(temp_txt_Location);
                        bool flag1 = EntitiesReader(temp_txt_Location, length, dllpath, entityMissDic1, entityMissDic2, flag4, sr1);
                    }
                }
                // DLL size
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Size difference of the dlls which you are comparing are :" + "'{0}'KB", entityMissDic1["length"] - entityMissDic2["length"]);
                Console.ResetColor();
                Console.WriteLine();
                // Displays the count
                Console.WriteLine("Comparison details of dlls are as below :");
                Console.WriteLine("=========================================");
                Console.WriteLine("Classes = {0}", entityMissDic1[".class"] - entityMissDic2[".class"]);
                Console.WriteLine("Methods = {0}", entityMissDic1[".method"] - entityMissDic2[".method"]);
                Console.WriteLine("Interfaces = {0}", entityMissDic1["interface"] - entityMissDic2["interface"]);
                Console.WriteLine("Properties = {0}", entityMissDic1[".property"] - entityMissDic2[".property"]);
                Console.WriteLine("assembly = {0}", entityMissDic1[".assembly"] - entityMissDic2[".assembly"]);
                Console.WriteLine();
                p.Close();
                //Wait for user input
                Console.WriteLine("Analysis Done ");
                Console.WriteLine();
                // Shows the directory where we are going to push the IL CODE
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Looking for Your IL Code ? It will be pushed to this location: " + "'{0}' & '{1}'", ILCodePath_1ES, ILCodePath_CDMBuild);
                Console.ResetColor();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("finally block.");
            }
        }

        private static bool dllsCheck(List<string> dllsPaths1)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File which is compared between 1ES folder & CDMBuild Folder");
            Console.WriteLine("==============================================================");
            foreach (var filepath in dllsPaths1)
            {
                string filename1 = Path.GetFileName(filepath);
                Console.WriteLine("{0}", filename1);
            }
            Console.ResetColor();
            return true;
        }

        private static bool dllsCheck(IEnumerable<System.IO.FileInfo> list1, IEnumerable<System.IO.FileInfo> list2)
        {
            //Compare files from 2 different locations & Diplay if it is not present in the 2nd folder(CDM BUild)
            bool IsInDestination = false;
            Console.WriteLine();
            Console.WriteLine("List of files present/not present 1ES folder & CDMBuild Folder");
            Console.WriteLine("==============================================================");
            int noCount = 1;
            foreach (System.IO.FileInfo s in list1)
            {
                IsInDestination = true;
                foreach (System.IO.FileInfo s2 in list2)
                {
                    if (s.Name == s2.Name)
                    {
                        IsInDestination = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("{0} - {1}", noCount, s.Name);
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        IsInDestination = false;
                    }
                }

                if (!IsInDestination)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} - {1} - Not Present", noCount, s.Name);
                    Console.ResetColor();
                }
                noCount++;
            }
            Console.WriteLine();
            return true;
        }

        private static void DllToILCode(Process p, string strCmdText, string IldasmToolPath, string driveLetter)
        {
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
            p.StandardInput.WriteLine("cd {0}", driveLetter);
            p.StandardInput.WriteLine(@"cd {0}", IldasmToolPath);
            //p.StandardInput.WriteLine(@"cd E:\Shared_Tools");
            p.StandardInput.WriteLine(strCmdText);
        }

        private static string GetFileSizeInKB(string dllPath)
        {
            FileInfo fi = new FileInfo(dllPath);
            string fileLength = fi.Length.ToString();
            string length = string.Empty;
            if (fi.Length >= (1 << 10))
                length = string.Format("{0}", fi.Length >> 10);
            return length;
        }
        private static bool EntitiesReader(string temp_txt_Location, string length, string dllPath, Dictionary<string, int> entityMissDic1, Dictionary<string, int> entityMissDic2, bool flag3, StreamReader sr)
        {
            string line = sr.ReadToEnd();
            //StreamReader sr;
            //using (sr = new StreamReader(temp_txt_Location))
            //{
            //    line = sr.ReadToEnd();
            //}           
            int currentCount = 0;
            var keywords = new Dictionary<string, int>() {
                  {".class", 0},
                  {".method", 0},
                  {"interface", 0},
                  {".property", 0},
                  {".assembly", 0},
                };
            var matcher = new AhoCorasickDoubleArrayTrie<int>(keywords);
            var text = line;
            matcher.ParseText(text, (hit) =>
            {
                switch (text.Substring(hit.Begin, hit.Length))
                {
                    case ".class":
                        {
                            keywords.TryGetValue(".class", out currentCount);
                            keywords[".class"] = currentCount + 1;
                            break;
                        }
                    case ".method":
                        {
                            keywords.TryGetValue(".method", out currentCount);
                            keywords[".method"] = currentCount + 1;
                            break;
                        }
                    case "interface":
                        {
                            keywords.TryGetValue("interface", out currentCount);
                            keywords["interface"] = currentCount + 1;
                            break;
                        }
                    case ".property":
                        {
                            keywords.TryGetValue(".property", out currentCount);
                            keywords[".property"] = currentCount + 1;
                            break;
                        }
                    case ".assembly":
                        {
                            keywords.TryGetValue(".assembly", out currentCount);
                            keywords[".assembly"] = currentCount + 1;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            });
            if (flag3)
            {
                entityMissDic1[".class"] = keywords[".class"];
                entityMissDic1[".method"] = keywords[".method"];
                entityMissDic1["interface"] = keywords["interface"];
                entityMissDic1[".property"] = keywords[".property"];
                entityMissDic1[".assembly"] = keywords[".assembly"];
                entityMissDic1["length"] = Convert.ToInt32(length);
            }
            else
            {
                entityMissDic2[".class"] = keywords[".class"];
                entityMissDic2[".method"] = keywords[".method"];
                entityMissDic2["interface"] = keywords["interface"];
                entityMissDic2[".property"] = keywords[".property"];
                entityMissDic2[".assembly"] = keywords[".assembly"];
                entityMissDic2["length"] = Convert.ToInt32(length);
            }
            //close the file
            sr.Close();
            return true;
        }
    }
}
