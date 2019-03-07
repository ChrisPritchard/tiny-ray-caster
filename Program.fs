
open System.IO

/// Implementation of https://github.com/ssloy/tinyraycaster/wiki/Part-1:-crude-3D-renderings#step-1-save-an-image-to-disk
let ``Save an image to disk`` () =
    let fileName = "./out.ppm"

    if File.Exists fileName then File.Delete fileName
    use out = File.OpenWrite fileName

    let width, height = 512, 512
    // ppm header
    sprintf "P6\n%i %i\n255\n" width height 
    |> Seq.iter (fun c -> out.WriteByte (byte c))

    // bytes, in order rgb
    [1..width*height] 
    |> List.collect (fun i ->
        let red = (255.*float (i / width))/float height
        let green = (255.*float (i % width))/float width
        [byte red;byte green;0uy])
    |> Seq.iter out.WriteByte

[<EntryPoint>]
let main _ =
    
    ``Save an image to disk`` ()

    0