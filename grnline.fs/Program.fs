open Nessos.Argu
open Newtonsoft.Json
open System.IO
open System.Diagnostics
open System.Text

type CLIArguments =
    | Groonga_Path of string
    | DB_Path of string
    | Encoding of string
    | Pretty of bool
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Groonga_Path _ -> "specify groonga.exe path."
            | DB_Path _ -> "specify Groonga database path."
            | Encoding _ -> "Specify encoding such as UTF-8."
            | Pretty _ -> "Output pritty printed."

// build the argument parser
let parser = ArgumentParser.Create<CLIArguments>()

// get usage text
let usage = parser.Usage()

type Config =
    {Path:string;
     DBPath:string;
     DBEncoding:string;
     Pretty:bool}

let parseArgv argv =
    let results = parser.Parse(argv)
    let path = results.GetResult(<@ Groonga_Path @>)
    let db_path = results.GetResult (<@ DB_Path @>)
    let db_encoding = results.GetResult (<@ Encoding @>, defaultValue = "UTF-8")
    let pretty = results.GetResult (<@ Pretty @>, defaultValue = false)
    let config: Config = {Path=path; DBPath=db_path; DBEncoding=db_encoding; Pretty=pretty}
    config

let convertLineToUTF8 (line: string) =
    // Create two different encodings.
    let shiftJIS = Encoding.GetEncoding("shift_jis")
    let utf8 = Encoding.UTF8;
    let utf8Bytes = Encoding.Convert(shiftJIS, utf8, shiftJIS.GetBytes(line.ToCharArray()))
    let utf8Str = utf8.GetString(utf8Bytes)
    utf8Str

let pretty_print (json: string) =
    let parsedJson = JsonConvert.DeserializeObject(json)
    JsonConvert.SerializeObject(parsedJson, Formatting.Indented)

let check_groonga (config: Config) =
    System.IO.File.Exists <| config.Path

let check_dbpath (config: Config) =
    System.IO.File.Exists <| config.DBPath

let start_groonga (config: Config) (inputs: string list) =
    let encoding = config.DBEncoding |> Encoding.GetEncoding
    let psInfo = new ProcessStartInfo(config.Path)
    psInfo.UseShellExecute <- false
    psInfo.RedirectStandardOutput <- true
    psInfo.RedirectStandardInput <- true
    psInfo.Arguments <- @"" + config.DBPath
    if encoding = Encoding.UTF8 then
        psInfo.StandardOutputEncoding <- Encoding.UTF8
    let p = Process.Start(psInfo)
    if encoding = Encoding.UTF8 then
        let utf8Writer = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8)
        inputs |> List.map (fun s -> convertLineToUTF8 s |> utf8Writer.Write) |> ignore
        utf8Writer.Close()
    else
        let writer = new StreamWriter(p.StandardInput.BaseStream)
        inputs |> List.map (fun s -> writer.Write s) |> ignore
        writer.Close()
    let stdout = p.StandardOutput.ReadToEnd()
    let prompt = if stdout = "" then
                    ""
                 else
                    "-> " + stdout
    if config.Pretty then
        prompt |> pretty_print |> printf "%s"
    else
        prompt |> printf "%s"
    p.Close()
    stdout

[<EntryPoint>]
let main argv =
    try 
        let config: Config = parseArgv argv
        if not <| check_groonga(config) then
            "Groonga does not exist specified path: " + config.Path |> printfn "%s"
            exit 1

        if not <| check_dbpath(config) then
            "Groonga database does not exist specified path: " + config.DBPath |> printfn "%s"
            exit 1

        let prompt_default = Path.GetFileNameWithoutExtension config.DBPath |> sprintf "grnline.fs(%s)> "
        let mutable prompt = prompt_default
        let mutable inputs: string list = []
        let mutable continueLooping = true
        while continueLooping do
            prompt |> printf "%s"
            let tr = System.Console.In

            let line = tr.ReadLine()
            inputs <- List.append inputs [line]
            if line = "quit" then
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
       | :? System.ArgumentException -> usage |> printfn "Invalid argument(s) specified. See usage: %s"

    0 // return an integer exit code
