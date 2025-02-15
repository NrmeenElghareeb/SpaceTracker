using SpaceTrack.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceTrack.Services
{
    public interface IEciService
    {
        Task<EciPosition> ComputeEciAsync (string? Id, int NoradId, string FirstLine, string SecondLine, DateTime Timestamp);
        Task<List<EciPosition>> ComputeEciForDayAsync(string objectId, int noradId, string firstLine, string secondLine, DateTime date);
    }

}
