
open System.IO

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
        let row = float (i / width)
        let col = float (i % width)
        let red = byte ((255.*row)/float height)
        let green = byte ((255.*col)/float width)
        [red;green;0uy])
    |> Seq.iter out.WriteByte

[<EntryPoint>]
let main _ =
    
    ``Save an image to disk`` ()

    0