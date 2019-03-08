
open System.IO

let map = """
0000222222220000
1              0
1      11111   0
1     0        0
0     0  1110000
0     3        0
0   10000      0
0   0   11100  0
0   0   0      0
0   0   1  00000
0       1      0
2       1      0
0       0      0
0 0000000      0
0              0
0002222222200000
"""

let gradient array =
    let width, height = Array2D.length1 array, Array2D.length2 array
    for y = 0 to height - 1 do
        for x = 0 to width - 1 do
            let red = (255.*float y)/float height
            let green = (255.*float x)/float width
            array.[x, y] <- (byte red, byte green, 0uy)
    array

let drawRect x y w h v (array: 'a[,]) =
    for x = x to x + w - 1 do
        for y = y to y + h - 1 do
            array.[x, y] <- v

let drawMap array =
    let width, height = Array2D.length1 array, Array2D.length2 array
    let map = map.Trim([|'\r';'\n'|]).Split("\r\n")
    let tw, th = width / map.[0].Length, height / map.Length
    for y = 0 to map.Length - 1 do
        for x = 0 to map.[0].Length - 1 do
            if map.[y].[x] <> ' ' then
                drawRect (x * tw) (y * th) tw th (0uy, 255uy, 255uy) array
    array

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
    
    Array2D.create 512 512 (0uy, 0uy, 0uy)
    |> gradient
    |> drawMap
    |> dropPPM "./out.ppm"

    0