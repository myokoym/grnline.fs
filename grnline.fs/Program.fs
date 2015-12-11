// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Nessos.Argu
open System.IO
open System.Diagnostics
open System.Text
open Newtonsoft.Json

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
    let args = results.GetAllResults()
    let path = results.GetResult(<@ Groonga_Path @>)
    let db_path = results.GetResult (<@ DB_Path @>)
    let db_encoding = results.GetResult (<@ Encoding @>, defaultValue = "SHIFT_JIS")
    let pritty = results.GetResult (<@ Pritty @>, defaultValue = false)
    let config: Config = {Path=path; DBPath=db_path; DBEncoding=db_encoding; Pritty=pritty}
    config

let convertLineToUTF8 (line: string) =
    // Create two different encodings.
    let shiftJIS = Encoding.GetEncoding("shift_jis")
    let utf8 = Encoding.UTF8;
    let convertedBytes = Encoding.Convert(shiftJIS, utf8, shiftJIS.GetBytes(line.ToCharArray()))
    let unicodeStr = utf8.GetString(convertedBytes);
    unicodeStr

let convertLineToShiftJIS (line: string) =
    // Create two different encodings.
    let shiftJIS = Encoding.GetEncoding("shift_jis")
    let utf8 = Encoding.UTF8;
    let shiftJISBytes = Encoding.Convert(utf8, shiftJIS, utf8.GetBytes(line.ToCharArray()))
    let shiftJISStr = Encoding.UTF8.GetString(shiftJISBytes);
    shiftJISStr

let pritty_print (json: string) =
    let parsedJson = JsonConvert.DeserializeObject(json)
    JsonConvert.SerializeObject(parsedJson, Formatting.Indented)

let check_groonga (config: Config) =
    if System.IO.File.Exists(config.Path) then
        true
    else
        false

let start_groonga (config: Config) (line: string) =
    let psInfo = new System.Diagnostics.ProcessStartInfo(config.Path)
    psInfo.UseShellExecute <- false
    psInfo.RedirectStandardOutput <- true
    psInfo.RedirectStandardInput <- true
    psInfo.Arguments <- @"" + config.DBPath
    let p = Process.Start(psInfo)
    let utf8Writer = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8)
    convertLineToUTF8 line |> utf8Writer.Write
    utf8Writer.Close()
    let stdout = p.StandardOutput.ReadToEnd()
    if config.Pritty then
        convertLineToShiftJIS stdout |> pritty_print |> printfn "-> %s"
    else
        convertLineToShiftJIS stdout |> printfn "-> %s"
    p.Close()

[<EntryPoint>]
let main argv =
    let config: Config = parseArgv argv
    if not <| check_groonga(config) then
        "Groonga does not exists specified path: " + config.Path |> printfn "%s"
        exit 1

    let mutable continueLooping = true
    while continueLooping do
        Path.GetFileNameWithoutExtension config.DBPath |> printf "grnline.fs(%s)> "
        let tr = System.Console.In

        let line = tr.ReadLine()
        if line.Equals("quit") then
            exit 0
        start_groonga config line

    printfn "%A" config
    0 // return an integer exit code
