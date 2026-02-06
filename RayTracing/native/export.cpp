#include "rtweekend.h"
#include "hittable_list.h"
#include "material.h"
#include "sphere.h"
#include "color.h"
#include "camera.h"
#include <vector>
#include <cstring>
#include <memory>
#include <cstdint>


#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "external/stb_image_write.h"

#if defined(_WIN32)
  #define RT_API extern "C" __declspec(dllexport)
  #define RT_CALL __cdecl
#else
  #define RT_API extern "C"
  #define RT_CALL
#endif

struct triplet
{
    double x,y,z;
};
struct CameraConfig
{
    double aspect_ratio      = 1.0;  // Ratio of image width over height
    int    image_width       = 100;  // Rendered image width in pixel count
    int    samples_per_pixel = 10;   // Count of random samples for each pixel
    int    max_depth         = 10;   // Maximum number of ray bounces into scene

    double vfov     = 90;              // Vertical view angle (field of view)
    triplet lookfrom = {0,0,0};   // Point camera is looking from
    triplet lookat   = {0,0,-1};  // Point camera is looking at
    triplet   vup      = {0,1,0};// Camera-relative "up" direction

   double defocus_angle = 0;  // Variation angle of rays through each pixel
   double focus_dist = 10;
  
};

using SceneHandle = void*;
using MaterialHandle = void*;
using EyePointer = void*;
using MaterialPointer = std::shared_ptr<material>;
using HittablePtr = std::shared_ptr<hittable>;

struct SceneHolder
{
  hittable_list world;
};

typedef void (RT_CALL *RenderCallback)(int samples, uint8_t* buffer);


//----Materials
RT_API MaterialHandle RT_CALL CreateLambertian(double r, double g, double b)
{
    return new MaterialPointer(std::make_shared<lambertian>(color(r,g,b)));  
};
RT_API MaterialHandle RT_CALL CreateMetal(double r, double g, double b, double fuzz)
{
    return new MaterialPointer(std::make_shared<metal>(color(r,g,b), fuzz));  
};
RT_API MaterialHandle RT_CALL CreateDielectric(double refraction_index)
{
    return new MaterialPointer(std::make_shared<dielectric>(refraction_index));  
};

RT_API void RT_CALL DestroyMaterial(MaterialHandle material)
{
    delete static_cast<MaterialPointer*>(material);
}
//----Scenes

RT_API SceneHandle RT_CALL CreateScene()
{
  return new SceneHolder();  
};
RT_API void RT_CALL SceneAddSphere(SceneHandle scene, triplet center, double radius, MaterialHandle material)
{
    point3 c(center.x, center.y, center.z);
    auto* s = static_cast<SceneHolder*>(scene);
    auto* m = static_cast<MaterialPointer*>(material);
    auto sph = make_shared<sphere>(c, radius, *m);
    s->world.add(sph);
}

RT_API void RT_CALL RenderScene(CameraConfig config, SceneHandle scene, uint8_t* buffer, RenderCallback callback)
{   
    auto* s = static_cast<SceneHolder*>(scene);
    camera c;
    c.aspect_ratio = config.aspect_ratio;
    c.image_width = config.image_width;
    c.samples_per_pixel = config.samples_per_pixel;
    c.max_depth = config.max_depth;
    c.vfov = config.vfov;
    c.lookfrom.e[0] = config.lookfrom.x;
    c.lookfrom.e[1] = config.lookfrom.y;
    c.lookfrom.e[2] = config.lookfrom.z;
    c.lookat.e[0] = config.lookat.x;
    c.lookat.e[1] = config.lookat.y;
    c.lookat.e[2] = config.lookat.z;
    c.vup.e[0] = config.vup.x;
    c.vup.e[1] = config.vup.y;
    c.vup.e[2] = config.vup.z;
    c.defocus_angle = config.defocus_angle;
    c.focus_dist = config.focus_dist;
    c.render(s->world, buffer, callback);
    
}
RT_API void RT_CALL DestroyScene(SceneHandle scene)
{
    delete static_cast<SceneHolder*>(scene);
}
RT_API int RT_CALL SavePng(int w, int h, uint8_t* buffer , char const pathname[] )
{   
    int stridelength = sizeof(uint8_t)*w*4;
    return stbi_write_png(pathname, w,h, 4, buffer, stridelength);
}
