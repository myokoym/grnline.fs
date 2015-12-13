grnline.fs
===

A command line tool for Gronga on .NET 4.5.

## Usage

```cmd
cmd> grnline.fs.exe --groonga-path GROONGA_PATH --db-path GROONGA_DB [--encoding ENCODING] [--pritty true]
```

### Example

```cmd
cmd> grnline.fs.exe --groonga-path "C:\\groonga-5.1.0-x64\\groonga-5.1.0-x64\\bin\\groonga.exe" --db-path "test.db" --encoding UTF-8 --pritty true
```

### Detail

`grnline.fs` create child Groonga process and Groonga child process redirects stdin/stdout to its parent.

You can use pritty printing with adding `--pritty true` to command line arguments.

## Developing

* Install Visual Studio 2013 or higher. (Including Express Edition)
* Use nuget to restore dependent libraries.
  * Argu (for command line arguments parser)
  * Json.NET (for pritty printing)

## LICENSE

[MIT](LICENSE).
