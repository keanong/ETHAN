using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.BS
{
    public class Msg_ContentType
    {
        public string Content { get; }
        public int pcs {  get; set; }
        public float weight { get; set; }

        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }


        public Msg_ContentType(string content)
        {
            this.Content = content;
        }

    }
}
