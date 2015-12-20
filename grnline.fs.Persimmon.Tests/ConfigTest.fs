namespace grnline.fs.Persimmon.Tests

open Persimmon
open Type

module ConfigTest =
  let config_path = "C:\\groonga-5.1.0-x64\\groonga-5.1.0-x64\\bin\\groonga.exe"
  let config = {
    Config.Path = config_path;
    DBPath = "test.db";
    DBEncoding = "UTF-8";
    Pretty = true
  }

  let ``path test`` = test "path test" {
    do! assertEquals "C:\\groonga-5.1.0-x64\\groonga-5.1.0-x64\\bin\\groonga.exe" config.Path
  }