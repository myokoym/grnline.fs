module Culture

open System.Runtime.InteropServices
open System.Globalization

let get_codepage =
    let ci = CultureInfo.CurrentCulture
    ci.TextInfo.OEMCodePage