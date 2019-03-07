
open System.IO

let gradient array =
    let width, height = Array2D.length1 array, Array2D.length2 array
    for y = 0 to height - 1 do
        for x = 0 to width - 1 do
            let red = (255.*float y)/float height
            let green = (255.*float x)/float width
            array.[x, y] <- (byte red, byte green, 0uy)

let dropPPM fileName array =
    if File.Exists fileName then File.Delete fileName
    use out = File.OpenWrite fileName
    let width, height = Array2D.length1 array, Array2D.length2 array

    sprintf "P6\n%i %i\n255\n" width height 
    |> Seq.iter (fun c -> out.WriteByte (byte c))

    for y = 0 to height - 1 do
        for x = 0 to width - 1 do
            let (r, g, b) = array.[x, y]
            Seq.iter out.WriteByte [r;g;b]

[<EntryPoint>]
let main _ =
    
    let map = Array2D.create 512 512 (0uy, 0uy, 0uy)
    gradient map
    dropPPM "./out2.ppm" map

    0