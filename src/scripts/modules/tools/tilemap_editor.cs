using Proj.Modules.Input;
using Proj.Modules.Ui;
using Proj.Modules.Debug;
using Proj.Modules.Graphics;
using Proj.Modules.Camera;
using Proj.Modules.Tools;
using Proj.Modules.Misc;
using System.Collections.Generic;
using SDL2;
using System;
using System.Numerics;

namespace Proj.Game {
    public class tilemap_editor : scene {
        
        float zoom = 1;
        Vector2 pos;

        bool load_tex = false;
        string load_texture_string = "";

        bool load_tex_button = false;

        string asset_pack = "general";
        bool change_asset_pack = false;

        tilemap Tilemap = new tilemap(20, 20, 16, 16);
        List<IntPtr> texs = new List<IntPtr>();

        int grid_x, grid_y, grid_x_end, grid_y_end;

        int selected_tile = -1;

        public override void on_load() {
            game_manager.set_asset_pack("general");
            font_handler.load_font("font", "FiraCode", 22);
        }

        public override void update() {
            if(mouse.scrolling(0)) {
                zoom += 0.1f;
            } else if(mouse.scrolling(1)) {
                zoom -= 0.1f;
            }

            int move_sp = 4;
            if(input.get_key_pressed(input.key_w, "general")) {
                pos.Y -= move_sp;
            } else if(input.get_key_pressed(input.key_s, "general")) {
                pos.Y += move_sp;
            }

            if(input.get_key_pressed(input.key_a, "general")) {
                pos.X -= move_sp;
            } else if(input.get_key_pressed(input.key_d, "general")) {
                pos.X += move_sp;
            }   

            camera.set_viewport(pos.X, pos.Y);   

            if(math_uti.mouse_inside(470, 10, 200, 30) && mouse.button_just_pressed(0)) {
                change_asset_pack = !change_asset_pack;
            }  

            if(change_asset_pack) {
                input.set_input_state("set_asset_pack");
            } else if(input.input_state == "set_asset_pack") {
                input.set_input_state("general");
            }

            if(math_uti.mouse_inside(260, 10, 200, 30) && mouse.button_just_pressed(0)) {
                load_tex = !load_tex;
            }    

            if(load_tex) {
                input.set_input_state("load_texture");
            } else if(input.input_state == "load_texture") {
                input.set_input_state("general");
            }

            if(load_tex_button) {
                string ass = game_manager.asset_pack;
                game_manager.set_asset_pack(asset_pack);
                texs.Add(texture_handler.load_texture(load_texture_string, game_manager.renderer));
                Tilemap.textures.Add(load_texture_string);
                load_tex_button = false;
                load_tex = false;
                load_texture_string = "";
                game_manager.set_asset_pack(ass);
            }

            grid_x = (int)(Tilemap.grid_w * zoom) - (int)camera.camera_position.X - (int)(Tilemap.grid_w * zoom);
            grid_y = (int)(Tilemap.grid_h * zoom) - (int)camera.camera_position.Y - (int)(Tilemap.grid_h * zoom);
            grid_x_end = grid_x + Tilemap.w * (int)(Tilemap.grid_w * zoom);
            grid_y_end = grid_y + Tilemap.h * (int)(Tilemap.grid_h * zoom);

            if(mouse.button_just_pressed(0) && selected_tile != -1 && math_uti.mouse_inside(grid_x, grid_y, grid_x_end, grid_y_end)) {
                var x_ = (mouse.x - grid_x);
                var y_ = (mouse.y - grid_y);
                var w_ = Tilemap.grid_w * zoom;
                var h_ = Tilemap.grid_h * zoom;
    
                x_ = x_ / (int)w_;
                y_ = y_ / (int)h_;

                if(x_ < Tilemap.w && x_ > -1 && y_ < Tilemap.h && y_ > -1)
                    Tilemap.map[(int)x_, (int)y_] = 1;
            }
        }

        public override void render() {
            for(var i = 0; i < Tilemap.w; i++) {
                for(var j = 0; j < Tilemap.h; j++) {
                    SDL.SDL_Rect rect;
                    rect.x = i * (int)(Tilemap.grid_w * zoom) + (grid_x);
                    rect.y = j * (int)(Tilemap.grid_h * zoom) + (grid_y);
                    rect.w = (int)(Tilemap.grid_w * zoom);
                    rect.h = (int)(Tilemap.grid_h * zoom);
                    draw.rect(game_manager.renderer, rect, 255, 255, 255, 255, false);

                    if(Tilemap.map[i, j] == 1) {
                        int w, h;
                        w = (int)(Tilemap.grid_w * zoom);
                        h = (int)(Tilemap.grid_h * zoom);
                        draw.texture_ext(game_manager.renderer, texs[0], rect.x + 8, rect.y + 8, 0, w, h);
                    }
                }
            }

            SDL.SDL_Rect rect1;
            rect1.x = 0;
            rect1.y = 0;
            rect1.w = 250;
            rect1.h = 1280;
            draw.rect(game_manager.renderer, rect1, 0, 0, 0, 200, true);

            rect1.x = grid_x;
            rect1.y = grid_y;
            rect1.w = grid_x_end - grid_x;
            rect1.h = grid_y_end - grid_y;
            draw.rect(game_manager.renderer, rect1, 0, 255, 0, 255, false);

            int iw = 0;
            foreach(IntPtr tex in texs) {
                if(iw == selected_tile) {
                    SDL.SDL_Rect rect;
                    rect.x = 10;
                    rect.y = 10;
                    rect.w = 69;
                    rect.h = 69;
                    draw.rect(game_manager.renderer, rect, 52, 134, 235, 255, true);
                }
                draw.texture_ext(game_manager.renderer, tex, 20, 20, 0, 64, 64);
                if(math_uti.mouse_inside(20, 20, 64, 64) && mouse.button_just_pressed(0)) {
                    selected_tile = iw;
                }
                iw++;
            }

            zgui.input_box(260, 10, 200, 30, "font", ref load_texture_string, "texture path", "load_texture");
            zgui.button(260, 50, 80, 30, ref load_tex_button, "font", "Load");

            zgui.input_box(470, 10, 200, 30, "font", ref asset_pack, "", "set_asset_pack");
        }
    }
}