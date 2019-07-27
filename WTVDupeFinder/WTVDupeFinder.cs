using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;



namespace WTVDupeFinder
{
    class Recording
    {
        private string _ShowTitle = String.Empty;
        /// <summary>
        /// Gets the show title.
        /// </summary>
        /// <value>The show title.</value>
        public string ShowTitle
        {
            get { return _ShowTitle; }
        }

        private DateTime _RecordingDate;
        public DateTime RecordingDate
        {
            get { return _RecordingDate; }
        }
        
        private string _EpisodeTitle;
        /// <summary>
        /// Gets the episode title.
        /// </summary>
        /// <value>The episode title.</value>
        public string EpisodeTitle
        {
            get { return _EpisodeTitle; }
        }

        private string _EpisodeDescription = String.Empty;
        /// <summary>
        /// Gets the episode description.
        /// </summary>
        /// <value>The show title.</value>
        public string EpisodeDescription
        {
            get { return _EpisodeDescription; }
        }

        private string _FileName;
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get { return _FileName; }
        }

        private string _ChannelName;
        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        /// <value>The name of the channel.</value>
        public string ChannelName
        {
            get { return _ChannelName; }
        }


        private bool _IsHD;
        /// <summary>
        /// Gets a value indicating whether this instance is HD.
        /// </summary>
        /// <value><c>true</c> if this instance is HD; otherwise, <c>false</c>.</value>
        public bool IsHD
        {
            get { return _IsHD; }
        }

        private bool _IsDupe;
        /// <summary>
        /// Gets a value indicating whether this instance is dupe.
        /// </summary>
        /// <value><c>true</c> if this instance is dupe; otherwise, <c>false</c>.</value>
        public bool IsDupe
        {
            get { return _IsDupe; }
            set { _IsDupe = value; }
        }

