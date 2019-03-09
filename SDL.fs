/// Wrappers and PInvoke of SDL2
/// Note I have only implemented the methods and constants I actually use - this is not a complete set of SDL by any means.
module SDL

open System.Runtime.InteropServices
open System

[<Literal>]
let libName = "SDL2.dll"

type SDL_WindowFlags =
| SDL_WINDOW_SHOWN = 0x00000004

let SDL_TEXTUREACCESS_STREAMING = 1
let SDL_PIXELFORMAT_ABGR8888 = 376840196u // derived by turning SDL2-CS into a console app and finding out the exact value for this.
    
[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_CreateWindowAndRenderer (int width, int height, SDL_WindowFlags flags, IntPtr& window, IntPtr& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr SDL_CreateTexture (IntPtr renderer, uint32 format, int access, int width, int height)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderClear(IntPtr renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture, IntPtr srcrect, IntPtr destrect);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_RenderPresent(IntPtr renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyTexture(IntPtr texture);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyRenderer(IntPtr renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyWindow(IntPtr window)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_Quit()