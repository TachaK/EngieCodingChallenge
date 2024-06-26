﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class PowerplantResponse
    {
        public PowerplantResponse(string name, double power)
        {
            Name = name;
            Power = power;
        }

        public string Name { get; set; }
        public double Power { get; set; }

    }

    public class Response
    {
        public List<PowerplantResponse> PowerplantResponses { get; set; }

        public Response()
        {
            PowerplantResponses= new List<PowerplantResponse>();    
        }
    }
}
