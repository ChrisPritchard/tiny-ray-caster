
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

let white = (255uy, 255uy, 255uy, 255uy)
let black = (255uy, 0uy, 0uy, 0uy)
let random = Random 0
let randomByte () = random.Next (0, 255) |> byte
let walls = [|0..3|] |> Array.map (fun _ -> (255uy, randomByte (), randomByte (), randomByte ()))
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
    let cpa = cos pa
    let spa = sin pa
    (None, [0.0..0.1..19.0])
    ||> List.fold (fun stopPoint c ->
        match stopPoint with
        | Some _ -> stopPoint
        | None ->
            let cx =  px + c * cpa
            let cy = py + c * spa
            if map.[int cx, int cy] <> ' ' then Some (c, int map.[int cx, int cy] - int '0')
            else
                let pixelx, pixely = int (cx * float tilew), int (cy * float tileh)
                Array2D.set array pixelx pixely black
                None)

let drawView px py pa array =
    let da = fov / (float arrayw/2.)
    [0..(arrayw/2)-1] |> List.iter (fun i ->
        let angle = pa-(fov/2.) + (da*float i)
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
            let (r, g, b, a) = array.[x, y]
            Seq.iter out.WriteByte [a;b;g;r]

[<EntryPoint>]
let main _ =

    let mutable window, renderer = IntPtr.Zero, IntPtr.Zero
    SDL_CreateWindowAndRenderer(arrayw, arrayh, SDL_WindowFlags.SDL_WINDOW_SHOWN, &window, &renderer) |> ignore
    SDL_SetRenderDrawColor(renderer, 255uy, 0uy, 255uy, 255uy) |> ignore
    let mutable texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ABGR8888, SDL_TEXTUREACCESS_STREAMING, arrayw, arrayh)

    let frameBuffer = Array.create (arrayw * arrayh * 4) 255uy
    let pos = Marshal.UnsafeAddrOfPinnedArrayElement (frameBuffer, 0)
    let ptr = IntPtr (pos.ToPointer ())

    let rec drawLoop px py pa =
        let map = Array2D.create arrayw arrayh white
        map
        |> drawMap
        |> drawPlayer px py
        |> drawView px py pa
        |> ignore

        for y = 0 to arrayh - 1 do
            for x = 0 to arrayw - 1 do
                let pos = ((y * arrayw) + x) * 4
                let (a, g, b, r) = map.[x, y]
                frameBuffer.[pos] <- a
                frameBuffer.[pos+1] <- b
                frameBuffer.[pos+2] <- g
                frameBuffer.[pos+3] <- r

        SDL_UpdateTexture(texture, IntPtr.Zero, ptr, arrayw * 4) |> ignore
        SDL_RenderClear(renderer) |> ignore
        SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero) |> ignore
        SDL_RenderPresent(renderer) |> ignore

        drawLoop px py (pa + (Math.PI/360.))

    let px, py, pa = 3.456, 2.345, 15.
    drawLoop px py pa

    SDL_DestroyTexture(texture)
    SDL_DestroyRenderer(renderer)
    SDL_DestroyWindow(window)
    SDL_Quit()

    0