namespace Backup

module internal Loader =

    open System.IO

    let tryGetFile s = 
        let fi = FileInfo s
        match fi.Exists with
        | true -> Some fi
        | false -> None 

    let readLines (x : FileInfo option) =
        match x with
        | Some fi -> File.ReadLines(fi.FullName)
        | None -> Seq.empty

    let isNotBlankLine = not << System.String.IsNullOrWhiteSpace 
    let getLines x = Seq.filter isNotBlankLine x 

    let load file = 
        file
        |> tryGetFile 
        |> readLines
        |> getLines
        |> Seq.toList