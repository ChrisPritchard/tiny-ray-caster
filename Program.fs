
open System.IO
open SDL
open System
open System.Runtime.InteropServices

let map = 
    [|
        "0000222222220000"
        "1              0"
        "1      11111   0"
        "1     0        0"
        "0     0  1110000"
        "0     3        0"
        "0   10000      0"
        "0   0   11100  0"
        "0   0   0      0"
        "0   0   1  00000"
        "0       1      0"
        "2       1      0"
        "0       0      0"
        "0 0000000      0"
        "0              0"
        "0002222222200000"
    |] |> array2D

let arrayw, arrayh = 1024, 512
let mapw, maph = map.GetLength(0), map.GetLength(1)
let tilew, tileh = 32, 32

let white = (255uy, 255uy, 255uy)
let random = Random 0
let randomByte () = random.Next (0, 255) |> byte
let walls = [|0..3|] |> Array.map (fun _ -> (randomByte (), randomByte (), randomByte ()))
let fov = Math.PI/3.

let drawRect x y w h v array =
    for dx = x to x + w - 1 do
        for dy = y to y + h - 1 do
            Array2D.set array dx dy v

let drawMap array =
    for y = 0 to maph - 1 do
        for x = 0 to mapw - 1 do
            if map.[x, y] <> ' ' then
                let wallType = int map.[x, y] - int '0'
                drawRect (x * tilew) (y * tileh) tilew tileh walls.[wallType] array
    array

let drawPlayer px py array =
    drawRect (int (px * float tilew)) (int (py * float tileh)) 5 5 white array
    array

let drawRay px py pa array =
    (None, [0.0..0.1..19.0])
    ||> List.fold (fun stopPoint c ->
        match stopPoint with
        | Some _ -> stopPoint
        | None ->
            let cx =  px + c * cos pa
            let cy = py + c * sin pa
            if map.[int cx, int cy] <> ' ' then Some (c, int map.[int cx, int cy] - int '0')
            else
                let pixelx, pixely = int (cx * float tilew), int (cy * float tileh)
                Array2D.set array pixelx pixely (0uy, 0uy, 0uy)
                None)

let drawView px py pa array =
    [0..(arrayw/2)-1] |> List.iter (fun i ->
        let angle = pa-fov/2. + fov*float i/float (arrayw/2)
        match drawRay px py angle array with
        | None -> ()
        | Some (stopPoint, wallType) ->
            let viewPlaneDist = stopPoint * cos (angle - pa) // I understand what this does, and why. But not how (not a math guy :)
            let columnHeight = int (float arrayh / viewPlaneDist)
            drawRect (arrayw / 2 + i) ((arrayh - columnHeight) / 2) 1 columnHeight walls.[wallType] array)
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

    let mutable window, renderer = 0, 0
    SDL_CreateWindowAndRenderer(arrayw, arrayh, SDL_WindowFlags.SDL_WINDOW_SHOWN, &window, &renderer) |> ignore
    let mutable texture = SDL_CreateTexture(&renderer, SDL_PIXELFORMAT_ABGR8888, SDL_TEXTUREACCESS_STREAMING, arrayw, arrayh)
    
    let frameBuffer = Array2D.create arrayw arrayh white
    let pos = Marshal.UnsafeAddrOfPinnedArrayElement (frameBuffer, 0)
    let ptr = IntPtr (pos.ToPointer ())

    let rec drawLoop px py pa =
        frameBuffer
        |> drawMap
        |> drawPlayer px py
        |> drawView px py pa
        |> ignore

        SDL_UpdateTexture(texture, IntPtr.Zero, ptr, arrayw * 4) |> ignore
        SDL_RenderClear(&renderer) |> ignore
        SDL_RenderCopy(&renderer, texture, IntPtr.Zero, IntPtr.Zero) |> ignore
        SDL_RenderPresent(&renderer) |> ignore

        drawLoop px py pa

    let px, py, pa = 3.456, 2.345, 3.
    drawLoop px py pa

    SDL_DestroyTexture(texture)
    SDL_DestroyRenderer(&renderer)
    SDL_DestroyWindow(&window)
    SDL_Quit()

    0