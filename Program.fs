
open SDL
open System
open System.Runtime.InteropServices
open System.Drawing

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
let fov = Math.PI/3.

let asUint32 (r, g, b) = BitConverter.ToUInt32 (ReadOnlySpan [|b; g; r; 255uy|])

let white = asUint32 (255uy, 255uy, 255uy)
let ceilingColour = asUint32 (200uy, 200uy, 200uy)
let floorColour = asUint32 (150uy, 150uy, 150uy)
let black = asUint32 (0uy, 0uy, 0uy)

let wallRows () =
    let image = Image.FromFile "walltext.png" :?> Bitmap
    [|0..image.Width-1|] 
    |> Array.map (fun x -> 
        [|0..image.Height-1|] |> Array.map (fun y -> image.GetPixel(x, y).ToArgb() |> uint32))

let drawRect x y w h v array =
    for dy = y to y + h - 1 do
        let pos = (dy * arrayw) + x
        Array.fill array pos w v

let drawMap wallRows array =
    drawRect 0 0 (arrayw/2) arrayh white array
    for y = 0 to maph - 1 do
        for x = 0 to mapw - 1 do
            if map.[x, y] <> ' ' then
                let wallType = int map.[x, y] - int '0'
                let wallColour = Array.get wallRows (wallType*64) |> Array.head
                drawRect (x * tilew) (y * tileh) tilew tileh wallColour array
                
let fraction (f: float) = abs (f - truncate f)

let drawRay px py pa array =
    let cpa = cos pa
    let spa = sin pa
    (None, [0.0..0.01..20.0])
    ||> List.fold (fun stopPoint c ->
        match stopPoint with
        | Some _ -> stopPoint
        | None ->
            let cx =  px + c * cpa
            let cy = py + c * spa
            if map.[int cx, int cy] <> ' ' then 
                let fcx, fcy = fraction cx, fraction cy
                let ratio = if fcx > 0.01 && fcx < 0.99 then fcx else fcy
                Some (c, int map.[int cx, int cy] - int '0', ratio)
            else
                let pixelx, pixely = int (cx * float tilew), int (cy * float tileh)
                drawRect pixelx pixely 1 1 black array
                None)

let drawView px py pa wallRows array =
    drawRect (arrayw/2) 0 (arrayw/2) (arrayh/2) ceilingColour array
    drawRect (arrayw/2) (arrayh/2) (arrayw/2) (arrayh/2) floorColour array
    let da = fov / (float arrayw/2.)
    [0..(arrayw/2)-1] |> List.iter (fun i ->
        let angle = pa-(fov/2.) + (da*float i)
        match drawRay px py angle array with
        | None -> ()
        | Some (stopPoint, wallType, wallCol) ->
            let viewPlaneDist = stopPoint * cos (angle - pa) // I understand what this does, and why. But not how (not a math guy :)
            let columnHeight = int (float arrayh / viewPlaneDist)
            let wallRow = Array.get wallRows (wallType * 64 + int (wallCol * 64.))
            let wy = 64. / float columnHeight
            let x = (arrayw / 2) + i
            let y = (arrayh - columnHeight) / 2
            for dy = y to y + columnHeight - 1 do
                let pos = dy * arrayw + x
                let pix = int (wy * (float dy - float y))
                array.[pos] <- Array.get wallRow pix)
            
[<EntryPoint>]
let main _ =

    SDL_Init(SDL_INIT_VIDEO) |> ignore

    let mutable window, renderer = IntPtr.Zero, IntPtr.Zero
    let windowFlags = SDL_WindowFlags.SDL_WINDOW_SHOWN ||| SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS
    SDL_CreateWindowAndRenderer(arrayw, arrayh, windowFlags, &window, &renderer) |> ignore
    let texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, arrayw, arrayh)

    let frameBuffer = Array.create (arrayw * arrayh) black
    let bufferPtr = IntPtr ((Marshal.UnsafeAddrOfPinnedArrayElement (frameBuffer, 0)).ToPointer ())
    let mutable keyEvent = Unchecked.defaultof<SDL_KeyboardEvent>

    let wallRows = wallRows ()

    let rec drawLoop px py pa =
        drawMap wallRows frameBuffer
        drawView px py pa wallRows frameBuffer

        if SDL_PollEvent(&keyEvent) <> 0 && keyEvent.``type`` = SDL_KEYDOWN then
            ()

        SDL_UpdateTexture(texture, IntPtr.Zero, bufferPtr, arrayw * 4) |> ignore
        SDL_RenderClear(renderer) |> ignore
        SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero) |> ignore
        SDL_RenderPresent(renderer) |> ignore

        drawLoop px py (pa + (Math.PI/360.))

    let px, py, pa = 3.456, 2.345, 0.
    drawLoop px py pa

    SDL_DestroyTexture(texture)
    SDL_DestroyRenderer(renderer)
    SDL_DestroyWindow(window)
    SDL_Quit()

    0