        private long _FileSize;

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public long FileSize
        {
            get { return _FileSize; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="episode"/> class.
        /// </summary>
        /// <param name="showTitle">The show title.</param>
        /// <param name="episodeTitle">The episode title.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="chName">Name of the ch.</param>
        /// <param name="isHD">if set to <c>true</c> [is HD].</param>
        /// <param name="fileSize">Size of the file.</param>
        public Recording(string showTitle, string episodeTitle, string fileName, string chName, bool isHD, long fileSize, DateTime recordingDate)
        {
            this._ShowTitle = showTitle;
            this._EpisodeTitle = episodeTitle;
            this._FileName = fileName;
            this._IsHD = isHD;
            this._IsDupe = true;
            this._ChannelName = chName;
            this._FileSize = fileSize;
            this._RecordingDate = recordingDate;
        }
        
        public Recording(string showTitle, string episodeTitle,string episodeDescription, string fileName, string chName, bool isHD, long fileSize, DateTime recordingDate)
        {
            this._ShowTitle = showTitle;
            this._EpisodeTitle = episodeTitle;
            this._EpisodeDescription = episodeDescription;
            this._FileName = fileName;
            this._IsHD = isHD;
            this._IsDupe = true;
            this._ChannelName = chName;
            this._FileSize = fileSize;
            this._RecordingDate = recordingDate;
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to episode return false.
            Recording e = obj as Recording;
            if ((System.Object)e == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (_ShowTitle == e._ShowTitle) && (_EpisodeTitle == e._EpisodeTitle) && (_EpisodeDescription == e.EpisodeDescription);
        }


        /// <summary>
        /// Equalses the specified e.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public bool Equals(Recording e)
        {
            // If parameter is null return false.
            if (e == null)
            {
                return false;
            }
            // Return true if the fields match:
            return (_ShowTitle == e._ShowTitle) && (_EpisodeTitle == e._EpisodeTitle) && (_EpisodeDescription == e.EpisodeDescription);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    class DupeFinder
    {
        // The master list of all recordings that will be examined
        private List<Recording> allRecordings;
        //A temorary list of recordings that represents items to be preserved
        private List<Recording> preservedRcordings;
        //Shows names to be exculded fomr all processing (and only if the show lacks a spcific title)
        private List<String> excludedGenericShows;
        //Channel names to bias matching against. Give two identical shows, one on a prefered channel will be selected.
        private List<String> preferedChannels;

        private bool _DeleteGeneric = true;
        /// <summary>
        /// Gets a value indicating whether [delete empty titles].
        /// Causes shows with empty episode names to be deleted. Has no effect if exclude option is set.
        /// </summary>
        /// <value><c>true</c> if [delete empty titles]; otherwise, <c>false</c>.</value>
        public bool DeleteGeneric
        {
            get { return _DeleteGeneric; }
        }

        private string _logFile = null;
        /// <summary>
        /// Gets a value indicating whether [delete empty titles].
        /// Causes shows with empty episode names to be deleted. Has no effect if exclude option is set.
        /// </summary>
        /// <value><c>true</c> if [delete empty titles]; otherwise, <c>false</c>.</value>
        public string logFile
        {
            get { return _logFile; }
        }

        private bool _DateBias = false;
        /// <summary>
        /// If set the the oldest dupe that meets other parameters (is HD, prefered channel) will be used. 
        /// Otherwise the age of the dupe that is preserved is indeterminate.
        /// </summary>
        /// <value><c>false</c> if [DateBias]; otherwise, <c>true</c>.</value>
        public bool DateBias
        {
            get { return _DateBias; }
        }

        private string _ScanPath;
        /// <summary>
        /// Gets or sets the scan path.
        /// </summary>
        /// <value>The scan path.</value>
        public string ScanPath
        {
            get { return _ScanPath; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DupeFinder"/> class.
        /// </summary>
        public DupeFinder()
        {
            allRecordings = new List<Recording>();
            preservedRcordings = new List<Recording>();
            excludedGenericShows = new List<String>();
            preferedChannels = new List<String>();
        }

        /// <summary>
        /// Adds the excluded show.
        /// </summary>
        /// <param name="s">The s.</param>
        public void AddExcludedShow(string s)
        {
            this.excludedGenericShows.Add(s);
        }

        /// <summary>
        /// Adds the prefered ch.
        /// </summary>
        /// <param name="s">The s.</param>
        public void AddPreferedCh(string s)
        {
            this.preferedChannels.Add(s);
        }

        /// <summary>
        /// Fetches property value for indicated property name
        /// </summary>
        /// <returns></returns>
        public string GetPropertyValue(FileInfo file, string name)
        {
            string value = null;

            var p = ShellFile.FromFilePath(file.FullName).Properties.GetProperty(name);
            if (p != null)
            {
                if (p.ValueAsObject != null)
                    value = p.ValueAsObject.ToString();
            }
            else
                value = null;

            p = null;
            return (value);
        }


        /// <summary>
        /// Builds the file list.
        /// </summary>
        /// <returns></returns>
        public bool BuildFileList()
        {
     
            //episode properties
            string showTitle;
            string episodeTitle;
            string episodeDescription;
            string fileName;
            string chName;
            long fileSize;
            bool isHD;
            DateTime recordingDate;

            string[] files = Directory.GetFiles(this.ScanPath, "*.wtv", SearchOption.AllDirectories);

            foreach (string f in files)
            {
                FileInfo file = new FileInfo(f);

                showTitle = GetPropertyValue(file, "System.Title");
               
                if (showTitle != null)
                {
                    //Get station call sign
                    chName = GetPropertyValue(file, "System.RecordedTV.StationCallSign");
                    
                    //Get episode title
                    episodeTitle = GetPropertyValue(file, "System.RecordedTV.EpisodeName");

                    //Get episode description
                    episodeDescription = GetPropertyValue(file, "System.RecordedTV.ProgramDescription");
                   
                    //If we are not deleteing generic shows and episode title is empty then ignore it
                    if (!DeleteGeneric && episodeTitle == null)
                    {
                        goto Exit;
                    }
                    else
                    {
                        //If series is in list of excluded shows and there is no unique title then set exclude flag
                        //These shows will not be included in the search for duplicates.
                        
                        foreach (string fstring in excludedGenericShows)
                        {
                            if (showTitle.Contains(fstring) && (episodeTitle == null))
                            {
                                goto Exit;
                            }
                        }
                    }
                    //Get Filename
                    fileName = file.Name;
                    //Get fileSize
                    fileSize = file.Length;

                    //Get recording time
                    recordingDate = Convert.ToDateTime (GetPropertyValue(file, "System.DateCreated"));

                    //Get HD content tag
                    isHD = Convert.ToBoolean(GetPropertyValue(file, "System.RecordedTV.IsHDContent"));
                    //Add recording to master list
                    //allRecordings.Add(new Recording(showTitle, episodeTitle, fileName, chName, isHD, fileSize, recordingDate));
                    allRecordings.Add(new Recording(showTitle, episodeTitle,episodeDescription, fileName, chName, isHD, fileSize, recordingDate));
                Exit: ;
                
                } 
            }

            //Um. Error handler would be nice.
            return true;
        }

        /// <summary>
        /// Finds the dupes. We assume all shows to be dupes. When a new 
        /// show/episode is found it is added to the list of processed/unique 
        /// shows and the isDupe flag is cleared.
        /// </summary>
        public void FindDupes()
        {
            foreach (Recording recording in allRecordings)
            {
                //If recording is not in the list of already processed recordings then add it.
                //If DeleteGeneric is true then any recording that lacks a specific episode title will be skipped and therefor deleted.
                //if (!preservedRcordings.Contains(recording) && !(DeleteGeneric && (recording.EpisodeTitle == null)))
                if (!preservedRcordings.Contains(recording))
                {
                    //If the item isHD then add it.
                    if (recording.IsHD)
                    {
                        preservedRcordings.Add(recording);
                        recording.IsDupe = false;
                    }
                    //If the item is not HD then try to find one that is. Or one that is in the prefered channel list.
                    else
                    {
                        preservedRcordings.Add(FindPreferedEpisode(recording, allRecordings, preferedChannels));
                        recording.IsDupe = false;
                    }

                }
            }

            //This is brute force stupid. 
            if (DateBias)
            {
                //for(int x = 0;x < processedEpisodes.Count;x++)
                foreach (Recording processedRecording in preservedRcordings)
                {
                    //brute force search for oldest item that meets requirements
                    //for (int y = 0; y < allEpisodes.Count; y++)
                    foreach (Recording unprocessedRecording in allRecordings)
                    {
                        //This case is for the condition where a recording falls on a prefered channel.
                        //in this case we constrain the date hunt to the prefered channel only.
                        if ((unprocessedRecording.RecordingDate < processedRecording.RecordingDate)
                            && (preferedChannels.Contains(processedRecording.ChannelName))
                            && (processedRecording.ChannelName == unprocessedRecording.ChannelName)
                            && (processedRecording == unprocessedRecording)
                            && (processedRecording.IsHD == unprocessedRecording.IsHD))
                        {
                            //processed episode is now obsolete and is a dupe. Find it in "all episodes" and set it back to dupe
                            foreach (Recording recording in allRecordings)
                            {
                                if (recording.FileName == processedRecording.FileName)
                                    recording.IsDupe = true;
                            }

                            //remove obsolete item from processed recording list
                            //and add newer one...
                            preservedRcordings.Remove(processedRecording);
                            preservedRcordings.Add(unprocessedRecording);
                            unprocessedRecording.IsDupe = false;
                        }
                        //no prefered channel. Pick older episode from any channel.
                        else if ((unprocessedRecording.RecordingDate < processedRecording.RecordingDate)
                            && (processedRecording == unprocessedRecording)
                            && (processedRecording.IsHD == unprocessedRecording.IsHD))
                        {
                           
                            //processed episode is now obsolete and is a dupe. Find it in "all episodes" and set it back to dupe
                            foreach (Recording episode in allRecordings)
                            {
                                if (episode.FileName == processedRecording.FileName)
                                    episode.IsDupe = true;
                            }
                            
                            //remove obsolete item from processed peisode list
                            preservedRcordings.Remove(processedRecording);
                            preservedRcordings.Add(unprocessedRecording);
                            unprocessedRecording.IsDupe = false;
                        }
                    }
                }
            
            }
        }

        /// <summary>
        /// Finds the prefered episode. Given an episode 'episode', we try to find an HD version
        /// if that fails we try to find a version in the prefered channel list.
        /// </summary>
        /// <param name="recording">The episode.</param>
        /// <param name="recordingList">The episode list.</param>
        /// <param name="preferedChannels">The prefered channels.</param>
        /// <returns></returns>
        private static Recording FindPreferedEpisode(
            Recording recording,
            List<Recording> recordingList,
            List<String> preferedChannels)
        {
            //Scan for HD variant
            foreach (Recording candidateRecording in recordingList)
            {
                //found HD version in list
                if ((recording == candidateRecording) && candidateRecording.IsHD) { return candidateRecording; }
            }
            //Scan for prefered CH variant
            foreach (Recording candidateRecording in recordingList)
            {
                //found item in prefered channel list
                if ((recording == candidateRecording) && preferedChannels.Contains(candidateRecording.ChannelName)) { return candidateRecording; }
            }

            //Did not find either version, return original (presumed) SD variant.
            return recording;
        }

        /// <summary>
        /// Dumps the list to console
        /// </summary>
        /// <param name="c">The c.</param>
        public void DumpList(List<Recording> episodes)
        {
            foreach (Recording episode in episodes)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Show Title    = " + episode.ShowTitle);
                Console.WriteLine("Episode Title = " + episode.EpisodeTitle);
                Console.WriteLine("Filename      = " + episode.FileName);
                Console.WriteLine("Callsign      = " + episode.ChannelName);
                if (episode.IsHD) Console.WriteLine("IS HD         = YES");
                Console.WriteLine("--------------------------------");
            }
        }

        /// <summary>
        /// Settingses from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool SettingsFromXML(String path)
        {
            if (!File.Exists(path)) { return false; }
            
            //Get settings from disk
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            Stream reader = new FileStream(path, FileMode.Open);
            Settings settings;
            settings = (Settings)serializer.Deserialize(reader);

            this._ScanPath = settings.ScanPath;
            this._DeleteGeneric = settings.DeleteGeneric;
            this._DateBias = settings.DateBias;
            foreach (string s in settings.PreferredChannel)
                this.preferedChannels.Add(s);
            foreach (string s in settings.ExcludedGenericShow)
                this.excludedGenericShows.Add(s);

            if (settings.log != null)
            {
                this._logFile = settings.log;
            }

            reader.Close();
            
            return true;
        }

        /// <summary>
        /// Deletes duplicates.
        /// ToDo: optionally back them up before deletion.
        /// </summary>
        public void DeleteDupes()
        {

            long totalspace = 0;
            long dupespace = 0;
            long showcount = 0;
            long dupecount = 0;
            StreamWriter streamWriter = null;
            bool writeLog = false;

            try
            {
                //Log?
                if (this._logFile != null)
                {
                    streamWriter = File.CreateText(this._logFile);
                    writeLog = true;
                }

                foreach (Recording episode in this.allRecordings)
                {
                    FileInfo f = new FileInfo(this.ScanPath + episode.FileName);
                    totalspace += f.Length;
                    showcount++;

                    if (episode.IsDupe)
                    {
                        dupespace += f.Length;
                        dupecount++;
                        Console.WriteLine("---------------DELETED-----------------");
                        Console.WriteLine("Show Title    = " + episode.ShowTitle);
                        Console.WriteLine("Episode Title = " + episode.EpisodeTitle);
                        Console.WriteLine("Filename      = " + episode.FileName);
                        File.Delete(this.ScanPath + episode.FileName);
                        Console.WriteLine("---------------DELETED-----------------");
                        if (writeLog)
                        {
                            streamWriter.WriteLine("---------------DELETED-----------------");
                            streamWriter.WriteLine("Show Title    = " + episode.ShowTitle);
                            streamWriter.WriteLine("Episode Title = " + episode.EpisodeTitle);
                            streamWriter.WriteLine("Filename      = " + episode.FileName);
                            streamWriter.WriteLine("---------------DELETED-----------------");
                        }
                    }
                }

                Console.WriteLine("Total Space taken by " + showcount + " recordings: " + totalspace / 1024 / 1024 / 1024 + " GB");
                Console.WriteLine("Total Space taken by " + dupecount + " dupes: " + dupespace / 1024 / 1024 / 1024 + " GB");
                
                if (writeLog)
                {
                    streamWriter.WriteLine("Total Space taken by " + showcount + " recordings: " + totalspace / 1024 / 1024 / 1024 + " GB");
                    streamWriter.WriteLine("Total Space taken by " + dupecount + " dupes: " + dupespace / 1024 / 1024 / 1024 + " GB");
                    streamWriter.Flush();
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            
            }
        }

        /// <summary>
        /// Dumps the batch file. 
        /// </summary>
        /// <param name="batchFile">The batch file.</param>
        public void DumpBatchFile(String batchFile)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = File.CreateText(batchFile);

                long totalspace = 0;
                long dupespace = 0;
                long showcount = 0;
                long dupecount = 0;

                foreach (Recording e in this.allRecordings)
                {
                    FileInfo f = new FileInfo(this.ScanPath + e.FileName);
                    totalspace += f.Length;
                    showcount++;

                    if (e.IsDupe)
                    {
                        streamWriter.WriteLine(String.Concat(
                        "del \"",
                        this.ScanPath,
                        e.FileName,
                        "\""));
                        dupespace += f.Length;
                        dupecount++;
                    }
                }
                
                streamWriter.WriteLine("REM Total Space taken by "+ showcount+ " recordings: " + totalspace/1024/1024/1024 + " GB \n");
                streamWriter.WriteLine("REM Total Space taken by " + dupecount + " dupes: " + dupespace/1024/1024/1024 + " GB\n");      

                streamWriter.Flush();
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }
    }
}
