#load "packages/FsLab/FsLab.fsx"

open FSharp.Data
open System
open System.IO
open System.Linq
open System.Text
open System.Text.RegularExpressions
open System.Net

let results = HtmlDocument.Load("https://forums.somethingawful.com/misc.php?action=showsmilies")

// Get the smilie headers. They are not part of the ul groups, so they need to be gotten separately
let smilieHeaders = results.Descendants["h3"]
                    |> Seq.map (fun x -> "Smilies/" + Regex.Replace(x.DirectInnerText(), @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5)))
                    |> Seq.toArray

// Get the smiilie categories
let smilieGroups = results.Descendants["ul"]
                   |> Seq.filter (fun x -> x.HasClass("smilie_group"))

// Download files to storage. Make sure the directory exists first.
let get (url: string) (filename: string) (directory: string) =
        printfn "%s" filename
        Directory.CreateDirectory(directory) |> ignore
        let req = HttpWebRequest.Create(url) :?> HttpWebRequest 
        req.Method <- "GET"
        let resp = req.GetResponse() 
        use stream = resp.GetResponseStream() 
        use fs = new FileStream(path = filename, mode = FileMode.Create)
        stream.CopyTo(fs)
        true

// TODO: Figure out a better way to add the headers, rather than through a mutable value
// Download the smilies!
let count = ref 0
for smilieGroup in smilieGroups do
    printfn "%s" smilieHeaders.[count.Value]
    let smilies = smilieGroup.Descendants["li"]
                  |> Seq.filter (fun x -> x.HasClass("smilie"))
                  |> Seq.map (fun x ->
                    let uri = new Uri(x.Descendants["img"].First().AttributeValue("src"))
                    x.Descendants["div"].First().DirectInnerText(), x.Descendants["img"].First().AttributeValue("src"), Path.GetFileName(uri.LocalPath)
                    )
    smilies |> Seq.iter (fun (x,y,z) -> get y (smilieHeaders.[count.Value] + "/" + z) (smilieHeaders.[count.Value]) |> ignore)
    incr count
