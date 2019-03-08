/// Wrappers and PInvoke of SDL2
module SDL

open System.Runtime.InteropServices
open System

[<Literal>]
let libName = "SDL2.dll"

type SDL_WindowFlags =
| SDL_WINDOW_FULLSCREEN = 0x00000001
| SDL_WINDOW_OPENGL = 0x00000002
| SDL_WINDOW_SHOWN = 0x00000004

let SDL_TEXTUREACCESS_STREAMING = 1
let SDL_PIXELTYPE_PACKED32 = 6
let SDL_PACKEDORDER_ABGR = 10
let SDL_PACKEDLAYOUT_8888 = 6

//SDL_DEFINE_PIXELFORMAT(
//				SDL_PIXELTYPE_ENUM.SDL_PIXELTYPE_PACKED32,
//				SDL_PIXELORDER_ENUM.SDL_PACKEDORDER_ABGR,
//				SDL_PACKEDLAYOUT_ENUM.SDL_PACKEDLAYOUT_8888,
//				32, 4
//);

let SDL_PIXELFORMAT_ABGR8888 = 
    uint32 ((1 <<< 28) ||| ((SDL_PIXELTYPE_PACKED32) <<< 24) ||| ((SDL_PACKEDORDER_ABGR) <<< 20) ||| ((SDL_PACKEDLAYOUT_8888) <<< 16) ||| (32 <<< 8) ||| (6))
    
[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_CreateWindowAndRenderer (int width, int height, SDL_WindowFlags flags, int& window, int& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr SDL_CreateTexture (int& renderer, uint32 format, int access, int width, int height)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderClear(int& renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderCopy(int& renderer, IntPtr texture, IntPtr srcrect, IntPtr destrect);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_RenderPresent(int& renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyTexture(IntPtr texture);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyRenderer(int& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyWindow(int& window)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_Quit()