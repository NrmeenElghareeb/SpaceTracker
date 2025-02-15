using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceTrack.DAL.Model;


namespace SpaceTrack.Services
{
    public interface ITLEService
    {
        
        Task SaveOrUpdateAllPayloadsTLEsAsync();
        Task SaveOrUpdateAllDebrisTLEsAsync();

        Task SaveOrUpdateAllRocketsTLEsAsync();
        Task SaveOrUpdateAllUnknownTLEsAsync();
        Task ComputeEciAsync();
        
    }















    ////////////////////////////////////////////////////////////////////////////////////////Excellent
    //Task SaveOrUpdateTLEsAsync();
    ///////////////////////////////////////////////////////////////////////////////////////Excellent

    //Task SaveOrUpdateTLEOfISSAsync();///////////////Change 3
    //Task SaveTLEOfISSAsync(); change //////////////////////2
    //Task<string> FetchTLEFromSpaceTrack(int[] noradIds);
    //Task SaveTLEToDatabase(string tleData);
}

//https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/DEBRIS/orderby/ORDINAL%20asc/limit/40000/format/tle/emptyresult/show
//https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/orderby/ORDINAL%20asc/limit/11100/format/tle/emptyresult/show
//https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/ROCKET%20BODY/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show
//Active payloads //https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/PAYLOAD/DECAYED/0%20/orderby/ORDINAL%20asc/limit/8000/format/tle/emptyresult/show
//Unknown //https://www.space-track.org/basicspacedata/query/class/tle_latest/OBJECT_TYPE/UNKNOWN/orderby/ORDINAL%20asc/limit/10000/format/tle/emptyresult/show