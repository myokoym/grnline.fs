module Culture

open System.Runtime.InteropServices
open System.Globalization

#if WIN32
[<DllImport("kernel32.dll")>]
extern int GetSystemDefaultLCID()


let get_codepage = 
    let lcid = GetSystemDefaultLCID()
    let ci = CultureInfo(lcid)
    ci.TextInfo.OEMCodePage
#else
let get_codepage = 65001 // For *nix like environment, it always assumes UTF-8 for now.
#endif