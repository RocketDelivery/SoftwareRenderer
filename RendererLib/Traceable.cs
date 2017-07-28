using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRenderTest
{
    public abstract class Traceable
    {
        public TraceResult IntersectClosest(Vector3 pos, Vector3 dir, bool query = false)
        {
            TraceResult traceResult = null;
            foreach(TraceResult curResult in Intersect(pos, dir, query))
            {
                if(traceResult == null)
                {
                    traceResult = curResult;
                }
                else
                {
                    if(curResult.Distance < traceResult.Distance)
                    {
                        traceResult = curResult;
                    }
                }
            }
            return traceResult;
        }

        public TraceResult IntersectClosest(Vector3 pos)
        {
            TraceResult traceResult = null;
            foreach(TraceResult curResult in Intersect(pos))
            {
                if(traceResult == null)
                {
                    traceResult = curResult;
                }
                else
                {
                    if(curResult.Distance < traceResult.Distance)
                    {
                        traceResult = curResult;
                    }
                }
            }
            return traceResult;
        }

        public abstract IEnumerable<TraceResult> Intersect(Vector3 pos);
        public abstract IEnumerable<TraceResult> Intersect(Vector3 pos, Vector3 dir, bool query);
    }
}
