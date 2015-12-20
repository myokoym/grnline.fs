namespace grnline.fs.Persimmon.Tests

open Persimmon
open Type

module ConfigTest =

  let config = {
    Config.Path = "C:\\groonga-5.1.0-x64\\groonga-5.1.0-x64\\bin\\groonga.exe";
    DBPath = "test.db";
    DBEncoding = "UTF-8";
    Pretty = true
  }

  let ``path test`` = test "path test" {
    do! assertEquals "\\path\\to\\groonga" config.Path
  }