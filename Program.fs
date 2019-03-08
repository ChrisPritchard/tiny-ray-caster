
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

let arrayw, arrayh = 512, 512
let mapArray = map.Trim([|'\r';'\n'|]).Split("\r\n")
let mapw, maph = mapArray.[0].Length, mapArray.Length
let tilew, tileh = arrayw / mapw, arrayh / maph

let white = (255uy, 255uy, 255uy)
let wall = (0uy, 255uy, 255uy)
let fov = System.Math.PI/3.

let fillGradient array =
    for y = 0 to arrayh - 1 do
        for x = 0 to arrayw - 1 do
            let red = (255.*float y)/float arrayh
            let green = (255.*float x)/float arrayw
            Array2D.set array x y (byte red, byte green, 0uy)
    array

let drawRect x y w h v array =
    for x = x to x + w - 1 do
        for y = y to y + h - 1 do
            Array2D.set array x y v

let drawMap array =
    for y = 0 to maph - 1 do
        for x = 0 to mapw - 1 do
            if mapArray.[y].[x] <> ' ' then
                drawRect (x * tilew) (y * tileh) tilew tileh wall array
    array

let drawPlayer px py array =
    drawRect (int (px * float tilew)) (int (py * float tileh)) 5 5 white array
    array

let drawRay px py pa array =
    (false, [0.0..0.1..19.0])
    ||> List.fold (fun stopped c ->
        if stopped then stopped
        else
            let cx =  px + c * cos pa
            let cy = py + c * sin pa
            if mapArray.[int cy].[int cx] <> ' ' then true
            else
                let pixelx, pixely = int (cx * float tilew), int (cy * float tileh)
                Array2D.set array pixelx pixely white
                false) |> ignore
    array

let drawView px py pa array =
    [0..arrayw-1] |> List.iter (fun i ->
        let angle = pa-fov/2. + fov*float i/float arrayw
        drawRay px py angle array |> ignore)
    array

let saveAsPPM fileName array =
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
    
    let px, py, pa = 3.456, 2.345, 1.523
    Array2D.create arrayw arrayh (0uy, 0uy, 0uy)
    |> fillGradient
    |> drawMap
    |> drawPlayer px py
    |> drawView px py pa
    |> saveAsPPM "./out.ppm"

    0