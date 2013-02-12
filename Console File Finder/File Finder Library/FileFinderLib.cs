// File Finder Library (in C-sharp)

using System;
using System.Collections.Generic;
using System.IO;
using WelchAllyn.ProgressBarInterface;

namespace File_Finder_Library
{
    /// <summary>
    /// 
    /// </summary>
    public class FoundFileInfo
    {
        private String _name;
        private DateTime _datetime;
        private long _length;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nn"></param>
        /// <param name="dt"></param>
        /// <param name="ll"></param>
        public FoundFileInfo( String nn, DateTime dt, long ll )
        {
            _name = nn;
            _datetime = dt;
            _length = ll;
        }
        /// <summary>
        /// 
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public String Time
        {
            get
            {
                return _datetime.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public String Length
        {
            get
            {
                return _length.ToString();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileInfoComparer : IComparer<FoundFileInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare( FoundFileInfo x, FoundFileInfo y )
        {
            if ( null == x )
            {
                if ( null == y )
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if ( null == y )
                {
                    return 1;
                }
                else
                {
                    int rv = x.Name.Length.CompareTo( y.Name.Length );

                    if ( rv != 0 )
                    {
                        return rv;
                    }
                    else
                    {
                        return x.Name.CompareTo( y.Name );
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileFinderHelper
    {
        /// <summary>
        /// Retreives a list of logical drive names attached to this computer.
        /// </summary>
        /// <param name="fLocalOnly">Limits the list of drives names to those that are phyically attached.</param>
        /// <returns>A list of strings containing the logical drive names.</returns>
        public List<String> GetDrives( bool fLocalOnly )
        {
            DriveInfo[] myDrives = DriveInfo.GetDrives();
            List<String> rvDrives = new List<String>();

            foreach ( DriveInfo d in myDrives )
            {
                if ( fLocalOnly )
                {
                    switch ( d.DriveType )
                    {
                        case DriveType.Network:
                            /* fall through */
                        case DriveType.NoRootDirectory:
                            /* fall through */
                        case DriveType.Unknown:
                            continue;	// Not a local drive
                        case DriveType.Ram:
                        /* fall through */
                        case DriveType.CDRom:
                        /* fall through */
                        case DriveType.Fixed:
                        /* fall through */
                        case DriveType.Removable:
                        /* fall through */
                        default:
                            break;		// A local drive
                    }
                }

                if ( d.IsReady )
                {
                    rvDrives.Add( d.Name );
                }
            }

            rvDrives.Sort();

            return rvDrives;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public List<String> GetFolders( String root )
        {
            DirectoryInfo myDir = new DirectoryInfo( root );
            DirectoryInfo[] myFolders;
            List<String> rvFolders = new List<String>();

            try
            {
                myFolders = myDir.GetDirectories();
            }
            catch //( Exception ex )
            {
                //Console.Error.WriteLine( "ERROR: {0}", ex.Message );

                return rvFolders; // return an empty list
            }

            foreach ( DirectoryInfo di in myFolders )
            {
                if ( FileAttributes.System != (di.Attributes & FileAttributes.System) )
                {
                    rvFolders.Add( di.Name );
                }
            }

            rvFolders.Sort();

            return rvFolders;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <returns></returns>
        public List<FoundFileInfo> GetFiles( String branch )
        {
            DirectoryInfo myDir = new DirectoryInfo( branch );
            FileInfo[] myFiles;
            List<FoundFileInfo> rvFiles = new List<FoundFileInfo>();

            try
            {
                myFiles = myDir.GetFiles();
            }
            catch //( Exception ex )
            {
                //Console.Error.WriteLine( "ERROR: {0}", ex.Message );
                return rvFiles; // return an empty list;
            }

            foreach ( FileInfo fi in myFiles )
            {
                if ( FileAttributes.System != (fi.Attributes & FileAttributes.System) )
                {
                    rvFiles.Add( new FoundFileInfo( fi.Name, fi.CreationTime, fi.Length ) );
                }
            }

            rvFiles.Sort(new FileInfoComparer());

            return rvFiles;
        }
        /// <summary>
        /// Get the current working directory name.
        /// </summary>
        /// <returns>A string containing the name of the current working directory.</returns>
        public String GetCurrentDirectory()
        {
            String cwd;

            try
            {
                cwd = Directory.GetCurrentDirectory();
            }
            catch ( UnauthorizedAccessException uae )
            {
                Console.Error.WriteLine( "ERROR: {0}", uae.ToString() );
                cwd = null;
            }

            return cwd;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FileFinderDriver
    {
        private FileFinderHelper _finder;
        private long _count_of_files_found;
        /// <summary>
        /// Constructors
        /// </summary>
        public FileFinderDriver()
        {
            _finder = new FileFinderHelper();
            _count_of_files_found = 0L;
        }
        /// <summary>
        /// Constructor #2
        /// </summary>
        /// <param name="fflib"></param>
        public FileFinderDriver( FileFinderHelper ff )
        {
            _finder = ff;
            _count_of_files_found = 0L;
        }
        /// <summary>
        /// 
        /// </summary>
        public long Count
        {
            get
            {
                return _count_of_files_found;
            }

            set
            {
                if (0 > value)
                {
                    value = 0L;
                }
                
                _count_of_files_found = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="seeds"></param>
        /// <param name="justnames"></param>
        public void FindFiles( String branch, List<String> seeds, bool justnames )
        {
            List<FoundFileInfo> files = _finder.GetFiles( branch );

            foreach ( FoundFileInfo file in files )
            {
                foreach ( String s in seeds )
                {
                    if ( file.Name.ToLower().Contains( s.ToLower() ) )
                    {
                        if ( justnames == false )
                        {
                            Console.Write( "{0,7},{1,22},", file.Length, file.Time );
                        }

                        Console.WriteLine( "{0}", Path.Combine( branch, file.Name ) );
                        _count_of_files_found++;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <param name="pbn"></param>
        public void FindFolders( String root, List<String> seeds, bool fJustFiles, IProgressBarNotification pbn )
        {
            List<String> folders = _finder.GetFolders( root );

            if ( null != pbn )
            {
                pbn.IncrementBar( folders.Count );
            }

            FindFiles( root, seeds, fJustFiles );

            foreach ( String folder in folders )
            {
                String newRoot = Path.Combine( root, folder );
                FindFolders( newRoot, seeds, fJustFiles, pbn );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="seeds"></param>
        /// <param name="dest"></param>
        private void GetFilesHelper( String branch, String[] seeds, ref List<String> dest )
        {
            List<FoundFileInfo> files = _finder.GetFiles( branch );

            foreach ( FoundFileInfo file in files )
            {
                foreach ( String s in seeds )
                {
                    if ( file.Name.ToLower().Contains( s.ToLower() ) )
                    {
                        dest.Add( Path.Combine( branch, file.Name ) );
                        _count_of_files_found++;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <param name="dest"></param>
        private void GetFileFoldersHelper( String root, String[] seeds, ref List<String> dest )
        {
            GetFilesHelper( root, seeds, ref dest );
            List<String> folders = _finder.GetFolders( root );

            foreach ( String folder in folders )
            {
                String newRoot = Path.Combine( root, folder );
                GetFileFoldersHelper( newRoot, seeds, ref dest );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <returns></returns>
        public List<String> GetFilePaths( String root, String[] seeds )
        {
            List<String> rvFoundFilePaths = new List<String>();

            _count_of_files_found = 0L;
            GetFileFoldersHelper( root, seeds, ref rvFoundFilePaths );

            return rvFoundFilePaths;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SuperFileFinderDriver : FileFinderHelper
    {
        private long _count_of_files_found;

        /// <summary>
        /// Constructor
        /// </summary>
        public SuperFileFinderDriver()
        {
            _count_of_files_found = 0L;
        }
        /// <summary>
        /// 
        /// </summary>
        public long Count
        {
            get
            {
                return _count_of_files_found;
            }

            set
            {
                if ( 0 > value )
                {
                    value = 0L;
                }

                _count_of_files_found = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="seeds"></param>
        /// <param name="justnames"></param>
        public void FindFiles( String branch, List<String> seeds, bool justnames )
        {
            List<FoundFileInfo> files = GetFiles( branch );

            foreach ( FoundFileInfo file in files )
            {
                foreach ( String s in seeds )
                {
                    if ( file.Name.ToLower().Contains( s.ToLower() ) )
                    {
                        if ( justnames == false )
                        {
                            Console.Write( "{0,7},{1,22},", file.Length, file.Time );
                        }

                        Console.WriteLine( "{0}", Path.Combine( branch, file.Name ) );
                        _count_of_files_found++;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <param name="pbn"></param>
        public void FindFolders( String root, List<String> seeds, bool fJustFiles, IProgressBarNotification pbn )
        {
            List<String> folders = GetFolders( root );
            
            if ( null != pbn )
            {
                pbn.IncrementBar( folders.Count );
            }

            FindFiles( root, seeds, fJustFiles );

            foreach ( String folder in folders )
            {
                String newRoot = Path.Combine( root, folder );
                FindFolders( newRoot, seeds, fJustFiles, pbn );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="seeds"></param>
        /// <param name="dest"></param>
        private void GetFilesHelper( String branch, String[] seeds, ref List<String> dest )
        {
            List<FoundFileInfo> files = GetFiles( branch );

            foreach ( FoundFileInfo file in files )
            {
                foreach ( String s in seeds )
                {
                    if ( file.Name.ToLower().Contains( s.ToLower() ) )
                    {
                        dest.Add( Path.Combine( branch, file.Name ) );
                        _count_of_files_found++;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <param name="dest"></param>
        private void GetFileFoldersHelper( String root, String[] seeds, ref List<String> dest )
        {
            GetFilesHelper( root, seeds, ref dest );
            List<String> folders = GetFolders( root );

            foreach ( String folder in folders )
            {
                String newRoot = Path.Combine( root, folder );
                GetFileFoldersHelper( newRoot, seeds, ref dest );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="seeds"></param>
        /// <returns></returns>
        public List<String> GetFilePaths( String root, String[] seeds )
        {
            List<String> rvFoundFilePaths = new List<String>();

            _count_of_files_found = 0L;
            GetFileFoldersHelper( root, seeds, ref rvFoundFilePaths );

            return rvFoundFilePaths;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns>The number of folders found</returns>
        public int GetFoldersCount( String root, IProgressBarNotification pbn )
        {
            List<String> folders = GetFolders( root );
            int count_of_folders = folders.Count;

            if ( null != pbn )
            {
                pbn.IncrementBar( count_of_folders );
            }

            foreach ( String folder in folders )
            {
                String newRoot = Path.Combine( root, folder );
                count_of_folders += GetFoldersCount( newRoot, pbn );
            }

            return count_of_folders;
        }
    }
}
