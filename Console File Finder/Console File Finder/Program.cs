using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Threading;
using File_Finder_Library;
using WelchAllyn.ProgressBarForm;

namespace Console_File_Finder
{
    /// <summary>
    /// 
    /// </summary>
    class CMyOptions
    {
        public CMyOptions()
        {
            fOptionLocalDrivesOnly = false;
            fJustFilenames = false;
            myArgs = new List<String>();
            aOptionDrives = new List<String>();
        }

        public bool fOptionLocalDrivesOnly;
        public bool fJustFilenames;
        public List<String> myArgs;
        public List<String> aOptionDrives;
    }
    /// <summary>
    /// 
    /// </summary>
    class WorkerThread
    {
        /// <summary>
        /// A forms-based progress bar
        /// </summary>
        static private ProgressBarForm pbf;
        static private ProgressBarSink pbs;
        /// <summary>
        /// The main worker thread
        /// </summary>
        /// <param name="options">A CMyOptions object</param>
        public static void ThreadFunc(Object options)
        {
            CMyOptions opts = (CMyOptions) options;
            SuperFileFinderDriver ffd = new SuperFileFinderDriver();
            List<String> availableDrives = ffd.GetDrives( opts.fOptionLocalDrivesOnly );
            List<String> drives = new List<String>();
            DateTime start = DateTime.Now;

            if ( opts.aOptionDrives.Count > 0 )
            {
                String str;

                foreach ( String o in opts.aOptionDrives )
                {
                    str = o;

                    if ( o == "." )
                    {
                        str = ffd.GetCurrentDirectory();

                        if ( !str.EndsWith( "\\" ) )
                        {
                            str += "\\";
                        }
                    }

                    foreach ( String d in availableDrives )
                    {
                        if ( str.ToUpper().Contains( d.ToUpper() ) )
                        {
                            try
                            {
                                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( str );

                                if ( di.Exists )
                                {
                                    drives.Add( str );
                                }
                            }
                            catch ( SecurityException se )
                            {
                                Console.Error.WriteLine( se.ToString() );
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                drives = availableDrives;
            }

            if ( drives.Count > 0 )
            {
                int folder_count = 0;

                if ( opts.fJustFilenames == false )
                {
                    Console.WriteLine( "Searching for filenames containing [{0}] on drive{1} {2}",
                                        String.Join( " | ", opts.myArgs.ToArray() ).ToUpper(),
                                        drives.Count > 1 ? "s" : "",
                                        String.Join( ",", drives.ToArray() ).ToUpper() );
                }

                pbf = new ProgressBarForm();
                pbf.Show( 1000 );
                pbs = new ProgressBarSink( pbf );

                foreach ( String driveLetter in drives )
                {
                    folder_count += ffd.GetFoldersCount( driveLetter, pbs );
                }

                pbf.SetMax( folder_count );
                pbf.SetValue( 0 );

                foreach ( String driveLetter in drives )
                {
                    ffd.FindFolders( driveLetter, opts.myArgs, opts.fJustFilenames, pbs );
                }

                pbf.Hide();
            }
            else
            {
                Console.Error.WriteLine( "ERROR: Specified drives or folders do not exist- {0}",
                                         String.Join( ", ", opts.aOptionDrives.ToArray() ).ToUpper() );
            }

            if ( opts.fJustFilenames == false )
            {
                DateTime end = DateTime.Now;
                TimeSpan diff = end - start;
                Console.WriteLine( "{0} files found\nExecution time: {1}", ffd.Count, diff );
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main( string[] args )
        {
            if ( args.Length > 0 )
            {
                CMyOptions myOpts = new CMyOptions();

                for ( int nn = 0; nn < args.Length; nn++ )
                {
                    if ( args[ nn ].ToLower() == "-lo" )		// Local-only options
                    {
                        myOpts.fOptionLocalDrivesOnly = true;
                    }
                    else if ( args[ nn ].ToLower() == "-fo" )	// Files-only option
                    {
                        myOpts.fJustFilenames = true;
                    }
                    else if ( args[ nn ].Contains( ":" ) )		// Is a drive letter specified?
                    {
                        if ( !args[ nn ].EndsWith( "\\" ) )		// make sure it ends with "\\"
                        {
                            args[ nn ] += "\\";
                        }

                        myOpts.aOptionDrives.Add( args[ nn ] );
                    }
                    else if ( args[ nn ] == "." )				// Is 'current directory' specified?
                    {
                        myOpts.aOptionDrives.Add( args[ nn ] );
                    }
                    else
                    {
                        char[] argN = args[ nn ].ToLower().ToCharArray();

                        if ( argN[ 0 ] == '-' || argN[ 0 ] == '/' )
                        {
                            if ( argN[ 1 ] == '?' || argN[ 1 ] == 'h' )
                            {
                                Assembly assem = Assembly.GetExecutingAssembly();
                                AssemblyName assemName = assem.GetName();
                                Assembly assem_fflib = Assembly.Load("File Finder Library");
                                AssemblyName assemNamefflib = assem_fflib.GetName();
                                Assembly assem_pbar = Assembly.Load( "ProgressBarForm" );
                                AssemblyName assemNamepbar = assem_pbar.GetName();

                                Console.WriteLine( "CONSOLE FILE FINDER, version {0}", assemName.Version.ToString( 3 ) );
                                Console.WriteLine( "    File Finder Lib, version {0}", assemNamefflib.Version.ToString( 3 ) );
                                Console.WriteLine( "   Progress Bar Lib, version {0}", assemNamepbar.Version.ToString( 3 ) );
                                Console.WriteLine( "USAGE:  cff [-lo] [-fo] [[d1: ... dN:] | [d:\\path\\] | [.]] patt-1 ... patt-N" );
                                Console.WriteLine( "MAIN OPTIONS:" );
                                Console.WriteLine( " -lo             Restrict search to locally attached drives" );
                                Console.WriteLine( " -fo             Just report on file pathnames (no stats)" );
                                Console.WriteLine( " d1: ... dN:     One or more drives to search, or" );
                                Console.WriteLine( " d:\\path\\        specifies the drive letter and path to start searching, or" );
                                Console.WriteLine( " . (dot)         specifies to search the current folder," );
                                Console.WriteLine( "                 otherwise when nothing specified searches all available drives" );
                                Console.WriteLine( " patt-N          One or more strings to search for in the filename" );
#if DEBUG
                                Console.ReadLine();
#endif
                                return;
                            }
                        }
                        else
                        {
                            myOpts.myArgs.Add( args[ nn ] );
                        }
                    }
                }

                if ( myOpts.myArgs.Count > 0 )
                {
                    Thread thd = new Thread( new ParameterizedThreadStart( WorkerThread.ThreadFunc ) );

                    thd.Priority = ThreadPriority.AboveNormal;
                    thd.Start( myOpts );
                    thd.Join();
                }
                else
                {
                    Console.Error.WriteLine( "WARNING: No filename pattern specified." );
                }
            }
            else
            {
                Console.Error.WriteLine( "ERROR: No arguments specified." );
            }
        }
    }
}
