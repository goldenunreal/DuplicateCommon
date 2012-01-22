using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CommonDup
{
    class Program
    {
        public void Duplicate(string path,string participating){
            System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(path);
            System.IO.DirectoryInfo[] subDirs = null;
            subDirs = root.GetDirectories();

            string[] lines = System.IO.File.ReadAllLines(participating);

            foreach (string line in lines)
            {
                string dirname = line.Substring(0,line.IndexOf("#"));
                string dirInfo = Path.Combine(path,dirname);
                string[] files = new string[0];                
                string[] files2 = new string[0];
                string[] filesInPackaginDir = new string[0];

                if (System.IO.Directory.Exists(dirInfo))
                {
                    files = System.IO.Directory.GetFiles(dirInfo, "*.fbp6");
                }
                if (System.IO.Directory.Exists(dirInfo + "\\Compilation"))
                {
                    files2 = System.IO.Directory.GetFiles(dirInfo + "\\Compilation", "*.fbp6");
                }

                if (System.IO.Directory.Exists(dirInfo + "\\Packaging"))
                {
                    filesInPackaginDir = System.IO.Directory.GetFiles(dirInfo + "\\Packaging", "*.fbp6");
                }

                foreach (string f in files)
                {
                    ReplaceInFile(f,"%BM_PATH%\\Projects\\Common","%BM_PATH%\\Projects\\"+dirname + "Common");
                    ReplaceInFile(f, "%BUILD_PATH%\\BuildManagement\\Projects\\Common", "%BUILD_PATH%\\BuildManagement\\Projects\\" + dirname + "Common");
                    ReplaceInFile(f, "%SCM_Root_Path%\\BuildManagement\\Projects\\Common", "%SCM_Root_Path%\\BuildManagement\\Projects\\" + dirname + "Common");
                }

                foreach (string f in files2)
                {
                    ReplaceInFile(f, "%BM_PATH%\\Projects\\Common", "%BM_PATH%\\Projects\\" + dirname + "Common");
                    ReplaceInFile(f, "%BUILD_PATH%\\BuildManagement\\Projects\\Common", "%BUILD_PATH%\\BuildManagement\\Projects\\" + dirname + "Common");
                    //ReplaceInFile(f, "FBProjectRemoteExecution.fbz6", "FBProjectRemoteExecution.fbp6");
                }

                foreach (string f in filesInPackaginDir)
                {
                    ReplaceInFile(f, "%BM_PATH%\\Projects\\Common", "%BM_PATH%\\Projects\\" + dirname + "Common");
                    ReplaceInFile(f, "%BUILD_PATH%\\BuildManagement\\Projects\\Common", "%BUILD_PATH%\\BuildManagement\\Projects\\" + dirname + "Common");
                    ReplaceInFile(f, "%SCM_Root_Path%\\BuildManagement\\Projects\\Common", "%SCM_Root_Path%\\BuildManagement\\Projects\\" + dirname + "Common");
                }

                // Resursive call for each subdirectory.
                string fileName = null;
                string destFile = null;

                //duplicate common directory for the current directory

                if (!System.IO.Directory.Exists(Path.Combine(root.ToString(),dirname + "Common")))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(root.ToString(),dirname + "Common"));
                    files = System.IO.Directory.GetFiles(Path.Combine(root.ToString(), "Common"));

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = System.IO.Path.GetFileName(s);
                        destFile = System.IO.Path.Combine(Path.Combine(root.ToString(), dirname + "Common"), fileName);
                        System.IO.File.Copy(s, destFile, true);
                    }

                    files = System.IO.Directory.GetFiles(Path.Combine(root.ToString(), dirname + "Common"), "*.fbp6");
                    foreach (string f in files)
                    {
                        ReplaceInFile(f, "%BUILD_MAPPING_PATH%\\BuildManagement\\Projects\\Common\\CopyWrapper.fbp6", "%BUILD_MAPPING_PATH%\\BuildManagement\\Projects\\" + dirname + "Common\\CopyWrapper.fbp6");
                        ReplaceInFile(f, "FBProjectRemoteExecution.fbz6", "FBProjectRemoteExecution.fbp6");
                        ReplaceInFile(f, "%SCM_Root_Path%\\BuildManagement\\Projects\\Common", "%SCM_Root_Path%\\BuildManagement\\Projects\\" + dirname + "Common");
                        if (f.ToLower().Contains("FBProjectRemoteExecution.fbp6".ToLower()))
                        {
                            ReplaceInFile(f, @"l:\BuildManagement\Projects\Common", @"l:\BuildManagement\Projects\" + dirname + "Common");
                        }
                        if (f.ToLower().Contains("PackagingStep.fbp6".ToLower()))
                        {
                            ReplaceInFile(f, @"L:\BuildManagement\Projects\Common", @"L:\BuildManagement\Projects\" + dirname + "Common");
                        }
                    }

                }
            }
        }
                //MessageBox.Show((subDirs.Length-1).ToString()+" Common Directories Created!");
        static public void ReplaceInFile(string filePath, string searchText, string replaceText)
        {
            FileInfo myFile = new FileInfo(filePath);

            // remove the read only attribute
            if (myFile.IsReadOnly == true)
                myFile.IsReadOnly = false;
            
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();


            StreamWriter streamWriter = File.CreateText(filePath);

            streamWriter.Write(content.Replace(searchText, replaceText));
            streamWriter.Close();

       }

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Duplicate(args[0],args[1]);
        }
    }
}
