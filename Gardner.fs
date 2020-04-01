namespace Backup

module internal Gardner =

    let isWhiteSpace = System.Char.IsWhiteSpace

    let getIndent (line : string) =
        line.ToCharArray()
        |> Array.takeWhile(isWhiteSpace)
        |> Array.length

    let toStatement line : Statement =
        (getIndent line), line.Trim()

    let getTree root ls =
        let idx = List.findIndex (fun x -> root = x) ls
        let _, tree = List.splitAt idx ls 
        let children = List.tail tree |> List.takeWhile (fun x -> fst x > fst root)

        root::children

    let rec buildTree rootLevel branches ls = 
        match ls with
        | [] -> branches, [] 
        | (level, _)::_ when level <= rootLevel -> branches, ls 
        | (level, s)::xs ->
            let rec getBranch xs branches = 
                match buildTree level [] xs with
                | [], rest -> branches, rest
                | newBranches, rest -> getBranch rest (branches @ newBranches)

            let newBranches, rest = getBranch xs []

            match newBranches with
            | [] -> [Leaf(s)], rest
            | _ -> [Branch(s, newBranches)], rest

    let plant from ls =
        ls
        |> Seq.filter (fun x -> fst x = from)
        |> Seq.collect ((fun root -> buildTree (from - 1) [] (getTree root ls)) >> fst)

    let joinPath x y = System.IO.Path.Combine(x, y)

    let rec getFullPaths (fromBranch : Tree) rootPath = 
        match fromBranch with
        | Branch(b, children) -> List.collect (fun tree -> getFullPaths tree (joinPath rootPath b)) children
        | Leaf(path) -> [ joinPath rootPath path ]
 
    let grow ls = 
        ls
        |> Seq.map toStatement
        |> Seq.toList
        |> plant 0
        |> Seq.collect (fun x -> getFullPaths x NilString)