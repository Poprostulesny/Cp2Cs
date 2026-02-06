using System.Runtime.InteropServices;

namespace RayTracing;

public class NativeStructs
{   
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public  struct Triplet
    {
        public double x,y,z;
        public Triplet(double _x, double _y, double _z)
        {
            this.x = _x; this.y = _y; this.z = _z;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public  struct Camera()
    {
        public double aspect_ratio      = 1.0;  // Ratio of image width over height
        public int    image_width       = 100;  // Rendered image width in pixel count
        public int    samples_per_pixel = 10;   // Count of random samples for each pixel
        public int    max_depth         = 10;   // Maximum number of ray bounces into scene
        
        public double vfov     = 90;              // Vertical view angle (field of view)
        public Triplet lookfrom = new Triplet(0,0,0);   // Point camera is looking from
        public Triplet lookat   = new Triplet(0,0,-1);  // Point camera is looking at
        public Triplet   vup      = new Triplet(0,1,0);     // Camera-relative "up" direction

        public double defocus_angle = 0;  // Variation angle of rays through each pixel
        public double focus_dist = 10;
    }
    
}
