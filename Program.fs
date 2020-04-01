(*

.Synopsis
    A simple backup tool.
.Description
    A simple backup tool.

    Pulls files set in `include.txt` and creates a backup zip archive.
    The include file, `include.txt`, MUST be in the same directory as backup.exe.

    The backup file will be saved as `backup[DTG].zip` where [DTG] is the date-time group at the time of backup.
    
    The include data file structure is file tree nested on whitespace indentation.

    You may list full paths:
    
    c:\fullpath\to\file1
    c:\fullpath\to\aDifferent\file2
    c:\root\to\users\file3
    
    ...or nest paths in branch/leaf form:

    c:\path\to
        a\dir
            file1
            file2
        another\dir
    d:\dir
        file 3
        subdir

    If the leaf item is a directory, all files in that directory will be added.
    However, this is not recursive, and subdirectories will be ignored unless they are listed.
    Any line-based whitespace can be used for indentation.
    One tab is an indent. One space is also an indent.
    Automatic whitespace conversion does not occur, so be intentional and consistent. 

    The parser will attempt to combine paths intelligently.
    The parser will also check ignore paths to items that do not exist.

    Currently, globbing is not allowed, and only valid path characters can be parsed.  

*)

namespace Backup

module Cleaner = 

    open System 
    open System.IO

    open ConsoleWriter

    let filesToClean = 
        Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.zip")
        |> Seq.map FileInfo
        |> Seq.filter (fun f -> f.CreationTime < DateTime.Now.AddMonths(-2)) 
        |> Seq.toList

    let clean = 
        if filesToClean.Length > 0
        then
            printLine NilString
            filesToClean
            |> List.iter (fun f -> 
                prettyPrintWord "Cleaning " ConsoleColor.Blue
                printLine f.FullName
                f.Delete())
            printLine NilString
        else 
            printLine NilString
            prettyPrintLine "Cleaned" ConsoleColor.Green

module Backup =

    open System 
    open System.IO 

    open ConsoleWriter 

    [<EntryPoint>]
    let main argv =
        let dateTime = DateTime.Now.ToString(DateFormat)
        let relativeZipPath = String.concat NilString [ FileName; dateTime; ZipExtension ]    
        let zipPath = Path.GetFullPath(relativeZipPath) 
        let target = Directory.GetCurrentDirectory() + @"\include.txt" 

        Cleaner.clean 
        printLine NilString
        printfn "Include -> %s" target
        printLine NilString
        printfn "Zipping"

        try 
            Loader.load target
            |> Gardner.grow
            |> Zipper.getObject
            |> Zipper.create zipPath

            prettyPrintLine "Done" ConsoleColor.Green
            printLine NilString
            prettyPrintWord "Destination " ConsoleColor.Blue
            printLine zipPath
            printLine NilString
            0
        with 
            | _ ->
                printLine NilString
                prettyPrintLine "Fail" ConsoleColor.Red
                printLine NilString
                1
