///////////////////////////////////////////////////////////////////
// Aniket Kudale
// Email: aniket.kudale@hotmail.com 
// github.com/aniketkudale
// Feel Free to modify
// Thanks to Lucas Darnell
////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using System.Threading; ///<- useful for c


namespace MyAR
{
    public partial class Form1 : Form
    {

        private Thread thr; // suitable for this application
        private String option; // for combo box

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            execute();
        }
        private void execute()
        {
            if (button1.Text.Equals("Start"))
            {
                option = comboBox1.Text;
                thr = new Thread(new ThreadStart(Run));
                thr.Start();
                button1.Text = "Stop";
            }
            else
            {
                if (thr != null && thr.IsAlive)
                {
                    
                    thr.Abort();
                    thr = null;
                    pictureBox1.Image = null;
                }
                button1.Text = "Start";
            }

        }

        private void Run()
        {
            CvCapture cap = Cv.CreateCameraCapture(0);  // Camera object
            CvCapture vid = CvCapture.FromFile("trailer.avi");  /// object for video
            IplImage pic = new IplImage("pic.jpg", LoadMode.AnyColor | LoadMode.AnyDepth); ///object for image

            Cv.Flip(pic, pic, FlipMode.Y);

            int marker_width = 5;
            int marker_height = 4;
            int marker_sqr = 20;

            CvSize marker_size = new CvSize(marker_width, marker_height);

            CvMat warp_matrix = Cv.CreateMat(3, 3, MatrixType.F32C1);
            CvPoint2D32f[] corners = new CvPoint2D32f[marker_sqr];

            IplImage image;
            IplImage frame;
            IplImage display;
            IplImage copy_image;
            IplImage neg_image;

            int cor_count;   ///count corners
                             ///
            while (thr != null)
            {
                image = Cv.QueryFrame(cap);

            
                display = Cv.CreateImage(Cv.GetSize(image), BitDepth.U8, 3);
                copy_image = Cv.CreateImage(Cv.GetSize(image), BitDepth.U8, 3);
                neg_image = Cv.CreateImage(Cv.GetSize(image), BitDepth.U8, 3);

               IplImage gray = Cv.CreateImage(Cv.GetSize(image), image.Depth, 1);
               bool found = Cv.FindChessboardCorners(image, marker_size, out corners, out cor_count, ChessboardFlag.AdaptiveThresh | ChessboardFlag.FilterQuads);

                Cv.CvtColor(image, gray, ColorConversion.BgrToGray);
                
                if (cor_count == marker_sqr)
                {
                    if (option == "Picture")
                    {
                        CvPoint2D32f[] p = new CvPoint2D32f[4];
                        CvPoint2D32f[] q = new CvPoint2D32f[4];

                        IplImage blank = Cv.CreateImage(Cv.GetSize(pic), BitDepth.U8, 3);
                        Cv.Zero(blank);
                        Cv.Not(blank, blank);

                        q[0].X = (float)pic.Width * 0;
                        q[0].Y = (float)pic.Height * 0;
                        q[1].X = (float)pic.Width;
                        q[1].Y = (float)pic.Height * 0;

                        q[2].X = (float)pic.Width;
                        q[2].Y = (float)pic.Height;
                        q[3].X = (float)pic.Width * 0;
                        q[3].Y = (float)pic.Height;

                        p[0].X = corners[0].X;
                        p[0].Y = corners[0].Y;
                        p[1].X = corners[4].X;
                        p[1].Y = corners[4].Y;

                        p[2].X = corners[19].X;
                        p[2].Y = corners[19].Y;
                        p[3].X = corners[15].X;
                        p[3].Y = corners[15].Y;

                        Cv.GetPerspectiveTransform(q, p, out warp_matrix);

                        Cv.Zero(neg_image);
                        Cv.Zero(copy_image);

                        Cv.WarpPerspective(pic, neg_image, warp_matrix);
                        Cv.WarpPerspective(blank, copy_image, warp_matrix);
                        Cv.Not(copy_image, copy_image);

                        Cv.And(copy_image, image, copy_image);
                        Cv.Or(copy_image, neg_image, image);

                        Cv.Flip(image, image, FlipMode.Y);
                        
                        Bitmap bm = BitmapConverter.ToBitmap(image);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                    else
                    {
                        CvPoint2D32f[] p = new CvPoint2D32f[4];
                        CvPoint2D32f[] q = new CvPoint2D32f[4];

                        frame = Cv.QueryFrame(vid);

                        Cv.Flip(frame, frame, FlipMode.Y);

                        IplImage blank = Cv.CreateImage(Cv.GetSize(frame), BitDepth.U8, 3);
                        Cv.Zero(blank);
                        Cv.Not(blank, blank);

                        q[0].X = (float)frame.Width * 0;
                        q[0].Y = (float)frame.Height * 0;
                        q[1].X = (float)frame.Width;
                        q[1].Y = (float)frame.Height * 0;

                        q[2].X = (float)frame.Width;
                        q[2].Y = (float)frame.Height;
                        q[3].X = (float)frame.Width * 0;
                        q[3].Y = (float)frame.Height;

                        p[0].X = corners[0].X;
                        p[0].Y = corners[0].Y;
                        p[1].X = corners[4].X;
                        p[1].Y = corners[4].Y;

                        p[2].X = corners[19].X;
                        p[2].Y = corners[19].Y;
                        p[3].X = corners[15].X;
                        p[3].Y = corners[15].Y;

                        Cv.GetPerspectiveTransform(q, p, out warp_matrix);

                        Cv.Zero(neg_image);
                        Cv.Zero(copy_image);

                        Cv.WarpPerspective(frame, neg_image, warp_matrix);
                        Cv.WarpPerspective(blank, copy_image, warp_matrix);
                        Cv.Not(copy_image, copy_image);

                        Cv.And(copy_image, image, copy_image);
                        Cv.Or(copy_image, neg_image, image);

                        Cv.Flip(image, image, FlipMode.Y);
                        //Cv.ShowImage("video", img);
                        Bitmap bm = BitmapConverter.ToBitmap(image);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                  
                }
                else
                {
                    Cv.Flip(gray, gray, FlipMode.Y);
                   Bitmap bm = BitmapConverter.ToBitmap(gray);
                    bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.Image = bm;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

       
            

        }
    }

