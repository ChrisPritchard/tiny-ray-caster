/// Wrappers and PInvoke of SDL2
module SDL

open System.Runtime.InteropServices

[<Literal>]
let libName = "SDL2.dll"

type SDL_WindowFlags =
| SDL_WINDOW_FULLSCREEN = 0x00000001
| SDL_WINDOW_OPENGL = 0x00000002
| SDL_WINDOW_SHOWN = 0x00000004

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_CreateWindowAndRenderer (int width, int height, SDL_WindowFlags flags, int& window, int& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern nativeint SDL_CreateTexture (int& renderer, uint32 format, int access, int width, int height)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_UpdateTexture(int& texture, int& rect, int& pixels, int pitch);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderClear(int& renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_RenderCopy(int& renderer, int& texture, int& srcrect, int& destrect);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_RenderPresent(int& renderer);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyTexture(int& texture);

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyRenderer(int& renderer)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_DestroyWindow(int& window)

[<DllImport(libName, CallingConvention = CallingConvention.Cdecl)>]
extern unit SDL_Quit()