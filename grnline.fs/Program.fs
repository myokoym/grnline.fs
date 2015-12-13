open Nessos.Argu
open Newtonsoft.Json
open System.IO
open System.Diagnostics
open System.Text

type CLIArguments =
    | Groonga_Path of string
    | DB_Path of string
    | Encoding of string
    | Pritty of bool
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Groonga_Path _ -> "specify groonga.exe path."
            | DB_Path _ -> "specify Groonga database path."
            | Encoding _ -> "Specify encoding such as UTF-8."
            | Pritty _ -> "Output pritty printed."

// build the argument parser
let parser = ArgumentParser.Create<CLIArguments>()

// get usage text
let usage = parser.Usage()

type Config =
    {Path:string;
     DBPath:string;
     DBEncoding:string;
     Pritty:bool}

let parseArgv argv =
    let results = parser.Parse(argv)
    let path = results.GetResult(<@ Groonga_Path @>)
    let db_path = results.GetResult (<@ DB_Path @>)
    let db_encoding = results.GetResult (<@ Encoding @>, defaultValue = "UTF-8")
    let pritty = results.GetResult (<@ Pritty @>, defaultValue = false)
    let config: Config = {Path=path; DBPath=db_path; DBEncoding=db_encoding; Pritty=pritty}
    config

let convertLineToUTF8 (line: string) =
    // Create two different encodings.
    let shiftJIS = Encoding.GetEncoding("shift_jis")
    let utf8 = Encoding.UTF8;
    let utf8Bytes = Encoding.Convert(shiftJIS, utf8, shiftJIS.GetBytes(line.ToCharArray()))
    let utf8Str = utf8.GetString(utf8Bytes)
    utf8Str

let pritty_print (json: string) =
    let parsedJson = JsonConvert.DeserializeObject(json)
    JsonConvert.SerializeObject(parsedJson, Formatting.Indented)

let check_groonga (config: Config) =
    System.IO.File.Exists <| config.Path

let start_groonga (config: Config) (line: string) =
    let encoding = config.DBEncoding |> Encoding.GetEncoding
    let psInfo = new System.Diagnostics.ProcessStartInfo(config.Path)
    psInfo.UseShellExecute <- false
    psInfo.RedirectStandardOutput <- true
    psInfo.RedirectStandardInput <- true
    psInfo.Arguments <- @"" + config.DBPath
    if encoding.Equals <| Encoding.UTF8 then
        psInfo.StandardOutputEncoding <- Encoding.UTF8
    let p = Process.Start(psInfo)
    if encoding.Equals <| Encoding.UTF8 then
        let utf8Writer = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8)
        convertLineToUTF8 line |> utf8Writer.Write
        utf8Writer.Close()
    else
        let writer = new StreamWriter(p.StandardInput.BaseStream)
        line |> writer.Write
        writer.Close()
    let stdout = p.StandardOutput.ReadToEnd()
    if config.Pritty then
        stdout |> pritty_print |> printfn "-> %s"
    else
        stdout |> printfn "-> %s"
    p.Close()

[<EntryPoint>]
let main argv =
    try 
        let config: Config = parseArgv argv
        if not <| check_groonga(config) then
            "Groonga does not exists specified path: " + config.Path |> printfn "%s"
            exit 1

        let prompt = Path.GetFileNameWithoutExtension config.DBPath |> sprintf "grnline.fs(%s)> "
        let mutable continueLooping = true
        while continueLooping do
            prompt |> printf "grnline.fs(%s)> "
            let tr = System.Console.In

            let line = tr.ReadLine()
            if line.Equals <| "quit" then
                printf "Bye!"
                exit 0
            start_groonga config line

    with
       | :? System.ArgumentException -> usage |> printfn "Invalid argument(s) specified. See usage: %s"

    0 // return an integer exit code