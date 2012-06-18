using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CommonDup
{
    static class Program
    {
        //Hard-coded strings in *.fbp6 files
        private const string PROJECTS_PATH = @"\Projects\";
        private const string GENERAL_COMMON = "Common";
        private const string PROJECT_COMMON = "_CommonFiles";
        private const string GENERAL_COMMON_PATH = PROJECTS_PATH + GENERAL_COMMON;

        public static bool Duplicate(string path, string participating)
        {
            bool result = true;

            #region Check if "Common" folder exists and read data from "participating" variable

            //Get directory path and check if general "Common" folder exists
            System.IO.DirectoryInfo rootProjectPath = null;
            try
            {
                rootProjectPath = new System.IO.DirectoryInfo(path);
                if (System.IO.Directory.Exists(Path.Combine(rootProjectPath.ToString(), GENERAL_COMMON)) == false)
                {
                    Console.WriteLine("Common folder couldn't be found.");
                    result = false;
                }
            }
            catch
            {
                Console.WriteLine("Root directory couldn't be found.");
                return false;
            }

            //Get project list from participating given path
            string[] projectsGotFromParticipating = null;
            if (result == true)
            {
                try
                {
                    projectsGotFromParticipating = System.IO.File.ReadAllLines(participating);
                }
                catch
                {
                    Console.WriteLine("Failed to Read participating. Participating=" + participating);
                    result = false;
                }
            }
            #endregion

            if (result == true)
            {
                foreach (string projectGotFromParticipating in projectsGotFromParticipating)
                {
                    #region Get folder path and list of fbp6 files in it

                    //Get folder path
                    string directoryName = null;
                    string fullDirectoryName = null;

                    try
                    {
                        directoryName = projectGotFromParticipating.Substring(0, projectGotFromParticipating.IndexOf("#"));
                        fullDirectoryName = Path.Combine(path, directoryName);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to get project directory UNC. ");
                        result = false;
                    }

                    //Get list of "*.fbp6" files
                    string[] projectRelevantFilesList = null;
                    if (System.IO.Directory.Exists(fullDirectoryName) && (result == true))
                    {
                        try
                        {
                            projectRelevantFilesList = System.IO.Directory.GetFiles(fullDirectoryName, "*.fbp6", SearchOption.AllDirectories);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to get list of .fbp6 files in folder " + fullDirectoryName);
                            result = false;
                        }
                    }
                    #endregion

                    #region Editing fbp6 files in project folder

                    //Replace links to "Common" location in project *.fbp6 files
                    if (result == true)
                    {
                        try
                        {
                            string PROJECT_AND_CUSTOM_COMMON_PATH = PROJECTS_PATH + directoryName + PROJECT_COMMON;
                            string PROJECT_AND_GENERAL_COMMON_PATH = PROJECTS_PATH + directoryName + GENERAL_COMMON;

                            foreach (string fileNameToFixInnerInfo in projectRelevantFilesList)
                            {
                                ReplaceInFile(fileNameToFixInnerInfo, GENERAL_COMMON_PATH, PROJECT_AND_CUSTOM_COMMON_PATH);
                                ReplaceInFile(fileNameToFixInnerInfo, PROJECT_AND_GENERAL_COMMON_PATH, PROJECT_AND_CUSTOM_COMMON_PATH);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Could not change values in *.fbp6 files in " + fullDirectoryName);
                            result = false;
                        }
                    }
                    #endregion

                    #region Copying of general "Common" folder content to "Common_per_project" folder

                    if (result == true)
                    {
                        //Clear attributes and delete previous Common_per_project folder if exists
                        try
                        {
                            if (System.IO.Directory.Exists(Path.Combine(rootProjectPath.ToString(), (directoryName + PROJECT_COMMON))))
                            {
                                try
                                {
                                    //Get list of files in existing Common_per_project folder and clear attributes
                                    string[] projectCommonFilesList = null;
                                    projectCommonFilesList = System.IO.Directory.GetFiles(Path.Combine(rootProjectPath.ToString(), (directoryName + PROJECT_COMMON)));
                                    foreach (string fileInProjectCommonFolder in projectCommonFilesList)
                                    {
                                        File.SetAttributes(fileInProjectCommonFolder, FileAttributes.Normal);
                                    }
                                    //Delete existing Common_per_project folder
                                    System.IO.Directory.Delete(Path.Combine(rootProjectPath.ToString(), (directoryName + PROJECT_COMMON)), true);
                                }
                                catch
                                {
                                    Console.WriteLine("Could not delete previous \"Common_per_project\" folder. ");
                                    return false;
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Could not prepare previous \"Common_per_project\" folder. ");
                            result = false;
                        }
                    }

                    //Create new Common_per_project folder
                    if (result == true)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(Path.Combine(rootProjectPath.ToString(), (directoryName + PROJECT_COMMON)));
                        }
                        catch
                        {
                            Console.WriteLine("Could not create \"Common_per_project\" folder. ");
                            result = false;
                        }
                    }

                    //Get list of files in general "Common" folder
                    string[] filesInGeneralCommon = null;

                    if (result == true)
                    {
                        try
                        {
                            filesInGeneralCommon = System.IO.Directory.GetFiles(Path.Combine(rootProjectPath.ToString(), GENERAL_COMMON));
                        }
                        catch
                        {
                            Console.WriteLine("Could not get list of files in general Common folder. ");
                            result = false;
                        }
                    }

                    //Copy the files from general "Common" folder to Common_per_project folder
                    string sourceFileName = null;
                    string targetFileNameAndPath = null;

                    if (result == true)
                    {
                        try
                        {
                            foreach (string sourceFileNameAndPath in filesInGeneralCommon)
                            {
                                sourceFileName = System.IO.Path.GetFileName(sourceFileNameAndPath);
                                targetFileNameAndPath = System.IO.Path.Combine(Path.Combine(rootProjectPath.ToString(), directoryName + PROJECT_COMMON), sourceFileName);
                                try
                                {
                                    System.IO.File.Copy(sourceFileNameAndPath, targetFileNameAndPath, true);
                                }
                                catch
                                {
                                    Console.WriteLine("Could not copy file " + sourceFileName + " from general Common folder. ");
                                    return false;
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Could not create source or target filename. ");
                            result = false;
                        }
                    }

                    #endregion

                    #region Editing fbp6 files in "Common_per_project" folder

                    //Get list of files in "Common_per_project" folder
                    string[] filesInCommonPerProject = null;

                    if (result == true)
                    {
                        try
                        {
                            filesInCommonPerProject = System.IO.Directory.GetFiles(Path.Combine(rootProjectPath.ToString(), (directoryName + PROJECT_COMMON)));
                        }
                        catch
                        {
                            Console.WriteLine("Could not get list of files in Common_per_project folder. ");
                            result = false;
                        }
                    }

                    //Replace links to "Common" location in Common_per_project *.fbp6 files
                    if (result == true)
                    {
                        try
                        {
                            string PROJECT_AND_CUSTOM_COMMON_PATH = PROJECTS_PATH + directoryName + PROJECT_COMMON;
                            string PROJECT_AND_GENERAL_COMMON_PATH = PROJECTS_PATH + directoryName + GENERAL_COMMON;

                            foreach (string fileNameToFixInnerInfo in filesInCommonPerProject)
                            {
                                ReplaceInFile(fileNameToFixInnerInfo, GENERAL_COMMON_PATH, PROJECT_AND_CUSTOM_COMMON_PATH);
                                ReplaceInFile(fileNameToFixInnerInfo, PROJECT_AND_GENERAL_COMMON_PATH, PROJECT_AND_CUSTOM_COMMON_PATH);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Could not change values in *.fbp6 files in " + fullDirectoryName);
                            result = false;
                        }
                    }
                    #endregion

                    #region Fixing "FBProjectRemoteExecution.fbz6" to "FBProjectRemoteExecution.fbp6"

                    string[] fileListToFixFbz6Issue = null;
                    
                    //Get list of files to fix .fbz6 issue
                    if (result == true)
                    {
                        try
                        {
                            fileListToFixFbz6Issue = System.IO.Directory.GetFiles(Path.Combine(rootProjectPath.ToString(), directoryName + PROJECT_COMMON), "*.fbp6");
                        }
                        catch
                        {
                            Console.WriteLine("Could not get list of files in \"Common_per_project\" folder to fix fbz6 issue. ");
                            result = false;
                        }
                    }

                    //Fix .fbz6 issue in Common_per_project folder
                    if (result == true)
                    {
                        foreach (string fileNameForFBZ6Remove in fileListToFixFbz6Issue)
                        {
                            try
                            {
                                ReplaceInFile(fileNameForFBZ6Remove, "FBProjectRemoteExecution.fbz6", "FBProjectRemoteExecution.fbp6");
                            }
                            catch
                            {
                                Console.WriteLine("Could not fix fbz6 issuein file " + fileNameForFBZ6Remove);
                                result = false;
                            }
                        }
                    }
                    #endregion
                }
            }
            return result;
        }

        //Inner method ReplaceInFile
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

        static int Main(string[] args)
        {
            if (Duplicate(args[0], args[1]))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}