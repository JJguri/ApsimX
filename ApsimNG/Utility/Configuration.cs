﻿using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>Handle the reading and writing of the configuration settings file</summary>
    public class Configuration
    {
        /// <summary>The instance</summary>
        private static Configuration instance = null;

        /// <summary>The configuration file</summary>
        private string configurationFile = null;

        /// <summary>The location for the form</summary>
        public Point MainFormLocation { get; set; }

        /// <summary>The size of the main form</summary>
        public Size MainFormSize { get; set; }

        /// <summary>The state (max, min, norm) of the form</summary>
        public Boolean MainFormMaximized { get; set; }

        /// <summary>List of the most recently opened files</summary>
        public List<string> MruList { get; set; }

        /// <summary>The maximum number of files allowed in the mru list</summary>
        public int FilesInHistory { get; set; }

        /// <summary>The previous folder where a file was opened or saved</summary>
        public string PreviousFolder { get; set; }

        /// <summary>The previous height of the status panel</summary>
        public int StatusPanelHeight { get; set; }

        /// <summary>Keeps track of whether the dark theme is enabled.</summary>
        public bool DarkTheme { get; set; }

        /// <summary>Return the name of the summary file JPG.</summary>
        public string SummaryPngFileName
        {
            get
            {
                // Make sure the summary JPG exists in the configuration folder.
                string summaryJpg = Path.Combine(ConfigurationFolder, "ApsimSummary.png");
                if (!File.Exists(summaryJpg))
                {
                    try
                    {
                        Bitmap b = ApsimNG.Properties.Resources.ResourceManager.GetObject("ApsimSummary") as Bitmap;
                        b.Save(summaryJpg);
                    }
                    catch
                    {

                    }

                }
                return summaryJpg;
            }
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organisation { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }

        /// <summary>The maximum number of rows to show on a report grid</summary>
        public int MaximumRowsOnReportGrid { get; set; }

        /// <summary>
        /// Store the style name used in the editor
        /// </summary>
        public string EditorStyleName { get; set; } = "Visual Studio";

        /// <summary>
        /// Store the zoom level for editors
        /// </summary>
        public double EditorZoom { get; set; } = 1.0;

        /// <summary>
        /// Store the user's preferred font size
        /// </summary>
        public double BaseFontSize { get; set; } = 12.5;

        /// <summary>
        /// Stores the user's preferred font.
        /// </summary>
        /// <value></value>
        public Pango.FontDescription Font { get; set; }

        /// <summary>Add a filename to the list.</summary>
        /// <param name="filename">File path</param>
        public void AddMruFile(string filename)
        {
            if (filename.Length > 0)
            {
                if (MruList.Count > 0)
                {
                    if (MruList.IndexOf(filename) < 0)
                    {
                        // First time that filename has been added 
                        if (MruList.Count >= FilesInHistory)
                            MruList.RemoveAt(MruList.Count - 1);  // Delete the last item 
                    }
                    else
                    {
                        // Item is in the history list => move to top 
                        MruList.RemoveAt(MruList.IndexOf(filename));
                    }
                    MruList.Insert(0, filename);
                }
                else
                    MruList.Add(filename);
            }
        }

        /// <summary>Remove a specified file from the list</summary>
        /// <param name="filename">The file name to delete</param>
        public void DelMruFile(string filename)
        {
            if (filename.Length > 0)
            {
                if (MruList.Count > 0)
                {
                    if (MruList.IndexOf(filename) >= 0)
                    {
                        MruList.RemoveAt(MruList.IndexOf(filename));
                    }
                }
            }
        }

        /// <summary>Rename a specified file in the list</summary>
        /// <param name="filename">The file name to rename</param>
        /// <param name="newname">The new file name</param>
        public void RenameMruFile(string filename, string newname)
        {
            if (filename.Length > 0)
            {
                if (MruList.Count > 0)
                {
                    int idx = MruList.IndexOf(filename);
                    if (idx >= 0)
                    {
                        MruList.RemoveAt(idx);
                        MruList.Insert(idx, newname);
                    }
                }
            }
        }

        /// <summary>Clean the list by removing missing files</summary>
        public void CleanMruList()
        {
            string filename;
            int i = MruList.Count - 1;
            while (i >= 0)
            {
                filename = MruList[i];
                if (!File.Exists(filename))
                {
                    DelMruFile(filename);
                }
                i--;
            }
        }

        /// <summary>Return the configuration folder.</summary>
        /// <value>The configuration folder.</value>
        private static string ConfigurationFolder
        {
            get
            {
                // On Linux and Mac the path will be .config/
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "ApsimInitiative",
                                    "ApsimX");
            }
        }

        /// <summary>Private constructor</summary>
        private Configuration() { }

        /// <summary>Finalizes an instance of the <see cref="Configuration"/> class.</summary>
        ~Configuration()
        {
            Save();
        }

        /// <summary>Gets the configuration settings.</summary>
        public static Configuration Settings
        {
            get
            {
                if (instance != null)
                    return instance;

                string configurationFile = Path.Combine(ConfigurationFolder, "ApsimX.xml");
                // deserialise the file
                if (File.Exists(configurationFile))
                {
                    System.Xml.Serialization.XmlSerializer xmlreader = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
                    StreamReader filereader = null;

                    // Dean (Oct 2014): I changed the class that is serialized from Settings to Configuration.
                    // This will cause the code below to throw. When this happens just delete the old
                    // configuration file.
                    try
                    {
                        filereader = new StreamReader(configurationFile);
                        instance = (Configuration)xmlreader.Deserialize(filereader);
                        filereader.Close();
                    }
                    catch (Exception)
                    {
                        filereader.Close();
                        File.Delete(configurationFile);
                    }
                    
                }

                if (instance == null)
                {
                    instance = new Configuration();
                    instance.MainFormSize = new Size(640, 480);
                    instance.MainFormMaximized = true;
                    instance.MruList = new List<string>();
                    instance.PreviousFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    instance.FilesInHistory = 20;
                }

                if (instance.FilesInHistory == 0)
                    instance.FilesInHistory = 20;

                instance.configurationFile = configurationFile;
                return instance;
            }
        }

        /// <summary>Store the configuration settings to file</summary>
        public void Save()
        {
            string configPath = Path.GetDirectoryName(configurationFile);
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);
            StreamWriter filewriter = new StreamWriter(configurationFile);
            System.Xml.Serialization.XmlSerializer xmlwriter = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
            xmlwriter.Serialize(filewriter, Settings);
            filewriter.Close();
        }
    }
}
