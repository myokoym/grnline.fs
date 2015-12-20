open System.IO
open System.Text
open Type
open CommandLineParser
open Process

let validate_environment config =
    if not <| check_groonga(config) then
        "Groonga does not exist specified path: " + config.Path |> printfn "%s"
        exit 1

    if not <| check_dbpath(config) then
        "Groonga database does not exist specified path: " + config.DBPath |> printfn "%s"
        exit 1

[<EntryPoint>]
let main argv =
    try
        let config: Config = parseArgv argv
        validate_environment config

        let prompt_default = Path.GetFileNameWithoutExtension config.DBPath |> sprintf "grnline.fs(%s)> "
        let mutable prompt = prompt_default
        let mutable inputs: string list = []
        let mutable continueLooping = true
        while continueLooping do
            prompt |> printf "%s"
            let tr = System.Console.In

            let line = tr.ReadLine()
            inputs <- List.append inputs [line]
            if line = "quit" || line = "shutdown" then
                printf "Bye!"
                exit 0

            let result = start_groonga config inputs
            if result = "" then
                inputs <- List.append inputs ["\n"]
                prompt <- ">> "
            elif not <| (result = "") then
                inputs <- []
                prompt <- prompt_default

    with
       | :? System.ArgumentException -> usage |> printfn "usage: %s"

    0 // return an integer exit code
