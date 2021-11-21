using static SDL2.SDL;
using static SDL2.SDL_image;
using Fjord.Modules.Mathf;
using System;
using System.IO;

namespace Fjord.Modules.Graphics {
    public static class texture_handler {
        public static IntPtr default_texture;

        public static void init() {
            IntPtr tmp_surface = IMG_Load(game.get_resource_folder() + "/general/assets/images/error.png");
            default_texture = SDL_CreateTextureFromSurface(game.renderer, tmp_surface);
            SDL_FreeSurface(tmp_surface);       
        }
    }

    public class texture {
        private IntPtr sdl_texture;
        private V2 origin = new V2(0, 0);
        private flip_type flip = flip_type.none;
        private double angle = 0;
        private V2f scale = new V2f(1, 1);
        private int alpha = 255;

        public texture() {
            sdl_texture = texture_handler.default_texture;
        }

        public void set_texture(string path) {
            IntPtr tmp_surface = IMG_Load(game.get_resource_folder() + "/" + game.asset_pack + "/assets/images/" + path);
            IntPtr texture = texture_handler.default_texture;
            if(File.Exists(game.get_resource_folder() + "/" + game.asset_pack + "/assets/images/" + path)) {
                texture = SDL_CreateTextureFromSurface(game.renderer, tmp_surface);
            } else {
                Debug.Debug.error("Image not found: " + game.get_resource_folder() + "/" + game.asset_pack + "/assets/images/" + path);
                game.stop();
                this.sdl_texture = texture;
            }
             
            SDL_FreeSurface(tmp_surface);
            this.sdl_texture = texture;
        }

        public void set_texture(IntPtr tex) {
            sdl_texture = tex;
        }

        public IntPtr get_texture() {
            return sdl_texture;
        }

        public void set_origin(V2 origin) {
            this.origin = origin;
        }

        public void set_origin(int x, int y) {
            this.origin = new V2(x, y);
        }

        public void set_origin(draw_origin set_origin) {
            V2 texture_size = get_size();

            switch(set_origin) {
                case draw_origin.TOP_LEFT:
                    this.origin = new V2(0, 0);
                    break;
                case draw_origin.TOP_MIDDLE:
                    this.origin = new V2(texture_size.x / 2, 0);
                    break;
                case draw_origin.TOP_RIGHT:
                    this.origin = new V2(texture_size.x, 0);
                    break;
                case draw_origin.BOTTOM_RIGHT:
                    this.origin = new V2(texture_size.x, texture_size.y);
                    break;
                case draw_origin.BOTTOM_MIDDLE:
                    this.origin = new V2(texture_size.x / 2, texture_size.y);
                    break;     
                case draw_origin.BOTTOM_LEFT:
                    this.origin = new V2(0, texture_size.y);
                    break;      
                case draw_origin.MIDDLE_LEFT:
                    this.origin = new V2(0, texture_size.y / 2);
                    break;        
                case draw_origin.MIDDLE_RIGHT:
                    this.origin = new V2(texture_size.x, texture_size.y / 2);
                    break;       
                case draw_origin.CENTER:
                    this.origin = new V2(texture_size.x / 2, texture_size.y / 2);
                    break;  
            }
        } 

        public V2 get_origin() {
            return origin;
        }

        public void set_scale(V2f scale) {
            this.scale = scale;
        }

        public void set_scale(float w, float h) {
            this.scale = new V2f(w, h);
        }

        public V2f get_scale() {
            return scale;
        }

        public void set_fliptype(flip_type flip) {
            this.flip = flip;
        }

        public flip_type get_fliptype() {
            return flip;
        }

        public void set_angle(double angle) {
            this.angle = angle;
        }

        public double get_angle() {
            return angle;
        }

        public void set_alpha(int alpha) {
            this.alpha = alpha;
        }

        public int get_alpha() {
            return alpha;
        }

        public V2 get_size() {
            V2 texture_size = new V2();
            uint format;
            int access;
            SDL_QueryTexture(sdl_texture, out format, out access, out texture_size.x, out texture_size.y);

            texture_size.x = (int)(texture_size.x * scale.x);
            texture_size.y = (int)(texture_size.y * scale.y); 
            return texture_size;
        }
    }
}