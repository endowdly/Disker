namespace Disker
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    class Program
    {
        const string DiskerTargetsFile = "disker.targets"; 

        static readonly string DirPath = Environment.CurrentDirectory;
        static readonly DirectoryInfo DirPathInfo = new DirectoryInfo(DirPath); 

        static void Main(string[] args)
        {
            IEnumerable<FileInfo> files; 

            // First if args is empty, check the current directory and all subdirectories for a Disker file 
            // ! This program is not meant to be bootstrapped, so this will be the current process directory 
            files = args.Length < 1
                ? DirPathInfo.EnumerateFiles(DiskerTargetsFile, SearchOption.AllDirectories)
                : args
                    .Select(s => new FileInfo(s))
                    .Where(fi => fi.Exists);

            Debug.WriteLine("DirPath: " + DirPath);
            Debug.WriteLine("Files Count: " + files.Count().ToString()); 
            foreach (var file in files)
            {
                Debug.WriteLine(file.FullName);
            }
        }
    }
}
