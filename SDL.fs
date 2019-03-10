/// Wrappers and PInvoke of SDL2
/// Note I have only implemented the methods and constants I actually use - this is not a complete set of SDL by any means.
module SDL

open System.Runtime.InteropServices
open System

[<Literal>]
let libName = "SDL2.dll"

let SDL_INIT_VIDEO = 0x00000020u

type SDL_WindowFlags =
| SDL_WINDOW_SHOWN = 0x00000004
| SDL_WINDOW_INPUT_FOCUS = 0x00000200

let SDL_TEXTUREACCESS_STREAMING = 1
let SDL_PIXELFORMAT_ABGR8888 = 376840196u // derived by turning SDL2-CS into a console app and finding out the exact value for this.

type SDL_EventType =
| SDL_QUIT = 0x100
| SDL_KEYDOWN = 0x300
| SDL_KEYUP = 769

[<type:StructLayout(LayoutKind.Sequential)>]
type SDL_Keysym = {
    scancode: SDL_Scancode
    sym: SDL_Keycode
    ``mod``: SDL_Keymod
    unicode: uint32
} 
and SDL_Scancode = 
| SDL_SCANCODE_ESCAPE = 41
and SDL_Keycode =
| SDLK_ESCAPE = 27
and SDL_Keymod =
| KMOD_NONE = 0x0000

[<type:StructLayout(LayoutKind.Sequential)>]
type SDL_KeyboardEvent = {
    ``type``: SDL_EventType
    timestamp: uint32
    windowID: uint32
    state: byte
    repeat: byte
    padding2: byte
    padding3: byte
    keysym: SDL_Keysym
} 
    
[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_Init(uint32 flags)
    
[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_CreateWindowAndRenderer (int width, int height, SDL_WindowFlags flags, IntPtr& window, IntPtr& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr SDL_CreateTexture (IntPtr renderer, uint32 format, int access, int width, int height)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderClear(IntPtr renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture, IntPtr srcrect, IntPtr destrect);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_RenderPresent(IntPtr renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_PollEvent(SDL_KeyboardEvent& _event)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyTexture(IntPtr texture);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyRenderer(IntPtr renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyWindow(IntPtr window)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_Quit()