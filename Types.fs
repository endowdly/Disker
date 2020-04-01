namespace Backup

[<AutoOpen>]
module internal Types = 

    open System.IO

    type Statement = int * string

    type Tree = 
        | Branch of string * Tree list
        | Leaf of string

    type SystemInfoItem = 
        | File of FileInfo
        | Directory of DirectoryInfo
