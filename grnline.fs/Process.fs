module Process

open Nessos.Argu
open Newtonsoft.Json
open System.IO
open System.Diagnostics
open System.Text
open Type
open Culture

let convertLineToUTF8 (line: string) =
    // Create two different encodings.
    let codepage = get_codepage
    let shiftJIS = Encoding.GetEncoding(codepage)
    let utf8 = Encoding.UTF8;
    let utf8Bytes = Encoding.Convert(shiftJIS, utf8, shiftJIS.GetBytes(line.ToCharArray()))
    let utf8Str = utf8.GetString(utf8Bytes)
    utf8Str

let pretty_print (json: string) =
    try
        let parsedJson = JsonConvert.DeserializeObject(json)
        JsonConvert.SerializeObject(parsedJson, Formatting.Indented)
    with
        | :? JsonReaderException -> sprintf "%s" json // When error occurred, printing json string as-is.

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
                    stdout
    if config.Pretty then
        prompt |> pretty_print |> printf "%s"
    else
        prompt |> printf "%s"
    p.Close()
    stdout