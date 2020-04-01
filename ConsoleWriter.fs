namespace Backup

module internal ConsoleWriter =

    open System

    let print = printf "%s"
    let printLine = printfn "%s"

    let setColor (c : ConsoleColor) = 
        Console.ForegroundColor <- c    
    
    let prettyPrintWord (word : string) (color : ConsoleColor) =
        let old = Console.ForegroundColor

        setColor color
        print word
        setColor old

    let prettyPrintLine (line : string) (color : ConsoleColor) = 
        let old = Console.ForegroundColor

        setColor color
        printLine line
        setColor old

    let writeFile (name : string) indent =
        if indent then print Tab 
        prettyPrintWord FilePrefix ConsoleColor.Cyan
        printLine name

    let writeDir (name : string) = 
        prettyPrintWord DirPrefix ConsoleColor.Magenta
        printLine name 
