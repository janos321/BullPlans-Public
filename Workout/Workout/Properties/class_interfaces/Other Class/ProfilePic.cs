using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workout.Properties.class_interfaces.Other_Class
{
    public class ProfilePic
    {
        public class FileDownloadResult
        {
            public byte[] Bytes { get; init; } = Array.Empty<byte>();
            public string FileName { get; init; } = string.Empty;
        }
    }
}
