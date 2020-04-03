namespace Disker
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Endowdly.Utility;

    class Program
    {
        const string DiskerTargetsFile = "disker.targets"; 

        static readonly string DirPath = Environment.CurrentDirectory;
        static readonly DirectoryInfo DirPathInfo = new DirectoryInfo(DirPath); 

        static void Main(string[] args)
        {
            var nArgs = args.Length;

            IEnumerable<FileInfo> files; 

            // First if args is empty, check the current directory and all subdirectories for a Disker file 
            // ! This program is not meant to be bootstrapped, so this will be the current process directory 
            files = args.Length < 1
                ? DirPathInfo.EnumerateFiles(DiskerTargetsFile, SearchOption.AllDirectories)
                : args
                    .Select(s => new FileInfo(s))
                    .Where(fi => fi.Exists);

            foreach (var file in files)
            {
                var ls = File.ReadLines(file.FullName); 
                var tree = IndentTree.Parse(ls);

                Console.WriteLine(tree.ToString());

                // Try to get all the leafs
                var children = GetLowestChildPaths(tree, string.Empty)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new FileInfo(x));
                var filesFromChildren = children
                    .Where(x => x.Exists);
                var dirsFromChildren = children
                    .Where(x => x.IsDirectory())
                    .Select(x => new DirectoryInfo(x.FullName));

                // Get all the files in leaf dirs
                var filesInDirs = dirsFromChildren.SelectMany(x => GetFilesFromDir(x));

                filesFromChildren
                    .Concat(filesInDirs)
                    .ToList()
                    .ForEach(x => Console.WriteLine(x));
        }

        static IEnumerable<FileInfo> GetFilesFromDir(DirectoryInfo x) =>
            x
                .EnumerateFiles()
                .Concat(
                    x
                        .EnumerateDirectories()
                        .SelectMany(y => GetFilesFromDir(y))
                );

        // static IEnumerable<IndentTree> GetLowestChildren(IndentTree branch) =>
        //     branch.Traverse().Where(child => child.Children.Count < 1);

        static IEnumerable<string> GetLowestChildPaths(IndentTree branch, string rootPath) =>
            branch.TraverseWithPaths(rootPath).Where(x => x.Item1.Children.Count < 1).Select(x => x.Item2);
    }
}

    public static class ITExt
    {
        public static IEnumerable<IndentTree> Traverse(this IndentTree branch)
        {
            var stack = new Stack<IndentTree>();
            stack.Push(branch);

            while (stack.Count > 0)
            {
                var x = stack.Pop();

                yield return x;

                foreach (var child in x.Children)
                    stack.Push(child);
            }
        }

        // How to do this without tuples? 
        // Note: If C# 6.0 supports native tuples likes this, ignore
        public static IEnumerable<(IndentTree, string)> TraverseWithPaths(this IndentTree branch, string rootPath)
        {
            var stack = new Stack<IndentTree>();
            var pathStack = new Stack<string>();
            stack.Push(branch);
            pathStack.Push(rootPath);

            while (stack.Count > 0)
            {
                var x = stack.Pop();
                var y = pathStack.Pop();
                var newRoot = System.IO.Path.Combine(y, x.Value);

                yield return (x, newRoot);

                foreach (var child in x.Children)
                { 
                    stack.Push(child);
                    pathStack.Push(newRoot);
                }
            } 
        } 
    }

    public static class FileSystemInfoExtension
    {
        public static bool IsDirectory(this FileSystemInfo x)
        {
            if (x == null)
                return false;

            if ((int)x.Attributes != -1)
                return x.Attributes.HasFlag(FileAttributes.Directory);

            return x is DirectoryInfo;
        }
    } 
}
