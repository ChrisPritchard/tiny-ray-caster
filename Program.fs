
open SDL
open System
open System.IO
open System.Runtime.InteropServices
open System.Drawing

let map = File.ReadAllLines "map.txt" |> array2D
let isOpen (x: float) (y: float) = map.[int x, int y] = ' '

let viewWidth, viewHeight = 1024, 512
let mapw, maph = map.GetLength(0), map.GetLength(1)
let tilew, tileh = viewWidth/2/mapw, viewHeight/maph
let fov = Math.PI/3.
let turnSpeed, walkSpeed = 0.1, 0.1

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
        let pos = (dy * viewWidth) + x
        Array.fill array pos w v

let drawMap wallRows array =
    drawRect 0 0 (viewWidth/2) viewHeight white array
    for y = 0 to maph - 1 do
        for x = 0 to mapw - 1 do
            if not (isOpen (float x) (float y)) then
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
            if not (isOpen cx cy) then 
                let fcx, fcy = fraction cx, fraction cy
                let ratio = if fcx > 0.01 && fcx < 0.99 then fcx else fcy
                Some (c, int map.[int cx, int cy] - int '0', ratio)
            else
                let pixelx, pixely = int (cx * float tilew), int (cy * float tileh)
                Array.set array (pixely * viewWidth + pixelx) black
                None)

let drawView px py pa wallRows array =
    drawRect (viewWidth/2) 0 (viewWidth/2) (viewHeight/2) ceilingColour array
    drawRect (viewWidth/2) (viewHeight/2) (viewWidth/2) (viewHeight/2) floorColour array
    let da = fov / (float viewWidth/2.)
    [0..(viewWidth/2)-1] |> List.iter (fun i ->
        let angle = pa-(fov/2.) + (da*float i)
        match drawRay px py angle array with
        | None -> ()
        | Some (stopPoint, wallType, wallCol) ->
            let viewPlaneDist = stopPoint * cos (angle - pa) // I understand what this does, and why. But not how (not a math guy :)
            let columnHeight = int (float viewHeight / viewPlaneDist)
            let wallRow = Array.get wallRows (wallType * 64 + int (wallCol * 64.))
            let wy = 64. / float columnHeight
            let x = (viewWidth / 2) + i
            let y = (viewHeight - columnHeight) / 2
            for dy = y to y + columnHeight - 1 do
                if dy >= 0 && dy < viewHeight then
                    let pos = dy * viewWidth + x
                    let pix = int (wy * (float dy - float y))
                    array.[pos] <- Array.get wallRow pix)
            
[<EntryPoint>]
let main _ =

    SDL_Init(SDL_INIT_VIDEO) |> ignore

    let mutable window, renderer = IntPtr.Zero, IntPtr.Zero
    let windowFlags = SDL_WindowFlags.SDL_WINDOW_SHOWN ||| SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS
    SDL_CreateWindowAndRenderer(viewWidth, viewHeight, windowFlags, &window, &renderer) |> ignore
    let texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, viewWidth, viewHeight)

    let frameBuffer = Array.create (viewWidth * viewHeight) black
    let bufferPtr = IntPtr ((Marshal.UnsafeAddrOfPinnedArrayElement (frameBuffer, 0)).ToPointer ())
    let mutable keyEvent = Unchecked.defaultof<SDL_KeyboardEvent>

    let wallRows = wallRows ()

    let rec drawLoop px py pa =
        drawMap wallRows frameBuffer
        drawView px py pa wallRows frameBuffer

        SDL_UpdateTexture(texture, IntPtr.Zero, bufferPtr, viewWidth * 4) |> ignore
        SDL_RenderClear(renderer) |> ignore
        SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero) |> ignore
        SDL_RenderPresent(renderer) |> ignore

        if SDL_PollEvent(&keyEvent) = 0 || keyEvent.``type`` <> SDL_KEYDOWN then
            drawLoop px py pa // no keys were pressed, so continue as normal
        else if keyEvent.keysym.sym = SDLK_ESCAPE then 
            () // quit the game by executing the loop
        else
            let turn, walk = 
                match keyEvent.keysym.sym with
                | c when c = uint32 'a' -> -1., 0.
                | c when c = uint32 'd' -> 1., 0.
                | c when c = uint32 'w' -> 0., 1.
                | c when c = uint32 's' -> 0., -1.
                | _ -> 0., 0.
            let pa = pa + (turnSpeed * turn)
            let px, py = 
                let dx, dy = 
                    px + (cos pa * walkSpeed * walk),
                    py + (sin pa * walkSpeed * walk)
                if isOpen dx dy then dx, dy else px, py
            drawLoop px py pa    

    let px, py, pa = 3.456, 2.345, 0.
    drawLoop px py pa

    SDL_DestroyTexture(texture)
    SDL_DestroyRenderer(renderer)
    SDL_DestroyWindow(window)
    SDL_Quit()

    0