﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms_bl.Models
{
    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; } = null;
    }
}
