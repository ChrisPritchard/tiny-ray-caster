
open System.IO

[<EntryPoint>]
let main _ =
    
    let fileName = "./out.ppm"

    if File.Exists fileName then File.Delete fileName
    use out = File.OpenWrite fileName

    let width, height = 512, 512
    sprintf "P6\n%i %i\n" width height |> Seq.iter (fun c -> out.WriteByte (byte c))
    [1..width*height] 
    |> List.collect (fun i ->
        let row = float (i / height)
        let col = float (i % height)
        let red = byte ((255.*col)/float height)
        let green = byte ((255.*row)/float width)

        [red;green;0uy])
    |> Seq.iter out.WriteByte

    0
