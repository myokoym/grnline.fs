module CommandLineParser

open Nessos.Argu
open Type

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

let parseArgv argv =
    let results = parser.Parse(argv)
    let path = results.GetResult(<@ Groonga_Path @>)
    let db_path = results.GetResult (<@ DB_Path @>)
    let db_encoding = results.GetResult (<@ Encoding @>, defaultValue = "UTF-8")
    let pretty = results.GetResult (<@ Pretty @>, defaultValue = false)
    let config: Config = {Path=path; DBPath=db_path; DBEncoding=db_encoding; Pretty=pretty}
    config