
using System;
using System.IO;
using WTVDupeFinder;

namespace WTVDupeFinder_Client
{
    class Program
    {
        /// <summary>
        /// Displays the useage.
        /// </summary>
        static void DisplayUseage()
        {
            Console.WriteLine("Useage:");
            Console.WriteLine("\"WTVDupeFinder settings.xml\"");
        }
        
        static void Main(string[] args)
        {
            //Error handling? WTF is that?
            //Nothing can possibly go wrong! :-)
                                          
            DupeFinder dupeFinder = new DupeFinder();
            bool batch = false;
           
            for(int x = 0; x<args.Length;x++)
            {   
                switch (args[x].ToLower()) 
                {
                    case "/settings":
                        if (!dupeFinder.SettingsFromXML(args[x + 1]))
                        {
                            Console.WriteLine("Problem reading or parsing settings: " + args[x +1]);
                            DisplayUseage();
                            return;
                        }
                        x++;
                    break;
                    
                    case "/batch":
                        batch = true;
                    break;

                    default:
                        Console.WriteLine("Unknown argument:" + args[x]);
                        DisplayUseage();
                        return;
                }
            }

            //Build list of files to scan for dupes
            Console.WriteLine("Building file list...");
            dupeFinder.BuildFileList();
            
            //Find the dupes, biased towards HD and the prefered channel list
            Console.WriteLine("Searching for duplicate items...");
            dupeFinder.FindDupes();
            
            //This generates a batch file that does the dirty work
            if (batch)
            {
                Console.WriteLine("Generating batch file and saving it to the desktop...");
                dupeFinder.DumpBatchFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"nukedupes.bat"));
            }
            else
                dupeFinder.DeleteDupes();
        }
    }
}
