namespace Backup

module internal Zipper =

    open System.IO
    open System.IO.Compression 

    open ConsoleWriter
    
    let (|File|Directory|Nothing|) (s : string) = 
        if Directory.Exists(s) then Directory
        elif File.Exists(s) then File
        else Nothing

    let isFileSystemInfo x = Directory.Exists(x) || File.Exists(x) 
    let getRealPaths ls = Seq.filter isFileSystemInfo ls
    let getAsInfoObject ls : SystemInfoItem seq = 
        seq {
            for item in ls -> 
                match item with
                | File -> Types.File (FileInfo item)  
                | Directory -> Types.Directory (DirectoryInfo item) 
                | Nothing -> Types.File (FileInfo NilString) // This item does not exist but this should not be reached.
        }
    let getObject ls = ls |> getRealPaths |> getAsInfoObject 
    let getFullPath = function 
       | SystemInfoItem.File f -> f.FullName
       | SystemInfoItem.Directory d -> d.FullName

    let getName = function
        | SystemInfoItem.File f -> f.Name
        | SystemInfoItem.Directory d -> d.Name

    let getEntry x = getFullPath x, getName x

    let rec create (zipName : string) (objects : SystemInfoItem seq) =
        use f = File.Create(zipName)
        use z = new ZipArchive(f, ZipArchiveMode.Create)

        objects
        |> Seq.iter (fun x -> 
            let path, name = getEntry x 
            match x with
            | SystemInfoItem.File _ -> 
                writeFile path false
                z.CreateEntryFromFile(path, name) |> ignore 
            | SystemInfoItem.Directory d -> 
                writeDir d.FullName
                System.IO.Directory.GetFiles(d.FullName)
                |> getAsInfoObject
                |> Seq.iter (fun y -> 
                    let path, name = getEntry y
                    writeFile path true    
                    z.CreateEntryFromFile(path, name) |> ignore))