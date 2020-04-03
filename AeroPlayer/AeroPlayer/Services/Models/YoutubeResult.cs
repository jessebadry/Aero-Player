using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace AeroPlayer.Models
{
    public class YoutubeResult
    {
        public string Url { get; set; }
        public string ImgUrl
        {
            get; set;
        }
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Views { get; set; }
        public ImageSource ImageBitMap { get; set; }
    }
}
