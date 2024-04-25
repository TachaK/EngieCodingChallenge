using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interface
{
    public interface IComputeSolution
    {
        Response Solution(Payload payload);
    }
}
